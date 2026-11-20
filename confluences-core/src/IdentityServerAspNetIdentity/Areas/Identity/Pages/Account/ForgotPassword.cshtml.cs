using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Confluences.Domain.Entities;
using IdentityServerAspNetIdentity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IdentityServerAspNetIdentity.Areas.Identity.Pages.Account
{
    // « Mot de passe oublié ? » (page de connexion de l'application) : envoie un lien de
    // réinitialisation à l'adresse e-mail du compte. Le compte se retrouve par e-mail ou identifiant.
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ConfigurableEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ForgotPasswordModel> _logger;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, ConfigurableEmailSender emailSender,
            IConfiguration configuration, ILogger<ForgotPasswordModel> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _configuration = configuration;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string Erreur { get; private set; }

        public string UrlConnexion => (_configuration["URLVueJsGestionStagiaire"] ?? "/") + "login";

        public class InputModel
        {
            [Required(ErrorMessage = "Saisissez votre adresse e-mail ou votre identifiant.")]
            [Display(Name = "Adresse e-mail ou identifiant")]
            public string Identifiant { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // Vérifié avant de chercher le compte : le message ne révèle pas si le compte existe.
            if (!_emailSender.EstConfigure)
            {
                Erreur = "L'envoi d'e-mails n'est pas encore configuré sur ce serveur. Demandez à l'administratrice de réinitialiser votre mot de passe.";
                return Page();
            }

            var saisie = Input.Identifiant.Trim();
            var user = saisie.Contains('@')
                ? await _userManager.FindByEmailAsync(saisie) ?? await _userManager.FindByNameAsync(saisie)
                : await _userManager.FindByNameAsync(saisie);

            // Compte inconnu ou sans e-mail : même confirmation, sans révéler si le compte existe.
            // (Les comptes des formatrices sont créés par l'administration : l'e-mail n'a pas été
            // « confirmé » au sens d'Identity, ce n'est donc pas exigé.)
            if (user != null && !string.IsNullOrWhiteSpace(user.Email))
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var lien = Url.Page("/Account/ResetPassword", pageHandler: null,
                    values: new { area = "Identity", code, email = user.Email }, protocol: Request.Scheme);
                var nom = $"{user.Firstname}".Trim();

                try
                {
                    await _emailSender.SendEmailAsync(user.Email, "Confluences : réinitialiser votre mot de passe",
                        $"<p>Bonjour{(nom.Length > 0 ? " " + HtmlEncoder.Default.Encode(nom) : "")},</p>"
                        + "<p>Vous avez demandé à réinitialiser votre mot de passe pour la Base emploi Confluences.</p>"
                        + $"<p><a href='{HtmlEncoder.Default.Encode(lien)}'>Choisir un nouveau mot de passe</a></p>"
                        + "<p>Ce lien ne sert qu'une fois. Si vous n'avez rien demandé, ignorez simplement ce message : votre mot de passe actuel reste valable.</p>"
                        + "<p>Confluences</p>");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Échec de l'envoi de l'e-mail de réinitialisation.");
                    Erreur = "L'e-mail n'a pas pu être envoyé. Réessayez plus tard, ou demandez à l'administratrice de réinitialiser votre mot de passe.";
                    return Page();
                }
            }

            return RedirectToPage("./ForgotPasswordConfirmation");
        }
    }
}
