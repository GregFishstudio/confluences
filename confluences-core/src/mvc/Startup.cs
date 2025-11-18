using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Confluences.Infrastructure.Hubs;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using mvc.Utility;
using Syncfusion.Licensing;

namespace mvc
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; set; }

        // ----------------------------
        // SERVICES
        // ----------------------------
        public void ConfigureServices(IServiceCollection services)
        {
            // Licence Syncfusion
            SyncfusionLicenseProvider.RegisterLicense(Configuration["SyncfusionKey"]);

            services.AddControllersWithViews();

            // Taille max upload (1 GB)
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 1073741824; // 1 GB
            });

            // Ne pas remapper automatiquement les claims JWT
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            Console.WriteLine("Authority is : " + Configuration["URLIdentityServer4"]);

            // ----------------------------
            // AUTHENTIFICATION / OIDC
            // ----------------------------
            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = "Cookies";
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie("Cookies", options =>
                {
                    // IMPORTANT : on est en HTTP -> pas de Secure obligatoire
                    options.Cookie.SameSite = SameSiteMode.Unspecified;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.SignInScheme = "Cookies";
                    options.UsePkce = true;

                    // Authority = IdentityServer (dans Docker)
                    options.Authority = Configuration["URLIdentityServer4"]; // ex: http://host.docker.internal:5000/
                    options.RequireHttpsMetadata = false;                     // car HTTP en local

                    options.ClientId = "mvc";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";

                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.Scope.Add("api1");
                    options.Scope.Add("roles");
                    options.Scope.Add("offline_access");

                    // Mapping des rôles
                    options.ClaimActions.MapJsonKey("role", "role", "role");
                    options.TokenValidationParameters.NameClaimType = "name";
                    options.TokenValidationParameters.RoleClaimType = "role";

                    // FIX CORRELATION / NONCE POUR HTTP (sinon Correlation failed)
                    options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
                    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.None;

                    options.NonceCookie.SameSite = SameSiteMode.Unspecified;
                    options.NonceCookie.SecurePolicy = CookieSecurePolicy.None;

                    // Chemin de callback du MVC
                    options.CallbackPath = "/signin-oidc";
                });

            // ----------------------------
            // AUTORISATION
            // ----------------------------
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Teacher",
                    policy => policy.RequireClaim(JwtClaimTypes.Role, "Teacher"));
            });

            // ----------------------------
            // LOCALISATION
            // ----------------------------
            services.AddLocalization(opts => { opts.ResourcesPath = "Resources"; });

            services.AddMvc()
                .AddViewLocalization(
                    LanguageViewLocationExpanderFormat.Suffix,
                    opts => { opts.ResourcesPath = "Resources"; })
                .AddDataAnnotationsLocalization();

            services.Configure<RequestLocalizationOptions>(opts =>
            {
                var supportedCultures = new List<CultureInfo>
                {
                    new CultureInfo("fr-FR"),
                    new CultureInfo("fr")
                };

                opts.DefaultRequestCulture = new RequestCulture("fr-FR");
                opts.SupportedCultures = supportedCultures;
                opts.SupportedUICultures = supportedCultures;
            });

            // Helper déjà présent dans ton projet pour SameSite
            services.ConfigureNonBreakingSameSiteCookies();

            // SignalR
            services.AddSignalR();
        }

        // ----------------------------
        // PIPELINE HTTP
        // ----------------------------
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Proxy / Docker (X-Forwarded-For / Proto)
            var forwardedHeaderOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            forwardedHeaderOptions.KnownNetworks.Clear();
            forwardedHeaderOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedHeaderOptions);

            if (env.IsDevelopment() || Configuration["UseDeveloperExceptionPage"] == "true")
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Fichiers statiques (CSS, JS, images)
            app.UseStaticFiles();

            // Localisation
            var locOptions = app.ApplicationServices
                .GetService<IOptions<RequestLocalizationOptions>>();
            if (locOptions != null)
            {
                app.UseRequestLocalization(locOptions.Value);
            }

            app.UseRouting();

            // Politique cookies : laisser SameSite tel que défini sur chaque cookie
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Unspecified
            });

            // Auth + Autorisation
            app.UseAuthentication();
            app.UseAuthorization();

            // Endpoints MVC + SignalR
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}/{id2?}");

                endpoints.MapHub<ChatHub>(ChatHub.Url);
                endpoints.MapHub<JitsiHub>(JitsiHub.Url);
            });
        }
    }
}
