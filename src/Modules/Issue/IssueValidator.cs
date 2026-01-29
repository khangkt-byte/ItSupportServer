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
                .WithMessage("Tên vấn đề không được quá 255 ký tự.");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Mô tả không được quá 1000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.Category)
                .MaximumLength(100)
                .WithMessage("Danh mục không được quá 100 ký tự.")
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
                .MaximumLength(255)
                .WithMessage("Tên vấn đề không được quá 255 ký tự.")
                .When(x => x.Name != null);

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Mô tả không được quá 1000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.Category)
                .MaximumLength(100)
                .WithMessage("Danh mục không được quá 100 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Category));

            RuleFor(x => x.Severity)
                .InclusiveBetween(1, 5)
                .WithMessage("Độ nghiêm trọng phải từ 1 đến 5.")
                .When(x => x.Severity.HasValue);
        }
    }
}
