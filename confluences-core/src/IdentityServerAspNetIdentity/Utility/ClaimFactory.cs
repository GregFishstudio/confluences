using IdentityModel;
using Confluences.Persistence;
using Confluences.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Data;

namespace IdentityServerAspNetIdentity.Utility
{
    public class ClaimsFactory : UserClaimsPrincipalFactory<ApplicationUser>
    {
        private readonly ConfluencesDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClaimsFactory(
            UserManager<ApplicationUser> userManager,
            IOptions<IdentityOptions> optionsAccessor,
            ConfluencesDbContext context) : base(userManager, optionsAccessor)
        {
            _context = context;
            _userManager = userManager;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            identity.AddClaims(roles.Select(role => new Claim(JwtClaimTypes.Role, role)));
            identity.AddClaim(new Claim(JwtClaimTypes.GivenName, string.Join(" ", user.Firstname, user.LastName)));

            return identity;
        }
    }
}
