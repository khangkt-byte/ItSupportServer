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

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Mô tả không được quá 500 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.ClaimIds)
                .Must(ids => ids == null || ids.All(id => id > 0))
                .WithMessage("Vui lòng chọn quyền.")
                .When(x => x.ClaimIds != null);
        }
    }

    public class UpdateRoleDtoValidator : AbstractValidator<UpdateRoleDto>
    {
        public UpdateRoleDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên vai trò là bắt buộc.")
                .MaximumLength(150).WithMessage("Tên vai trò không được quá 150 ký tự.")
                .Matches(@"^[\p{L}\p{M}\p{N} _-]+$").WithMessage("Tên vai trò chỉ được chứa chữ cái có dấu, số, khoảng trắng, gạch ngang (-) và gạch dưới (_).")
                .When(x => x.Name != null);

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Mô tả không được quá 500 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.ClaimIds)
                .Must(ids => ids == null || ids.All(id => id > 0))
                .WithMessage("Vui lòng chọn quyền.")
                .When(x => x.ClaimIds != null);
        }
    }

    public class AccountRoleDtoValidator : AbstractValidator<AssignRolesDto>
    {
        public AccountRoleDtoValidator()
        {
            RuleFor(x => x.AccountId)
                .NotEmpty().WithMessage("Vui lòng chọn một tài khoản.");

            RuleFor(x => x.RoleIds)
                .NotNull().WithMessage("Vui lòng chọn ít nhất một vai trò.")
                .NotEmpty().WithMessage("Vui lòng chọn ít nhất một vai trò.")
                .Must(ids => ids.All(id => id >= 0))
                .WithMessage("Role IDs không hợp lệ.");
        }
    }
}
