using ItSupportServer.Data.Models.Entities;
using Riok.Mapperly.Abstractions;

namespace ItSupportServer.src.Modules.IssueLog
{
    /// <summary>
    /// Import-specific mapper using Mapperly source generation
    /// Pattern: Repository pattern for data transformation
    /// Performance: Zero-reflection, compile-time code generation
    /// References:
    /// - Mapperly GitHub: https://github.com/riok/mapperly
    /// - Microsoft: Performance Best Practices
    /// Security: Type-safe, no reflection vulnerabilities
    /// </summary>
    [Mapper]
    public partial class IssueLogImportMapper
    {
        // ===== QUERY PROJECTIONS (CQRS Read Models) =====

        /// <summary>
        /// Project to recent issue log DTO (for duplicate detection)
        /// Performance: Only 5 fields vs 15+ in full entity
        /// Reference: CQRS pattern (Greg Young)
        /// </summary>
        [MapperIgnoreSource(nameof(IssueLogs.Requester))]
        [MapperIgnoreSource(nameof(IssueLogs.AreaId))]
        [MapperIgnoreSource(nameof(IssueLogs.IssueId))]
        [MapperIgnoreSource(nameof(IssueLogs.CauseId))]
        [MapperIgnoreSource(nameof(IssueLogs.Cause))]
        [MapperIgnoreSource(nameof(IssueLogs.Resolution))]
        [MapperIgnoreSource(nameof(IssueLogs.PermanentFix))]
        [MapperIgnoreSource(nameof(IssueLogs.Notes))]
        [MapperIgnoreSource(nameof(IssueLogs.Status))]
        [MapperIgnoreSource(nameof(IssueLogs.CreatedAt))]
        [MapperIgnoreSource(nameof(IssueLogs.UpdatedAt))]
        [MapperIgnoreSource(nameof(IssueLogs.DeletedAt))]
        [MapperIgnoreSource(nameof(IssueLogs.Department))]
        [MapperIgnoreSource(nameof(IssueLogs.Area))]
        [MapperIgnoreSource(nameof(IssueLogs.Issue))]
        [MapperIgnoreSource(nameof(IssueLogs.CauseRef))]
        public partial IQueryable<RecentIssueLogDto> ProjectToRecentIssueLogDto(
            IQueryable<IssueLogs> query);

        /// <summary>
        /// Project to department reference DTO with normalized name
        /// Performance: Only load DptId and Name
        /// Security: No sensitive department data
        /// </summary>
        public IQueryable<DepartmentReferenceDto> ProjectToDepartmentReference(
            IQueryable<Departments> query)
        {
            return query.Select(d => new DepartmentReferenceDto
            {
                DptId = d.DptId,
                Name = d.Name,
                NormalizedName = d.Name.ToLower()
            });
        }

        /// <summary>
        /// Project to area reference DTO with normalized name
        /// </summary>
        public IQueryable<AreaReferenceDto> ProjectToAreaReference(
            IQueryable<Areas> query)
        {
            return query.Select(a => new AreaReferenceDto
            {
                AreaId = a.AreaId,
                Name = a.Name,
                NormalizedName = a.Name.ToLower()
            });
        }

        // ===== ENTITY CREATION (Write Models) =====

        /// <summary>
        /// Map Excel row DTO to IssueLogs entity
        /// Pattern: Factory pattern via mapper
        /// Mapperly: Generates optimized mapping code at compile-time
        /// </summary>
        [MapperIgnoreTarget(nameof(IssueLogs.IssLogId))]  // Set manually
        [MapperIgnoreTarget(nameof(IssueLogs.IssueId))]
        [MapperIgnoreTarget(nameof(IssueLogs.CauseId))]
        [MapperIgnoreTarget(nameof(IssueLogs.Notes))]
        [MapperIgnoreTarget(nameof(IssueLogs.CreatedAt))]
        [MapperIgnoreTarget(nameof(IssueLogs.UpdatedAt))]
        [MapperIgnoreTarget(nameof(IssueLogs.DeletedAt))]
        [MapperIgnoreTarget(nameof(IssueLogs.Department))]
        [MapperIgnoreTarget(nameof(IssueLogs.Area))]
        [MapperIgnoreTarget(nameof(IssueLogs.Issue))]
        [MapperIgnoreTarget(nameof(IssueLogs.CauseRef))]
        public IssueLogs MapToEntity(ExcelRowDto dto)
        {
            // ✅ Manual implementation instead of partial (Mapperly can't handle Guid.CreateVersion7)
            var entity = new IssueLogs
            {
                IssLogId = Guid.CreateVersion7(),
                Operator = dto.Operator,
                Requester = NormalizeNullableString(dto.Requester),
                DepartmentId = dto.DepartmentId,
                AreaId = dto.AreaId,
                IssueDescription = dto.IssueDescription,
                Cause = NormalizeNullableString(dto.Cause),
                Resolution = NormalizeNullableString(dto.Resolution),
                PermanentFix = NormalizeNullableString(dto.PermanentFix),
                DateReported = dto.DateReported,
                Status = NormalizeNullableString(dto.Status)
            };

            return entity;
        }

