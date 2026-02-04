using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ItSupportServer.Data.Models.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employees>
    {
        public void Configure(EntityTypeBuilder<Employees> builder)
        {
            // Table
            builder.ToTable("employees");

            // Primary Key
            builder.HasKey(e => e.EmpId);
            builder.Property(e => e.EmpId)
                  .HasColumnName("emp_id");

            // Ignore the inherited Id property
            builder.Ignore(e => e.Id);

            // Indexes
            builder.HasIndex(e => e.EmpCode)
                   .IsUnique()
                   .HasFilter("deleted_at IS NULL");

            builder.HasIndex(e => e.FullName)
                   .HasMethod("gin")
                   .HasOperators("gin_trgm_ops")
                   .HasFilter("deleted_at IS NULL");

            builder.HasIndex(e => e.Email)
                   .IsUnique()
                   .HasFilter("deleted_at IS NULL");

            builder.HasIndex(e => e.PhoneNumber)
                   .IsUnique()
                   .HasFilter("deleted_at IS NULL");

            builder.HasIndex(e => e.DptId);

            builder.HasIndex(e => e.AreaId);

            // Properties
            builder.Property(e => e.EmpCode)
                  .HasColumnName("emp_code")
                  .HasMaxLength(20);

            builder.Property(e => e.FullName)
                  .HasColumnName("full_name")
                  .IsRequired()
                  .HasMaxLength(150);

            builder.Property(e => e.PhoneNumber)
                  .HasColumnName("phone_number")
                  .HasMaxLength(15);

            builder.Property(e => e.Email)
                  .HasColumnName("email")
                  .HasMaxLength(254);

            builder.Property(e => e.DptId)
                  .HasColumnName("dpt_id")
                  .IsRequired();

            builder.Property(e => e.AreaId)
                  .HasColumnName("area_id")
                  .IsRequired();

            builder.Property(e => e.Position)
                  .HasColumnName("position")
                  .HasMaxLength(150);

            // Relationships
            builder.HasOne(e => e.Department)
                  .WithMany(d => d.Employees)
                  .HasForeignKey(e => e.DptId)
                  .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Area)
                  .WithMany(a => a.Employees)
                  .HasForeignKey(e => e.AreaId)
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
