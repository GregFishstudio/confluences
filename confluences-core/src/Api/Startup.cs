using Confluences.Persistence;
using Api.Utility;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Syncfusion.Licensing;
using Microsoft.AspNetCore.Http.Features;

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

            // ✅ CORS bien défini
            services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:8080",             // Front local (vite dev)
                "https://localhost:8080",            // Si ton vite tourne en https
                "http://host.docker.internal:8080",  // Front vu depuis Docker
                "https://host.docker.internal:8080", // Variante https
                "http://localhost:5002",             // MVC local
                "https://localhost:5002"             // MVC via https
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("Content-Disposition");
    });
});


            // ✅ JSON
            services.AddControllers().AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );

            // ✅ Form upload 1 GB
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 1073741824; // 1 GB
            });

            // ✅ Swagger
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Confluences API",
                    Description = "Api for the baseemploi and plateforme collaborative",
                    Contact = new OpenApiContact
                    {
                        Name = "Tim Allemann",
                        Url = new Uri("https://auxitech.ch/contact/")
                    }
                });
            });

            // ✅ JWT config
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = Configuration["URLIdentityServer4"];
                    options.RequireHttpsMetadata = false;
                    options.Audience = "api1";
                });

            // ✅ PostgreSQL
            services.AddEntityFrameworkNpgsql().AddDbContext<ConfluencesDbContext>(
                opt => opt.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"))
            );

            // ✅ Claims
            services.AddSingleton<Microsoft.AspNetCore.Authentication.IClaimsTransformation, KarekeClaimsTransformer>();

            // ✅ Roles
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Teacher", policy => policy.RequireClaim(ClaimTypes.Role, "Teacher"));
                options.AddPolicy("Student", policy => policy.RequireClaim(ClaimTypes.Role, "Student"));
            });

            services.AddAutoMapper(typeof(Startup));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment() || Configuration["UseDeveloperExceptionPage"] == "true")
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // ⚠️ L'ordre ici est CRUCIAL

            app.UseRouting();

            // ✅ CORS doit être entre UseRouting et UseAuthentication
            app.UseCors("default");

            app.UseAuthentication();
            app.UseAuthorization();

            var forwardedHeaderOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            forwardedHeaderOptions.KnownNetworks.Clear();
            forwardedHeaderOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedHeaderOptions);

            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });
        }
    }
}