        // ===== ENTITY UPDATE (Partial Updates) =====

        /// <summary>
        /// Update entity from Excel update DTO (for duplicate update strategy)
        /// Pattern: Partial update (RESTful PATCH semantics)
        /// Security: Only update provided fields
        /// Reference: RFC 7396 - JSON Merge Patch
        /// </summary>
        public void UpdateEntityFromExcelRow(
            IssueLogs existingLog,
            ExcelUpdateDto updateDto)
        {
            if (ShouldUpdate(updateDto.Cause, existingLog.Cause))
                existingLog.Cause = updateDto.Cause;

            if (ShouldUpdate(updateDto.Resolution, existingLog.Resolution))
                existingLog.Resolution = updateDto.Resolution;

            if (ShouldUpdate(updateDto.PermanentFix, existingLog.PermanentFix))
                existingLog.PermanentFix = updateDto.PermanentFix;

            if (ShouldUpdate(updateDto.Status, existingLog.Status))
                existingLog.Status = updateDto.Status;
        }

        /// <summary>
        /// Update entity from Excel row (5 parameter overload for backward compatibility)
        /// </summary>
        public void UpdateEntityFromExcelRow(
            IssueLogs existingLog,
            string? cause,
            string? resolution,
            string? permanentFix,
            string? status)
        {
            var updateDto = new ExcelUpdateDto
            {
                Cause = cause,
                Resolution = resolution,
                PermanentFix = permanentFix,
                Status = status
            };

            UpdateEntityFromExcelRow(existingLog, updateDto);
        }

        // ===== LOOKUP BUILDERS =====

        /// <summary>
        /// Build department lookup dictionary
        /// Pattern: Builder pattern for complex object construction
        /// Performance: O(1) lookup time
        /// </summary>
        public Dictionary<string, DepartmentReferenceDto> BuildDepartmentLookup(
            List<DepartmentReferenceDto> departments)
        {
            return departments.ToDictionary(
                d => d.NormalizedName,
                d => d,
                StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Build area lookup dictionary
        /// </summary>
        public Dictionary<string, AreaReferenceDto> BuildAreaLookup(
            List<AreaReferenceDto> areas)
        {
            return areas.ToDictionary(
                a => a.NormalizedName,
                a => a,
                StringComparer.OrdinalIgnoreCase);
        }

        // ===== PREVIEW DATA CREATION =====

        /// <summary>
        /// Create type-safe preview data DTO
        /// Pattern: Factory method
        /// Security: Controlled data exposure to frontend
        /// </summary>
        public ImportPreviewDataDto CreatePreviewData(
            string operatorText,
            string? requesterText,
            string departmentText,
            string? mappedDepartment,
            string areaName,
            string issueDesc,
            DateTime? dateReported,
            bool isDuplicate)
        {
            return new ImportPreviewDataDto
            {
                Operator = operatorText,
                Requester = requesterText,
                Department = departmentText,
                MappedDepartment = mappedDepartment,
                Area = areaName,
                IssueDescription = issueDesc?.Length > 50
                    ? issueDesc[..50] + "..."
                    : issueDesc ?? string.Empty,
                DateReported = dateReported,
                IsDuplicate = isDuplicate
            };
        }

        // ===== FUZZY MATCHING HELPERS =====

        /// <summary>
        /// Create fuzzy match result DTO
        /// Pattern: Value Object (DDD)
        /// Purpose: Type-safe fuzzy match results
        /// </summary>
        public FuzzyMatchResultDto CreateFuzzyMatchResult(
            RecentIssueLogDto log,
            int score)
        {
            return new FuzzyMatchResultDto
            {
                Log = log,
                Score = score
            };
        }

        // ===== PRIVATE HELPERS =====

        /// <summary>
        /// Normalize nullable string
        /// Pattern: Data sanitization (OWASP Top 10)
        /// Security: Prevent whitespace-only values
        /// </summary>
        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        /// <summary>
        /// Check if field should be updated
        /// Pattern: Guard clause
        /// Reference: Martin Fowler - Refactoring
        /// </summary>
        private static bool ShouldUpdate(string? newValue, string? currentValue)
        {
            return !string.IsNullOrWhiteSpace(newValue) && newValue != currentValue;
        }
    }
}