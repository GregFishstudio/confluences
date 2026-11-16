"""
Assemble les pages du site Confluences.

  src/pages/*.html     contenu de chaque page (avec un bloc de métadonnées en tête)
  src/partials/*.html  fragments réutilisables, insérés avec {{> nom}}
  public/              site final, prêt à déposer chez l'hébergeur

Usage :  python build.py
"""
from pathlib import Path
import html
import re
import sys

sys.stdout.reconfigure(encoding="utf-8")

ROOT = Path(__file__).parent
PAGES = ROOT / "src" / "pages"
PARTIALS = ROOT / "src" / "partials"
OUT = ROOT / "public"

SITE = "Confluences"
BASE_URL = "https://www.confluences.ch/"

# Navigation principale : (clé, libellé, lien)
NAV = [
    ("association", "L’association", "association.html"),
    ("cours", "Cours de français", "cours.html"),
    ("projets", "Projets & formations", "projets.html"),
    ("parrainage", "Parrainage", "parrainage.html"),
    ("benevolat", "Bénévolat", "benevolat.html"),
]

# Sous-pages de chaque rubrique (affichées en sous-navigation)
SECTIONS = {
    "association": [
        ("association.html", "Mission"),
        ("equipe.html", "Équipe"),
        ("partenaires.html", "Partenaires"),
    ],
    "cours": [
        ("cours.html", "Vue d’ensemble"),
        ("francais-integration.html", "Français & Intégration"),
        ("francais-telecours.html", "Français en Télécours"),
        ("cours-evam.html", "Cours EVAM"),
    ],
    "projets": [
        ("projets.html", "Vue d’ensemble"),
        ("projets-culturels.html", "Projets culturels"),
        ("formation-formateurs.html", "Formation de formateurs·trices"),
    ],
}

SECTION_LABEL = {k: label for k, label, _ in NAV}
SECTION_LABEL["contact"] = "Contact"

# Les 5 rayons du logo (viewBox 0 0 480 850)
RAY_PATHS = [
    "M222 78 L292 10 L468 356 L448 374 Z",
    "M152 296 L288 296 L436 414 L420 436 Z",
    "M18 372 L420 474 L424 500 L164 500 Z",
    "M14 604 L420 540 L428 566 L298 636 L18 630 Z",
    "M296 700 L452 596 L466 616 L314 838 L286 830 Z",
]


def rays_svg(css_class, accent=None):
    paths = []
    for i, d in enumerate(RAY_PATHS, start=1):
        cls = f"r{i}" + (" is-accent" if accent and int(accent) == i else "")
        paths.append(f'<path class="{cls}" d="{d}"/>')
    return (f'<svg class="rays {css_class}" viewBox="-10 -10 500 870" aria-hidden="true" focusable="false">'
            + "".join(paths) + "</svg>")


def parse_page(text):
    m = re.match(r"\s*<!--(.*?)-->\s*", text, re.S)
    meta = {}
    if m:
        for line in m.group(1).strip().splitlines():
            if ":" in line:
                k, v = line.split(":", 1)
                meta[k.strip()] = v.strip()
        text = text[m.end():]
    return meta, text


def render_nav(active):
    items = []
    for key, label, href in NAV:
        cur = ' aria-current="page"' if key == active else ""
        items.append(f'<li><a href="{href}"{cur}>{label}</a></li>')
    cur = ' aria-current="page"' if active == "contact" else ""
    items.append(f'<li class="nav-cta"><a href="contact.html"{cur}>Nous contacter</a></li>')
    return "\n          ".join(items)


def render_subnav(section, filename):
    pages = SECTIONS.get(section)
    if not pages:
        return ""
    lis = []
    for href, label in pages:
        cur = ' aria-current="page"' if href == filename else ""
        lis.append(f'<li><a href="{href}"{cur}>{label}</a></li>')
    return (f'<nav class="subnav" aria-label="{SECTION_LABEL[section]}">'
            f'<div class="wrap"><ul>{"".join(lis)}</ul></div></nav>')


def render_page_hero(meta, filename):
    if "h1" not in meta:
        return ""
    section = meta.get("nav", "")
    crumbs = ['<li><a href="index.html">Accueil</a></li>']
    if section in SECTIONS and filename != SECTIONS[section][0][0]:
        top_href = SECTIONS[section][0][0]
        crumbs.append(f'<li><a href="{top_href}">{SECTION_LABEL[section]}</a></li>')
    crumbs.append(f'<li aria-current="page">{html.escape(meta.get("crumb", meta["h1"]))}</li>')
    eyebrow = meta.get("eyebrow", "")
    lede = meta.get("lede", "")
    return f"""
<header class="page-hero">
  {rays_svg("page-hero-rays", meta.get("accent"))}
  <div class="wrap">
    <ol class="breadcrumb" aria-label="Fil d’Ariane">{"".join(crumbs)}</ol>
    {f'<p class="eyebrow">{eyebrow}</p>' if eyebrow else ""}
    <h1>{meta["h1"]}</h1>
    {f'<p class="lede">{lede}</p>' if lede else ""}
  </div>
</header>"""


def include_partials(text, depth=0):
    if depth > 5:
        return text

    def repl(m):
        name = m.group(1)
        if name == "rays":
            return rays_svg("hero-rays")
        part = (PARTIALS / f"{name}.html").read_text(encoding="utf-8")
        return include_partials(part, depth + 1)

    return re.sub(r"\{\{>\s*([\w-]+)\s*\}\}", repl, text)


def build():
    layout = (PARTIALS / "_layout.html").read_text(encoding="utf-8")
    count = 0
    for page in sorted(PAGES.glob("*.html")):
        meta, body = parse_page(page.read_text(encoding="utf-8"))
        filename = page.name
        section = meta.get("nav", "")
        title = meta.get("title", SITE)
        full_title = title if filename == "index.html" else f"{title} — {SITE}"
        out = layout
        replacements = {
            "{{title}}": html.escape(full_title),
            "{{description}}": html.escape(meta.get("description", "")),
            "{{canonical}}": BASE_URL + ("" if filename == "index.html" else filename),
            "{{accent}}": meta.get("accent", "1"),
            "{{nav}}": render_nav(section),
            "{{subnav}}": render_subnav(section, filename),
            "{{page_hero}}": render_page_hero(meta, filename),
            "{{content}}": body,
        }
        for k, v in replacements.items():
            out = out.replace(k, v)
        out = include_partials(out)
        (OUT / filename).write_text(out, encoding="utf-8")
        count += 1
        print(f"  ✓ {filename}")
    print(f"{count} pages générées dans {OUT}")


if __name__ == "__main__":
    build()
