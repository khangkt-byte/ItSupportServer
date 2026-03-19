using FluentValidation;

namespace ItSupportServer.src.Modules.Role
{
    public class CreateRoleValidator : AbstractValidator<CreateRoleDto>
    {
        public CreateRoleValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Vui lòng nhập tên vai trò.")
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

    public class UpdateRoleValidator : AbstractValidator<UpdateRoleDto>
    {
        public UpdateRoleValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Vui lòng nhập tên vai trò.")
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

    public class AccountRoleValidator : AbstractValidator<AssignRolesDto>
    {
        public AccountRoleValidator()
        {
            RuleFor(x => x.AccountId)
                .NotEmpty().WithMessage("Vui lòng chọn một tài khoản.");

            RuleFor(x => x.RoleIds)
                .NotNull()
                .WithMessage("Danh sách vai trò không được null.")
                .NotEmpty()
                .WithMessage("Vui lòng chọn ít nhất một vai trò.")
                .Must(ids => ids.All(id => id > 0))
                .WithMessage("Các vai trò được chọn không hợp lệ.");
        }
    }

    public class AccountClaimValidator : AbstractValidator<AssignClaimsDto>
    {
        public AccountClaimValidator()
        {
            RuleFor(x => x.AccountId)
                .NotEmpty().WithMessage("Vui lòng chọn một tài khoản.");

            RuleFor(x => x.ClaimIds)
                .NotNull()
                .WithMessage("Danh sách quyền không được null.")
                .NotEmpty()
                .WithMessage("Vui lòng chọn ít nhất một quyền.")
                .Must(ids => ids.All(id => id > 0))
                .WithMessage("Các quyền được chọn không hợp lệ.");
        }
    }
}
