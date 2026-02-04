using System.ComponentModel.DataAnnotations;
using static ITSupportServer.src.Modules.User.UsersEnum;

namespace ITSupportServer.src.Modules.User.Customer
{
    public class RegisterCustomerDto
    {
        [Required(ErrorMessage = "Tên là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Tên không được quá 200 ký tự")]
        [RegularExpression(@"^[\p{L}\p{M}\p{N} _-]+$",
        ErrorMessage = "Tên chỉ được chứa chữ cái có dấu, số, khoảng trắng, gạch ngang (-) và gạch dưới (_)")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        public GENDER Gender { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Tên đăng nhập phải có độ dài từ 5 đến 50 ký tự")]
        [RegularExpression(@"^[A-Za-z0-9_-]+$",
        ErrorMessage = "Tên đăng nhập không được chứa ký tự đặc biệt")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có độ dài từ 6 đến 100 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9_@#]+$",
        ErrorMessage = "Mật khẩu chỉ được chứa chữ cái, @, #, gạch ngang (-) và gạch dưới (_)")]
        public string Password { get; set; }

    }
    public class ResultRegisterCustomerDto : RegisterCustomerDto
    {
        public Guid IdUser { get; set; }
    }

    public class ListCustomerDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string? ImgUrl { get; set; }

    }
}
