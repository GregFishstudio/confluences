using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    public class Marrainage
    {
        [Key]
        public int MarrainageId { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
        public string? Numero { get; set; }
        public string? Commune { get; set; }
        public string? Email { get; set; }
        public int? Age { get; set; }
        public DateTime? DateNaissance { get; set; }
        public string? EtatCivil { get; set; }
        public string? Profession { get; set; }
        // "F" / "M" / "autre".
        [StringLength(10)]
        public string? Sexe { get; set; }
        public bool Actif { get; set; } = true;

        // "en attente" / "à relancer" — nul si aucun de ces deux statuts ne s'applique.
        public string? Statut { get; set; }

        public string? PeriodeDemande { get; set; }
        public string? RemarqueParrain { get; set; }
        public string? FilleulNom { get; set; }
        public string? TelephoneFilleul { get; set; }
        public string? EmailFilleul { get; set; }
        public string? Suivi { get; set; }
        public string? ExtraitCasier { get; set; }
        public bool CharteSignee { get; set; }
        public string? RelanceNotes { get; set; }
        public string? RaisonInactivite { get; set; }

        // Région (Lausanne, Riviera, Moudon...) : déduite de la commune si vide, modifiable.
        [StringLength(50)]
        public string? Region { get; set; }

        // Suivi : création (statut "Nouveau" pendant un mois) et dernière action sur la fiche
        // (modification, relation, réponses, fichiers) — avec la date du dernier commentaire,
        // sert à signaler "Prendre des nouvelles" après trois mois sans suivi.
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Questionnaire d'inscription : qui l'a saisi, et quand.
        [StringLength(200)]
        public string? QuestionnaireSaisiPar { get; set; }
        public DateTime? QuestionnaireSaisiLe { get; set; }

        // Date du dernier commentaire — calculée par l'API pour la liste, non stockée.
        [NotMapped]
        public DateTime? DernierCommentaire { get; set; }

        // Champ historique conservé pour compatibilité (un seul participant lié) ;
        // la relation multiple se fait maintenant via MarrainageParticipant.
        public string? ParticipantId { get; set; }
        [ForeignKey(nameof(ParticipantId))]
        public virtual ApplicationUser? Participant { get; set; }

        [InverseProperty(nameof(MarrainageParticipant.Marrainage))]
        public virtual ICollection<MarrainageParticipant>? ParticipantLinks { get; set; }

        [InverseProperty(nameof(MarrainageComment.Marrainage))]
        public virtual ICollection<MarrainageComment>? Comments { get; set; }

        [InverseProperty(nameof(MarrainageFile.Marrainage))]
        public virtual ICollection<MarrainageFile>? Files { get; set; }

        [InverseProperty(nameof(MarrainageAnswer.Marrainage))]
        public virtual ICollection<MarrainageAnswer>? Answers { get; set; }
    }
}
