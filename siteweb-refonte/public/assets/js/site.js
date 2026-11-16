(function () {
  // Menu mobile
  var toggle = document.querySelector(".menu-toggle");
  var nav = document.getElementById("main-nav");
  if (toggle && nav) {
    var setOpen = function (open) {
      toggle.setAttribute("aria-expanded", open ? "true" : "false");
      nav.classList.toggle("is-open", open);
    };
    toggle.addEventListener("click", function () {
      setOpen(toggle.getAttribute("aria-expanded") !== "true");
    });
    document.addEventListener("keydown", function (e) {
      if (e.key === "Escape" && nav.classList.contains("is-open")) { setOpen(false); toggle.focus(); }
    });
    window.addEventListener("resize", function () { if (window.innerWidth > 1040) setOpen(false); });
  }

  // Ombre de l'en-tête au défilement
  var header = document.querySelector(".site-header");
  if (header) {
    var onScroll = function () { header.classList.toggle("is-scrolled", window.scrollY > 8); };
    onScroll();
    window.addEventListener("scroll", onScroll, { passive: true });
  }

  // Année du pied de page
  var y = document.getElementById("year");
  if (y) y.textContent = new Date().getFullYear();

  // Retour du formulaire de contact (?envoi=ok|erreur)
  var status = document.getElementById("form-status");
  if (status) {
    var p = new URLSearchParams(window.location.search).get("envoi");
    if (p === "ok") {
      status.textContent = "Message envoyé. Nous vous répondons en général sous quelques jours.";
      status.className = "form-status is-ok";
    } else if (p === "erreur") {
      status.textContent = "Le message n’a pas pu être envoyé. Vérifiez votre adresse e-mail, ou écrivez-nous directement à confluences.vaud@gmail.com.";
      status.className = "form-status is-error";
    }
    if (p) status.setAttribute("tabindex", "-1"), status.focus();
  }
})();
