using FluentValidation;

namespace ItSupportServer.src.Modules.Account
{
    public class CreateAccountDtoValidator : AbstractValidator<CreateAccountDto>
    {
        public CreateAccountDtoValidator()
        {
            RuleFor(x => x.EmpId)
                .NotEmpty()
                .WithMessage("Vui lòng chọn nhân viên.");

            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("Username là bắt buộc.")
                .Length(3, 32)
                .WithMessage("Username phải có độ dài từ 3 đến 32 ký tự.")
                .Matches(@"^[a-zA-Z0-9_]+$")
                .WithMessage("Username chỉ được chứa chữ cái, số và gạch dưới (_).");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Mật khẩu là bắt buộc.")
                .Length(6, 100)
                .WithMessage("Mật khẩu phải có độ dài từ 6 đến 100 ký tự.")
                .Matches(@"^[a-zA-Z0-9_@#]+$")
                .WithMessage("Mật khẩu chỉ được chứa chữ cái, số và các ký tự đặc biệt (@, #, _).");

            RuleFor(x => x.RoleIds)
                .Must(ids => ids == null || ids.All(id => id > 0))
                .WithMessage("Role IDs không hợp lệ.")
                .When(x => x.RoleIds != null);
        }
    }

    public class UpdateAccountDtoValidator : AbstractValidator<UpdateAccountDto>
    {
        public UpdateAccountDtoValidator()
        {
            RuleFor(x => x.Username)
                .Length(3, 32)
                .WithMessage("Username phải có độ dài từ 3 đến 32 ký tự.")
                .Matches(@"^[a-zA-Z0-9_]+$")
                .WithMessage("Username chỉ được chứa chữ cái, số và gạch dưới (_).")
                .When(x => x.Username != null);
        }
    }

    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
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
