using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ItSupportServer.Data.Models.Configurations
{
    public class IssueLogOperatorConfiguration : IEntityTypeConfiguration<IssueLogOperators>
    {
        public void Configure(EntityTypeBuilder<IssueLogOperators> builder)
        {
            // Table
            builder.ToTable("issue_log_operators");

            // Primary Key
            builder.HasKey(ilo => new { ilo.IssLogId, ilo.EmpId });
            builder.Property(ilo => ilo.IssLogId)
                   .HasColumnName("iss_log_id");
            builder.Property(ilo => ilo.EmpId)
                   .HasColumnName("emp_id");

            // Indexes
            builder.HasIndex(ilo => ilo.EmpId);

            // Properties
            builder.Property(ilo => ilo.OperatorRole)
                   .HasColumnName("operator_role")
                   .HasMaxLength(100);

            builder.Property(ilo => ilo.HoursSpent)
                   .HasColumnName("hours_spent")
                   .HasColumnType("decimal(5, 2)");

            // Relationships
            builder.HasOne(ilo => ilo.IssueLog)
                   .WithMany(il => il.IssueLogOperators)
                   .HasForeignKey(ilo => ilo.IssLogId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ilo => ilo.Employee)
                   .WithMany(e => e.IssueLogOperators)
                   .HasForeignKey(ilo => ilo.EmpId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
