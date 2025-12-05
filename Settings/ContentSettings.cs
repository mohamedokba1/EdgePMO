namespace EdgePMO.API.Settings
{
    public class ContentSettings
    {
        public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
        public long MaxFileSizeBytes { get; set; } = 200_000_000; // default 200 MB
        public string UploadsRelative { get; set; } = "uploads";
    }
}
