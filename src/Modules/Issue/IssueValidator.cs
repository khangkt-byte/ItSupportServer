using FluentValidation;

namespace ItSupportServer.src.Modules.Issue
{
    public class CreateIssueDtoValidator : AbstractValidator<CreateIssueDto>
    {
        public CreateIssueDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên là bắt buộc.");
        }
    }

    public class UpdateIssueDtoValidator : AbstractValidator<UpdateIssueDto>
    {
        public UpdateIssueDtoValidator()
        {
            RuleFor(x => x.IssId)
                .NotEmpty().WithMessage("Vui lòng chọn vấn đề.");
        }
    }
}
