using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace IdentityServerAspNetIdentity
{
    public static class Config
    {
        private static IConfiguration Configuration => Startup.Configuration;

        // 1️⃣ Identity Resources
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource
                {
                    Name = "roles",
                    DisplayName = "User roles",
                    UserClaims = { JwtClaimTypes.Role }
                }
            };

        // 2️⃣ API Resources
        public static IEnumerable<ApiResource> ApiResources =>
            new ApiResource[]
            {
                new ApiResource("api1", "Confluences API")
                {
                    Scopes = { "api1" },
                    UserClaims = { JwtClaimTypes.Role }
                }
            };

        // 3️⃣ API Scopes
        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("api1", "Confluences API"),
            };

        // 4️⃣ Clients
        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                // --- A) Client WebComponent pour login (Resource Owner Password)
                new Client
                {
                    ClientId = "vue",
                    ClientName = "Confluences Login WebComponent",

                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    RequireClientSecret = true,
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api1",
                        "roles",
                        IdentityServerConstants.StandardScopes.OfflineAccess
                    },

                    AllowedCorsOrigins =
                    {
                        Configuration["URLClientMVC"].TrimEnd('/'),
                        Configuration["URLVueJsGestionStagiaire"].TrimEnd('/')
                    },

                    AllowOfflineAccess = true,
                    AccessTokenLifetime = 3600,
                    RefreshTokenUsage = TokenUsage.ReUse
                },


                // --- B) Client SPA Vue.js (Resource Owner Password)
                new Client
                {
                    ClientId = "confluences-web",
                    ClientName = "Confluences Front (Vue.js)",

                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    RequireClientSecret = false,

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api1",
                        "roles"
                    },

                    AllowedCorsOrigins =
                    {
                        Configuration["URLVueJsGestionStagiaire"].TrimEnd('/')
                    },

                    AllowOfflineAccess = true,
                    AccessTokenLifetime = 3600
                },


                // --- C) Client MVC (OIDC + PKCE)
                new Client
{
    ClientId = "mvc",
    ClientName = "Confluences MVC",

    AllowedGrantTypes = GrantTypes.Code,
    RequirePkce = true,
    RequireClientSecret = false,

    RedirectUris =
    {
        $"{Configuration["URLClientMVC"].TrimEnd('/')}/signin-oidc"
    },

    PostLogoutRedirectUris =
    {
        $"{Configuration["URLClientMVC"].TrimEnd('/')}/signout-callback-oidc"
    },

    FrontChannelLogoutUri =
        $"{Configuration["URLClientMVC"].TrimEnd('/')}/signout-oidc",

    AllowedScopes =
    {
        IdentityServerConstants.StandardScopes.OpenId,
        IdentityServerConstants.StandardScopes.Profile,
        "api1",
        "roles"
    },

    AllowOfflineAccess = true
}

            };
    }
}
