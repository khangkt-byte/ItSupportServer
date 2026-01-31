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

            // Properties
            builder.Property(at => at.AccountId)
                   .IsRequired()
                   .HasColumnName("account_id");

            builder.Property(at => at.ExpiryTime)
                   .IsRequired()
                   .HasColumnName("expiry_time");

            builder.Property(at => at.RevokedAt)
                   .HasColumnName("revoked_at");

            // Relationships
            builder.HasOne(at => at.Account)
                   .WithMany(a => a.AccountTokens)
                   .HasForeignKey(at => at.AccountId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
