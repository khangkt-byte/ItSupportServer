using ItSupportServer.src.Modules.Role;
using ItSupportServer.src.Shared.Helper;
using System.ComponentModel.DataAnnotations;
using static ItSupportServer.src.Modules.User.UsersEnum;

namespace ItSupportServer.src.Modules.User.Employee
{
    public class CreateEmployeeDto
    {
        [Required(ErrorMessage = "Tên là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Tên không được quá 200 ký tự")]
        [RegularExpression(@"^[\p{L}\p{M}\p{N} _-]+$",
        ErrorMessage = "Tên chỉ được chứa chữ cái có dấu, số, khoảng trắng, gạch ngang (-) và gạch dưới (_)")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [MaxLength(15, ErrorMessage = "Số điện thoại không được quá 15 ký tự")]
        [RegularExpression(
            @"^(0\d{9,12}|\+?84\d{9,12})$",
            ErrorMessage = "Số điện thoại không hợp lệ (ví dụ: 0912345678 hoặc +84912345678)")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải có độ dài từ 8 đến 100 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9_@#]+$",
        ErrorMessage = "Mật khẩu chỉ được chứa chữ cái, @, #, gạch ngang (-) và gạch dưới (_)")]
        public string Password { get; set; }
        public string? Address { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [DataType(DataType.Date, ErrorMessage = "Ngày sinh không đúng định dạng")]
        [CustomValidation(typeof(MyValidate), nameof(MyValidate.ValidateBirthdayStaff))]
        public DateTime Birthday { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        public GENDER Gender { get; set; }

        public IFormFile? UrlImage { get; set; }

    }

    public class UpdateEmployeeDto
    {
        [Required(ErrorMessage = "Tên là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Tên không được quá 200 ký tự")]
        [RegularExpression(
        @"^[\p{L}\p{M}\p{N} _-]+$",
        ErrorMessage = "Tên chỉ được chứa chữ cái (a-z, A-Z), số (0-9), dấu cách, gạch ngang (-) và gạch dưới (_)")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [MaxLength(15, ErrorMessage = "Số điện thoại không được quá 15 ký tự")]
        [RegularExpression(
            @"^(0\d{9,12}|\+?84\d{9,12})$",
            ErrorMessage = "Số điện thoại không hợp lệ (ví dụ: 0912345678 hoặc +84912345678)")]
        public string PhoneNumber { get; set; } = null!;

        public string? Address { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [DataType(DataType.Date, ErrorMessage = "Ngày sinh không đúng định dạng")]
        [CustomValidation(typeof(MyValidate), nameof(MyValidate.ValidateBirthdayStaff))]
        public DateTime Birthday { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        public GENDER Gender { get; set; }

        [Required(ErrorMessage = "Chức vụ là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Chức vụ không được quá 100 ký tự")]
        public string Position { get; set; } = null!;

        public string? UrlImage { get; set; }

        public IFormFile? NewImage { get; set; }

    }

    public class ListEmployeeDto
    {
        public Guid Id { get; set; }
        public string EmployeeCode { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Position { get; set; }
        public string? UrlImage { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class updateProfileDto
    {

        [Required(ErrorMessage = "Tên là bắt buộc")]
        public string Name { get; set; }

        [MaxLength(15, ErrorMessage = "Số điện thoại không được quá 15 ký tự")]
        [RegularExpression(
            @"^(0\d{9,12}|\+?84\d{9,12})$",
            ErrorMessage = "Số điện thoại không hợp lệ (ví dụ: 0912345678 hoặc +84912345678)")]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public GENDER? Gender { get; set; } = GENDER.Other;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Ngày sinh không đúng định dạng")]
        public DateTime? Birthday { get; set; }
        public IFormFile? Img { get; set; }
    }

    public class DetailUserDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public Dictionary<string, object>? Address { get; set; }
        public DateTime Birthday { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Position { get; set; }
        public string? UrlImage { get; set; }
        public bool Status { get; set; }
        public List<RolesDto>? Roles { get; set; } = new List<RolesDto>();
    }

}