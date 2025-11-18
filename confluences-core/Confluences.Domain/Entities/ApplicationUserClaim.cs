using Microsoft.AspNetCore.Identity;

namespace Confluences.Domain.Entities
{
    public class ApplicationUserClaim : IdentityUserClaim<string>
    {
        public virtual ApplicationUser? User { get; set; }
    }
}