namespace EdgePMO.API.Models
{
    public class MediaFolder
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public Guid? ParentFolderId { get; set; }
        public MediaFolder? ParentFolder { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
