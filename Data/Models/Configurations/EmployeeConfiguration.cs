//using DocumentFormat.OpenXml.Vml.Office;
//using ItSupportServer.Data.Models.Entities;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;

//namespace ItSupportServer.Data.Models.Configurations
//{
//    public class EmployeeConfiguration : IEntityTypeConfiguration<Employees>
//    {
//        public void Configure(EntityTypeBuilder<Employees> builder)
//        {
//            // 1. Khai báo bảng và Schema (tùy chọn)
//            builder.ToTable("employees");

//            // 2. Cấu hình Khóa chính và Ánh xạ với BaseEntity
//            builder.HasKey(e => e.EmpId);
//            builder.Property(e => e.EmpId)
//                  .HasColumnName("emp_id")
//                  .ValueGeneratedOnAdd(); // Tự động tạo Guid

//            // Bỏ qua thuộc tính Id từ BaseEntity vì bạn đã dùng EmpId làm khóa chính
//            builder.Ignore(e => e.Id);

//            // 3. Cấu hình Index và Unique (Duy nhất)
//            builder.HasIndex(e => e.EmpCode).IsUnique();
//            builder.HasIndex(e => e.Email).IsUnique();
//            builder.HasIndex(e => e.PhoneNumber).IsUnique();

//            // 4. Cấu hình các thuộc tính (Properties)
//            builder.Property(e => e.EmpCode)
//                  .HasColumnName("emp_code")
//                  .HasMaxLength(20);

//            builder.Property(e => e.FullName)
//                  .HasColumnName("full_name")
//                  .IsRequired()
//                  .HasMaxLength(150);

//            builder.Property(e => e.PhoneNumber)
//                  .HasColumnName("phone_number")
//                  .HasMaxLength(15);

//            builder.Property(e => e.Email)
//                  .HasColumnName("email")
//                  .HasMaxLength(254);

//            builder.Property(e => e.Position)
//                  .HasColumnName("position")
//                  .HasMaxLength(150);

//            // 5. Cấu hình Quan hệ (Relationships)

//            // Employees -> Departments (n : 1)
//            builder.HasOne(e => e.Department)
//                  .WithMany() // Giả định Department không có Collection<Employees>
//                  .HasForeignKey(e => e.DptId)
//                  .OnDelete(DeleteBehavior.Restrict);

//            // Employees -> Areas (n : 1)
//            builder.HasOne(e => e.Area)
//                  .WithMany()
//                  .HasForeignKey(e => e.AreaId)
//                  .OnDelete(DeleteBehavior.Restrict);

//            // Employees -> Accounts (1 : 1)
//            builder.HasOne(e => e.Account)
//                  .WithOne(a => a.Employee)
//                  .HasForeignKey<Accounts>(a => a.AccountId); // Giả định khóa ngoại nằm ở Accounts
//        }
//    }
//}
