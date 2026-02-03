using FluentValidation;

namespace ItSupportServer.src.Modules.Area
{
    public class CreateAreaValidator : AbstractValidator<CreateAreaDto>
    {
        public CreateAreaValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Vui lòng nhập tên.")
                .MaximumLength(255).WithMessage("Tên không được quá 255 ký tự.")
                .Matches(@"^[\p{L}\p{M}\p{N} _-]+$").WithMessage("Tên chỉ được chứa chữ cái có dấu, số, khoảng trắng, gạch ngang (-) và gạch dưới (_).");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Mô tả không được quá 500 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }

    public class UpdateAreaValidator : AbstractValidator<UpdateAreaDto>
    {
        public UpdateAreaValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(255).WithMessage("Tên không được quá 255 ký tự.")
                .Matches(@"^[\p{L}\p{M}\p{N} _-]+$").WithMessage("Tên chỉ được chứa chữ cái có dấu, số, khoảng trắng, gạch ngang (-) và gạch dưới (_).");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Mô tả không được quá 500 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}
