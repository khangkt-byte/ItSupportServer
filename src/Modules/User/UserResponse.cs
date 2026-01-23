using ItSupportServer.src.Shared.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ItSupportServer.src.Modules.User
{
    public class ProfileDto
    {
        [Description("Mã định danh duy nhất của người dùng (GUID)")]
        [OpenApiExample("3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        required public string EmpId { get; set; }

        [Description("Tên đăng nhập dùng để truy cập hệ thống")]
        [OpenApiExample("nguyenvana")]
        public string? Username { get; set; }

        [Description("Mã nhân viên nội bộ (duy nhất)")]
        [OpenApiExample("NV2024001")]
        required public string EmpCode { get; set; }

        [Description("Họ và tên đầy đủ")]
        [OpenApiExample("Nguyễn Văn An")]
        required public string FullName { get; set; }

        [Description("Địa chỉ Email liên hệ")]
        [DataType(DataType.EmailAddress)] // Scalar sẽ hiện đúng format email
        [OpenApiExample("an.nguyen@nhahang.com")]
        public string? Email { get; set; }

        [Description("Số điện thoại cá nhân")]
        [DataType(DataType.PhoneNumber)]
        [OpenApiExample("0979123456")]
        public string? PhoneNumber { get; set; }

        [Description("Ngày tháng năm sinh")]
        [DataType(DataType.Date)] // Scalar sẽ hiện Date Picker (bỏ phần giờ)
        [OpenApiExample("1995-08-15")]
        public DateTime? Birthday { get; set; }

        [Description("Thời điểm tài khoản được tạo")]
        [OpenApiExample("2023-01-01T08:30:00Z")]
        public DateTime CreatedAt { get; set; }

        [Description("Thời điểm cập nhật thông tin gần nhất")]
        [OpenApiExample("2024-02-20T10:15:00Z")]
        public DateTime? UpdatedAt { get; set; }

        [Description("Chức vụ hoặc vị trí công việc")]
        [OpenApiExample("Quản lý chi nhánh")]
        public string? Position { get; set; }

        [Description("Link ảnh đại diện (Avatar)")]
        [DataType(DataType.ImageUrl)]
        [OpenApiExample("https://i.imgur.com/example-avatar.png")]
        public string? UrlImage { get; set; }

        [Description("Trạng thái hoạt động (True: Active, False: Inactive)")]
        [OpenApiExample("true")]
        public bool? Status { get; set; }
    }
}
