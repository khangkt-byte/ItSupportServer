using FluentValidation;

namespace ItSupportServer.src.Modules.Role
{
    public class CreateRoleDtoValidator : AbstractValidator<CreateRoleDto>
    {
        public CreateRoleDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên vai trò là bắt buộc.")
                .MaximumLength(150).WithMessage("Tên vai trò không được quá 150 ký tự.")
                .Matches(@"^[\p{L}\p{M}\p{N} _-]+$").WithMessage("Tên vai trò chỉ được chứa chữ cái có dấu, số, khoảng trắng, gạch ngang (-) và gạch dưới (_).");
        }
    }

    public class UpdateRoleDtoValidator : AbstractValidator<UpdateRoleDto>
    {
        public UpdateRoleDtoValidator()
        {
            RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage("Vui lòng chọn vai trò.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên vai trò là bắt buộc.")
                .MaximumLength(150).WithMessage("Tên vai trò không được quá 150 ký tự.")
                .Matches(@"^[\p{L}\p{M}\p{N} _-]+$").WithMessage("Tên vai trò chỉ được chứa chữ cái có dấu, số, khoảng trắng, gạch ngang (-) và gạch dưới (_).");
        }
    }

    public class AccountRoleDtoValidator : AbstractValidator<AccountRoleDto>
    {
        public AccountRoleDtoValidator()
        {
            RuleFor(x => x.AccountId)
                .NotEmpty().WithMessage("Vui lòng chọn một tài khoản.");

            RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage("Vui lòng chọn ít nhất một vai trò.");
        }
    }
}
