using FluentValidation;

namespace ItSupportServer.src.Modules.Authentication
{
    public class LoginValidator : AbstractValidator<LoginDto>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Identifier)
                .NotEmpty().WithMessage("Vui lòng nhập tên đăng nhập hoặc email.")
                .MaximumLength(254).WithMessage("Tên đăng nhập hoặc email không được quá 254 ký tự.")
                .Matches(@"^(?:[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,63}|[A-Za-z0-9._\-]{3,32})$")
                .WithMessage("Tên đăng nhập hoặc email không đúng định dạng.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Vui lòng nhập mật khẩu.")
                .MaximumLength(128)
                .WithMessage("Mật khẩu không được quá 128 ký tự.");
        }
    }

    public class OtpValidator : AbstractValidator<OtpDto>
    {
        public OtpValidator()
        {
            RuleFor(x => x.Otp)
                .NotEmpty().WithMessage("Vui lòng nhập mã OTP.")
                .Length(6).WithMessage("Mã OTP phải đủ 6 ký tự.")
                .Matches(@"^\d{6}$").WithMessage("Mã OTP phải gồm 6 chữ số.");

            RuleFor(x => x.AccountId)
                .NotEmpty().WithMessage("Không tìm thấy tài khoản.");
        }
    }

    /// <summary>
    /// Validator for password reset
    /// </summary>
    public class ResetPasswordValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty()
                .WithMessage("Token không được để trống")
                .MinimumLength(40)  // Base64(32 bytes) = ~44 chars
                .WithMessage("Token không hợp lệ");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("Vui lòng nhập mật khẩu.")
                .MinimumLength(8)  // ✅ NIST: Min 8 chars
                .WithMessage("Mật khẩu phải có ít nhất 8 ký tự.")
                .MaximumLength(128)  // ✅ NIST: Max 128 chars
                .WithMessage("Mật khẩu không được quá 128 ký tự.")
                .Matches(@"[A-Z]")
                .WithMessage("Mật khẩu phải có ít nhất 1 chữ hoa.")
                .Matches(@"[a-z]")
                .WithMessage("Mật khẩu phải có ít nhất 1 chữ thường.")
                .Matches(@"[0-9]")
                .WithMessage("Mật khẩu phải có ít nhất 1 chữ số.")
                .Matches(@"[\W_]")
                .WithMessage("Mật khẩu phải có ít nhất 1 ký tự đặc biệt.");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword)
                .WithMessage("Xác nhận mật khẩu không khớp");
        }
    }
}
