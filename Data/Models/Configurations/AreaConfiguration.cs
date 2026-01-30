using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ItSupportServer.Data.Models.Configurations
{
    public class AreaConfiguration : IEntityTypeConfiguration<Areas>
    {
        public void Configure(EntityTypeBuilder<Areas> builder)
        {
            // Table
            builder.ToTable("areas");

            // Primary Key
            builder.HasKey(a => a.AreaId);
            builder.Property(a => a.AreaId)
                   .HasColumnName("area_id")
                   .ValueGeneratedOnAdd();

            // Ignore the inherited Id property
            builder.Ignore(a => a.Id);

            // Indexes
            builder.HasIndex(a => a.Name).IsUnique();

            // Properties
            builder.Property(a => a.Name)
                   .HasColumnName("name")
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(a => a.Description)
                   .HasColumnName("description")
                   .HasMaxLength(500);
        }
    }
}
