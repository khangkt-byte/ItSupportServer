using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ItSupportServer.Data.Models.Configurations
{
    public class IssueLogConfiguration : IEntityTypeConfiguration<IssueLogs>
    {
        public void Configure(EntityTypeBuilder<IssueLogs> builder)
        {
            // Table
            builder.ToTable("issue_logs");

            // Primary Key
            builder.HasKey(il => il.IssLogId);
            builder.Property(il => il.IssLogId)
                   .HasColumnName("iss_log_id");

            // Ignore the inherited Id property
            builder.Ignore(il => il.Id);

            // Indexes
            builder.HasIndex(il => il.DptId);

            builder.Property(il => il.AreaId);

            builder.Property(il => il.IssueId);

            builder.HasIndex(il => il.CauseId);

            // Properties
            builder.Property(il => il.Operator)
                   .IsRequired()
                   .HasMaxLength(500)
                   .HasColumnName("operator");

            builder.Property(il => il.Requester)
                   .HasMaxLength(500)
                   .HasColumnName("requester");

            builder.Property(il => il.DptId)
                   .IsRequired()
                   .HasColumnName("dpt_id");

            builder.Property(il => il.AreaId)
                   .IsRequired()
                   .HasColumnName("area_id");

            builder.Property(il => il.IssueId)
                   .HasColumnName("issue_id");

            builder.Property(il => il.IssueDescription)
                   .IsRequired()
                   .HasMaxLength(2000)
                   .HasColumnName("issue_description");

            builder.Property(il => il.CauseId)
                   .HasColumnName("cause_id");

            builder.Property(il => il.Cause)
                   .HasMaxLength(1000)
                   .HasColumnName("cause");

            builder.Property(il => il.Resolution)
                   .HasMaxLength(2000)
                   .HasColumnName("resolution");

            builder.Property(il => il.PermanentFix)
                   .HasMaxLength(2000)
                   .HasColumnName("permanent_fix");

            builder.Property(il => il.Notes)
                   .HasMaxLength(1000)
                   .HasColumnName("notes");

            builder.Property(il => il.DateReported)
                   .HasColumnName("date_reported");

            builder.Property(il=>il.Status)
                   .HasMaxLength(50)
                   .HasColumnName("status");

            // Relationships
            builder.HasOne(il => il.Department)
                   .WithMany(d => d.IssueLogs)
                   .HasForeignKey(il => il.DptId);
                   //.OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(il => il.Area)
                   .WithMany(a => a.IssueLogs)
                   .HasForeignKey(il => il.AreaId);
                   //.OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(il => il.Issue)
                   .WithMany(i => i.IssueLogs)
                   .HasForeignKey(il => il.IssueId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(il => il.CauseRef)
                   .WithMany(c => c.IssueLogs)
                   .HasForeignKey(il => il.CauseId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
