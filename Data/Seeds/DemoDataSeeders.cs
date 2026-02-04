using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.Data.Seeds
{
    /// <summary>
    /// Demo/sample data seeders for testing and demonstration
    /// Pattern: Realistic test data
    /// Use: Development, staging environments (NOT production)
    /// Reference: Test data best practices
    /// </summary>
    public static class DemoDataSeeders
    {
        private static readonly DateTime SeedDate = new(2025, 10, 30, 9, 38, 50, DateTimeKind.Utc);

        public static void SeedDemoData(ModelBuilder modelBuilder)
        {
            // Only seed if in development
            // Note: Call this conditionally in AppDbContext based on environment
            SeedSampleIssueLogs(modelBuilder);
        }

        /// <summary>
        /// Seed sample issue logs for demo
        /// Pattern: Realistic IT support tickets
        /// </summary>
        private static void SeedSampleIssueLogs(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IssueLogs>().HasData(
                // Ticket 1: Network issue
                new IssueLogs
                {
                    IssLogId = Guid.Parse("22222222-2222-2222-2222-222222222221"),
                    Operator = "Nguyễn Văn A",
                    Requester = "Phòng Kế toán, Trần Thị B",
                    DepartmentId = 3,  // Kế toán
                    AreaId = 2,        // Tầng 2
                    IssueId = 5,       // Mất kết nối mạng
                    IssueDescription = "Máy tính tại phòng kế toán không vào được mạng nội bộ",
                    CauseId = 6,       // Dây mạng đứt
                    Cause = "Dây mạng bị đứt do bị kéo lê",
                    Resolution = "Thay dây mạng Cat6 mới, test kết nối OK",
                    PermanentFix = "Dùng ống luồn dây để bảo vệ cáp mạng",
                    Notes = "Đã cảnh báo user không kéo lê dây mạng",
                    DateReported = SeedDate.AddDays(-5),
                    Status = "Resolved",
                    CreatedAt = SeedDate.AddDays(-5)
                },

                // Ticket 2: Printer issue
                new IssueLogs
                {
                    IssLogId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Operator = "Nguyễn Văn A",
                    Requester = "Phòng Hành chính, Lê Văn C",
                    DepartmentId = 2,  // Hành chính
                    AreaId = 1,        // Tầng 1
                    IssueId = 4,       // Máy in kẹt giấy
                    IssueDescription = "Máy in tầng 1 bị kẹt giấy liên tục",
                    CauseId = null,
                    Cause = "Giấy bị ẩm do để gần cửa sổ",
                    Resolution = "Lấy giấy kẹt ra, thay giấy mới khô ráo",
                    PermanentFix = "Di chuyển máy in ra xa cửa sổ, bảo quản giấy trong tủ kín",
                    Notes = "Đã hướng dẫn user cách xử lý kẹt giấy cơ bản",
                    DateReported = SeedDate.AddDays(-3),
                    Status = "Resolved",
                    CreatedAt = SeedDate.AddDays(-3)
                },

                // Ticket 3: Software issue
                new IssueLogs
                {
                    IssLogId = Guid.Parse("22222222-2222-2222-2222-222222222223"),
                    Operator = "Nguyễn Văn A",
                    Requester = "Phòng Kinh doanh, Phạm Thị D",
                    DepartmentId = 4,  // Kinh doanh
                    AreaId = 2,        // Tầng 2
                    IssueId = 10,      // Quên mật khẩu
                    IssueDescription = "Nhân viên quên mật khẩu đăng nhập Windows",
                    CauseId = null,
                    Cause = "User tự đổi mật khẩu và quên",
                    Resolution = "Reset mật khẩu Windows qua Active Directory",
                    PermanentFix = "Hướng dẫn user sử dụng password manager",
                    Notes = "Đã gửi email hướng dẫn sử dụng LastPass cho toàn công ty",
                    DateReported = SeedDate.AddDays(-1),
                    Status = "Resolved",
                    CreatedAt = SeedDate.AddDays(-1)
                },

                // Ticket 4: Slow computer
                new IssueLogs
                {
                    IssLogId = Guid.Parse("22222222-2222-2222-2222-222222222224"),
                    Operator = "Nguyễn Văn A",
                    Requester = "Phòng CNTT, Hoàng Văn E",
                    DepartmentId = 1,  // CNTT
                    AreaId = 3,        // Tầng 3
                    IssueId = 13,      // Máy chạy chậm
                    IssueDescription = "Laptop chạy rất chậm khi mở nhiều ứng dụng",
                    CauseId = 14,      // RAM không đủ
                    Cause = "RAM 4GB không đủ cho Visual Studio + Docker",
                    Resolution = "Nâng cấp RAM từ 4GB lên 16GB",
                    PermanentFix = "Khuyến nghị tối thiểu 16GB RAM cho dev machine",
                    Notes = "Đã đề xuất mua thêm RAM cho 5 máy dev khác",
                    DateReported = SeedDate,
                    Status = "In Progress",
                    CreatedAt = SeedDate
                }
            );
        }
    }
}