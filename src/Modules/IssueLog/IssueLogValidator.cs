using FluentValidation;

namespace ItSupportServer.src.Modules.IssueLog
{
    public class CreateIssueLogDtoValidator : AbstractValidator<CreateIssueLogDto>
    {
        public CreateIssueLogDtoValidator()
        {
            RuleFor(x => x.Operator)
                .NotEmpty()
                .WithMessage("Người thực hiện là bắt buộc.")
                .MaximumLength(500)
                .WithMessage("Người thực hiện không được quá 500 ký tự.");

            RuleFor(x => x.Requester)
                .MaximumLength(255)
                .WithMessage("Người yêu cầu không được quá 255 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Requester));

            RuleFor(x => x.Department)
                .NotEmpty()
                .WithMessage("Bộ phận là bắt buộc.")
                .MaximumLength(255)
                .WithMessage("Bộ phận không được quá 255 ký tự.");

            RuleFor(x => x.Area)
                .NotEmpty()
                .WithMessage("Khu vực là bắt buộc.")
                .MaximumLength(255)
                .WithMessage("Khu vực không được quá 255 ký tự.");

            RuleFor(x => x.IssueDescription)
                .NotEmpty()
                .WithMessage("Mô tả sự cố là bắt buộc.")
                .MaximumLength(2000)
                .WithMessage("Mô tả sự cố không được quá 2000 ký tự.");

            RuleFor(x => x.Cause)
                .MaximumLength(1000)
                .WithMessage("Nguyên nhân không được quá 1000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Cause));

            RuleFor(x => x.Resolution)
                .MaximumLength(2000)
                .WithMessage("Cách xử lý không được quá 2000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Resolution));

            RuleFor(x => x.PermanentFix)
                .MaximumLength(2000)
                .WithMessage("Giải pháp lâu dài không được quá 2000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.PermanentFix));

            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .WithMessage("Ghi chú không được quá 1000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Notes));

            RuleFor(x => x.DateReported)
                .NotEmpty()
                .WithMessage("Ngày báo cáo là bắt buộc.")
                .Must(date => date <= DateTime.UtcNow.AddDays(1))
                .WithMessage("Ngày báo cáo không được lớn hơn ngày hiện tại.");

            RuleFor(x => x.Status)
                .MaximumLength(50)
                .WithMessage("Trạng thái không được quá 50 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Status));
        }
    }

    public class UpdateIssueLogDtoValidator : AbstractValidator<UpdateIssueLogDto>
    {
        public UpdateIssueLogDtoValidator()
        {
            RuleFor(x => x.Operator)
                .NotEmpty()
                .WithMessage("Nếu cung cấp người thực hiện, không được để trống.")
                .MaximumLength(500)
                .WithMessage("Người thực hiện không được quá 500 ký tự.")
                .When(x => x.Operator != null);

            RuleFor(x => x.Requester)
                .MaximumLength(255)
                .WithMessage("Người yêu cầu không được quá 255 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Requester));

            RuleFor(x => x.Department)
                .NotEmpty()
                .WithMessage("Nếu cung cấp bộ phận, không được để trống.")
                .MaximumLength(255)
                .WithMessage("Bộ phận không được quá 255 ký tự.")
                .When(x => x.Department != null);

            RuleFor(x => x.Area)
                .NotEmpty()
                .WithMessage("Nếu cung cấp khu vực, không được để trống.")
                .MaximumLength(255)
                .WithMessage("Khu vực không được quá 255 ký tự.")
                .When(x => x.Area != null);

            RuleFor(x => x.IssueDescription)
                .NotEmpty()
                .WithMessage("Nếu cung cấp mô tả, không được để trống.")
                .MaximumLength(2000)
                .WithMessage("Mô tả sự cố không được quá 2000 ký tự.")
                .When(x => x.IssueDescription != null);

            RuleFor(x => x.Cause)
                .MaximumLength(1000)
                .WithMessage("Nguyên nhân không được quá 1000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Cause));

            RuleFor(x => x.Resolution)
                .MaximumLength(2000)
                .WithMessage("Cách xử lý không được quá 2000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Resolution));

            RuleFor(x => x.PermanentFix)
                .MaximumLength(2000)
                .WithMessage("Giải pháp lâu dài không được quá 2000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.PermanentFix));

            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .WithMessage("Ghi chú không được quá 1000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Notes));

            RuleFor(x => x.DateReported)
                .Must(date => !date.HasValue || date.Value <= DateTime.UtcNow.AddDays(1))
                .WithMessage("Ngày báo cáo không được lớn hơn ngày hiện tại.")
                .When(x => x.DateReported.HasValue);

            RuleFor(x => x.Status)
                .MaximumLength(50)
                .WithMessage("Trạng thái không được quá 50 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Status));
        }
    }
}
