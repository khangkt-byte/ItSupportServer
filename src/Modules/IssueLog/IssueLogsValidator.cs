using FluentValidation;

namespace ItSupportServer.src.Modules.IssueLog
{
    public class CreateIssueLogDtoValidator : AbstractValidator<CreateIssueLogDto>
    {
        public CreateIssueLogDtoValidator()
        {
            RuleFor(x => x.Operator)
                .NotEmpty().WithMessage("Người thực hiện là bắt buộc.");

            //RuleFor(x => x.DptId)
            //    .GreaterThan(0).WithMessage("Bộ phận là bắt buộc.");

            //RuleFor(x => x.AreaId)
            //    .GreaterThan(0).WithMessage("Khu vực là bắt buộc.");

            RuleFor(x => x.Department)
                .NotEmpty().WithMessage("Bộ phận là bắt buộc.");

            RuleFor(x => x.Area)
                .NotEmpty().WithMessage("Khu vực là bắt buộc.");

            RuleFor(x => x.IssueDescription)
                .NotEmpty().WithMessage("Tình trạng lỗi là bắt buộc.");

            RuleFor(x => x.DateReported)
                .NotEmpty().WithMessage("Ngày thực hiện là bắt buộc.");
        }
    }
}
