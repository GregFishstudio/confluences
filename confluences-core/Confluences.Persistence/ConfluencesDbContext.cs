using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Confluences.Domain.Entities;
using Confluences.Domain.Entities.Views;
using System; // Ajouté pour utiliser DateTime

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
        
        // NOUVEAU : DbSet pour les entrées de présence
        public virtual DbSet<Presence> Presences { get; set; } // <--- AJOUTÉ

        // Views
        public virtual DbSet<Ressource> Ressources => Set<Ressource>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            
            // --- Configuration de l'entité PRESENCE (AJOUTÉ) ---
            builder.Entity<Presence>(entity =>
            {
                entity.ToTable("Presences"); // Nom de la table
                entity.HasKey(p => p.Id);

                // Clé étrangère vers ApplicationUser (Stagiaire)
                entity.HasOne(p => p.Stagiaire)
                      .WithMany() 
                      .HasForeignKey(p => p.StagiaireId)
                      .IsRequired();
                
                // Clé étrangère vers Stage (optionnel)
                entity.HasOne(p => p.Stage)
                      .WithMany() 
                      .HasForeignKey(p => p.StageId)
                      .IsRequired(false); 

                // Index unique pour éviter qu'un stagiaire ait deux entrées pour le même jour
                // Nous utilisons Date.Date pour s'assurer qu'on compare la partie jour de la DateTime
                entity.HasIndex(p => new { p.StagiaireId, p.Date }).IsUnique();
                
                // Assurez-vous que le statut est bien stocké
                entity.Property(p => p.Statut).HasMaxLength(2).IsRequired();
            });
            // ----------------------------------------------------


            builder.Entity<ApplicationUser>(b =>
            {
                // Each User can have many UserClaims
                b.HasMany(e => e.Claims)
                    .WithOne(e => e.User)
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();

                // Each User can have many UserLogins
                b.HasMany(e => e.Logins)
                    .WithOne(e => e.User)
                    .HasForeignKey(ul => ul.UserId)
                    .IsRequired();

                // Each User can have many UserTokens
                b.HasMany(e => e.Tokens)
                    .WithOne(e => e.User)
                    .HasForeignKey(ut => ut.UserId)
                    .IsRequired();

                // Each User can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.User)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();

                b.HasMany(e => e.EntreprisFormateurIdDernierContacts)
                    .WithOne(e => e.FormateurDernierContact)
                    .HasForeignKey(ur => ur.FormateurIdDernierContact);

                b.HasMany(e => e.EntreprisStagiaireIdDernierContactNavigations)
                    .WithOne(e => e.StagiaireDernier)
                    .HasForeignKey(ur => ur.StagiaireIdDernierContact);

            });

            builder.Entity<ApplicationRole>(b =>
            {
                // Each Role can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                // Each Role can have many associated RoleClaims
                b.HasMany(e => e.RoleClaims)
                    .WithOne(e => e.Role)
                    .HasForeignKey(rc => rc.RoleId)
                    .IsRequired();

            });

            builder.Entity<SessionTeacher>().HasKey(st => new { st.SessionId, st.TeacherId });
            builder.Entity<SessionStudent>().HasKey(st => new { st.SessionId, st.StudentId });
            builder.Entity<EntrepriseMetier>().HasKey(st => new { st.EntrepriseId, st.TypeMetierId });
            builder.Entity<EntrepriseOffre>().HasKey(st => new { st.EntrepriseId, st.TypeOffreId });
            builder.Entity<EntrepriseDomaine>().HasKey(st => new { st.EntrepriseId, st.TypeDomaineId });
            
            builder.Entity<TypeMetier>(entity => {
                entity.HasIndex(e => e.Libelle).IsUnique();
            });
            builder.Entity<TypeAffiliation>(entity => {
                entity.HasIndex(e => e.Code).IsUnique();
            });
            builder.Entity<TypeAffiliation>(entity => {
                entity.HasIndex(e => e.Libelle).IsUnique();
            });
            builder.Entity<TypeAnnonce>(entity => {
                entity.HasIndex(e => e.Libelle).IsUnique();
            });
            builder.Entity<TypeDomaine>(entity => {
                entity.HasIndex(e => e.Libelle).IsUnique();
            });
            builder.Entity<TypeEntreprise>(entity => {
                entity.HasIndex(e => e.Nom).IsUnique();
            });
            builder.Entity<TypeMoyen>(entity => {
                entity.HasIndex(e => e.Libelle).IsUnique();
            });
            builder.Entity<TypeOffre>(entity => {
                entity.HasIndex(e => e.Libelle).IsUnique();
            });
            builder.Entity<TypeStage>(entity => {
                entity.HasIndex(e => e.Nom).IsUnique();
            });

            builder.Entity<Ressource>(entity =>
            {
                entity.HasNoKey();
            });
        }
    }
}