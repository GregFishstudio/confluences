using Confluences.Domain.Entities;
using Confluences.Persistence;
using IdentityServerAspNetIdentity.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace IdentityServerAspNetIdentity
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public static IConfiguration Configuration { get; private set; }

        private const string XForwardedPathBase = "X-Forwarded-PathBase";
        private const string XForwardedProto = "X-Forwarded-Proto";

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            // --- CORS pour ton front Vue.js ---
            services.AddCors(options =>
            {
                options.AddPolicy("default", policy =>
                {
                    policy.WithOrigins(
                        Configuration["URLVueJsGestionStagiaire"].TrimEnd('/')
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
            });

            services.AddControllersWithViews();
services.AddRazorPages();

            // --- Database ---
            services.AddEntityFrameworkNpgsql().AddDbContext<ConfluencesDbContext>(
                opt => opt.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"))
            );

            // --- Identity ---
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ConfluencesDbContext>()
                .AddDefaultTokenProviders()
                .AddClaimsPrincipalFactory<ClaimsFactory>();

            // --- IdentityServer ---
           var builder = services.AddIdentityServer(options =>
{
    options.IssuerUri = "http://localhost:5000";

    options.UserInteraction = new IdentityServer4.Configuration.UserInteractionOptions
    {
       LoginUrl = "/Identity/Account/Login",
        LogoutUrl = "/Identity/Account/Logout",
        ErrorUrl = "/Identity/Account/AccessDenied",
        LoginReturnUrlParameter = "returnUrl"
    };

    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;
})
.AddInMemoryIdentityResources(Config.IdentityResources)
.AddInMemoryApiResources(Config.ApiResources)
.AddInMemoryApiScopes(Config.ApiScopes)
.AddInMemoryClients(Config.Clients)
.AddAspNetIdentity<ApplicationUser>();

builder.AddDeveloperSigningCredential();


            services.ConfigureNonBreakingSameSiteCookies();
        }

        public void Configure(IApplicationBuilder app, ConfluencesDbContext dataContext)
        {
            // --- Cookie Policy ---
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Lax
            });

            app.UseAuthentication();
            app.UseCors("default");

            if (Environment.IsDevelopment() || Configuration["UseDeveloperExceptionPage"] == "true")
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();

            // --- Important pour Docker / reverse proxy ---
            var forwardedHeaderOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
            };
            forwardedHeaderOptions.KnownNetworks.Clear();
            forwardedHeaderOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedHeaderOptions);

            // --- IdentityServer ---
            app.UseIdentityServer();

            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                    endpoints.MapRazorPages();

            });
        }
    }
}
