using FluentValidation;

namespace ItSupportServer.src.Modules.Area
{
    public class CreateAreaDtoValidator : AbstractValidator<CreateAreaDto>
    {
        public CreateAreaDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên là bắt buộc.")
                .MaximumLength(255).WithMessage("Tên không được quá 255 ký tự.")
                .Matches(@"^[\p{L}\p{M}\p{N} _-]+$").WithMessage("Tên chỉ được chứa chữ cái có dấu, số, khoảng trắng, gạch ngang (-) và gạch dưới (_).");
        }
    }

    public class UpdateAreaDtoValidator : AbstractValidator<UpdateAreaDto>
    {
        public UpdateAreaDtoValidator()
        {
            RuleFor(x => x.AreaId)
                .NotEmpty().WithMessage("Mã khu vực là bắt buộc.");

            RuleFor(x => x.Name)
                .MaximumLength(255).WithMessage("Tên không được quá 255 ký tự.")
                .Matches(@"^[\p{L}\p{M}\p{N} _-]+$").WithMessage("Tên chỉ được chứa chữ cái có dấu, số, khoảng trắng, gạch ngang (-) và gạch dưới (_).");
        }
    }
}
