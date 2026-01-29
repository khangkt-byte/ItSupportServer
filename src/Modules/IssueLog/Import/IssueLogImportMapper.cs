using ItSupportServer.Data.Models.Entities;

namespace ItSupportServer.src.Modules.IssueLog
{
    /// <summary>
    /// Import-specific mapper
    /// Pattern: Separate mapper for different use cases
    /// Purpose: Projection for duplicate detection, export
    /// </summary>
    public class IssueLogImportMapper
    {
        /// <summary>
        /// Project to recent issue log DTO (for duplicate detection)
        /// Performance: Only select needed fields
        /// </summary>
        public IQueryable<RecentIssueLogDto> ProjectToRecentIssueLogDto(IQueryable<IssueLogs> query)
        {
            return query.Select(il => new RecentIssueLogDto
            {
                IssLogId = il.IssLogId,
                Operator = il.Operator,
                DepartmentId = il.DepartmentId,
                IssueDescription = il.IssueDescription,
                DateReported = il.DateReported
            });
        }

        /// <summary>
        /// Map Excel row to IssueLogs entity
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
            return new IssueLogs
            {
                IssLogId = Guid.CreateVersion7(),
                Operator = operatorText,
                Requester = string.IsNullOrWhiteSpace(requesterText) ? null : requesterText,
                DepartmentId = departmentId,
                AreaId = areaId,
                IssueDescription = issueDescription,
                Cause = string.IsNullOrWhiteSpace(cause) ? null : cause,
                Resolution = string.IsNullOrWhiteSpace(resolution) ? null : resolution,
                PermanentFix = string.IsNullOrWhiteSpace(permanentFix) ? null : permanentFix,
                DateReported = dateReported,
                Status = string.IsNullOrWhiteSpace(status) ? null : status
            };
        }

        /// <summary>
        /// Update entity from Excel row (for duplicate updates)
        /// </summary>
        public void UpdateEntityFromExcelRow(
            IssueLogs existingLog,
            string? cause,
            string? resolution,
            string? permanentFix,
            string? status)
        {
            if (!string.IsNullOrWhiteSpace(cause) && existingLog.Cause != cause)
            {
                existingLog.Cause = cause;
            }

            if (!string.IsNullOrWhiteSpace(resolution) && existingLog.Resolution != resolution)
            {
                existingLog.Resolution = resolution;
            }

            if (!string.IsNullOrWhiteSpace(permanentFix) && existingLog.PermanentFix != permanentFix)
            {
                existingLog.PermanentFix = permanentFix;
            }

            if (!string.IsNullOrWhiteSpace(status) && existingLog.Status != status)
            {
                existingLog.Status = status;
            }
        }
    }
}