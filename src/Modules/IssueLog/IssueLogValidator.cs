using FluentValidation;

namespace ItSupportServer.src.Modules.IssueLog
{
    /// <summary>
    /// Create DTO validator
    /// Pattern: FluentValidation (industry standard)
    /// Security: Input validation, XSS prevention
    /// </summary>
    public class CreateIssueLogDtoValidator : AbstractValidator<CreateIssueLogDto>
    {
        public CreateIssueLogDtoValidator()
        {
            // ===== Operators & Requesters =====
            
            RuleFor(x => x.Operator)
                .NotEmpty()
                .WithMessage("Người thực hiện là bắt buộc.")
                .MaximumLength(500)
                .WithMessage("Người thực hiện không được quá 500 ký tự.")
                .Matches(@"^[\p{L}\p{N}\s,;.-]+$")
                .WithMessage("Người thực hiện chứa ký tự không hợp lệ.");

            RuleFor(x => x.Requester)
                .MaximumLength(500)
                .WithMessage("Người yêu cầu không được quá 500 ký tự.")
                .Matches(@"^[\p{L}\p{N}\s,;.-]+$")
                .WithMessage("Người yêu cầu chứa ký tự không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.Requester));

            // ===== Department & Area (Validated IDs) =====
            
            RuleFor(x => x.DepartmentId)
                .GreaterThan(0)
                .WithMessage("Bộ phận là bắt buộc.");

            RuleFor(x => x.AreaId)
                .GreaterThan(0)
                .WithMessage("Khu vực là bắt buộc.");

            // ===== Issue =====
            
            RuleFor(x => x.IssueId)
                .GreaterThan(0)
                .WithMessage("Issue ID không hợp lệ.")
                .When(x => x.IssueId.HasValue);

            RuleFor(x => x.IssueDescription)
                .NotEmpty()
                .WithMessage("Mô tả sự cố là bắt buộc.")
                .MaximumLength(2000)
                .WithMessage("Mô tả sự cố không được quá 2000 ký tự.");

            // ===== Cause =====
            
            RuleFor(x => x.CauseId)
                .GreaterThan(0)
                .WithMessage("Cause ID không hợp lệ.")
                .When(x => x.CauseId.HasValue);

            RuleFor(x => x.Cause)
                .MaximumLength(1000)
                .WithMessage("Nguyên nhân không được quá 1000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Cause));

            // ===== Resolution =====
            
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

    /// <summary>
    /// Update DTO validator
    /// Pattern: Partial update validation (PATCH semantics)
    /// </summary>
    public class UpdateIssueLogDtoValidator : AbstractValidator<UpdateIssueLogDto>
    {
        public UpdateIssueLogDtoValidator()
        {
            RuleFor(x => x.Operator)
                .NotEmpty()
                .WithMessage("Nếu cung cấp người thực hiện, không được để trống.")
                .MaximumLength(500)
                .WithMessage("Người thực hiện không được quá 500 ký tự.")
                .Matches(@"^[\p{L}\p{N}\s,;.-]+$")
                .WithMessage("Người thực hiện chứa ký tự không hợp lệ.")
                .When(x => x.Operator != null);

            RuleFor(x => x.Requester)
                .MaximumLength(500)
                .WithMessage("Người yêu cầu không được quá 500 ký tự.")
                .Matches(@"^[\p{L}\p{N}\s,;.-]+$")
                .WithMessage("Người yêu cầu chứa ký tự không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.Requester));

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0)
                .WithMessage("Department ID không hợp lệ.")
                .When(x => x.DepartmentId.HasValue);

            RuleFor(x => x.AreaId)
                .GreaterThan(0)
                .WithMessage("Area ID không hợp lệ.")
                .When(x => x.AreaId.HasValue);

            RuleFor(x => x.IssueId)
                .GreaterThan(0)
                .WithMessage("Issue ID không hợp lệ.")
                .When(x => x.IssueId.HasValue);

            RuleFor(x => x.IssueDescription)
                .NotEmpty()
                .WithMessage("Nếu cung cấp mô tả, không được để trống.")
                .MaximumLength(2000)
                .WithMessage("Mô tả sự cố không được quá 2000 ký tự.")
                .When(x => x.IssueDescription != null);

            RuleFor(x => x.CauseId)
                .GreaterThan(0)
                .WithMessage("Cause ID không hợp lệ.")
                .When(x => x.CauseId.HasValue);

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
