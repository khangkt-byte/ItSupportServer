using FluentValidation;

namespace ItSupportServer.src.Modules.Issue
{
    public class CreateIssueDtoValidator : AbstractValidator<CreateIssueDto>
    {
        public CreateIssueDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên vấn đề là bắt buộc.")
                .MaximumLength(255)
                .WithMessage("Tên vấn đề không được quá 255 ký tự.")
                .Matches(@"^[\p{L}\p{M}\p{N}\s\-_.,()]+$")
                .WithMessage("Tên vấn đề chỉ được chứa chữ cái, số và ký tự đặc biệt cơ bản.")
                .Must(name => !string.IsNullOrWhiteSpace(name?.Trim()))
                .WithMessage("Tên vấn đề không được chỉ chứa khoảng trắng.");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Mô tả không được quá 1000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.Category)
                .MaximumLength(100)
                .WithMessage("Danh mục không được quá 100 ký tự.")
                .Matches(@"^[\p{L}\p{M}\p{N}\s\-_]+$")
                .WithMessage("Danh mục chỉ được chứa chữ cái, số, khoảng trắng và dấu gạch ngang.")
                .When(x => !string.IsNullOrWhiteSpace(x.Category));

            RuleFor(x => x.Severity)
                .InclusiveBetween(1, 5)
                .WithMessage("Độ nghiêm trọng phải từ 1 đến 5.")
                .When(x => x.Severity.HasValue);
        }
    }

    public class UpdateIssueDtoValidator : AbstractValidator<UpdateIssueDto>
    {
        public UpdateIssueDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên vấn đề không được để trống khi cập nhật.")
                .MaximumLength(255)
                .WithMessage("Tên vấn đề không được quá 255 ký tự.")
                .Matches(@"^[\p{L}\p{M}\p{N}\s\-_.,()]+$")
                .WithMessage("Tên vấn đề chỉ được chứa chữ cái, số và ký tự đặc biệt cơ bản.")
                .Must(name => !string.IsNullOrWhiteSpace(name?.Trim()))
                .WithMessage("Tên vấn đề không được chỉ chứa khoảng trắng.")
                .When(x => x.Name != null);

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Mô tả không được quá 1000 ký tự.")
                .When(x => x.Description != null);

            RuleFor(x => x.Category)
                .MaximumLength(100)
                .WithMessage("Danh mục không được quá 100 ký tự.")
                .Matches(@"^[\p{L}\p{M}\p{N}\s\-_]*$")
                .WithMessage("Danh mục chỉ được chứa chữ cái, số, khoảng trắng và dấu gạch ngang.")
                .When(x => x.Category != null && !string.IsNullOrWhiteSpace(x.Category));

            RuleFor(x => x.Severity)
                .InclusiveBetween(1, 5)
                .WithMessage("Độ nghiêm trọng phải từ 1 đến 5.")
                .When(x => x.Severity.HasValue);

            RuleFor(x => x)
                .Must(dto => 
                    dto.Name != null || 
                    dto.Description != null || 
                    dto.Category != null || 
                    dto.Severity.HasValue)
                .WithMessage("Phải cung cấp ít nhất một trường để cập nhật.")
                .WithName("UpdateIssueDto");
        }
    }
}
