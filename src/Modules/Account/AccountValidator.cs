using FluentValidation;

namespace ItSupportServer.src.Modules.Account
{
    public class CreateAccountsDtoValidator : AbstractValidator<CreateAccountsDto>
    {
        public CreateAccountsDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username là bắt buộc.")
                .MaximumLength(32).WithMessage("Username không được quá 32 ký tự.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu là bắt buộc.")
                .Length(6, 100).WithMessage("Mật khẩu phải có độ dài từ 6 đến 100 ký tự.")
                .Matches(@"^[a-zA-Z0-9_@#]+$").WithMessage("Mật khẩu chỉ được chứa chữ cái, @, #, gạch ngang (-) và gạch dưới (_).");
        }
    }

    public class UpdateAccountsDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public UpdateAccountsDtoValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Vui lòng nhập mật khẩu hiện tại.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Vui lòng nhập mật khẩu mới.")
                .Length(6, 100).WithMessage("Mật khẩu phải có độ dài từ 6 đến 100 ký tự.")
                .Matches(@"^[a-zA-Z0-9_@#]+$").WithMessage("Mật khẩu chỉ được chứa chữ cái, @, #, gạch ngang (-) và gạch dưới (_).")
                .When(x => !string.IsNullOrEmpty(x.NewPassword));
        }
    }
}
