using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Confluences.Domain.Entities
{
    public class ApplicationUser : IdentityUser<string>
    {
        public ApplicationUser()
    {
        Id = Guid.NewGuid().ToString();
    }

        [Required]
        public string Firstname { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        public DateTime Birthday { get; set; }
        public string? PathImage { get; set; }
        public bool WantsMoreHomeworks { get; set; }
        public bool HasSeenHelpVideo { get; set; }
        public string? Language { get; set; }
        public string? Nationality { get; set; }
        public DateTime? DateDebutParticipation { get; set; }
        // Date d'interruption de la mesure avant sa fin normale, si applicable — les jours après
        // cette date ne comptent plus comme jours cibles dans les calculs de présence, comme les
        // jours avant DateDebutParticipation.
        public DateTime? DateFinParticipation { get; set; }
        public string? RaisonInterruptionMesure { get; set; }

        public string? OralLevel { get; set; }
        public string? WrittenLevel { get; set; }

        public int? CurrentSessionNumber { get; set; }
        public int? CurrentSessionYear { get; set; }

        // --- Fiche participant : informations générales (saisie manuelle) ---
        public bool ContratMaori { get; set; }
        public string? EtatCivil { get; set; }
        public int? NombreEnfants { get; set; }
        // Adresse complète sur une ligne : recomposée à partir de Rue / Npa / Localite quand ils
        // sont renseignés ; sinon ancienne saisie libre (à compléter dans les champs structurés).
        public string? Adresse { get; set; }
        // Adresse structurée : rue et numéro, NPA (4 chiffres), localité.
        [StringLength(200)]
        public string? Rue { get; set; }
        [StringLength(4)]
        public string? Npa { get; set; }
        [StringLength(60)]
        public string? Localite { get; set; }
        public string? BeneficiaireDe { get; set; }
        public string? AssistantSocialEmail { get; set; }
        public int? AnneeArriveeSuisse { get; set; }
        public string? AutresLangues { get; set; }

        // --- Fiche participant : champs libres formatrices ---
        public string? ParcoursDepuisArriveeSuisse { get; set; }
        public string? ParcoursFormationSuisse { get; set; }
        public string? DomaineEtudeTitreObtenu { get; set; }
        public string? ParcoursProfessionnel { get; set; }
        public string? BesoinsSouhaitsFrancais { get; set; }
        public string? Autre { get; set; }

        public string? PlusHautesEtudes { get; set; }
        public string? PlusHautesEtudesCommentaire { get; set; }

        public bool? ActiviteProfSuisse { get; set; }
        public string? ActiviteProfSuisseCommentaire { get; set; }

        public string? RechercheLogement { get; set; }
        public string? RechercheLogementCommentaire { get; set; }

        public string? BesoinsSouhaitsStages { get; set; }
        public string? BesoinsSouhaitsStagesCommentaire { get; set; }

        public string? BesoinsSouhaitsBenevolat { get; set; }
        public string? BesoinsSouhaitsBenevoletCommentaire { get; set; }
        // Bénévolat – domaines d'intérêt et types d'activité souhaités (choix multiples, libellés de
        // la liste fixe de l'application) ; "Autre (à préciser)" se détaille dans BenevolatDomaineAutre.
        public List<string> BenevolatDomaines { get; set; } = new();
        [StringLength(200)]
        public string? BenevolatDomaineAutre { get; set; }
        public List<string> BenevolatTypesActivite { get; set; } = new();

        public string? BesoinsSouhaitsMarrainage { get; set; }
        public string? BesoinsSouhaitsMarrainageCommentaire { get; set; }

        // --- Suivi formatrices : rubriques ajoutées (parcours en Suisse / pays d'origine, besoins) ---
        // Besoins / souhaits (logement, stages, bénévolat, marrainage, emploi, formation, sport) :
        // "oui", "non" ou "a_verifier".
        // Permis : l'année suffit ; la date exacte reste facultative.
        public int? AnneeObtentionPermis { get; set; }
        public DateTime? DateObtentionPermis { get; set; }
        public string? ProjetProfessionnel { get; set; }
        public string? BesoinsSouhaitsSport { get; set; }
        public string? Disponibilites { get; set; }
        // Souhaits pour le marrainage : "homme" / "femme" / "egal" ; "meme_age" / "plus_age" / "plus_jeune" / "egal".
        [StringLength(20)]
        public string? SouhaitGenreMarrainage { get; set; }
        [StringLength(20)]
        public string? SouhaitAgeMarrainage { get; set; }
        // Priorité de la recherche de marrainage : "haute" (très preneur·euse) / "normale" / "basse".
        [StringLength(20)]
        public string? PrioriteMarrainage { get; set; }
        // Région de marrainage (nom d'une Region) : saisie sur la fiche, sinon déduite de l'adresse.
        [StringLength(50)]
        public string? Region { get; set; }
        // Degré d'intérêt pour un stage/du bénévolat : "fort" / "normal" / "a_confirmer".
        [StringLength(20)]
        public string? DegreInteretStage { get; set; }
        public string? TitresObtenus { get; set; }

        public bool? ActiviteProfOrigine { get; set; }

        public string? BesoinsSouhaitsEmploi { get; set; }
        public string? BesoinsSouhaitsEmploiCommentaire { get; set; }

        public string? BesoinsSouhaitsFormation { get; set; }
        public string? BesoinsSouhaitsFormationCommentaire { get; set; }

        // --- Fiche participant : fin de session ---
        public string? NiveauDebutContrat { get; set; }
        public string? NiveauFinContrat { get; set; }
        public DateTime? ProfilUpdatedAt { get; set; }
        // Nom "à plat" (pas de navigation FK vers soi-même) de la formatrice ayant fait la
        // dernière modification du bloc "Suivi formatrices" — évite un self-join pour un simple
        // affichage "mis à jour le ... par ...".
        public string? ProfilUpdatedByName { get; set; }

        [InverseProperty(nameof(ParticipantFile.Student))]
        public virtual ICollection<ParticipantFile>? ParticipantFiles { get; set; }

        [InverseProperty(nameof(Marrainage.Participant))]
        public virtual ICollection<Marrainage>? Marrainages { get; set; }

        [InverseProperty(nameof(MarrainageParticipant.Participant))]
        public virtual ICollection<MarrainageParticipant>? MarrainageParticipants { get; set; }

        [InverseProperty(nameof(MarrainageComment.Author))]
        public virtual ICollection<MarrainageComment>? MarrainageComments { get; set; }

        public virtual ICollection<ApplicationUserClaim>? Claims { get; set; }
        public virtual ICollection<ApplicationUserLogin>? Logins { get; set; }
        public virtual ICollection<ApplicationUserToken>? Tokens { get; set; }
        public virtual ICollection<ApplicationUserRole>? UserRoles { get; set; }

        public short GenderId { get; set; }
        [ForeignKey(nameof(GenderId))]
        public virtual Gender? Gender { get; set; }

        public int? TypeAffiliationId { get; set; }
        [ForeignKey(nameof(TypeAffiliationId))]
        public virtual TypeAffiliation? TypeAffiliation { get; set; }

        [InverseProperty(nameof(AppointmentStudent.Student))]
        public virtual ICollection<AppointmentStudent>? AppointmentStudents { get; set; }

        [InverseProperty(nameof(Appointment.Teacher))]
        public virtual ICollection<Appointment>? Appointments { get; set; }

        public virtual ICollection<Contact>? ContactCreateurs { get; set; }

        [InverseProperty(nameof(Contact.ContactModificateur))]
        public virtual ICollection<Contact>? ContactModificateurs { get; set; }

        [InverseProperty(nameof(Entreprise.Createur))]
        public virtual ICollection<Entreprise>? EntreprisCreateurs { get; set; }

        public virtual ICollection<Entreprise>? EntreprisFormateurIdDernierContacts { get; set; }
        public virtual ICollection<Entreprise>? EntreprisStagiaireIdDernierContactNavigations { get; set; }

        [InverseProperty(nameof(Exercice.Teacher))]
        public virtual ICollection<Exercice>? Exercices { get; set; }

        [InverseProperty(nameof(ExerciceAlone.Teacher))]
        public virtual ICollection<ExerciceAlone>? ExerciceAlones { get; set; }

        [InverseProperty(nameof(HomeworkV2Student.Student))]
        public virtual ICollection<HomeworkV2Student>? HomeworkV2Students { get; set; }

        [InverseProperty("Teacher")]
        public virtual ICollection<HomeworkV2>? HomeworkV2s { get; set; }

        [InverseProperty("Student")]
        public virtual ICollection<HomeworkV2StudentExerciceAlone>? HomeworkV2StudentExerciceAlones { get; set; }

        [InverseProperty(nameof(SessionStudent.Student))]
        public virtual ICollection<SessionStudent>? SessionStudents { get; set; }

        [InverseProperty(nameof(SessionTeacher.Teacher))]
        public virtual ICollection<SessionTeacher>? SessionTeachers { get; set; }

        [InverseProperty(nameof(Stage.Createur))]
        public virtual ICollection<Stage>? StageCreateurs { get; set; }
        
        [InverseProperty(nameof(Stage.Stagiaire))]
        public virtual ICollection<Stage>? StageStagiaires { get; set; }

        [InverseProperty(nameof(Theory.Teacher))]
        public virtual ICollection<Theory>? Theories { get; set; }
    }
}
