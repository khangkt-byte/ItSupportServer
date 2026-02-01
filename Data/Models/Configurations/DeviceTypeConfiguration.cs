using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ItSupportServer.Data.Models.Configurations
{
    public class DeviceTypeConfiguration : IEntityTypeConfiguration<DeviceTypes>
    {
        public void Configure(EntityTypeBuilder<DeviceTypes> builder)
        {
            // Table
            builder.ToTable("device_types");

            // Primary Key
            builder.HasKey(dt => dt.DeviceTypeId);
            builder.Property(dt => dt.DeviceTypeId)
                   .ValueGeneratedOnAdd()
                   .HasColumnName("device_type_id");

            // Ignore the inherited Id property
            builder.Ignore(dt => dt.Id);

            // Indexes
            builder.HasIndex(dt => dt.Name)
                   .HasMethod("gin")
                   .HasOperators("gin_trgm_ops")
                   .HasFilter("deleted_at IS NULL");

            // Properties
            builder.Property(dt => dt.Name)
                   .IsRequired()
                   .HasMaxLength(100)
                   .HasColumnName("name");

            builder.Property(dt => dt.Description)
                   .HasMaxLength(500)
                   .HasColumnName("description");
        }
    }
}
