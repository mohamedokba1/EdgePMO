namespace EdgePMO.API.Models
{
    public class Template
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public string CoverImageUrl { get; set; }
        public string FilePath { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
        public ICollection<UserTemplate> UserTemplates { get; set; } = new List<UserTemplate>();
    }
}
