using Confluences.Persistence;
using Api.Utility;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http.Features;
using Syncfusion.Licensing;
using System;
using System.Security.Claims;
using QuestPDF.Infrastructure;

namespace Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            SyncfusionLicenseProvider.RegisterLicense(Configuration["SyncfusionKey"]);
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            //
            // 🔥 CORS UNIQUE — autorisation du front Vue + MVC
            //
            services.AddCors(options =>
{
    options.AddPolicy("Frontend", builder =>
    {
        builder
            .WithOrigins(
                "http://localhost:8080",
                "http://localhost:5002",
                "http://localhost"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("Content-Disposition"); // 🔥 OBLIGATOIRE POUR PDF
    });
});

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling =
                    Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 1073741824; // 1GB
            });

            //
            // Swagger
            //
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Confluences API",
                    Description = "API Confluences (Base Emploi + Plateforme)",
                });
            });

            //
            // Auth JWT / IdentityServer4
            //
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = Configuration["URLIdentityServer4"];
                    options.RequireHttpsMetadata = false;
                    options.Audience = "api1";
                });

            //
            // EF Core Postgres
            //
            services.AddEntityFrameworkNpgsql().AddDbContext<ConfluencesDbContext>(opt =>
                opt.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"))
            );

            services.AddSingleton<
                Microsoft.AspNetCore.Authentication.IClaimsTransformation,
                KarekeClaimsTransformer>();

            //
            // Authorization
            //
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Teacher", p => p.RequireClaim(ClaimTypes.Role, "Teacher"));
                options.AddPolicy("Student", p => p.RequireClaim(ClaimTypes.Role, "Student"));
            });

            services.AddAutoMapper(typeof(Startup));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;


            app.UseSwagger();
            app.UseSwaggerUI();

            //
            // 🔥 CORS DOIT être ici avant UseRouting
            //
            //app.UseCors("AllowAllFrontend");
            app.UseCors("Frontend");

            //
            // Reverse proxy / docker / nginx
            //
            var forwardOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            forwardOptions.KnownNetworks.Clear();
            forwardOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardOptions);

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
