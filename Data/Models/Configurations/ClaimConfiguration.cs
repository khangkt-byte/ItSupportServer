using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ItSupportServer.Data.Models.Configurations
{
    public class ClaimConfiguration : IEntityTypeConfiguration<Claims>
    {
        public void Configure(EntityTypeBuilder<Claims> builder)
        {
            // Table
            builder.ToTable("claims");

            // Primary Key
            builder.HasKey(c => c.ClaimId);
            builder.Property(c => c.ClaimId)
                   .ValueGeneratedOnAdd()
                   .HasColumnName("claim_id");

            // Indexes
            builder.HasIndex(c => c.Claim)
                   .IsUnique();

            // Properties
            builder.Property(c => c.Claim)
                   .IsRequired()
                   .HasMaxLength(256)
                   .HasColumnName("claim");

            builder.Property(c => c.Category)
                   .HasMaxLength(128)
                   .HasColumnName("category");
        }
    }
}
