using Microsoft.AspNetCore.Mvc;
using Confluences.Persistence;
using Confluences.Domain.Entities; 
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestPopulateController : ControllerBase
    {
        private readonly ConfluencesDbContext _db;
        private readonly PasswordHasher<ApplicationUser> _hasher;

        public TestPopulateController(ConfluencesDbContext db)
        {
            _db = db;
            _hasher = new PasswordHasher<ApplicationUser>();
        }

        [HttpPost("populate")]
        public async Task<IActionResult> Populate()
        {
            if (!Request.Host.Host.Contains("localhost") && !Request.Host.Host.Contains("127.0.0.1"))
                return BadRequest("Cette route n'est disponible qu'en local.");

            try
            {
                await NuclearCleanupAsync();
                string result = await SeedDataAsync();
                return Ok(result);
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, $"Erreur BD: {innerMessage}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur: {ex.Message}");
            }
        }

        // ====================================================================
        // 🧹 PHASE 1: Nettoyage Total + Reset des Séquences
        // ====================================================================
        private async Task NuclearCleanupAsync()
        {
            Console.WriteLine("🧹 Nettoyage...");

            _db.Stages.RemoveRange(_db.Stages);
            _db.Contacts.RemoveRange(_db.Contacts);
            _db.EntrepriseDomaines.RemoveRange(_db.EntrepriseDomaines);
            _db.EntrepriseMetiers.RemoveRange(_db.EntrepriseMetiers);
            _db.EntrepriseOffres.RemoveRange(_db.EntrepriseOffres);
            await _db.SaveChangesAsync();

            _db.Entreprises.RemoveRange(_db.Entreprises);
            await _db.SaveChangesAsync();

            _db.UserRoles.RemoveRange(_db.UserRoles);

            var allUsers = await _db.Users.ToListAsync();
            if (allUsers.Any())
            {
                _db.Users.RemoveRange(allUsers);
                await _db.SaveChangesAsync();
            }

            _db.Roles.RemoveRange(_db.Roles);

            _db.TypeMetiers.RemoveRange(_db.TypeMetiers);
            _db.TypeIntershipActivities.RemoveRange(_db.TypeIntershipActivities);
            _db.TypeAffiliations.RemoveRange(_db.TypeAffiliations);
            _db.TypeEntreprises.RemoveRange(_db.TypeEntreprises);
            _db.TypeAnnonces.RemoveRange(_db.TypeAnnonces);
            _db.TypeDomaines.RemoveRange(_db.TypeDomaines);
            _db.TypeMoyens.RemoveRange(_db.TypeMoyens);
            _db.Genders.RemoveRange(_db.Genders);

            await _db.SaveChangesAsync();

            // === Reset sequences PostgreSQL ===
            var sequences = new[]
            {
                "Genders_GenderId_seq",
                "TypeAffiliations_TypeAffiliationId_seq",
                "TypeEntreprises_TypeEntrepriseId_seq",
                "TypeDomaines_TypeDomaineId_seq",
                "TypeMetiers_TypeMetierId_seq",
                "TypeIntershipActivities_TypeIntershipActivityId_seq",
                "Entreprises_EntrepriseId_seq",
                "Contacts_ContactId_seq",
                "Stages_StageId_seq"
            };

            foreach (var seq in sequences)
            {
                try
                {
                    await _db.Database.ExecuteSqlRawAsync($"ALTER SEQUENCE \"{seq}\" RESTART WITH 1;");
                }
                catch { }
            }

            Console.WriteLine("✅ Nettoyage terminé.");
        }

        // ====================================================================
        // 🌱 PHASE 2: Peuplement des Données
        // ====================================================================
        private async Task<string> SeedDataAsync()
        {
            Console.WriteLine("🌱 Peuplement...");

            // ---- 1. GENDERS ----
            _db.Genders.AddRange(
                new Gender { GenderName = "Homme" },
                new Gender { GenderName = "Femme" },
                new Gender { GenderName = "Autre" }
            );
            await _db.SaveChangesAsync();

            // ---- 2. TypeAffiliations ----
            _db.TypeAffiliations.AddRange(
                new TypeAffiliation { Libelle = "Admin", Code = "ADM" },
                new TypeAffiliation { Libelle = "Stagiaire", Code = "STG" },
                new TypeAffiliation { Libelle = "Formateur", Code = "FRM" }
            );
            await _db.SaveChangesAsync();

            // ---- 3. ROLE ----
            var adminRole = new ApplicationRole { Id = "role_teacher", Name = "Teacher", NormalizedName = "TEACHER" };
            _db.Roles.Add(adminRole);
            await _db.SaveChangesAsync();

            // ---- 4. USERS ----
            var users = GetTestUsers(1, 2);

            foreach (var user in users)
            {
                user.PasswordHash = _hasher.HashPassword(user, "Confluences2025!");
                user.SecurityStamp = Guid.NewGuid().ToString();
                _db.Entry(user).Property(u => u.Id).IsTemporary = false;
            }

            _db.Users.AddRange(users);
            await _db.SaveChangesAsync();

            // Assign ROLE
            _db.UserRoles.Add(new ApplicationUserRole { UserId = "admin1", RoleId = "role_teacher" });
            await _db.SaveChangesAsync();

            // ---- 5. Types ----
            _db.TypeEntreprises.AddRange(GetTestTypeEntreprises());
            _db.TypeDomaines.AddRange(GetTestTypeDomaines());
            _db.TypeMetiers.AddRange(GetTestTypeMetiers());
            _db.TypeIntershipActivities.AddRange(GetTestTypeIntershipActivities());
            await _db.SaveChangesAsync();

            // ---- 6. Entreprises ----
            _db.Entreprises.AddRange(GetTestEntreprises("admin1"));
            await _db.SaveChangesAsync();

            // ---- 7. Contacts + Stages ----
            _db.Contacts.AddRange(GetTestContacts());
            _db.Stages.AddRange(GetTestStages("admin1"));
            await _db.SaveChangesAsync();

            return "✔ DB Reset Complete. Admin: admin@confluences.ch / Confluences2025!";
        }

        // ===========================================================
        // 📚 Données statiques
        // ===========================================================
        private static List<ApplicationUser> GetTestUsers(int adminAffiliationId, int stagiaireAffiliationId) => new()
        {
            new ApplicationUser { Id="admin1", Firstname="Admin", LastName="Confluences", Email="admin@confluences.ch", UserName="admin@confluences.ch", NormalizedEmail="ADMIN@CONFLUENCES.CH", NormalizedUserName="ADMIN@CONFLUENCES.CH", EmailConfirmed=true, TypeAffiliationId=adminAffiliationId, GenderId=1 },
            new ApplicationUser { Id="s1", Firstname="Jean", LastName="Dupont", Email="jean@test.com", UserName="jean@test.com", NormalizedEmail="JEAN@TEST.COM", NormalizedUserName="JEAN@TEST.COM", EmailConfirmed=true, GenderId=1, TypeAffiliationId=stagiaireAffiliationId },
            new ApplicationUser { Id="s2", Firstname="Maria", LastName="Lopez", Email="maria@test.com", UserName="maria@test.com", NormalizedEmail="MARIA@TEST.COM", NormalizedUserName="MARIA@TEST.COM", EmailConfirmed=true, GenderId=2, TypeAffiliationId=stagiaireAffiliationId },
            new ApplicationUser { Id="s3", Firstname="Alex", LastName="Martin", Email="alex@test.com", UserName="alex@test.com", NormalizedEmail="ALEX@TEST.COM", NormalizedUserName="ALEX@TEST.COM", EmailConfirmed=true, GenderId=1, TypeAffiliationId=stagiaireAffiliationId },
        };

        private static List<TypeEntreprise> GetTestTypeEntreprises() => new()
        {
            new TypeEntreprise { Nom="Privé" },
            new TypeEntreprise { Nom="Public" },
            new TypeEntreprise { Nom="Association" },
            new TypeEntreprise { Nom="Startup" }
        };

        private static List<TypeDomaine> GetTestTypeDomaines() => new()
        {
            new TypeDomaine { Libelle="Informatique", OldNames="IT, Dev, Data" },
            new TypeDomaine { Libelle="Ressources Humaines", OldNames="RH, Recrutement" },
            new TypeDomaine { Libelle="Comptabilité", OldNames="Finance, Budget" },
            new TypeDomaine { Libelle="Marketing", OldNames="Web, Communication" },
            new TypeDomaine { Libelle="Logistique", OldNames="Stock, Transport" },
        };

        private static List<TypeMetier> GetTestTypeMetiers() => new()
        {
            new TypeMetier { Libelle="Assistant administratif" },
            new TypeMetier { Libelle="Aide comptable" },
            new TypeMetier { Libelle="Employé de commerce" },
            new TypeMetier { Libelle="Développeur Web" },
            new TypeMetier { Libelle="Graphiste" },
        };

        private static List<TypeIntershipActivity> GetTestTypeIntershipActivities() => new()
        {
            new TypeIntershipActivity { Nom="Accueil" },
            new TypeIntershipActivity { Nom="Archivage" },
            new TypeIntershipActivity { Nom="Saisie" },
        };

        private static List<Entreprise> GetTestEntreprises(string adminId) => new()
        {
            new Entreprise { Nom="Migros Vaud", Ville="Ecublens", CreateurId=adminId, DateCreation=new DateTime(2023,10,1), TypeEntrepriseId=1, Remarque="Grande distribution." },
            new Entreprise { Nom="Ville de Lausanne", Ville="Lausanne", CreateurId=adminId, DateCreation=new DateTime(2022,5,15), TypeEntrepriseId=2, Remarque="Service public." },
            new Entreprise { Nom="InnovTech Sàrl", Ville="Fribourg", CreateurId=adminId, DateCreation=new DateTime(2024,1,1), TypeEntrepriseId=4, Remarque="Startup spécialisée en IA." },
            new Entreprise { Nom="Croix-Rouge Suisse", Ville="Berne", CreateurId=adminId, DateCreation=new DateTime(2021,8,10), TypeEntrepriseId=3, Remarque="Organisation humanitaire." }
        };

        private static List<Contact> GetTestContacts() => new()
        {
            new Contact { EntrepriseId=1, Prenom="Marc", Nom="Berger", Fonction="Directeur RH", Email="marc.b@migros.ch" },
            new Contact { EntrepriseId=2, Prenom="Elodie", Nom="Dubois", Fonction="Assistante", Email="elodie.d@lausanne.ch" },
            new Contact { EntrepriseId=3, Prenom="Sophie", Nom="Chen", Fonction="CTO", Email="sophie.c@innov.ch" },
        };

        private static List<Stage> GetTestStages(string adminId) => new()
        {
            new Stage { Nom="Stage WebDev", Debut=new DateTime(2024,3,1), Fin=new DateTime(2024,6,30), StagiaireId="s1", EntrepriseId=3, TypeMetierId=4, TypeIntershipActivityId=3, CreateurId=adminId },
            new Stage { Nom="Stage RH", Debut=new DateTime(2024,5,1), Fin=new DateTime(2024,7,31), StagiaireId="s2", EntrepriseId=2, TypeMetierId=2, TypeIntershipActivityId=2, CreateurId=adminId },
            new Stage { Nom="Stage Comptable", Debut=new DateTime(2024,9,1), Fin=new DateTime(2024,12,31), StagiaireId="s3", EntrepriseId=1, TypeMetierId=3, TypeIntershipActivityId=1, CreateurId=adminId },
        };
    }
}
