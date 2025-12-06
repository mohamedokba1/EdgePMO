namespace EdgePMO.API.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public byte[] PasswordSalt { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool EmailVerified { get; set; }

    public string? EmailVerificationToken { get; set; }

    public DateTime? EmailVerificationExpiresAt { get; set; }

    public string? LastCompnay { get; set; }

    public bool? IsActive { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenCreatedAt { get; set; }

    public DateTime? RefreshTokenExpiresAt { get; set; }

    public DateTime? RefreshTokenRevokedAt { get; set; }

    public bool? IsAdmin { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<Purchase> Purchases { get; set; } = new List<Purchase>();
    public List<UserTemplate> UserTemplates { get; set; } = new List<UserTemplate>();
    public List<CourseUser> CourseUsers { get; set; } = new();
}
