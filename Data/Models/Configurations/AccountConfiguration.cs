using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ItSupportServer.Data.Models.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Accounts>
    {
        public void Configure(EntityTypeBuilder<Accounts> builder)
        {
            // Table
            builder.ToTable("accounts");

            // Primary Key
            builder.HasKey(a => a.AccountId);
            builder.Property(a => a.AccountId)
                   .HasColumnName("acc_id");

            // Ignore the inherited Id property
            builder.Ignore(a => a.Id);

            // Indexes
            builder.HasIndex(a => a.Username)
                   .IsUnique()
                   .HasFilter("deleted_at IS NULL");

            // Properties
            builder.Property(a => a.Username)
                   .IsRequired()
                   .HasMaxLength(32)
                   .HasColumnName("username");

            builder.Property(a => a.Password)
                   .IsRequired()
                   .HasColumnName("password");

            builder.Property(a => a.IsLocked)
                   .HasColumnName("is_locked");

            builder.Property(a => a.FailedLoginAttempts)
                   .HasColumnName("failed_login_attempts");

            builder.Property(a => a.LastLoginAt)
                   .HasColumnName("last_login_at");

            builder.Property(a => a.LockedUntil)
                   .HasColumnName("locked_until");

            builder.Property(a => a.CurrentPoints)
                   .HasColumnName("current_points");

            builder.Property(a => a.LifetimePoints)
                   .HasColumnName("lifetime_points");

            builder.Property(a => a.Otp)
                   .HasColumnName("otp");

            builder.Property(a => a.ExpiredOtp)
                   .HasColumnName("expired_otp");

            // Relationships
            builder.HasOne(a => a.Employee)
                   .WithOne(a => a.Account)
                   .HasForeignKey<Accounts>(a => a.AccountId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
