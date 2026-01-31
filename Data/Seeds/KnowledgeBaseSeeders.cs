using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.Data.Seeds
{
    /// <summary>
    /// Knowledge base seeders for common IT issues and causes
    /// Pattern: ITIL Service Catalog, ServiceNow Knowledge Base
    /// Use: Pre-populate common problems for faster logging
    /// Reference: ITIL Problem Management, ServiceNow KB structure
    /// </summary>
    public static class KnowledgeBaseSeeders
    {
        private static readonly DateTime SeedDate = new(2025, 10, 30, 9, 38, 50, DateTimeKind.Utc);

        public static void SeedKnowledgeBase(ModelBuilder modelBuilder)
        {
            SeedIssues(modelBuilder);
            SeedCauses(modelBuilder);
        }

        /// <summary>
        /// Seed common IT issues (categorized)
        /// Pattern: ServiceNow incident categories
        /// </summary>
        private static void SeedIssues(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Issues>().HasData(
                // ===== HARDWARE ISSUES =====
                new Issues
                {
                    IssId = 1,
                    Name = "Máy tính không khởi động được",
                    Description = "PC/Laptop không lên nguồn hoặc không vào được Windows",
                    Category = "Hardware",
                    Severity = 4,  // High
                    CreatedAt = SeedDate
                },
                new Issues
                {
                    IssId = 2,
                    Name = "Màn hình không hiển thị",
                    Description = "Màn hình bị đen, không có tín hiệu",
                    Category = "Hardware",
                    Severity = 3,
                    CreatedAt = SeedDate
                },
                new Issues
                {
                    IssId = 3,
                    Name = "Bàn phím/Chuột không hoạt động",
                    Description = "Bàn phím hoặc chuột không phản hồi",
                    Category = "Hardware",
                    Severity = 2,
                    CreatedAt = SeedDate
                },
                new Issues
                {
                    IssId = 4,
                    Name = "Máy in bị kẹt giấy",
                    Description = "Máy in bị kẹt giấy, không in được",
                    Category = "Hardware",
                    Severity = 2,
                    CreatedAt = SeedDate
                },

                // ===== NETWORK ISSUES =====
                new Issues
                {
                    IssId = 5,
                    Name = "Mất kết nối mạng",
                    Description = "Không vào được internet, mất kết nối LAN",
                    Category = "Network",
                    Severity = 4,
                    CreatedAt = SeedDate
                },
                new Issues
                {
                    IssId = 6,
                    Name = "Kết nối mạng chậm",
                    Description = "Internet/Mạng nội bộ chậm, lag",
                    Category = "Network",
                    Severity = 3,
                    CreatedAt = SeedDate
                },
                new Issues
                {
                    IssId = 7,
                    Name = "Không kết nối được Wifi",
                    Description = "Không tìm thấy hoặc không kết nối được mạng Wifi",
                    Category = "Network",
                    Severity = 3,
                    CreatedAt = SeedDate
                },

                // ===== SOFTWARE ISSUES =====
                new Issues
                {
                    IssId = 8,
                    Name = "Phần mềm bị lỗi/crash",
                    Description = "Ứng dụng bị treo, thoát đột ngột",
                    Category = "Software",
                    Severity = 3,
                    CreatedAt = SeedDate
                },
                new Issues
                {
                    IssId = 9,
                    Name = "Không cài đặt được phần mềm",
                    Description = "Lỗi khi cài đặt ứng dụng/phần mềm",
                    Category = "Software",
                    Severity = 2,
                    CreatedAt = SeedDate
                },
                new Issues
                {
                    IssId = 10,
                    Name = "Quên mật khẩu",
                    Description = "Quên mật khẩu Windows, Email, ứng dụng nội bộ",
                    Category = "Software",
                    Severity = 2,
                    CreatedAt = SeedDate
                },

                // ===== EMAIL ISSUES =====
                new Issues
                {
                    IssId = 11,
                    Name = "Không gửi/nhận email được",
                    Description = "Lỗi khi gửi hoặc nhận email",
                    Category = "Email",
                    Severity = 4,
                    CreatedAt = SeedDate
                },
                new Issues
                {
                    IssId = 12,
                    Name = "Email bị đầy dung lượng",
                    Description = "Mailbox đạt giới hạn dung lượng",
                    Category = "Email",
                    Severity = 2,
                    CreatedAt = SeedDate
                },

                // ===== SYSTEM ISSUES =====
                new Issues
                {
                    IssId = 13,
                    Name = "Máy tính chạy chậm",
                    Description = "PC/Laptop chạy chậm, lag",
                    Category = "System",
                    Severity = 2,
                    CreatedAt = SeedDate
                },
                new Issues
                {
                    IssId = 14,
                    Name = "Blue Screen (BSOD)",
                    Description = "Màn hình xanh chết, Windows crash",
                    Category = "System",
                    Severity = 5,  // Critical
                    CreatedAt = SeedDate
                },
                new Issues
                {
                    IssId = 15,
                    Name = "Ổ cứng đầy",
                    Description = "Dung lượng ổ cứng không đủ",
                    Category = "System",
                    Severity = 3,
                    CreatedAt = SeedDate
                }
            );
        }

        /// <summary>
        /// Seed common causes for IT issues
        /// Pattern: Root cause analysis (RCA) - ITIL Problem Management
        /// </summary>
        private static void SeedCauses(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Causes>().HasData(
                // Causes for "Máy tính không khởi động" (IssId = 1)
                new Causes
                {
                    CauseId = 1,
                    IssId = 1,
                    Name = "Nguồn điện hỏng",
                    Description = "Bộ nguồn PSU bị hỏng hoặc dây nguồn lỏng",
                    CreatedAt = SeedDate
                },
                new Causes
                {
                    CauseId = 2,
                    IssId = 1,
                    Name = "RAM bị lỏng/hỏng",
                    Description = "RAM không nhận hoặc bị lỗi",
                    CreatedAt = SeedDate
                },
                new Causes
                {
                    CauseId = 3,
                    IssId = 1,
                    Name = "Mainboard hỏng",
                    Description = "Bo mạch chủ bị chập cháy hoặc hỏng",
                    CreatedAt = SeedDate
                },

                // Causes for "Màn hình không hiển thị" (IssId = 2)
                new Causes
                {
                    CauseId = 4,
                    IssId = 2,
                    Name = "Cáp VGA/HDMI lỏng",
                    Description = "Dây tín hiệu màn hình bị lỏng hoặc hỏng",
                    CreatedAt = SeedDate
                },
                new Causes
                {
                    CauseId = 5,
                    IssId = 2,
                    Name = "Card đồ họa hỏng",
                    Description = "VGA/GPU bị lỗi hoặc hỏng",
                    CreatedAt = SeedDate
                },

                // Causes for "Mất kết nối mạng" (IssId = 5)
                new Causes
                {
                    CauseId = 6,
                    IssId = 5,
                    Name = "Dây mạng bị đứt",
                    Description = "Cáp Ethernet bị đứt hoặc tiếp xúc kém",
                    CreatedAt = SeedDate
                },
                new Causes
                {
                    CauseId = 7,
                    IssId = 5,
                    Name = "Switch/Router hỏng",
                    Description = "Thiết bị mạng bị hỏng hoặc mất điện",
                    CreatedAt = SeedDate
                },
                new Causes
                {
                    CauseId = 8,
                    IssId = 5,
                    Name = "Cấu hình IP sai",
                    Description = "IP tĩnh sai hoặc DHCP không cấp IP",
                    CreatedAt = SeedDate
                },

                // Causes for "Phần mềm bị lỗi" (IssId = 8)
                new Causes
                {
                    CauseId = 9,
                    IssId = 8,
                    Name = "File hệ thống bị lỗi",
                    Description = "File DLL thiếu hoặc bị corrupt",
                    CreatedAt = SeedDate
                },
                new Causes
                {
                    CauseId = 10,
                    IssId = 8,
                    Name = "Phần mềm chưa update",
                    Description = "Phiên bản cũ có bug, cần update",
                    CreatedAt = SeedDate
                },

                // Causes for "Email không gửi được" (IssId = 11)
                new Causes
                {
                    CauseId = 11,
                    IssId = 11,
                    Name = "Cấu hình SMTP sai",
                    Description = "Thông tin server SMTP không đúng",
                    CreatedAt = SeedDate
                },
                new Causes
                {
                    CauseId = 12,
                    IssId = 11,
                    Name = "Tài khoản email bị khóa",
                    Description = "Account email bị khóa do gửi spam hoặc vi phạm chính sách",
                    CreatedAt = SeedDate
                },

                // Causes for "Máy tính chạy chậm" (IssId = 13)
                new Causes
                {
                    CauseId = 13,
                    IssId = 13,
                    Name = "Virus/Malware",
                    Description = "Máy nhiễm virus, phần mềm gián điệp",
                    CreatedAt = SeedDate
                },
                new Causes
                {
                    CauseId = 14,
                    IssId = 13,
                    Name = "RAM không đủ",
                    Description = "Dung lượng RAM không đủ cho ứng dụng",
                    CreatedAt = SeedDate
                },
                new Causes
                {
                    CauseId = 15,
                    IssId = 13,
                    Name = "Ổ cứng gần đầy",
                    Description = "HDD/SSD dung lượng >90%",
                    CreatedAt = SeedDate
                }
            );
        }
    }
}