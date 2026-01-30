using ItSupportServer.Data.Models.Entities;
using Riok.Mapperly.Abstractions;

namespace ItSupportServer.src.Modules.IssueLog
{
    /// <summary>
    /// Import-specific mapper using Mapperly
    /// Pattern: Source generator-based mapping (zero reflection)
    /// Performance: Compile-time code generation
    /// </summary>
    [Mapper]
    public partial class IssueLogImportMapper
    {
        // ===== QUERY PROJECTION =====
        
        /// <summary>
        /// Project to recent issue log DTO (for duplicate detection)
        /// Performance: Only select needed fields
        /// Mapperly: Supports IQueryable projection
        /// </summary>
        [MapperIgnoreSource(nameof(IssueLogs.IssLogId))]
        [MapperIgnoreSource(nameof(IssueLogs.CreatedAt))]
        [MapperIgnoreSource(nameof(IssueLogs.UpdatedAt))]
        [MapperIgnoreSource(nameof(IssueLogs.DeletedAt))]
        public partial IQueryable<RecentIssueLogDto> ProjectToRecentIssueLogDto(IQueryable<IssueLogs> query);
        
        // ===== ENTITY CREATION =====
        
        /// <summary>
        /// Map Excel row DTO to IssueLogs entity
        /// Mapperly: Auto-generates mapping code
        /// </summary>
        [MapProperty(nameof(ExcelRowDto.Operator), nameof(IssueLogs.Operator))]
        [MapProperty(nameof(ExcelRowDto.Requester), nameof(IssueLogs.Requester))]
        [MapProperty(nameof(ExcelRowDto.DepartmentId), nameof(IssueLogs.DepartmentId))]
        [MapProperty(nameof(ExcelRowDto.AreaId), nameof(IssueLogs.AreaId))]
        [MapProperty(nameof(ExcelRowDto.IssueDescription), nameof(IssueLogs.IssueDescription))]
        [MapProperty(nameof(ExcelRowDto.Cause), nameof(IssueLogs.Cause))]
        [MapProperty(nameof(ExcelRowDto.Resolution), nameof(IssueLogs.Resolution))]
        [MapProperty(nameof(ExcelRowDto.PermanentFix), nameof(IssueLogs.PermanentFix))]
        [MapProperty(nameof(ExcelRowDto.DateReported), nameof(IssueLogs.DateReported))]
        [MapProperty(nameof(ExcelRowDto.Status), nameof(IssueLogs.Status))]
        public partial IssueLogs MapToEntity(ExcelRowDto dto);
        
        /// <summary>
        /// Post-mapping configuration for new entity
        /// </summary>
        //private partial void MapToEntityAfterMapping(ExcelRowDto dto, IssueLogs entity)
        //{
        //    // Generate new ID
        //    entity.IssLogId = Guid.CreateVersion7();
            
        //    // Normalize nullable strings
        //    entity.Requester = NormalizeNullableString(dto.Requester);
        //    entity.Cause = NormalizeNullableString(dto.Cause);
        //    entity.Resolution = NormalizeNullableString(dto.Resolution);
        //    entity.PermanentFix = NormalizeNullableString(dto.PermanentFix);
        //    entity.Status = NormalizeNullableString(dto.Status);
        //}
        
        // ===== ENTITY UPDATE =====
        
        /// <summary>
        /// Update entity from Excel update DTO (for duplicate updates)
        /// Mapperly: Conditional mapping with custom logic
        /// </summary>
        public void UpdateEntityFromExcelRow(
            IssueLogs existingLog,
            ExcelUpdateDto updateDto)
        {
            // Only update if new value is provided and different
            if (ShouldUpdate(updateDto.Cause, existingLog.Cause))
            {
                existingLog.Cause = updateDto.Cause;
            }

            if (ShouldUpdate(updateDto.Resolution, existingLog.Resolution))
            {
                existingLog.Resolution = updateDto.Resolution;
            }

            if (ShouldUpdate(updateDto.PermanentFix, existingLog.PermanentFix))
            {
                existingLog.PermanentFix = updateDto.PermanentFix;
            }

            if (ShouldUpdate(updateDto.Status, existingLog.Status))
            {
                existingLog.Status = updateDto.Status;
            }
        }
        
        // ===== HELPER METHODS =====
        
        /// <summary>
        /// Normalize nullable string (convert whitespace to null)
        /// </summary>
        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
        
        /// <summary>
        /// Check if field should be updated
        /// </summary>
        private static bool ShouldUpdate(string? newValue, string? currentValue)
        {
            return !string.IsNullOrWhiteSpace(newValue) && newValue != currentValue;
        }
        
        // ===== CONVENIENCE METHODS (backward compatibility) =====
        
        /// <summary>
        /// Map Excel row parameters to entity (convenience wrapper)
        /// </summary>
        public IssueLogs MapExcelRowToEntity(
            string operatorText,
            string? requesterText,
            int departmentId,
            int areaId,
            string issueDescription,
            string? cause,
            string? resolution,
            string? permanentFix,
            DateTime dateReported,
            string? status)
        {
            var dto = new ExcelRowDto
            {
                Operator = operatorText,
                Requester = requesterText,
                DepartmentId = departmentId,
                AreaId = areaId,
                IssueDescription = issueDescription,
                Cause = cause,
                Resolution = resolution,
                PermanentFix = permanentFix,
                DateReported = dateReported,
                Status = status
            };
            
            return MapToEntity(dto);
        }
        
        /// <summary>
        /// Update entity from Excel row parameters (convenience wrapper)
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
    }
}