using IdentityServer4.Validation;
using Confluences.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerAspNetIdentity
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResourceOwnerPasswordValidator(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var userTask = _userManager.FindByNameAsync(context.UserName);
            var user = userTask.Result;

            context.Result = new GrantValidationResult(user.Id.ToString(), "password", null, "local", null);
            return Task.FromResult(context.Result);
        }
    }
}
