using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ItSupportServer.Data.Models.Configurations
{
    public class IssueLogRequesterConfiguration : IEntityTypeConfiguration<IssueLogRequesters>
    {
        public void Configure(EntityTypeBuilder<IssueLogRequesters> builder)
        {
            // Table
            builder.ToTable("issue_log_requesters");

            // Primary Key
            builder.HasKey(ilr => ilr.IssLogReqId);
            builder.Property(ilr => ilr.IssLogReqId)
                   .HasColumnName("iss_log_req_id")
                   .ValueGeneratedOnAdd();

            // Properties
            builder.Property(ilr => ilr.IssLogId)
                   .IsRequired()
                   .HasColumnName("iss_log_id");

            builder.Property(ilr => ilr.EmpId)
                   .HasColumnName("emp_id");

            builder.Property(ilr => ilr.RequesterName)
                   .HasMaxLength(255)
                   .HasColumnName("requester_name");

            builder.Property(ilr => ilr.RequesterType)
                   .HasMaxLength(100)
                   .HasColumnName("requester_type");

            // Relationships
            builder.HasOne(ilr => ilr.IssueLog)
                   .WithMany(il => il.IssueLogRequesters)
                   .HasForeignKey(ilr => ilr.IssLogId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ilr => ilr.Employee)
                   .WithMany(e => e.IssueLogRequesters)
                   .HasForeignKey(ilr => ilr.EmpId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
