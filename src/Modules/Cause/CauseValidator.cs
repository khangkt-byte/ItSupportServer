using FluentValidation;

namespace ItSupportServer.src.Modules.Cause
{
    /// <summary>
    /// Create Cause DTO Validator
    /// Pattern: FluentValidation
    /// Reference: Input validation best practices (OWASP)
    /// </summary>
    public class CreateCauseValidator : AbstractValidator<CreateCauseDto>
    {
        public CreateCauseValidator()
        {
            RuleFor(x => x.IssId)
                .GreaterThan(0)
                .WithMessage("Vui lòng chọn vấn đề.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Vui lòng nhập tên nguyên nhân.")
                .MaximumLength(255)
                .WithMessage("Tên nguyên nhân không được quá 255 ký tự.")
                .Matches(@"^[\p{L}\p{M}\p{N}\s\-_.,()'"";:/| \\]+$")
                .WithMessage("Tên nguyên nhân chỉ được chứa chữ cái, số và ký tự đặc biệt cơ bản.");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Mô tả không được quá 1000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }

    /// <summary>
    /// Update Cause DTO Validator
    /// </summary>
    public class UpdateCauseValidator : AbstractValidator<UpdateCauseDto>
    {
        public UpdateCauseValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên nguyên nhân không được để trống khi cập nhật.")
                .MaximumLength(255)
                .WithMessage("Tên nguyên nhân không được quá 255 ký tự.")
                .Matches(@"^[\p{L}\p{M}\p{N}\s\-_.,()'"";:/| \\]+$")
                .WithMessage("Tên nguyên nhân chỉ được chứa chữ cái, số và ký tự đặc biệt cơ bản.")
                .When(x => x.Name != null);

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Mô tả không được quá 1000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x)
                .Must(dto => dto.Name != null || dto.Description != null)
                .WithMessage("Phải cung cấp ít nhất một trong các trường: Tên hoặc Mô tả.")
                .WithName("UpdateCauseDto");
        }
    }
}