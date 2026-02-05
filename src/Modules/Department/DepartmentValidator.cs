using FluentValidation;

namespace ItSupportServer.src.Modules.Department
{
    /// <summary>
    /// Validator for creating department
    /// Pattern: FluentValidation for declarative validation
    /// Security: Input sanitization, length checks
    /// </summary>
    public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentDto>
    {
        public CreateDepartmentValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Vui lòng nhập tên bộ phận.")
                .MaximumLength(100).WithMessage("Tên bộ phận không được quá 100 ký tự.")
                .Matches(@"^[\p{L}\p{M}\p{N} _\-()/.]+$")
                .WithMessage("Tên bộ phận chỉ được chứa chữ cái có dấu, số, khoảng trắng và các ký tự: - _ ( ) / .");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Mô tả không được quá 500 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }

    /// <summary>
    /// Validator for updating department
    /// </summary>
    public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentDto>
    {
        public UpdateDepartmentValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Vui lòng nhập tên bộ phận.")
                .MaximumLength(100).WithMessage("Tên bộ phận không được quá 100 ký tự.")
                .Matches(@"^[\p{L}\p{M}\p{N} _\-()/.]+$")
                .WithMessage("Tên bộ phận chỉ được chứa chữ cái có dấu, số, khoảng trắng và các ký tự: - _ ( ) / .")
                .When(x => x.Name != null);

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Mô tả không được quá 500 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}