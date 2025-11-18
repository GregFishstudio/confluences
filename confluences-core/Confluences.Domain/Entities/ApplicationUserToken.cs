using Microsoft.AspNetCore.Identity;

namespace Confluences.Domain.Entities
{
    public class ApplicationUserToken : IdentityUserToken<string>
    {
        public virtual ApplicationUser? User { get; set; }
    }
}
