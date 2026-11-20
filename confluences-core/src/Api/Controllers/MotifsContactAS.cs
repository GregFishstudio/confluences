using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.Controllers
{
    // Motif du contact avec l'AS quand une candidature passe en « AS à contacter » ou « Attente du
    // retour AS » (allers-retours compris). Même liste et mêmes règles que le formulaire
    // (frontend : utils/inscriptionOptions.js).
    public static class MotifsContactAS
    {
        public static readonly string[] StatutsAvecMotif = { "AS à contacter", "Attente du retour AS" };

        // Problèmes de joignabilité : commentaire obligatoire dès la deuxième occurrence.
        public static readonly string[] Joignabilite =
        {
            "Numéro de téléphone manquant",
            "Numéro de téléphone erroné",
            "Ne répond pas",
        };

        // Commentaire toujours obligatoire.
        public static readonly string[] CommentaireObligatoire =
        {
            "Participant·e peu motivé·e",
            "Problème de santé",
            "Demande à vérifier",
            "Autre mesure en cours",
            "Autre",
        };

        // Pas de commentaire nécessaire.
        public static readonly string[] SansCommentaire = { "Évalué·e" };

        public static IEnumerable<string> Tous => Joignabilite.Concat(CommentaireObligatoire).Concat(SansCommentaire);

        public static bool RequiertMotif(string? statut) => StatutsAvecMotif.Contains(statut);

        // Message d'erreur, ou null si le motif et le commentaire conviennent.
        // `occurrencesJoignabilitePrecedentes` : contacts précédents de cette candidature pour un
        // motif de joignabilité (téléphone manquant / erroné, ne répond pas).
        public static string? Verifier(string? motif, string? commentaire, int occurrencesJoignabilitePrecedentes)
        {
            if (string.IsNullOrWhiteSpace(motif)) return "Choisissez le motif du contact avec l'AS.";
            if (!Tous.Contains(motif)) return $"Motif inconnu : « {motif} ».";
            var sansCommentaire = string.IsNullOrWhiteSpace(commentaire);
            if (CommentaireObligatoire.Contains(motif) && sansCommentaire)
                return $"Un commentaire est obligatoire pour le motif « {motif} ».";
            if (Joignabilite.Contains(motif) && occurrencesJoignabilitePrecedentes >= 1 && sansCommentaire)
                return $"Deuxième contact pour un problème de téléphone : un commentaire est obligatoire (motif « {motif} »).";
            return null;
        }
    }
}
