namespace ItSupportServer.src.Modules.IssueLog
{
    // ===== VALIDATION DTOs =====

    /// <summary>
    /// Validation result for Excel import (Step 1)
    /// Pattern: ServiceNow import validation response
    /// </summary>
    public record ImportValidationResultDto
    {
        public int TotalRows { get; init; }
        public int ValidCount { get; init; }
        public int WarningCount { get; init; }
        public int ErrorCount { get; init; }
        public int DuplicateCount { get; init; }
        public bool IsValid { get; init; }
        public List<ImportRowValidation> Rows { get; init; } = [];
        
        public string Summary => IsValid
            ? $"✅ {ValidCount} valid, {WarningCount} warnings, {DuplicateCount} duplicates"
            : $"❌ {ErrorCount} errors, {WarningCount} warnings, {DuplicateCount} duplicates";
    }

    /// <summary>
    /// Validation result for a single row
    /// </summary>
    public record ImportRowValidation
    {
        public int RowNumber { get; init; }
        public List<ImportError> Errors { get; init; } = [];
        public List<ImportError> Warnings { get; init; } = [];
        
        // Mapped values
        public int? MappedDepartmentId { get; init; }
        public string? MappedDepartmentName { get; init; }
        public int? MappedAreaId { get; init; }
        public string? MappedAreaName { get; init; }
        
        // Duplicate detection
        public Guid? DuplicateOf { get; init; }
        
        // Preview data for frontend display
        public object? PreviewData { get; init; }
        
        public bool HasErrors => Errors.Count > 0;
        public bool HasWarnings => Warnings.Count > 0;
    }

    /// <summary>
    /// Import error/warning details
    /// </summary>
    public record ImportError
    {
        public required string Field { get; init; }
        public required string Message { get; init; }
        public ErrorSeverity Severity { get; init; }
        public string? OriginalValue { get; init; }
        public string? SuggestedValue { get; init; }
        public int? SuggestedId { get; init; }
        public object? Suggestions { get; init; }
    }

    /// <summary>
    /// Error severity level
    /// </summary>
    public enum ErrorSeverity
    {
        Error,      // Blocks import
        Warning,    // Can proceed with user confirmation
        Info        // Informational only
    }

    // ===== IMPORT OPTIONS =====

    /// <summary>
    /// Import options (Step 2)
    /// Pattern: ServiceNow import configuration
    /// </summary>
    public record ImportOptionsDto
    {
        /// <summary>
        /// Auto-create missing departments during import
        /// </summary>
        public bool AutoCreateMissingDepartments { get; init; } = false;
        
        /// <summary>
        /// Skip rows with errors instead of failing entire import
        /// </summary>
        public bool SkipRowsWithErrors { get; init; } = false;
        
        /// <summary>
        /// Fuzzy matching threshold (0-100)
        /// </summary>
        public int FuzzyMatchThreshold { get; init; } = 85;
        
        /// <summary>
        /// Manual department name mappings
        /// Example: { "Phòng IT": 1, "IT Dept": 1 }
        /// </summary>
        public Dictionary<string, int>? ManualDepartmentMappings { get; init; }
        
        /// <summary>
        /// How to handle duplicate records
        /// </summary>
        public DuplicateHandlingStrategy DuplicateHandling { get; init; } = DuplicateHandlingStrategy.Skip;
    }

    /// <summary>
    /// Duplicate handling strategies
    /// Pattern: ServiceNow duplicate record handling
    /// </summary>
    public enum DuplicateHandlingStrategy
    {
        /// <summary>Skip duplicate rows (default)</summary>
        Skip,
        
        /// <summary>Update existing records with new data</summary>
        Update,
        
        /// <summary>Create new anyway (allow duplicates)</summary>
        CreateNew,
        
        /// <summary>Fail import if duplicates found</summary>
        Fail
    }

    // ===== IMPORT RESULT =====

    /// <summary>
    /// Import result (Step 2)
    /// Pattern: ServiceNow import execution response
    /// </summary>
    public record ImportResultDto
    {
        public int SuccessCount { get; init; }
        public int UpdatedCount { get; init; }
        public int SkippedCount { get; init; }
        public int AutoMatched { get; init; }
        public int DepartmentsCreated { get; init; }
        public List<string> Errors { get; init; } = [];
        public List<string> Warnings { get; init; } = [];
        
        public bool HasErrors => Errors.Count > 0;
        
        public string Summary => HasErrors
            ? $"❌ Import failed: {Errors.Count} errors"
            : $"✅ Imported {SuccessCount} created, {UpdatedCount} updated, {SkippedCount} skipped ({AutoMatched} auto-matched)";
    }
}