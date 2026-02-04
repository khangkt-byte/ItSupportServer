using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ItSupportServer.Data.Models.Configurations
{
    public class DeviceConfiguration : IEntityTypeConfiguration<Devices>
    {
        public void Configure(EntityTypeBuilder<Devices> builder)
        {
            // Table
            builder.ToTable("devices");

            // Primary Key
            builder.HasKey(d => d.DeviceId);
            builder.Property(d => d.DeviceId)
                   .ValueGeneratedOnAdd()
                   .HasColumnName("device_id");

            // Ignore the inherited Id property
            builder.Ignore(d => d.Id);

            // Indexes
            builder.HasIndex(d => d.DeviceTypeId);

            builder.HasIndex(d => d.Name)
                   .HasMethod("gin")
                   .HasOperators("gin_trgm_ops")
                   .HasFilter("deleted_at IS NULL");

            // Properties
            builder.Property(d => d.DeviceTypeId)
                   .IsRequired()
                   .HasColumnName("device_type_id");

            builder.Property(d => d.Name)
                   .IsRequired()
                   .HasMaxLength(255)
                   .HasColumnName("name");

            builder.Property(d => d.Brand)
                   .HasMaxLength(64)
                   .HasColumnName("brand");

            builder.Property(d => d.Model)
                   .IsRequired()
                   .HasMaxLength(128)
                   .HasColumnName("model");

            builder.Property(d => d.SerialNumber)
                   .HasMaxLength(64)
                   .HasColumnName("serial_number");

            builder.Property(d => d.Notes)
                   .HasMaxLength(1000)
                   .HasColumnName("notes");

            // Relationships
            builder.HasOne(d => d.DeviceType)
                   .WithMany(dt => dt.Devices)
                   .HasForeignKey(d => d.DeviceTypeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
