using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.Data.Seeds
{
    /// <summary>
    /// Master data seeders for reference tables
    /// Pattern: Immutable reference data (rarely changes)
    /// Use: Pre-populate dropdown lists, validation lookups
    /// Reference: Enterprise master data management practices
    /// </summary>
    public static class MasterDataSeeders
    {
        private static readonly DateTime SeedDate = new(2025, 10, 30, 9, 38, 50, DateTimeKind.Utc);

        public static void SeedMasterData(ModelBuilder modelBuilder)
        {
            SeedDepartments(modelBuilder);
            SeedAreas(modelBuilder);
        }

        /// <summary>
        /// Seed departments (phòng ban)
        /// Pattern: Common Vietnamese company structure
        /// </summary>
        private static void SeedDepartments(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Departments>().HasData(
                new Departments
                {
                    DptId = 1,
                    Name = "Phòng CNTT",
                    Description = "Phòng Công nghệ thông tin - Quản lý hạ tầng IT, phát triển phần mềm",
                    CreatedAt = SeedDate
                },
                new Departments
                {
                    DptId = 2,
                    Name = "Phòng Hành chính - Nhân sự",
                    Description = "Quản lý nhân sự, hành chính văn phòng",
                    CreatedAt = SeedDate
                },
                new Departments
                {
                    DptId = 3,
                    Name = "Phòng Kế toán",
                    Description = "Quản lý tài chính, kế toán doanh nghiệp",
                    CreatedAt = SeedDate
                },
                new Departments
                {
                    DptId = 4,
                    Name = "Phòng Kinh doanh",
                    Description = "Phát triển kinh doanh, chăm sóc khách hàng",
                    CreatedAt = SeedDate
                },
                new Departments
                {
                    DptId = 5,
                    Name = "Phòng Kỹ thuật",
                    Description = "Sản xuất, bảo trì thiết bị kỹ thuật",
                    CreatedAt = SeedDate
                }
            );
        }

        /// <summary>
        /// Seed areas (khu vực làm việc)
        /// Pattern: Typical office building layout
        /// </summary>
        private static void SeedAreas(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Areas>().HasData(
                new Areas
                {
                    AreaId = 1,
                    Name = "Tầng 1",
                    Description = "Khu vực tiếp tân, phòng họp chính",
                    CreatedAt = SeedDate
                },
                new Areas
                {
                    AreaId = 2,
                    Name = "Tầng 2",
                    Description = "Văn phòng làm việc phòng Kinh doanh, Kế toán",
                    CreatedAt = SeedDate
                },
                new Areas
                {
                    AreaId = 3,
                    Name = "Tầng 3",
                    Description = "Văn phòng phòng CNTT, Server room",
                    CreatedAt = SeedDate
                },
                new Areas
                {
                    AreaId = 4,
                    Name = "Tầng 4",
                    Description = "Phòng họp, khu vực đào tạo",
                    CreatedAt = SeedDate
                },
                new Areas
                {
                    AreaId = 5,
                    Name = "Nhà xưởng",
                    Description = "Khu vực sản xuất, bảo trì thiết bị",
                    CreatedAt = SeedDate
                }
            );
        }
    }
}