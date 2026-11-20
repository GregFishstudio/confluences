using Microsoft.AspNetCore.Identity;

namespace IdentityServerAspNetIdentity.Services
{
    // Messages d'Identity (en anglais) traduits pour les formatrices.
    public static class MessagesMotDePasse
    {
        public static string Regles(PasswordOptions o)
        {
            var r = $"Au moins {o.RequiredLength} caractères";
            if (o.RequireUppercase) r += ", une majuscule";
            if (o.RequireLowercase) r += ", une minuscule";
            if (o.RequireDigit) r += ", un chiffre";
            if (o.RequireNonAlphanumeric) r += ", un caractère spécial (ex. ! ? * -)";
            return r + ".";
        }

        public static string Traduire(IdentityError e, PasswordOptions o) => e.Code switch
        {
            "PasswordMismatch" => "Le mot de passe actuel est incorrect.",
            "InvalidToken" => "Ce lien de réinitialisation n'est plus valable (déjà utilisé ou trop ancien). Demandez-en un nouveau.",
            "PasswordTooShort" => $"Le mot de passe doit comporter au moins {o.RequiredLength} caractères.",
            "PasswordRequiresUpper" => "Le mot de passe doit contenir au moins une majuscule.",
            "PasswordRequiresLower" => "Le mot de passe doit contenir au moins une minuscule.",
            "PasswordRequiresDigit" => "Le mot de passe doit contenir au moins un chiffre.",
            "PasswordRequiresNonAlphanumeric" => "Le mot de passe doit contenir au moins un caractère spécial (ex. ! ? * -).",
            "PasswordRequiresUniqueChars" => "Le mot de passe doit contenir plus de caractères différents.",
            _ => e.Description,
        };
    }
}
