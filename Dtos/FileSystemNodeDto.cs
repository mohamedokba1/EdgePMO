using System.IO.Enumeration;

namespace EdgePMO.API.Dtos
{
    public class FileSystemNodeDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public bool IsFolder { get; set; }
        public List<FileSystemNodeDto> Children { get; set; } = new();
        public long? Size { get; set; }
        public string? Extension { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
