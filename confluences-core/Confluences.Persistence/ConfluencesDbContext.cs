using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Confluences.Domain.Entities;
using Confluences.Domain.Entities.Views;
using System;

namespace Confluences.Persistence
{
    public class ConfluencesDbContext
    : IdentityDbContext<
        ApplicationUser, ApplicationRole, string,
        ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin,
        ApplicationRoleClaim, ApplicationUserToken>
    {
        public ConfluencesDbContext()
        {
        }

        public ConfluencesDbContext(DbContextOptions<ConfluencesDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Gender> Genders => Set<Gender>();
        public virtual DbSet<Appointment> Appointments => Set<Appointment>();
        public virtual DbSet<AppointmentStudent> AppointmentStudents => Set<AppointmentStudent>();
        public virtual DbSet<SchoolClassRoom> SchoolClassRooms => Set<SchoolClassRoom>();
        public virtual DbSet<Session> Sessions => Set<Session>();
        public virtual DbSet<SessionStudent> SessionStudents => Set<SessionStudent>();
        public virtual DbSet<SessionTeacher> SessionTeachers => Set<SessionTeacher>();
        public virtual DbSet<HomeworkType> HomeworkTypes => Set<HomeworkType>();
        public virtual DbSet<SchoolClassRoomExplanation> SchoolClassRoomExplanations => Set<SchoolClassRoomExplanation>();
        public virtual DbSet<AnnouncePage> AnnouncePages => Set<AnnouncePage>();
        public virtual DbSet<AnnouncePageLink> AnnouncePageLinks => Set<AnnouncePageLink>();
        public virtual DbSet<SessionNumber> SessionNumbers => Set<SessionNumber>();
        public virtual DbSet<HomeworkV2> HomeworkV2s => Set<HomeworkV2>();
        public virtual DbSet<Theory> Theories => Set<Theory>();
        public virtual DbSet<Exercice> Exercices => Set<Exercice>();
        public virtual DbSet<HomeworkV2Student> HomeworkV2Students => Set<HomeworkV2Student>();
        public virtual DbSet<ExerciceAlone> ExercicesAlone => Set<ExerciceAlone>();
        public virtual DbSet<HomeworkV2StudentExerciceAlone> HomeworkV2StudentExerciceAlones => Set<HomeworkV2StudentExerciceAlone>();
        public virtual DbSet<TypeMoyen> TypeMoyens => Set<TypeMoyen>();
        public virtual DbSet<TypeStage> TypeStages => Set<TypeStage>();
        public virtual DbSet<TypeEntreprise> TypeEntreprises => Set<TypeEntreprise>();
        public virtual DbSet<TypeAffiliation> TypeAffiliations => Set<TypeAffiliation>();
        public virtual DbSet<TypeAnnonce> TypeAnnonces => Set<TypeAnnonce>();
        public virtual DbSet<TypeDomaine> TypeDomaines => Set<TypeDomaine>();
        public virtual DbSet<TypeMetier> TypeMetiers => Set<TypeMetier>();
        public virtual DbSet<TypeOffre> TypeOffres => Set<TypeOffre>();
        public virtual DbSet<TypeIntershipActivity> TypeIntershipActivities => Set<TypeIntershipActivity>();
        public virtual DbSet<Entreprise> Entreprises => Set<Entreprise>();
        public virtual DbSet<Stage> Stages => Set<Stage>();
        public virtual DbSet<Contact> Contacts => Set<Contact>();
        public virtual DbSet<EntrepriseMetier> EntrepriseMetiers => Set<EntrepriseMetier>();
        public virtual DbSet<EntrepriseOffre> EntrepriseOffres => Set<EntrepriseOffre>();
        public virtual DbSet<EntrepriseDomaine> EntrepriseDomaines => Set<EntrepriseDomaine>();
        public virtual DbSet<TypeJobSearchAssistance> TypeJobSearchAssistances => Set<TypeJobSearchAssistance>();
        public virtual DbSet<TypeJobSearchAssistanceOccurrence> TypeJobSearchAssistanceOccurrences => Set<TypeJobSearchAssistanceOccurrence>();
        public virtual DbSet<JobSearchAssistance> JobSearchAssistances => Set<JobSearchAssistance>();
        public virtual DbSet<LastContact> LastContacts => Set<LastContact>();
        public virtual DbSet<StageFile> StageFiles => Set<StageFile>();
        public virtual DbSet<Presence> Presences { get; set; }
        public virtual DbSet<SessionReport> SessionReports { get; set; }
        public virtual DbSet<AccompanyingPresence> AccompanyingPresences { get; set; }
        
        public DbSet<SessionFormation> SessionFormations { get; set; }
        public DbSet<JourFerie>        JoursFeries       { get; set; }
        public DbSet<PeriodeVacance>   PeriodesVacances  { get; set; }

        public virtual DbSet<Ressource> Ressources => Set<Ressource>();
        public DbSet<MatrixColumn> MatrixColumns { get; set; }
        public virtual DbSet<StagiaireNiveau> StagiaireNiveaux { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // --- CONFIGURATION PRESENCE (ON GARDE STAGIAIREID COMME AVANT) ---
            builder.Entity<Presence>(entity =>
            {
                entity.ToTable("Presences");
                entity.HasKey(p => p.Id);

                entity.HasOne(p => p.Stagiaire)
                      .WithMany() 
                      .HasForeignKey(p => p.StagiaireId)
                      .IsRequired();
                
                entity.HasOne(p => p.Stage)
                      .WithMany() 
                      .HasForeignKey(p => p.StageId)
                      .IsRequired(false); 

                entity.HasIndex(p => new { p.StagiaireId, p.Date }).IsUnique();
                entity.Property(p => p.Statut).HasMaxLength(2).IsRequired();
            });

            // --- CONFIGURATION ACCOMPANYING PRESENCE (CORRIGÉE POUR USERID) ---
            builder.Entity<AccompanyingPresence>(entity =>
            {
                entity.HasKey(ap => ap.AccompanyingPresenceId);

                // On utilise UserId ici car c'est le nouveau nom dans votre classe AccompanyingPresence.cs
                entity.HasIndex(ap => new { ap.UserId, ap.Date, ap.ActivityName }).IsUnique();

                // Relation avec l'utilisateur Identity
                entity.HasOne(ap => ap.User)
                      .WithMany()
                      .HasForeignKey(ap => ap.UserId)
                      .IsRequired();
            });
            
            // --- CONFIGURATION IDENTITY ---
            builder.Entity<ApplicationUser>(b =>
            {
                b.HasMany(e => e.Claims).WithOne(e => e.User).HasForeignKey(uc => uc.UserId).IsRequired();
                b.HasMany(e => e.Logins).WithOne(e => e.User).HasForeignKey(ul => ul.UserId).IsRequired();
                b.HasMany(e => e.Tokens).WithOne(e => e.User).HasForeignKey(ut => ut.UserId).IsRequired();
                b.HasMany(e => e.UserRoles).WithOne(e => e.User).HasForeignKey(ur => ur.UserId).IsRequired();

                b.HasMany(e => e.EntreprisFormateurIdDernierContacts)
                    .WithOne(e => e.FormateurDernierContact)
                    .HasForeignKey(ur => ur.FormateurIdDernierContact);

                b.HasMany(e => e.EntreprisStagiaireIdDernierContactNavigations)
                    .WithOne(e => e.StagiaireDernier)
                    .HasForeignKey(ur => ur.StagiaireIdDernierContact);
            });

            builder.Entity<ApplicationRole>(b =>
            {
                b.HasMany(e => e.UserRoles).WithOne(e => e.Role).HasForeignKey(ur => ur.RoleId).IsRequired();
                b.HasMany(e => e.RoleClaims).WithOne(e => e.Role).HasForeignKey(rc => rc.RoleId).IsRequired();
            });

            // --- RELATIONS TABLES DE LIAISON ---
            builder.Entity<SessionTeacher>().HasKey(st => new { st.SessionId, st.TeacherId });
            builder.Entity<SessionStudent>().HasKey(st => new { st.SessionId, st.StudentId });
            builder.Entity<EntrepriseMetier>().HasKey(st => new { st.EntrepriseId, st.TypeMetierId });
            builder.Entity<EntrepriseOffre>().HasKey(st => new { st.EntrepriseId, st.TypeOffreId });
            builder.Entity<EntrepriseDomaine>().HasKey(st => new { st.EntrepriseId, st.TypeDomaineId });
            
            // --- INDEX UNIQUES ---
            builder.Entity<TypeMetier>(e => e.HasIndex(x => x.Libelle).IsUnique());
            builder.Entity<TypeAffiliation>(e => {
                e.HasIndex(x => x.Code).IsUnique();
                e.HasIndex(x => x.Libelle).IsUnique();
            });
            builder.Entity<TypeAnnonce>(e => e.HasIndex(x => x.Libelle).IsUnique());
            builder.Entity<TypeDomaine>(e => e.HasIndex(x => x.Libelle).IsUnique());
            builder.Entity<TypeEntreprise>(e => e.HasIndex(x => x.Nom).IsUnique());
            builder.Entity<TypeMoyen>(e => e.HasIndex(x => x.Libelle).IsUnique());
            builder.Entity<TypeOffre>(e => e.HasIndex(x => x.Libelle).IsUnique());
            builder.Entity<TypeStage>(e => e.HasIndex(x => x.Nom).IsUnique());

            builder.Entity<Ressource>(e => e.HasNoKey());
        }
    }
}