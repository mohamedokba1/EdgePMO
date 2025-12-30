using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
    public DbSet<CourseDocument> CourseDocuments{ get; set; }
    public DbSet<CourseUser> CourseUsers { get; set; }
    public DbSet<Template> Templates { get; set; }
    public DbSet<Purchase> Purchases { get; set; }
    public DbSet<UserTemplate> UserTemplates { get; set; }
    public DbSet<PurchaseRequest> PurchaseRequests { get; set; }
    public DbSet<CourseOutline> CourseOutlines { get; set; }
    public DbSet<CourseReview> CourseReviews { get; set; }

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
            
        // ===== CourseVideo CONFIGURATION =====
        modelBuilder.Entity<CourseVideo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.DurationMinutes)
                .IsRequired();

            entity.Property(e => e.Order)
                .HasDefaultValue(0);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.CourseOutline)
                .WithMany(co => co.Videos)
                .HasForeignKey(e => e.CourseOutlineId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.CourseOutlineId);
            entity.HasIndex(e => new { e.CourseOutlineId, e.Order })
                .IsUnique();
        });

        // ===== CourseDocument CONFIGURATION =====
        modelBuilder.Entity<CourseDocument>(entity =>
        {
            entity.HasKey(e => e.CourseDocumentId);
            entity.Property(e => e.CourseDocumentId).HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.DocumentUrl)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.CourseOutline)
                .WithMany(co => co.Documents)
                .HasForeignKey(e => e.CourseOutlineId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.CourseOutlineId);
            entity.HasIndex(e => e.DocumentUrl)
                .IsUnique();
        });

        // ===== CourseUser CONFIGURATION =====
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

        // ===== Course CONFIGURATION =====
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId);
            entity.Property(e => e.CourseId).HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Subtitle).HasMaxLength(500);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Overview).HasColumnType("text");

            entity.Property(e => e.Price).HasPrecision(10, 2);
            entity.Property(e => e.Rating).HasPrecision(3, 1);

            // Store lists as jsonb
            JsonSerializerOptions? jsonSerializerOptions = new JsonSerializerOptions();

            entity.Property(e => e.SoftwareUsed)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                    v => JsonSerializer.Deserialize<List<string>>(v, jsonSerializerOptions) ?? new List<string>())
                .HasColumnType("jsonb");

            entity.Property(e => e.WhatStudentsLearn)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                    v => JsonSerializer.Deserialize<List<string>>(v, jsonSerializerOptions) ?? new List<string>())
                .HasColumnType("jsonb");

            entity.Property(e => e.WhoShouldAttend)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                    v => JsonSerializer.Deserialize<List<string>>(v, jsonSerializerOptions) ?? new List<string>())
                .HasColumnType("jsonb");

            entity.Property(e => e.Requirements)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                    v => JsonSerializer.Deserialize<List<string>>(v, jsonSerializerOptions) ?? new List<string>())
                .HasColumnType("jsonb");

            entity.HasOne(e => e.Instructor)
                .WithMany(i => i.Courses)
                .HasForeignKey(e => e.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.CourseOutline)
                .WithOne(co => co.Course)
                .HasForeignKey(co => co.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Reviews)
                .WithOne(cr => cr.Course)
                .HasForeignKey(cr => cr.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.InstructorId);
        });

        // ===== CourseOutline CONFIGURATION =====
        modelBuilder.Entity<CourseOutline>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Order)
                .HasDefaultValue(0);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Course)
                .WithMany(c => c.CourseOutline)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Videos)
                .WithOne(v => v.CourseOutline)
                .HasForeignKey(v => v.CourseOutlineId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Documents)
                .WithOne(d => d.CourseOutline)
                .HasForeignKey(d => d.CourseOutlineId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.CourseId);
            entity.HasIndex(e => new { e.CourseId, e.Order });
        });

        // ===== CourseReview CONFIGURATION =====
        modelBuilder.Entity<CourseReview>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.Header).HasMaxLength(200);
            entity.Property(e => e.Rating).HasPrecision(3, 2).HasDefaultValue(0.0);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(4000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Course)
                .WithMany(c => c.Reviews)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.CourseId);
            entity.HasIndex(e => e.UserId);
        });

        // ===== PURCHASE CONFIGURATION =====
        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.PurchaseType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("pending");

            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValue("USD");

            entity.Property(e => e.Amount)
                .HasPrecision(10, 2);

            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50);

            entity.Property(e => e.TransactionId)
                .HasMaxLength(255);

            entity.Property(e => e.PurchasedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Foreign Keys
            entity.HasOne(e => e.User)
                .WithMany(u => u.Purchases)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Template)
                .WithMany(t => t.Purchases)
                .HasForeignKey(e => e.TemplateId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Course)
                .WithMany(c => c.Purchases)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.PurchaseType);
        });

        // ===== TEMPLATE CONFIGURATION =====
        modelBuilder.Entity<Template>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Category)
                .HasMaxLength(100);

            entity.Property(e => e.CoverImageUrl)
                .HasMaxLength(500);

            entity.Property(e => e.Price)
                .HasPrecision(10, 2);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // ===== USER TEMPLATE CONFIGURATION =====
        modelBuilder.Entity<UserTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.PurchasedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsFavorite)
                .HasDefaultValue(false);

            // Unique constraint - user can only own template once
            entity.HasIndex(e => new { e.UserId, e.TemplateId })
                .IsUnique();

            // Foreign Keys
            entity.HasOne(e => e.User)
                .WithMany(u => u.UserTemplates)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Template)
                .WithMany(t => t.UserTemplates)
                .HasForeignKey(e => e.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Purchase)
                .WithMany()
                .HasForeignKey(e => e.PurchaseId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.UserId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
