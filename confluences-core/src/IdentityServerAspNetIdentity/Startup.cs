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

            services.AddCors(options =>
            {
                // this defines a CORS policy called "default"
                options.AddPolicy("default", policy =>
                {
                    policy.WithOrigins(Configuration["URLVueJsGestionStagiaire"].Remove(Configuration["URLVueJsGestionStagiaire"].Length - 1))
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            services.AddControllersWithViews();

            //services.AddDbContext<ConfluencesDbContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            var test = Configuration.GetConnectionString("DefaultConnection");
            services.AddEntityFrameworkNpgsql().AddDbContext<ConfluencesDbContext>(
                opt => opt.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));



            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ConfluencesDbContext>()
                .AddDefaultTokenProviders()
                // pour les roles
                .AddClaimsPrincipalFactory<ClaimsFactory>();


            var builder = services.AddIdentityServer(options =>
                {
                    //options.PublicOrigin = Configuration["URLIdentityServer4"];
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                 })
                .AddInMemoryIdentityResources(Config.Ids)
                .AddInMemoryApiResources(Config.Apis)
                .AddInMemoryClients(Config.Clients)
                .AddAspNetIdentity<ApplicationUser>();

            builder.AddDeveloperSigningCredential();

            services.ConfigureNonBreakingSameSiteCookies();


        }

        public void Configure(IApplicationBuilder app, ConfluencesDbContext dataContext)
        {
            // Add this before any other middleware that might write cookies
            app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });

            // This will write cookies, so make sure it's after the cookie policy
            app.UseAuthentication();

            app.UseCors("default");

            if (Environment.IsDevelopment() || Configuration["UseDeveloperExceptionPage"] == "true")
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            var fordwardedHeaderOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            fordwardedHeaderOptions.KnownNetworks.Clear();
            fordwardedHeaderOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(fordwardedHeaderOptions);

            app.UseIdentityServer();

            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}