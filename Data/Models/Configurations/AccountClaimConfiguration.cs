using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ItSupportServer.Data.Models.Configurations
{
    public class AccountClaimConfiguration : IEntityTypeConfiguration<AccountClaims>
    {
        public void Configure(EntityTypeBuilder<AccountClaims> builder)
        {
            // Table
            builder.ToTable("account_claims");

            // Primary Key
            builder.HasKey(ac => new { ac.AccountId, ac.ClaimId });
            builder.Property(ac => ac.AccountId)
                   .HasColumnName("acc_id");
            builder.Property(ac => ac.ClaimId)
                   .HasColumnName("claim_id");

            // Indexes
            builder.HasIndex(ac => ac.ClaimId);

            // Relationships
            builder.HasOne(ac => ac.Account)
                   .WithMany(a => a.AccountClaims)
                   .HasForeignKey(ac => ac.AccountId);

            builder.HasOne(ac => ac.Claim)
                   .WithMany(c => c.AccountClaims)
                   .HasForeignKey(ac => ac.ClaimId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
