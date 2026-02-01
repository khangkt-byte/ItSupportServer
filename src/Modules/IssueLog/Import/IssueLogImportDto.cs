namespace ItSupportServer.src.Modules.IssueLog
{
    // ===== REFERENCE DATA DTOs =====


    /// <summary>
    /// Department reference DTO with normalized name for matching
    /// </summary>
    public record DepartmentReferenceDto
    {
        public int DptId { get; init; }
        public required string Name { get; init; }
        public required string NormalizedName { get; init; }
    }

    /// <summary>
    /// Area reference DTO with normalized name
    /// </summary>
    public record AreaReferenceDto
    {
        public int AreaId { get; init; }
        public required string Name { get; init; }
        public required string NormalizedName { get; init; }
    }

    /// <summary>
    /// Recent issue log DTO for duplicate detection
    /// Pattern: Projection DTO (CQRS read model)
    /// Performance: Only 5 fields vs 15+ in full entity
    /// </summary>
    public record RecentIssueLogDto
    {
        public Guid IssLogId { get; init; }
        public required string Operator { get; init; }
        public int DepartmentId { get; init; }
        public required string IssueDescription { get; init; }
        public DateTime DateReported { get; init; }
    }

    /// <summary>
    /// Excel row data DTO (parsed from Excel)
    /// Pattern: Data Transfer Object
    /// Purpose: Strong typing for Excel row data
    /// </summary>
    public record ExcelRowDto
    {
        public required string Operator { get; init; }
        public string? Requester { get; init; }
        public int DepartmentId { get; init; }
        public int AreaId { get; init; }
        public required string IssueDescription { get; init; }
        public string? Cause { get; init; }
        public string? Resolution { get; init; }
        public string? PermanentFix { get; init; }
        public DateTime DateReported { get; init; }
        public string? Status { get; init; }
    }

    /// <summary>
    /// Excel update DTO (for duplicate update strategy)
    /// Pattern: Partial update DTO
    /// </summary>
    public record ExcelUpdateDto
    {
        public string? Cause { get; init; }
        public string? Resolution { get; init; }
        public string? PermanentFix { get; init; }
        public string? Status { get; init; }
    }

    /// <summary>
    /// Import preview data DTO
    /// Pattern: ViewModel (MVVM)
    /// Purpose: Type-safe preview for frontend
    /// Security: No sensitive data exposure
    /// </summary>
    public record ImportPreviewDataDto
    {
        public required string Operator { get; init; }
        public string? Requester { get; init; }
        public required string Department { get; init; }
        public string? MappedDepartment { get; init; }
        public required string Area { get; init; }
        public required string IssueDescription { get; init; }
        public DateTime? DateReported { get; init; }
        public bool IsDuplicate { get; init; }
    }

    // ===== DUPLICATE DETECTION DTOs =====

    /// <summary>
    /// Duplicate match result
    /// </summary>
    public record DuplicateMatch
    {
        public Guid IssLogId { get; init; }
        public int MatchScore { get; init; }
        public string MatchReason { get; init; } = string.Empty;
        public string? IssueDescription { get; init; }
        public DateTime? DateReported { get; init; }
    }

    // ===== VALIDATION DTOs =====

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
        public ImportPreviewDataDto? PreviewData { get; init; }  // ✅ FIX: Type-safe

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
        public bool AutoCreateMissingDepartments { get; init; } = false;
        public bool SkipRowsWithErrors { get; init; } = false;
        public int FuzzyMatchThreshold { get; init; } = 85;
        public Dictionary<string, int>? ManualDepartmentMappings { get; init; }
        public DuplicateHandlingStrategy DuplicateHandling { get; init; } = DuplicateHandlingStrategy.Skip;
    }

    /// <summary>
    /// Duplicate handling strategies
    /// </summary>
    public enum DuplicateHandlingStrategy
    {
        Skip,
        Update,
        CreateNew,
        Fail
    }

    // ===== IMPORT RESULT =====

    /// <summary>
    /// Import result (Step 2)
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
    /// Fuzzy match result DTO
    /// Pattern: Value Object (DDD)
    /// Purpose: Type-safe fuzzy matching results
    /// </summary>
    public record FuzzyMatchResultDto
    {
        public required RecentIssueLogDto Log { get; init; }
        public int Score { get; init; }
    }
}