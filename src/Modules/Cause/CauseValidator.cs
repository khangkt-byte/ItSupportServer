using FluentValidation;

namespace ItSupportServer.src.Modules.Cause
{
    public class CreateCauseDtoValidator : AbstractValidator<CreateCauseDto>
    {
        public CreateCauseDtoValidator()
        {
            RuleFor(x => x.IssId)
                .GreaterThan(0)
                .WithMessage("Issue ID là bắt buộc.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên nguyên nhân là bắt buộc.")
                .MaximumLength(255)
                .WithMessage("Tên nguyên nhân không được quá 255 ký tự.");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Mô tả không được quá 1000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }

    public class UpdateCauseDtoValidator : AbstractValidator<UpdateCauseDto>
    {
        public UpdateCauseDtoValidator()
        {
            // ✅ NO CauseId or IssId - comes from route and cannot change parent

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Nếu cung cấp tên, không được để trống.")
                .MaximumLength(255)
                .WithMessage("Tên nguyên nhân không được quá 255 ký tự.")
                .When(x => x.Name != null);

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Mô tả không được quá 1000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}