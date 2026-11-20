using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Confluences.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Api.Controllers
{
    // Changement de mot de passe par l'utilisatrice connectée, depuis l'application (menu
    // « Changer mon mot de passe »). Même base de comptes que le serveur de connexion : le nouveau
    // mot de passe vaut dès la prochaine connexion.
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MotDePasseController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PasswordOptions _options;
        private readonly ILogger<MotDePasseController> _logger;

        public MotDePasseController(UserManager<ApplicationUser> userManager, IOptions<IdentityOptions> identityOptions,
            ILogger<MotDePasseController> logger)
        {
            _userManager = userManager;
            _options = identityOptions.Value.Password;
            _logger = logger;
        }

        // GET: api/MotDePasse/regles — règles affichées dans le formulaire.
        [HttpGet("regles")]
        public ActionResult<object> GetRegles() => new
        {
            longueurMin = _options.RequiredLength,
            majuscule = _options.RequireUppercase,
            minuscule = _options.RequireLowercase,
            chiffre = _options.RequireDigit,
            caractereSpecial = _options.RequireNonAlphanumeric,
        };

        // POST: api/MotDePasse  { actuel, nouveau }
        [HttpPost]
        public async Task<IActionResult> Changer([FromBody] ChangerMotDePasseDto dto)
        {
            if (string.IsNullOrEmpty(dto?.Actuel) || string.IsNullOrEmpty(dto.Nouveau))
                return BadRequest("Saisissez le mot de passe actuel et le nouveau.");
            if (dto.Actuel == dto.Nouveau)
                return BadRequest("Le nouveau mot de passe doit être différent de l'actuel.");

            var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var user = id == null ? null : await _userManager.FindByIdAsync(id);
            if (user == null) return Unauthorized();

            var result = await _userManager.ChangePasswordAsync(user, dto.Actuel, dto.Nouveau);
            if (!result.Succeeded)
                return BadRequest(string.Join(" ", result.Errors.Select(Traduire).Distinct()));

            _logger.LogInformation("Mot de passe changé par l'utilisateur {UserId}.", user.Id);
            return NoContent();
        }

        private string Traduire(IdentityError e) => e.Code switch
        {
            "PasswordMismatch" => "Le mot de passe actuel est incorrect.",
            "PasswordTooShort" => $"Le nouveau mot de passe doit comporter au moins {_options.RequiredLength} caractères.",
            "PasswordRequiresUpper" => "Il doit contenir au moins une majuscule.",
            "PasswordRequiresLower" => "Il doit contenir au moins une minuscule.",
            "PasswordRequiresDigit" => "Il doit contenir au moins un chiffre.",
            "PasswordRequiresNonAlphanumeric" => "Il doit contenir au moins un caractère spécial (ex. ! ? * -).",
            "PasswordRequiresUniqueChars" => "Il doit contenir plus de caractères différents.",
            _ => e.Description,
        };
    }

    public class ChangerMotDePasseDto
    {
        public string? Actuel { get; set; }
        public string? Nouveau { get; set; }
    }
}
