# Confluences — refonte du site

Site statique multi-pages (HTML + CSS + un peu de JS, sans framework).

## Structure

- `src/pages/` — le contenu de chaque page (bloc de métadonnées en tête : titre, description, rubrique, couleur, h1, chapeau)
- `src/partials/` — gabarit commun (`_layout.html`), pied de page, calendrier des sessions, bandeau contact
- `build.py` — assemble les pages dans `public/`
- `public/` — le site final à déposer chez Infomaniak (avec `envoi.php` pour le formulaire)

## Travailler

```bash
python build.py                 # régénère public/*.html après une modification dans src/
cd public && php -S localhost:8080   # aperçu local (PHP nécessaire pour tester le formulaire)
```

Ne pas modifier les `.html` de `public/` à la main : ils sont écrasés à chaque build.
Le CSS (`public/assets/css/site.css`) et le JS (`public/assets/js/site.js`) se modifient directement.

## Couleurs

Les 5 rayons du logo = les 5 axes de la mission. Chaque page choisit son accent avec `accent: 1..5`.
Le calendrier des sessions se met à jour dans `src/partials/sessions.html`.
