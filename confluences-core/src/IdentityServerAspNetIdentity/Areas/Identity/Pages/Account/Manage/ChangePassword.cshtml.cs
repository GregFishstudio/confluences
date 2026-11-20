using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Confluences.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdentityServerAspNetIdentity.Areas.Identity.Pages.Account.Manage
{
    // Changement de mot de passe par l'utilisatrice elle-même, depuis le lien « Changer mon mot de
    // passe » de l'application (Base emploi). Il faut être connecté·e : sinon, passage par la page
    // de connexion, puis retour ici. `retour` = page de l'application où revenir ensuite.
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly PasswordOptions _passwordOptions;

        public ChangePasswordModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ChangePasswordModel> logger,
            IConfiguration configuration,
            IOptions<IdentityOptions> identityOptions)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _configuration = configuration;
            _passwordOptions = identityOptions.Value.Password;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        [BindProperty(SupportsGet = true)]
        public string Retour { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public string NomUtilisateur { get; private set; }

        // Adresse de retour vers l'application : seulement l'application elle-même (pas une
        // redirection vers un site extérieur), sinon sa page d'accueil.
        public string UrlRetour
        {
            get
            {
                var app = _configuration["URLVueJsGestionStagiaire"] ?? "/";
                return !string.IsNullOrEmpty(Retour) && Retour.StartsWith(app, StringComparison.OrdinalIgnoreCase)
                    ? Retour
                    : app;
            }
        }

        // Règles du mot de passe, telles que configurées dans Identity.
        public string Regles
        {
            get
            {
                var r = $"Au moins {_passwordOptions.RequiredLength} caractères";
                if (_passwordOptions.RequireUppercase) r += ", une majuscule";
                if (_passwordOptions.RequireLowercase) r += ", une minuscule";
                if (_passwordOptions.RequireDigit) r += ", un chiffre";
                if (_passwordOptions.RequireNonAlphanumeric) r += ", un caractère spécial (ex. ! ? * -)";
                return r + ".";
            }
        }

        public class InputModel
        {
            [Required(ErrorMessage = "Saisissez votre mot de passe actuel.")]
            [DataType(DataType.Password)]
            [Display(Name = "Mot de passe actuel")]
            public string OldPassword { get; set; }

            [Required(ErrorMessage = "Saisissez le nouveau mot de passe.")]
            [StringLength(100, ErrorMessage = "Le nouveau mot de passe doit comporter entre {2} et {1} caractères.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Nouveau mot de passe")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmer le nouveau mot de passe")]
            [Compare("NewPassword", ErrorMessage = "La confirmation ne correspond pas au nouveau mot de passe.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            NomUtilisateur = NomDe(user);

            if (!await _userManager.HasPasswordAsync(user))
                return RedirectToPage("./SetPassword");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            NomUtilisateur = NomDe(user);

            if (!ModelState.IsValid) return Page();

            var result = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, Traduire(error));
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("Mot de passe changé par l'utilisateur {UserId}.", user.Id);
            StatusMessage = "Votre mot de passe a été changé. Utilisez-le dès votre prochaine connexion.";

            return RedirectToPage(new { retour = Retour });
        }

        private static string NomDe(ApplicationUser u)
        {
            var nom = $"{u.Firstname} {u.LastName}".Trim();
            return nom.Length > 0 ? nom : u.UserName;
        }

        // Messages d'Identity (en anglais) traduits pour les formatrices.
        private string Traduire(IdentityError e) => e.Code switch
        {
            "PasswordMismatch" => "Le mot de passe actuel est incorrect.",
            "PasswordTooShort" => $"Le nouveau mot de passe doit comporter au moins {_passwordOptions.RequiredLength} caractères.",
            "PasswordRequiresUpper" => "Le nouveau mot de passe doit contenir au moins une majuscule.",
            "PasswordRequiresLower" => "Le nouveau mot de passe doit contenir au moins une minuscule.",
            "PasswordRequiresDigit" => "Le nouveau mot de passe doit contenir au moins un chiffre.",
            "PasswordRequiresNonAlphanumeric" => "Le nouveau mot de passe doit contenir au moins un caractère spécial (ex. ! ? * -).",
            "PasswordRequiresUniqueChars" => "Le nouveau mot de passe doit contenir plus de caractères différents.",
            _ => e.Description,
        };
    }
}
