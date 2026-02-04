using FluentValidation;

namespace ItSupportServer.src.Modules.IssueLog
{
    /// <summary>
    /// Import options validator
    /// Pattern: FluentValidation for complex objects
    /// </summary>
    public class ImportOptionsDtoValidator : AbstractValidator<ImportOptionsDto>
    {
        public ImportOptionsDtoValidator()
        {
            RuleFor(x => x.FuzzyMatchThreshold)
                .InclusiveBetween(0, 100)
                .WithMessage("Fuzzy match threshold phải từ 0-100");

            RuleFor(x => x.DuplicateHandling)
                .IsInEnum()
                .WithMessage("Duplicate handling strategy không hợp lệ");

            RuleFor(x => x.ManualDepartmentMappings)
                .Must(mappings => mappings == null || mappings.All(kvp => kvp.Value > 0))
                .WithMessage("Manual department mappings phải có ID > 0")
                .When(x => x.ManualDepartmentMappings != null);
        }
    }
}