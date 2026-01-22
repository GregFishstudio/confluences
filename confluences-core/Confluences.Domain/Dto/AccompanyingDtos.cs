// Dans Confluences.Application/DTOs/AccompanyingDtos.cs (ou similaire)

using System;
using System.Collections.Generic;

namespace Confluences.Application.DTOs
{
    // Pour la requête de sauvegarde (POST)
    public class SyncDatesRequest
    {
        public string StagiaireId { get; set; }
        public string ActivityKey { get; set; }
        public List<DateTime> Dates { get; set; }
    }

    // Pour la réponse de lecture (GET)
    public class AccompanyingMatrixItem
    {
        public string StagiaireId { get; set; }
        public string ActivityKey { get; set; }
        public List<string> Dates { get; set; } // Format "YYYY-MM-DD"
    }
}