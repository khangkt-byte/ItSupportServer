using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ItSupportServer.Data.Models.Configurations
{
    public class RoleClaimConfiguration : IEntityTypeConfiguration<RoleClaims>
    {
        public void Configure(EntityTypeBuilder<RoleClaims> builder)
        {
            // Table
            builder.ToTable("role_claims");

            // Primary Key
            builder.HasKey(rc => new { rc.RoleId, rc.ClaimId });
            builder.Property(rc => rc.RoleId)
                   .HasColumnName("role_id");
            builder.Property(rc => rc.ClaimId)
                   .HasColumnName("claim_id");

            // Indexes
            builder.HasIndex(rc => rc.ClaimId);

            // Relationships
            builder.HasOne(rc => rc.Role)
                   .WithMany(r => r.RoleClaims)
                   .HasForeignKey(rc => rc.RoleId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rc => rc.Claim)
                   .WithMany(c => c.RoleClaims)
                   .HasForeignKey(rc => rc.ClaimId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
