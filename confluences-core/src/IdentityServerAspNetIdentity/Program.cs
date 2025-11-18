using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Threading.Tasks;
using Confluences.Domain.Entities;
using Confluences.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
                Log.Information("🚀 Starting Confluences IdentityServer...");

                var host = CreateHostBuilder(args).Build();

                using (var scope = host.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;

                    try
                    {
                        var context = services.GetRequiredService<ConfluencesDbContext>();
                        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

                        // ✅ Ensure database exists
                        context.Database.EnsureCreated();

                        // 👇 Create default Gender if missing
                        var gender = await context.Genders.FirstOrDefaultAsync();
                        if (gender == null)
                        {
                            gender = new Gender { GenderName = "Unknown" };
                            context.Genders.Add(gender);
                            await context.SaveChangesAsync();
                            Log.Information("🧩 Default gender 'Unknown' created.");
                        }

                        // 👇 Create the single "Teacher" role
                        const string teacherRole = "Teacher";
                        if (!await roleManager.RoleExistsAsync(teacherRole))
                        {
                            var role = new ApplicationRole
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = teacherRole,
                                NormalizedName = teacherRole.ToUpperInvariant()
                            };
                            var roleResult = await roleManager.CreateAsync(role);
                            if (roleResult.Succeeded)
                                Log.Information("✅ Role created: Teacher");
                            else
                                Log.Warning("⚠️ Role creation failed: {0}", string.Join(", ", roleResult.Errors));
                        }

                        // 👇 Create default admin@confluences.ch account
                        var adminEmail = "admin@confluences.ch";
                        var adminPassword = "Confluences2025!";

                        var adminUser = await userManager.FindByEmailAsync(adminEmail);
                        if (adminUser == null)
                        {
                            adminUser = new ApplicationUser
                            {
                                Id = Guid.NewGuid().ToString(),
                                UserName = adminEmail,
                                Email = adminEmail,
                                EmailConfirmed = true,
                                Firstname = "Admin",
                                LastName = "Confluences",
                                GenderId = gender.GenderId, // ✅ correspond à ta clé short
                                HasSeenHelpVideo = false,
                                WantsMoreHomeworks = false,
                            };

                            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                            if (createResult.Succeeded)
                                Log.Information($"👤 User created: {adminEmail}");
                            else
                                Log.Warning($"⚠️ User creation failed: {string.Join(", ", createResult.Errors)}");
                        }

                        // 👇 Add the Teacher role to the admin
                        if (!await userManager.IsInRoleAsync(adminUser, teacherRole))
                        {
                            await userManager.AddToRoleAsync(adminUser, teacherRole);
                            Log.Information($"✅ Role {teacherRole} assigned to {adminEmail}");
                        }

                        Log.Information("🎉 Initialization complete!");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "💥 Error initializing roles or default user");
                    }
                }

                await host.RunAsync();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "💀 Host terminated unexpectedly");
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
    }
}
