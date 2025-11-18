using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Threading.Tasks;
using Confluences.Domain.Entities;
using Confluences.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace IdentityServerAspNetIdentity
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {Message:lj}{NewLine}{Exception}",
                    theme: AnsiConsoleTheme.Literate)
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("🚀 Starting IdentityServer...");
                var host = CreateHostBuilder(args).Build();

                // 🔥 Run seeding logic before running
                await SeedAsync(host);

                await host.RunAsync();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "💥 Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseSerilog();
                });

        /// <summary>
        /// 🔥 Seed database roles + admin user
        /// </summary>
        private static async Task SeedAsync(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<ConfluencesDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

                // Ensure DB exists
                await context.Database.EnsureCreatedAsync();

                // 1️⃣ Ensure Gender "Unknown"
                var gender = await context.Genders.FirstOrDefaultAsync();
                if (gender == null)
                {
                    gender = new Gender { GenderName = "Unknown" };
                    context.Genders.Add(gender);
                    await context.SaveChangesAsync();

                    Log.Information("🧩 Added default Gender 'Unknown'");
                }

                // 2️⃣ Ensure Teacher role exists
                const string teacherRole = "Teacher";
                if (!await roleManager.RoleExistsAsync(teacherRole))
                {
                    await roleManager.CreateAsync(new ApplicationRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = teacherRole,
                        NormalizedName = teacherRole.ToUpperInvariant()
                    });

                    Log.Information("👔 Role created: Teacher");
                }

                // 3️⃣ Ensure admin user exists
                string adminEmail = "admin@confluences.ch";

                // 🔥 Password loaded from environment (safe)
                string adminPassword =
                    Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
                    ?? "Confluences2025!"; // fallback for dev

                var admin = await userManager.FindByEmailAsync(adminEmail);
                if (admin == null)
                {
                    admin = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        Firstname = "Admin",
                        LastName = "Confluences",
                        GenderId = gender.GenderId
                    };

                    var createResult = await userManager.CreateAsync(admin, adminPassword);
                    if (createResult.Succeeded)
                        Log.Information("👤 Admin user created");
                    else
                        Log.Warning("⚠️ Failed to create admin: {0}", string.Join(", ", createResult.Errors));
                }

                // 4️⃣ Ensure admin has the teacher role
                if (!await userManager.IsInRoleAsync(admin, teacherRole))
                {
                    await userManager.AddToRoleAsync(admin, teacherRole);
                    Log.Information("🔗 Assigned role Teacher to admin");
                }

                Log.Information("🎉 Database seeding complete!");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "💥 Error during seeding");
            }
        }
    }
}
