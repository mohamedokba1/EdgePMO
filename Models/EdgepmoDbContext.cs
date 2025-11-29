using Microsoft.EntityFrameworkCore;

namespace EdgePMO.API.Models;

public partial class EdgepmoDbContext : DbContext
{
    public EdgepmoDbContext()
    {
    }

    public EdgepmoDbContext(DbContextOptions<EdgepmoDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=edgepmo_db;Username=edgepmo_user;Password=EdgeWebsite@123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.EmailVerificationExpiresAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("email_verification_expires_at");
            entity.Property(e => e.EmailVerificationToken).HasColumnName("email_verification_token");
            entity.Property(e => e.EmailVerified)
                .HasDefaultValue(false)
                .HasColumnName("email_verified");
            entity.Property(e => e.FirstName)
                .HasMaxLength(150)
                .HasColumnName("first_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsAdmin)
                .HasDefaultValue(false)
                .HasColumnName("is_admin");
            entity.Property(e => e.LastCompnay).HasColumnName("last_compnay");
            entity.Property(e => e.LastName)
                .HasMaxLength(200)
                .HasColumnName("last_name");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.PasswordSalt).HasColumnName("password_salt");
            entity.Property(e => e.RefreshToken).HasColumnName("refresh_token");
            entity.Property(e => e.RefreshTokenCreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("refresh_token_created_at");
            entity.Property(e => e.RefreshTokenExpiresAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("refresh_token_expires_at");
            entity.Property(e => e.RefreshTokenRevokedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("refresh_token_revoked_at");
            entity.Property(e => e.Role)
                .HasMaxLength(10)
                .HasColumnName("role");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
