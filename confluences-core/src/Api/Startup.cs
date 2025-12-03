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
using System.Security.Claims;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Confluences.Domain.Entities;

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
            // -----------------------------
            // Licences & context
            // -----------------------------
            SyncfusionLicenseProvider.RegisterLicense(Configuration["SyncfusionKey"]);
            System.AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true); // Fix EF Core + Postgres

            // -----------------------------
            // CORS
            // -----------------------------
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
                        .WithExposedHeaders("Content-Disposition");
                });
            });

            // -----------------------------
            // Controllers + JSON
            // -----------------------------
            services.AddControllers().AddNewtonsoftJson(opt =>
            {
                opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            services.Configure<FormOptions>(opt =>
            {
                opt.MultipartBodyLengthLimit = 1073741824; // 1 GB
            });

            // -----------------------------
            // Swagger
            // -----------------------------
            services.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Confluences API",
                    Description = "API Confluences (Base Emploi + Plateforme)"
                });
            });

            // -----------------------------
            // Authentication JWT (IdentityServer4)
            // -----------------------------
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = Configuration["URLIdentityServer4"];
                options.RequireHttpsMetadata = false;
                options.Audience = "api1";
            });

            // -----------------------------
            // EF Core Postgres
            // -----------------------------
            services.AddDbContext<ConfluencesDbContext>(options =>
    options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Confluences.Persistence")));


            // -----------------------------
            // Identity (API-friendly)
            // -----------------------------
            services.AddIdentityCore<ApplicationUser>(opt => { })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ConfluencesDbContext>()
                .AddDefaultTokenProviders();

            // Claims transformation
            services.AddSingleton<Microsoft.AspNetCore.Authentication.IClaimsTransformation, KarekeClaimsTransformer>();

            // -----------------------------
            // Authorization
            // -----------------------------
            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("Teacher", p => p.RequireClaim(ClaimTypes.Role, "Teacher"));
                opt.AddPolicy("Student", p => p.RequireClaim(ClaimTypes.Role, "Student"));
            });

            // -----------------------------
            // AutoMapper
            // -----------------------------
            services.AddAutoMapper(typeof(Startup));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // -----------------------------
            // PDF License (QuestPDF)
            // -----------------------------
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            // -----------------------------
            // CORS must be before routing
            // -----------------------------
            app.UseCors("Frontend");

            // -----------------------------
            // Reverse proxy / Docker
            // -----------------------------
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
