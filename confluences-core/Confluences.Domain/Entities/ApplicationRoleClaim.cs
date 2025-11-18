using Microsoft.AspNetCore.Identity;

namespace Confluences.Domain.Entities
{
    public class ApplicationRoleClaim : IdentityRoleClaim<string>
    {
        public virtual ApplicationRole? Role { get; set; }
    }
}