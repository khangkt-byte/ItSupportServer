using FluentValidation;

namespace ItSupportServer.src.Modules.Authentication
{
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Identifier)
                .NotEmpty().WithMessage("Tên đăng nhập hoặc email là bắt buộc.")
                .MaximumLength(254).WithMessage("Tên đăng nhập hoặc email không được quá 254 ký tự.")
                .Matches(@"^(?:[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,63}|[A-Za-z0-9._\-]{3,32})$")
                .WithMessage("Tên đăng nhập hoặc email không đúng định dạng.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu là bắt buộc.")
                .MaximumLength(100).WithMessage("Mật khẩu không được quá 100 ký tự.");
        }
    }

    public class OtpDtoValidator : AbstractValidator<OtpDto>
    {
        public OtpDtoValidator()
        {
            RuleFor(x => x.Otp)
                .NotEmpty().WithMessage("Mã OTP là bắt buộc.")
                .Length(6).WithMessage("Mã OTP phải đủ 6 ký tự.")
                .Matches(@"^\d{6}$").WithMessage("Mã OTP phải gồm 6 chữ số.");

            RuleFor(x => x.AccountId)
                .NotEmpty().WithMessage("UserId là bắt buộc.");
        }
    }

    /// <summary>
    /// Validator for password reset
    /// </summary>
    public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty()
                .WithMessage("Token không được để trống")
                .MinimumLength(40)  // Base64(32 bytes) = ~44 chars
                .WithMessage("Token không hợp lệ");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(8)
                .WithMessage("Mật khẩu phải có ít nhất 8 ký tự")
                .Matches(@"[A-Z]").WithMessage("Mật khẩu phải có ít nhất 1 chữ hoa")
                .Matches(@"[a-z]").WithMessage("Mật khẩu phải có ít nhất 1 chữ thường")
                .Matches(@"[0-9]").WithMessage("Mật khẩu phải có ít nhất 1 số")
                .Matches(@"[\W_]").WithMessage("Mật khẩu phải có ít nhất 1 ký tự đặc biệt");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword)
                .WithMessage("Xác nhận mật khẩu không khớp");
        }
    }
}
