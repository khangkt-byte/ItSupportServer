using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ItSupportServer.Data.Models.Configurations
{
    public class AccountTokenConfiguration : IEntityTypeConfiguration<AccountTokens>
    {
        public void Configure(EntityTypeBuilder<AccountTokens> builder)
        {
            // Table
            builder.ToTable("account_tokens");

            // Primary Key
            builder.HasKey(at => at.AccountTokenId);
            builder.Property(at => at.AccountTokenId)
                   .HasColumnName("account_token_id");

            // Indexes
            builder.HasIndex(at => at.AccountId);

            builder.HasIndex(at => at.ExpiryTime);

            // Fast lookup of revoked tokens
            builder.HasIndex(t => t.RevokedAt)
                .HasDatabaseName("idx_account_tokens_revoked_at")
                .HasFilter("revoked_at IS NOT NULL");

            // Composite index for active session queries
            builder.HasIndex(t => new { t.AccountId, t.RevokedAt, t.ExpiryTime })
                .HasDatabaseName("idx_account_tokens_active_sessions");

            // Session tracking
            builder.HasIndex(t => t.SessionId)
                .HasDatabaseName("idx_account_tokens_session_id")
                .HasFilter("session_id IS NOT NULL");

            // Properties
            builder.Property(at => at.AccountId)
                   .IsRequired()
                   .HasColumnName("account_id");

            builder.Property(at => at.ExpiryTime)
                   .IsRequired()
                   .HasColumnName("expiry_time");

            builder.Property(at => at.RevokedAt)
                   .HasColumnName("revoked_at");

            builder.Property(at => at.IpAddress)
                   .HasMaxLength(45)
                   .HasColumnName("ip_address");

            builder.Property(at => at.UserAgent)
                   .HasMaxLength(512)
                   .HasColumnName("user_agent");

            builder.Property(at => at.DeviceInfo)
                   .HasMaxLength(512)
                   .HasColumnName("device_info");

            builder.Property(at => at.LastAccessedAt)
                   .HasColumnName("last_accessed_at");

            builder.Property(at => at.SessionId)
                   .HasMaxLength(255)
                   .HasColumnName("session_id");

            // Relationships
            builder.HasOne(at => at.Account)
                   .WithMany(a => a.AccountTokens)
                   .HasForeignKey(at => at.AccountId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
