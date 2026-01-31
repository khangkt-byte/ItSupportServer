using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITSupportServer.Data.Models.Configurations
{
    public class IssueConfiguration : IEntityTypeConfiguration<Issues>
    {
        public void Configure(EntityTypeBuilder<Issues> builder)
        {
            // Table
            builder.ToTable("issues");

            // Primary Key
            builder.HasKey(i => i.IssId);
            builder.Property(i => i.IssId)
                   .HasColumnName("iss_id")
                   .ValueGeneratedOnAdd();

            // Ignore the inherited Id property
            builder.Ignore(i => i.Id);

            // Indexes
            builder.HasIndex(i => i.Name)
                   .HasMethod("gin")
                   .HasOperators("gin_trgm_ops")
                   .HasFilter("\"DeletedAt\" IS NULL");

            // Properties
            builder.Property(i => i.Name)
                   .IsRequired()
                   .HasMaxLength(255)
                   .HasColumnName("name");

            builder.Property(i => i.Description)
                   .HasMaxLength(1000)
                   .HasColumnName("description");

            builder.Property(i => i.Category)
                   .HasMaxLength(100)
                   .HasColumnName("category");

            builder.Property(i => i.Severity)
                   .HasColumnName("severity");
        }
    }
}
