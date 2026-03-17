using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ItSupportServer.Data.Models.Configurations
{
    /// <summary>
    /// Entity configuration for password reset tokens
    /// Pattern: Fluent API configuration (EF Core best practice)
    /// Reference: Microsoft EF Core conventions
    /// </summary>
    public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetTokens>
    {
        public void Configure(EntityTypeBuilder<PasswordResetTokens> builder)
        {
            // Table
            builder.ToTable("password_reset_tokens");

            // Primary key
            builder.HasKey(t => t.TokenId);
            builder.Property(t => t.TokenId)
                .HasColumnName("token_id");
                //.HasDefaultValueSql("uuidv7()");  // PostgreSQL function for UUID v7

            // Indexes
            builder.HasIndex(t => t.Token)
                .HasDatabaseName("ix_password_reset_tokens_token");

            builder.HasIndex(t => t.AccountId)
                .HasDatabaseName("ix_password_reset_tokens_account_id");

            builder.HasIndex(t => new { t.ExpiredAt, t.UsedAt })
                .HasDatabaseName("ix_password_reset_tokens_expiry_status");

            builder.HasIndex(t => t.TokenPrefix)
                .HasDatabaseName("ix_password_reset_tokens_prefix");

            // Properties
            builder.Property(t => t.AccountId)
                .HasColumnName("account_id")
                .IsRequired();

            builder.Property(t => t.Token)
                .HasColumnName("token")
                .IsRequired()
                .HasMaxLength(500);  // BCrypt hash = ~60 chars, SHA-256 = 64 chars

            builder.Property(t => t.ExpiredAt)
                .HasColumnName("expires_at")
                .IsRequired();

            builder.Property(t => t.UsedAt)
                .HasColumnName("used_at");

            builder.Property(t => t.TokenPrefix)
                .HasColumnName("token_prefix")
                .HasMaxLength(8);

            // ===== RELATIONSHIPS =====
            // One Account → Many PasswordResetTokens (Cascade delete)
            builder.HasOne(t => t.Account)
                .WithMany(a => a.PasswordResetTokens)
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}