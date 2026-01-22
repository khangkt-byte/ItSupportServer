using System.ComponentModel.DataAnnotations;

namespace ItSupportServer.src.Modules.Authentication
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Tên đăng nhập hoặc email là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Tên đăng nhập hoặc email không được quá 200 ký tự")]
        [RegularExpression(
           @"^(?:[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,63}|[A-Za-z0-9._\-]{3,32})$",
            ErrorMessage = "Tên đăng nhập hoặc email không đúng định dạng")]
        public string UserNameOrEmail { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Mật khẩu không được quá 100 ký tự")]
        public string Password { get; set; }
    }

    public class TokenResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string? AccountId { get; set; }
    }

    public class RefreshTokenRequestDto
    {
        public required string RefreshToken { get; set; }
    }

    public class OtpDto
    {
        [Required(ErrorMessage = "Mã OTP là bắt buộc")]
        [MaxLength(6, ErrorMessage = "Mã OTP không được quá 6 ký tự")]
        [MinLength(6, ErrorMessage = "Mã OTP phải đủ 6 ký tự")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP phải gồm 6 chữ số")]
        public string Otp { get; set; }

        [Required(ErrorMessage = "UserId là bắt buộc")]
        public string AccountId { get; set; }
    }
}
