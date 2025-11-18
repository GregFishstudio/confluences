namespace mvc.Models
{
    public class JitsiConfiguration
    {
        public string Url { get; set; } = string.Empty;
        public bool isPrivateRoom { get; set; } = false;
        public string UserNameToCall { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string Language { get; set; } = "fr";
        public string UserFriendlyName { get; set; } = "Unknown";
    }
}
