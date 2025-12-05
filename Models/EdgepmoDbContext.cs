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
    public DbSet<Course> Courses { get; set; }
    public DbSet<Instructor> Instructors { get; set; }
    public DbSet<Testimonial> Testimonials { get; set; }
    public DbSet<Certificate> Certificates { get; set; }
    public DbSet<CourseVideo> CourseVideos { get; set; }
    public DbSet<CourseUser> CourseUsers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }


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

        modelBuilder.Entity<Course>()
            .HasOne(c => c.Instructor)
            .WithMany(i => i.Courses)
            .HasForeignKey(c => c.InstructorId);

        modelBuilder.Entity<Testimonial>()
            .HasOne(t => t.Course)
            .WithMany(c => c.Testimonials)
            .HasForeignKey(t => t.CourseId);

        modelBuilder.Entity<Certificate>()
            .HasOne(c => c.Course)
            .WithMany(c => c.Certificates)
            .HasForeignKey(c => c.CourseId);

        modelBuilder.Entity<CourseVideo>(entity =>
        {
            entity.HasKey(e => e.CourseVideoId);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(e => e.Course)
                  .WithMany(c => c.CourseVideos)
                  .HasForeignKey(e => e.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CourseUser>(entity =>
        {
            entity.HasKey(e => new { e.CourseId, e.UserId });
            entity.HasOne(e => e.Course)
                  .WithMany(c => c.CourseUsers)
                  .HasForeignKey(e => e.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.CourseUsers)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
