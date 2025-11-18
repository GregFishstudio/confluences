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

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            SyncfusionLicenseProvider.RegisterLicense(Configuration["SyncfusionKey"]);
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            services.AddCors(options =>
            {
                // this defines a CORS policy called "default"
                options.AddPolicy("default", policy =>
                {
                    policy.WithOrigins(Configuration["URLVueJsGestionStagiaire"], Configuration["URLMVC"])
                    .WithExposedHeaders("Content-Disposition")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });


            services.AddControllers().AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );
            services.Configure<FormOptions>(options =>
            {
                // Set the limit to 1 GB
                options.MultipartBodyLengthLimit = 1073741824; // 1 GB
            });

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

            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = Configuration["URLIdentityServer4"];

                    options.RequireHttpsMetadata = false;

                    options.Audience = "api1";
                });
            //services.AddDbContext<ConfluencesDbContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddEntityFrameworkNpgsql().AddDbContext<ConfluencesDbContext>(
                opt => opt.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            services.AddSingleton<Microsoft.AspNetCore.Authentication.IClaimsTransformation, KarekeClaimsTransformer>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Teacher", policy => policy.RequireClaim(ClaimTypes.Role, "Teacher"));
                options.AddPolicy("Student", policy => policy.RequireClaim(ClaimTypes.Role, "Student"));
                //options.AddPolicy("Teacher", policy => {
                //    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                //    policy.RequireClaim("role", "Teacher");
                //});

            });

            services.AddAutoMapper(typeof(Startup));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
           // QuestPDF.Settings.License = LicenseType.Community;

            if (env.IsDevelopment() || Configuration["UseDeveloperExceptionPage"] == "true")
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("default");

            var fordwardedHeaderOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            fordwardedHeaderOptions.KnownNetworks.Clear();
            fordwardedHeaderOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(fordwardedHeaderOptions);

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

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
