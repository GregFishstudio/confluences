using Confluences.Persistence;
using Api.Utility;
using System;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using Syncfusion.Licensing;
using System.Security.Claims;
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
            // ----------------------------------------------------
            // LICENCES & COMPAT
            // ----------------------------------------------------
            SyncfusionLicenseProvider.RegisterLicense(Configuration["SyncfusionKey"]);
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            // ----------------------------------------------------
            // CORS
            // ----------------------------------------------------
            services.AddCors(options =>
            {
                options.AddPolicy("Frontend", builder =>
                {
                    builder
                        .WithOrigins(
                            "http://localhost:8080",
                            "http://localhost:5002",
                            "http://localhost",
                            "https://test-baseemploi.confluences.ch",
                            "https://baseemploi.confluences.ch",
                            "https://cours.confluences.ch"                            
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .WithExposedHeaders("Content-Disposition");
                });
            });

            // ----------------------------------------------------
            // CONTROLLERS / JSON
            // ----------------------------------------------------
            services.AddControllers()
                .AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.ReferenceLoopHandling =
                        Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

            services.Configure<FormOptions>(opt =>
            {
                opt.MultipartBodyLengthLimit = 1073741824; // 1GB
            });

            // ----------------------------------------------------
            // SWAGGER
            // ----------------------------------------------------
            services.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Confluences API",
                    Version = "v1",
                    Description = "API Confluences (Base Emploi + Plateforme)"
                });

                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            // ----------------------------------------------------
            // AUTHENTICATION JWT (IdentityServer4)
            // ----------------------------------------------------
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = Configuration["URLIdentityServer4"];
                    options.RequireHttpsMetadata = false;
                    options.Audience = "api1";

                    // 🔥 Empêche le JWT de casser les preflight OPTIONS
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = 401;
                            return System.Threading.Tasks.Task.CompletedTask;
                        }
                    };
                });

            // ----------------------------------------------------
            // EF CORE POSTGRES
            // ----------------------------------------------------
            services.AddDbContext<ConfluencesDbContext>(options =>
                options.UseNpgsql(
                    Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("Confluences.Persistence")
                ));

            // ----------------------------------------------------
            // IDENTITY
            // ----------------------------------------------------
            services.AddIdentityCore<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ConfluencesDbContext>()
                .AddDefaultTokenProviders();

            services.AddSingleton<
                Microsoft.AspNetCore.Authentication.IClaimsTransformation,
                KarekeClaimsTransformer
            >();

            // ----------------------------------------------------
            // AUTHORIZATION
            // ----------------------------------------------------
            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("Teacher", p => p.RequireClaim(ClaimTypes.Role, "Teacher"));
                opt.AddPolicy("Student", p => p.RequireClaim(ClaimTypes.Role, "Student"));
            });
            
            services.AddMemoryCache(); // 👈 manquait probablement

            // ----------------------------------------------------
            // AUTOMAPPER
            // ----------------------------------------------------
            services.AddAutoMapper(typeof(Startup));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // ----------------------------------------------------
            // REVERSE PROXY / HTTPS FIX
            // ----------------------------------------------------
            var forwardOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto
            };
            forwardOptions.KnownNetworks.Clear();
            forwardOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardOptions);

            // ----------------------------------------------------
            // PDF
            // ----------------------------------------------------
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // ----------------------------------------------------
            // SWAGGER
            // ----------------------------------------------------
            app.UseSwagger();
            app.UseSwaggerUI();

            // ----------------------------------------------------
            // PIPELINE
            // ----------------------------------------------------
            app.UseRouting();

            // 🔥 CORS AVANT AUTH
            app.UseCors("Frontend");

            app.UseAuthentication();
            app.UseAuthorization();

           app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers["Access-Control-Allow-Origin"] = 
            Configuration["URLMVC"];
    }
});

            app.UseEndpoints(endpoints =>
            {
                endpoints
                    .MapControllers()
                    .RequireCors("Frontend");
            });
        }
    }
}