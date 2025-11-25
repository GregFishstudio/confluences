using Microsoft.AspNetCore.Mvc;
using Confluences.Persistence;
using Confluences.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Api.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestPopulateController : ControllerBase
    {
        private readonly ConfluencesDbContext _db;

        public TestPopulateController(ConfluencesDbContext db)
        {
            _db = db;
        }

        [HttpPost("populate")]
        public IActionResult Populate()
        {
            // Security: Only run locally
            if (!Request.Host.Host.Contains("localhost"))
                return BadRequest("Cette route n'est disponible qu'en local.");

            // =================================================================
            // 🧹 PHASE 1: NUCLEAR CLEANUP (Order is CRITICAL)
            // =================================================================
            
            try 
            {
                // 1. Delete "Leaf" Data
                _db.Stages.RemoveRange(_db.Stages);
                _db.Contacts.RemoveRange(_db.Contacts);
                _db.EntrepriseDomaines.RemoveRange(_db.EntrepriseDomaines);
                _db.EntrepriseMetiers.RemoveRange(_db.EntrepriseMetiers);
                _db.EntrepriseOffres.RemoveRange(_db.EntrepriseOffres);
                _db.SaveChanges(); 

                // 2. Delete Entreprises
                _db.Entreprises.RemoveRange(_db.Entreprises);
                _db.SaveChanges(); 

                // 3. DELETE ALL USERS & ROLES
                var allUserRoles = _db.UserRoles.ToList();
                _db.UserRoles.RemoveRange(allUserRoles);
                
                var allUsers = _db.Users.ToList();
                if (allUsers.Any())
                {
                    _db.Users.RemoveRange(allUsers);
                    _db.SaveChanges(); // COMMIT: Releases FKs on TypeAffiliations
                }

                // 4. Delete Auxiliary Types (Now safe to delete)
                _db.Roles.RemoveRange(_db.Roles);
                _db.TypeMetiers.RemoveRange(_db.TypeMetiers);
                _db.TypeIntershipActivities.RemoveRange(_db.TypeIntershipActivities);
                _db.TypeAffiliations.RemoveRange(_db.TypeAffiliations);
                _db.TypeEntreprises.RemoveRange(_db.TypeEntreprises);
                _db.TypeAnnonces.RemoveRange(_db.TypeAnnonces);
                _db.TypeDomaines.RemoveRange(_db.TypeDomaines);
                _db.TypeMoyens.RemoveRange(_db.TypeMoyens);
                
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Cleanup Note: {ex.Message}");
            }


            // =================================================================
            // 🌱 PHASE 2: SEEDING (Using Custom ApplicationRole/UserRole)
            // =================================================================

            // 1. GENDERS
            if (!_db.Genders.Any())
            {
                _db.Genders.AddRange(
                    new Gender { GenderId = 1, GenderName = "Homme" },
                    new Gender { GenderId = 2, GenderName = "Femme" },
                    new Gender { GenderId = 3, GenderName = "Autre" }
                );
                _db.SaveChanges();
            }

            // 2. ROLES (Create 'Teacher' role for Frontend Access)
            var adminRoleId = "role_teacher";
            if (!_db.Roles.Any(r => r.Name == "Teacher"))
            {
                // FIX: Use ApplicationRole instead of IdentityRole
                _db.Roles.Add(new ApplicationRole { 
                    Id = adminRoleId, 
                    Name = "Teacher", 
                    NormalizedName = "TEACHER" 
                });
                _db.SaveChanges();
            }

            // 3. TYPE AFFILIATIONS (Required before Users)
            var adminType = new TypeAffiliation { TypeAffiliationId = 1, Libelle = "Admin", Code = "ADM" };
            var stagiaireType = new TypeAffiliation { TypeAffiliationId = 2, Libelle = "Stagiaire", Code = "STG" };
            var formateurType = new TypeAffiliation { TypeAffiliationId = 3, Libelle = "Formateur", Code = "FRM" };
            _db.TypeAffiliations.AddRange(adminType, stagiaireType, formateurType);
            _db.SaveChanges();

            // 4. ADMIN USER
            var adminId = "admin1";
            var hasher = new PasswordHasher<ApplicationUser>();
            var admin = new ApplicationUser
            {
                Id = adminId,
                Firstname = "Admin",
                LastName = "Confluences",
                Email = "admin@confluences.ch",
                UserName = "admin@confluences.ch",
                NormalizedEmail = "ADMIN@CONFLUENCES.CH",
                NormalizedUserName = "ADMIN@CONFLUENCES.CH",
                EmailConfirmed = true,
                TypeAffiliationId = 1,
                GenderId = 1,
                SecurityStamp = Guid.NewGuid().ToString() // CRITICAL: Required for IDP
            };
            admin.PasswordHash = hasher.HashPassword(admin, "Confluences2025!");

            _db.Entry(admin).Property(u => u.Id).IsTemporary = false;
            _db.Users.Add(admin);
            _db.SaveChanges();

            // 5. ASSIGN ROLE TO ADMIN
            if (!_db.UserRoles.Any(ur => ur.UserId == adminId && ur.RoleId == adminRoleId))
            {
                // FIX: Use ApplicationUserRole instead of IdentityUserRole<string>
                _db.UserRoles.Add(new ApplicationUserRole { 
                    UserId = adminId, 
                    RoleId = adminRoleId 
                });
                _db.SaveChanges();
            }

            // 6. EXPANDED AUXILIARY DATA
            _db.TypeEntreprises.AddRange(
                new TypeEntreprise { TypeEntrepriseId = 1, Nom = "Privé" },
                new TypeEntreprise { TypeEntrepriseId = 2, Nom = "Public" },
                new TypeEntreprise { TypeEntrepriseId = 3, Nom = "Association" },
                new TypeEntreprise { TypeEntrepriseId = 4, Nom = "Startup" }
            );
            _db.TypeDomaines.AddRange(
                new TypeDomaine { TypeDomaineId = 1, Libelle = "Informatique", OldNames = "IT, Dev, Data" },
                new TypeDomaine { TypeDomaineId = 2, Libelle = "Ressources Humaines", OldNames = "RH, Recrutement" },
                new TypeDomaine { TypeDomaineId = 3, Libelle = "Comptabilité", OldNames = "Finance, Budget" },
                new TypeDomaine { TypeDomaineId = 4, Libelle = "Marketing", OldNames = "Web, Communication" },
                new TypeDomaine { TypeDomaineId = 5, Libelle = "Logistique", OldNames = "Stock, Transport" }
            );
            _db.TypeMetiers.AddRange(
                new TypeMetier { TypeMetierId = 1, Libelle = "Assistant administratif" },
                new TypeMetier { TypeMetierId = 2, Libelle = "Aide comptable" },
                new TypeMetier { TypeMetierId = 3, Libelle = "Employé de commerce" },
                new TypeMetier { TypeMetierId = 4, Libelle = "Développeur Web" },
                new TypeMetier { TypeMetierId = 5, Libelle = "Graphiste" }
            );
            _db.TypeIntershipActivities.AddRange(
                new TypeIntershipActivity { TypeIntershipActivityId = 1, Nom = "Accueil" },
                new TypeIntershipActivity { TypeIntershipActivityId = 2, Nom = "Archivage" },
                new TypeIntershipActivity { TypeIntershipActivityId = 3, Nom = "Saisie" }
            );
            _db.SaveChanges();

            // 7. EXPANDED ENTREPRISES
            _db.Entreprises.AddRange(
                new Entreprise { EntrepriseId = 1, Nom = "Migros Vaud", Ville = "Ecublens", CreateurId = adminId, DateCreation = new DateTime(2023, 10, 1), TypeEntrepriseId = 1, Remarque = "Grande distribution." },
                new Entreprise { EntrepriseId = 2, Nom = "Ville de Lausanne", Ville = "Lausanne", CreateurId = adminId, DateCreation = new DateTime(2022, 5, 15), TypeEntrepriseId = 2, Remarque = "Service public." },
                new Entreprise { EntrepriseId = 3, Nom = "InnovTech Sàrl", Ville = "Fribourg", CreateurId = adminId, DateCreation = new DateTime(2024, 1, 1), TypeEntrepriseId = 4, Remarque = "Startup spécialisée en IA." },
                new Entreprise { EntrepriseId = 4, Nom = "Croix-Rouge Suisse", Ville = "Berne", CreateurId = adminId, DateCreation = new DateTime(2021, 8, 10), TypeEntrepriseId = 3, Remarque = "Organisation humanitaire." }
            );
            _db.SaveChanges();

            // 8. EXPANDED STAGIAIRES
            var s1 = new ApplicationUser { Id="s1", Firstname="Jean", LastName="Dupont", Email="jean@test.com", UserName="jean@test.com", NormalizedEmail="JEAN@TEST.COM", NormalizedUserName="JEAN@TEST.COM", EmailConfirmed=true, GenderId=1, TypeAffiliationId=2, SecurityStamp = Guid.NewGuid().ToString() };
            var s2 = new ApplicationUser { Id="s2", Firstname="Maria", LastName="Lopez", Email="maria@test.com", UserName="maria@test.com", NormalizedEmail="MARIA@TEST.COM", NormalizedUserName="MARIA@TEST.COM", EmailConfirmed=true, GenderId=2, TypeAffiliationId=2, SecurityStamp = Guid.NewGuid().ToString() };
            var s3 = new ApplicationUser { Id="s3", Firstname="Alex", LastName="Martin", Email="alex@test.com", UserName="alex@test.com", NormalizedEmail="ALEX@TEST.COM", NormalizedUserName="ALEX@TEST.COM", EmailConfirmed=true, GenderId=1, TypeAffiliationId=2, SecurityStamp = Guid.NewGuid().ToString() };
            
            s1.PasswordHash = hasher.HashPassword(s1, "Test123!");
            s2.PasswordHash = hasher.HashPassword(s2, "Test123!");
            s3.PasswordHash = hasher.HashPassword(s3, "Test123!");

            _db.Entry(s1).Property(u => u.Id).IsTemporary = false;
            _db.Entry(s2).Property(u => u.Id).IsTemporary = false;
            _db.Entry(s3).Property(u => u.Id).IsTemporary = false;
            _db.Users.AddRange(s1, s2, s3);
            _db.SaveChanges();

            // 9. CONTACTS & STAGES
            _db.Contacts.AddRange(
                new Contact { ContactId = 1, EntrepriseId = 1, Prenom = "Marc", Nom = "Berger", Fonction = "Directeur RH", Email = "marc.b@migros.ch" },
                new Contact { ContactId = 2, EntrepriseId = 2, Prenom = "Elodie", Nom = "Dubois", Fonction = "Assistante", Email = "elodie.d@lausanne.ch" },
                new Contact { ContactId = 3, EntrepriseId = 3, Prenom = "Sophie", Nom = "Chen", Fonction = "CTO", Email = "sophie.c@innov.ch" }
            );
            _db.Stages.AddRange(
                new Stage { StageId = 1, Nom = "Stage WebDev", Debut = new DateTime(2024,3,1), Fin = new DateTime(2024,6,30), StagiaireId = "s1", EntrepriseId = 3, TypeMetierId = 4, TypeIntershipActivityId = 3, CreateurId = adminId },
                new Stage { StageId = 2, Nom = "Stage RH", Debut = new DateTime(2024,5,1), Fin = new DateTime(2024,7,31), StagiaireId = "s2", EntrepriseId = 2, TypeMetierId = 2, TypeIntershipActivityId = 2, CreateurId = adminId },
                new Stage { StageId = 3, Nom = "Stage Comptable", Debut = new DateTime(2024,9,1), Fin = new DateTime(2024,12,31), StagiaireId = "s3", EntrepriseId = 1, TypeMetierId = 3, TypeIntershipActivityId = 1, CreateurId = adminId }
            );
            _db.SaveChanges();

            return Ok("✔ DB Reset Complete. Admin: admin@confluences.ch / Confluences2025! (Role: Teacher)");
        }
    }
}