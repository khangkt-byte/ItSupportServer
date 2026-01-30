namespace ItSupportServer.Data.Models.Entities
{
    /// <summary>
    /// Junction table: IssueLog ↔ Requesters
    /// Supports both employees and external requesters
    /// </summary>
    public class IssueLogRequesters
    {
        public long IssLogReqId { get; set; }

        public Guid IssLogId { get; set; }
        public IssueLogs IssueLog { get; set; } = null!;

        // ✅ NULL if requester is not an employee
        public Guid? EmpId { get; set; }
        public Employees? Employee { get; set; }

        // ✅ For non-employee requesters or Excel import
        public string? RequesterName { get; set; }

        public string? RequesterType { get; set; }  // "Employee", "Department", "External"
    }
}