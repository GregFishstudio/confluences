using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Confluences.Domain.Entities;
using IdentityServerAspNetIdentity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace IdentityServerAspNetIdentity.Areas.Identity.Pages.Account
{
    // Nouveau mot de passe, depuis le lien reçu par e-mail (« Mot de passe oublié »).
    [AllowAnonymous]
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PasswordOptions _passwordOptions;

        public ResetPasswordModel(UserManager<ApplicationUser> userManager, IOptions<IdentityOptions> identityOptions)
        {
            _userManager = userManager;
            _passwordOptions = identityOptions.Value.Password;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string Regles => MessagesMotDePasse.Regles(_passwordOptions);
        public bool LienInvalide { get; private set; }

        public class InputModel
        {
            public string Email { get; set; }

            [Required(ErrorMessage = "Saisissez le nouveau mot de passe.")]
            [StringLength(100, ErrorMessage = "Le mot de passe doit comporter entre {2} et {1} caractères.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Nouveau mot de passe")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmer le nouveau mot de passe")]
            [Compare("Password", ErrorMessage = "La confirmation ne correspond pas au nouveau mot de passe.")]
            public string ConfirmPassword { get; set; }

            public string Code { get; set; }
        }

        public IActionResult OnGet(string code = null, string email = null)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(email))
            {
                LienInvalide = true;
                return Page();
            }
            try
            {
                Input = new InputModel { Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code)), Email = email };
            }
            catch (FormatException)
            {
                LienInvalide = true;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Input.Code) || string.IsNullOrEmpty(Input.Email))
            {
                LienInvalide = true;
                return Page();
            }
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Ne révèle pas si le compte existe.
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            if (result.Succeeded) return RedirectToPage("./ResetPasswordConfirmation");

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, MessagesMotDePasse.Traduire(error, _passwordOptions));
            return Page();
        }
    }
}
