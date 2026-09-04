using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Api.Utility
{
    // Formats acceptés partout où l'appli permet de déposer un document (CV/LM participant·e,
    // documents de stage, conventions de marrainage, etc.) : PDF, Word, Excel, image.
    public static class AllowedDocumentTypes
    {
        private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg"
        };

        public static bool IsAllowed(IFormFile file)
        {
            if (file == null) return false;
            var ext = Path.GetExtension(file.FileName);
            return !string.IsNullOrEmpty(ext) && Extensions.Contains(ext);
        }

        public static string AllowedExtensionsLabel => string.Join(", ", Extensions.OrderBy(e => e));
    }
}
