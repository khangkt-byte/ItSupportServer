using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ItSupportServer.Data.Models.Configurations
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Departments>
    {
        public void Configure(EntityTypeBuilder<Departments> builder)
        {
            // Table
            builder.ToTable("departments");

            // Primary Key
            builder.HasKey(d => d.DptId);
            builder.Property(d => d.DptId)
                   .HasColumnName("dpt_id")
                   .ValueGeneratedOnAdd();

            // Ignore the inherited Id property
            builder.Ignore(d => d.Id);

            // Indexses
            builder.HasIndex(d => d.Name)
                   .HasMethod("gin")
                   .HasOperators("gin_trgm_ops")
                   .HasFilter("\"DeletedAt\" IS NULL");

            // Properties
            builder.Property(d => d.Name)
                   .IsRequired()
                   .HasMaxLength(100)
                   .HasColumnName("name");

            builder.Property(d => d.Description)
                   .HasMaxLength(500)
                   .HasColumnName("description");
        }
    }
}
