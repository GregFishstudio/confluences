using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confluences.Domain.Entities
{
    // Demande d'inscription envoyée par un·e AS pour une personne qui n'est pas (encore)
    // participant·e — remplace le tableau Excel des demandes. Une fois la personne inscrite et
    // son compte créé, ParticipantId fait le lien avec sa fiche participant.
    public class Candidature
    {
        [Key]
        public int CandidatureId { get; set; }

        public DateTime? DateDemande { get; set; }

        // "À évaluer", "AS à contacter", "Sur liste d'attente", "Inscrit-e", "Retiré-e"
        [StringLength(50)]
        public string? Statut { get; set; }

        // "F&I" ou "Télécours" — le niveau (A1/A2) est porté par le statut
        // ("Sur liste d'attente A1" / "Sur liste d'attente A2").
        [StringLength(50)]
        public string? MesureVisee { get; set; }

        // Sessions visées ("2027-1", "2027-2"…) : une candidature peut viser plusieurs sessions et
        // apparaît alors dans chacune (Inscriptions › Participant·e·s). AnneeVisee/SessionNumberVisee
        // reprennent la première d'entre elles (tri, affichage court).
        public List<string> SessionsVisees { get; set; } = new();
        public int? AnneeVisee { get; set; }
        public int? SessionNumberVisee { get; set; }

        public int? Priorite { get; set; }

        // --- Bénéficiaire ---
        [StringLength(20)]
        public string? Civilite { get; set; }
        [StringLength(100)]
        public string? Prenom { get; set; }
        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;
        public DateTime? DateNaissance { get; set; }
        public string? Adresse { get; set; }
        [StringLength(50)]
        public string? Telephone { get; set; }
        [StringLength(200)]
        public string? Email { get; set; }
        [StringLength(100)]
        public string? Origine { get; set; }
        // "CSIR", "CSR"
        [StringLength(50)]
        public string? Mandant { get; set; }

        // --- Assistant·e social·e ---
        public int? AssistantSocialId { get; set; }
        [ForeignKey(nameof(AssistantSocialId))]
        public virtual AssistantSocial? AssistantSocial { get; set; }
        // Remarques AS : fil de commentaires datés/signés (CandidatureComment).
        [InverseProperty(nameof(CandidatureComment.Candidature))]
        public virtual ICollection<CandidatureComment>? Comments { get; set; }

        // --- Responsable pédagogique ---
        public string? NotesRdv { get; set; }
        // Date de l'évaluation — obligatoire avant de rédiger l'évaluation.
        public DateTime? DateEvaluation { get; set; }
        public string? EvaluationNiveau { get; set; }
        // "Pour nous" : remarques internes, non officielles — jamais affichées dans les tableaux
        // ni transmises, seulement dans le formulaire de la candidature.
        public string? RemarquesPedagogiques { get; set; }

        // --- Remarques générales (ex-colonne "Remarques générales/AS" de l'Excel) ---
        public string? RemarquesGenerales { get; set; }

        // --- Administration ---
        public bool InscriptionConfirmee { get; set; }
        // Reporté sur la fiche participant (ApplicationUser.ContratMaori) à l'inscription.
        public bool ContratMaori { get; set; }
        [StringLength(200)]
        public string? TransmissionAS { get; set; }

        public string? ParticipantId { get; set; }
        [ForeignKey(nameof(ParticipantId))]
        public virtual ApplicationUser? Participant { get; set; }

        // Historique des statuts : date, auteur·e, motif et commentaire de chaque changement.
        [InverseProperty(nameof(CandidatureStatutChange.Candidature))]
        public virtual ICollection<CandidatureStatutChange>? StatutChanges { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
