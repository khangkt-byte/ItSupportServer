namespace ItSupportServer.src.Modules.IssueLog
{
    /// <summary>
    /// Extension methods for ImportRowValidation
    /// Pattern: Fluent API (Builder pattern)
    /// Purpose: Immutable updates with clean syntax
    /// </summary>
    public static class ImportRowValidationExtensions
    {
        /// <summary>
        /// Add error to validation (immutable)
        /// </summary>
        public static ImportRowValidation AddError(
            this ImportRowValidation validation,
            string field,
            string message,
            string? originalValue = null,
            string? suggestedValue = null,
            int? suggestedId = null)
        {
            var errors = validation.Errors.ToList();
            errors.Add(new ImportError
            {
                Field = field,
                Message = message,
                Severity = ErrorSeverity.Error,
                OriginalValue = originalValue,
                SuggestedValue = suggestedValue,
                SuggestedId = suggestedId
            });
            
            return validation with { Errors = errors };
        }

        /// <summary>
        /// Add warning to validation (immutable)
        /// </summary>
        public static ImportRowValidation AddWarning(
            this ImportRowValidation validation,
            string field,
            string message,
            string? originalValue = null,
            string? suggestedValue = null,
            int? suggestedId = null,
            object? suggestions = null)
        {
            var warnings = validation.Warnings.ToList();
            warnings.Add(new ImportError
            {
                Field = field,
                Message = message,
                Severity = ErrorSeverity.Warning,
                OriginalValue = originalValue,
                SuggestedValue = suggestedValue,
                SuggestedId = suggestedId,
                Suggestions = suggestions
            });
            
            return validation with { Warnings = warnings };
        }
    }
}