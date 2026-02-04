using FluentValidation;

namespace ItSupportServer.src.Modules.IssueLog
{
    /// <summary>
    /// Create DTO validator
    /// Pattern: FluentValidation (industry standard)
    /// Security: Input validation, XSS prevention
    /// </summary>
    public class CreateIssueLogValidator : AbstractValidator<CreateIssueLogDto>
    {
        public CreateIssueLogValidator()
        {
            // ===== Operators & Requesters =====
            
            RuleFor(x => x.Operator)
                .NotEmpty()
                .WithMessage("Vui lòng nhập người thực hiện.")
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
                .WithMessage("Vui lòng chọn bộ phận.");

            RuleFor(x => x.AreaId)
                .GreaterThan(0)
                .WithMessage("Vui lòng chọn khu vực.");

            // ===== Issue =====
            
            RuleFor(x => x.IssueId)
                .GreaterThan(0)
                .WithMessage("Vấn đề không hợp lệ.")
                .When(x => x.IssueId.HasValue);

            RuleFor(x => x.IssueDescription)
                .NotEmpty()
                .WithMessage("Mô tả sự cố là bắt buộc.")
                .MaximumLength(2000)
                .WithMessage("Mô tả sự cố không được quá 2000 ký tự.");

            // ===== Cause =====
            
            RuleFor(x => x.CauseId)
                .GreaterThan(0)
                .WithMessage("Nguyên nhân không hợp lệ.")
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
                .WithMessage("Vui lòng nhập ngày sửa chữa.")
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
                .WithMessage("Ngày sửa chữa không được lớn hơn ngày hiện tại.");

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
    public class UpdateIssueLogValidator : AbstractValidator<UpdateIssueLogDto>
    {
        public UpdateIssueLogValidator()
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
                .WithMessage("Bộ phận không hợp lệ.")
                .When(x => x.DepartmentId.HasValue);

            RuleFor(x => x.AreaId)
                .GreaterThan(0)
                .WithMessage("Khu vực không hợp lệ.")
                .When(x => x.AreaId.HasValue);

            RuleFor(x => x.IssueId)
                .GreaterThan(0)
                .WithMessage("Vấn đề không hợp lệ.")
                .When(x => x.IssueId.HasValue);

            RuleFor(x => x.IssueDescription)
                .NotEmpty()
                .WithMessage("Nếu cung cấp mô tả, không được để trống.")
                .MaximumLength(2000)
                .WithMessage("Mô tả sự cố không được quá 2000 ký tự.")
                .When(x => x.IssueDescription != null);

            RuleFor(x => x.CauseId)
                .GreaterThan(0)
                .WithMessage("Nguyên nhân không hợp lệ.")
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
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
                .WithMessage("Ngày sửa chữa không được lớn hơn ngày hiện tại.")
                .When(x => x.DateReported.HasValue);

            RuleFor(x => x.Status)
                .MaximumLength(50)
                .WithMessage("Trạng thái không được quá 50 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Status));
        }
    }
}
