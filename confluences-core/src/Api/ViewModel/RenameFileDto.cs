namespace Api.ViewModel
{
    public class RenameFileDto
    {
        public string FileName { get; set; } = string.Empty;

        // Nom saisi depuis la fiche ; l'extension d'origine est conservée si elle a été omise,
        // pour que le fichier téléchargé s'ouvre toujours avec le bon programme.
        public string Resolve(string currentFileName)
        {
            var name = (FileName ?? string.Empty).Trim();
            var extension = System.IO.Path.GetExtension(currentFileName);
            if (!string.IsNullOrEmpty(extension) && !name.EndsWith(extension, System.StringComparison.OrdinalIgnoreCase))
            {
                name += extension;
            }
            return name;
        }
    }
}
