using FluentValidation;

namespace ItSupportServer.src.Modules.Account
{
    public class CreateAccountValidator : AbstractValidator<CreateAccountDto>
    {
        public CreateAccountValidator()
        {
            RuleFor(x => x.EmpId)
                .NotEmpty()
                .WithMessage("Vui lòng chọn nhân viên.");

            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("Vui lòng nhập username.")
                .Length(3, 32)
                .WithMessage("Username phải có độ dài từ 3 đến 32 ký tự.")
                .Matches(@"^[a-zA-Z0-9_]+$")
                .WithMessage("Username chỉ được chứa chữ cái, số và gạch dưới (_).");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Mật khẩu là bắt buộc.")
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

            RuleFor(x => x.RoleIds)
                .Must(ids => ids == null || ids.All(id => id > 0))
                .WithMessage("Vui lòng chọn vai trò.")
                .When(x => x.RoleIds != null);
        }
    }

    public class UpdateAccountValidator : AbstractValidator<UpdateAccountDto>
    {
        public UpdateAccountValidator()
        {
            RuleFor(x => x.Username)
                .Length(3, 32)
                .WithMessage("Username phải có độ dài từ 3 đến 32 ký tự.")
                .Matches(@"^[a-zA-Z0-9_]+$")
                .WithMessage("Username chỉ được chứa chữ cái, số và gạch dưới (_).")
                .When(x => x.Username != null);
        }
    }

    public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty()
                .WithMessage("Vui lòng nhập mật khẩu hiện tại.");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("Vui lòng nhập mật khẩu mới.")
                .Length(6, 100)
                .WithMessage("Mật khẩu phải có độ dài từ 6 đến 100 ký tự.")
                .Matches(@"^[a-zA-Z0-9_@#]+$")
                .WithMessage("Mật khẩu chỉ được chứa chữ cái, số và các ký tự đặc biệt (@, #, _).");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .WithMessage("Vui lòng xác nhận mật khẩu mới.")
                .Equal(x => x.NewPassword)
                .WithMessage("Mật khẩu xác nhận không khớp.");
        }
    }
}
