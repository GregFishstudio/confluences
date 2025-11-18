using Microsoft.AspNetCore.Identity;

namespace Confluences.Domain.Entities
{
    public class ApplicationRole : IdentityRole<string>
    {
         public ApplicationRole()
        {
            Id = Guid.NewGuid().ToString();
        }
        public virtual ICollection<ApplicationUserRole>? UserRoles { get; set; }
        public virtual ICollection<ApplicationRoleClaim>? RoleClaims { get; set; }
    }
}
