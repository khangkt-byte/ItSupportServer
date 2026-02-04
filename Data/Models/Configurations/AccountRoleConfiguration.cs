using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ItSupportServer.Data.Models.Configurations
{
    public class AccountRoleConfiguration : IEntityTypeConfiguration<AccountRoles>
    {
        public void Configure(EntityTypeBuilder<AccountRoles> builder)
        {
            // Table
            builder.ToTable("account_roles");

            // Primary key
            builder.HasKey(ar => new { ar.AccountId, ar.RoleId });
            builder.Property(ar => ar.AccountId)
                   .HasColumnName("account_id");
            builder.Property(ar => ar.RoleId)
                   .HasColumnName("role_id");

            // Indexes
            builder.HasIndex(ar => ar.AccountId);
            builder.HasIndex(ar => ar.RoleId);

            // Relationships
            builder.HasOne(ar => ar.Account)
                   .WithMany(a => a.AccountRoles)
                   .HasForeignKey(ar => ar.AccountId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ar => ar.Role)
                   .WithMany(r => r.AccountRoles)
                   .HasForeignKey(ar => ar.RoleId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
