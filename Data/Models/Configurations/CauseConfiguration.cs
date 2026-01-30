using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ItSupportServer.Data.Models.Configurations
{
    public class CauseConfiguration : IEntityTypeConfiguration<Causes>
    {
        public void Configure(EntityTypeBuilder<Causes> builder)
        {
            // Table
            builder.ToTable("causes");

            // Primary Key
            builder.HasKey(c => c.CauseId);
            builder.Property(c => c.CauseId)
                .HasColumnName("cause_id")
                .ValueGeneratedOnAdd();

            // Ignore the inherited Id property
            builder.Ignore(c => c.Id);

            // Indexes
            builder.HasIndex(c => c.IssId);

            // Properties
            builder.Property(c => c.IssId)
                .HasColumnName("iss_id")
                .IsRequired();

            builder.Property(c => c.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(c => c.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);

            // Relationships
            builder.HasOne(c => c.Issues)
                .WithMany(i => i.Causes)
                .HasForeignKey(c => c.IssId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
