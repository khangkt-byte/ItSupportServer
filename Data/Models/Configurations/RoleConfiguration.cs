using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ItSupportServer.Data.Models.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Roles>
    {
        public void Configure(EntityTypeBuilder<Roles> builder)
        {
            // Table
            builder.ToTable("roles");

            // Primary Key
            builder.HasKey(r => r.RoleId);
            builder.Property(r => r.RoleId)
                .HasColumnName("role_id")
                .ValueGeneratedOnAdd();

            // Ignore the inherited Id property
            builder.Ignore(r => r.Id);

            // Properties
            builder.Property(r => r.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(r => r.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);
        }
    }
}
