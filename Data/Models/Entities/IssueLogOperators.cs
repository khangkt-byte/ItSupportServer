namespace ItSupportServer.Data.Models.Entities
{
    /// <summary>
    /// Junction table: IssueLog ↔ Operators (many-to-many)
    /// </summary>
    public class IssueLogOperators
    {
        public Guid IssLogId { get; set; }
        public IssueLogs IssueLog { get; set; } = null!;

        public Guid EmpId { get; set; }
        public Employees Employee { get; set; } = null!;

        public string? OperatorRole { get; set; }  // "Primary", "Assistant"

        public decimal? HoursSpent { get; set; }  // Track time
    }
}