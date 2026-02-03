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
    /// Pattern: OWASP Forgot Password best practices
    /// Reference: 
    /// - OWASP Forgot Password Cheat Sheet
    /// - Microsoft Azure AD Password Reset
    /// - Auth0 Password Reset Flow
    /// </summary>
    public class ResetPasswordValidator : AbstractValidator<ResetPasswordDto>
    {
        // ✅ Base64(32 bytes) = exactly 44 characters
        private const int EXACT_TOKEN_LENGTH = 44;

        public ResetPasswordValidator()
        {
            // ✅ TOKEN VALIDATION
            // Security: Token extracted from email link, not user input
            // Format: Base64-encoded 32-byte random (44 chars)
            RuleFor(x => x.Token)
                .NotEmpty()
                .WithMessage("Liên kết đặt lại mật khẩu không hợp lệ.")  // ✅ User-friendly
                .Length(EXACT_TOKEN_LENGTH)  // ✅ Exact length validation
                .WithMessage("Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.")
                .Matches(@"^[A-Za-z0-9+/=]+$")  // ✅ Base64 pattern
                .WithMessage("Liên kết đặt lại mật khẩu không đúng định dạng.");

            // ✅ PASSWORD VALIDATION (OWASP/NIST compliant)
            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("Vui lòng nhập mật khẩu mới.")
                .MinimumLength(8)
                .WithMessage("Mật khẩu phải có ít nhất 8 ký tự.")
                .MaximumLength(128)
                .WithMessage("Mật khẩu không được quá 128 ký tự.")
                .Matches(@"[A-Z]")
                .WithMessage("Mật khẩu phải có ít nhất 1 chữ hoa.")
                .Matches(@"[a-z]")
                .WithMessage("Mật khẩu phải có ít nhất 1 chữ thường.")
                .Matches(@"[0-9]")
                .WithMessage("Mật khẩu phải có ít nhất 1 chữ số.")
                .Matches(@"[\W_]")
                .WithMessage("Mật khẩu phải có ít nhất 1 ký tự đặc biệt.");

            // ✅ CONFIRM PASSWORD
            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .WithMessage("Vui lòng xác nhận mật khẩu mới.")
                .Equal(x => x.NewPassword)
                .WithMessage("Mật khẩu xác nhận không khớp.");
        }
    }
}
