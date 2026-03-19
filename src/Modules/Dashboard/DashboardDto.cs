namespace ItSupportServer.src.Modules.Dashboard
{
    public enum DashboardPeriod
    {
        Day,
        Month,
        Range,
        AllTime
    }

    public enum DashboardTrendGroupBy
    {
        Day,
        Month
    }

    public record DashboardSummaryQueryDto
    {
        public DashboardPeriod Period { get; init; } = DashboardPeriod.Day;
        public DateOnly? FromDate { get; init; }
        public DateOnly? ToDate { get; init; }
        public string? Timezone { get; init; }
        public DashboardTrendGroupBy GroupBy { get; init; } = DashboardTrendGroupBy.Day;
        public int MonthOffset { get; init; } = 0;
    }

    public record DashboardSummaryFilterDto
    {
        public DashboardPeriod Period { get; init; }
        public DashboardTrendGroupBy GroupBy { get; init; }
        public required string Timezone { get; init; }
        public DateOnly FromDate { get; init; }
        public DateOnly ToDate { get; init; }
        public int MonthOffset { get; init; }
    }

    public record DashboardOverviewDto
    {
        public int TotalIssueLogs { get; init; }
        public int IssueLogsToday { get; init; }
        public int OpenIssueLogs { get; init; }
        public int ResolvedIssueLogs { get; init; }
        public int TotalDepartments { get; init; }
        public int TotalEmployees { get; init; }
        public int TotalAccounts { get; init; }
        public int ActiveSessions { get; init; }
    }

    public record DashboardTrendPointDto
    {
        public DateOnly PeriodStart { get; init; }
        public required string Label { get; init; }
        public int Count { get; init; }
    }

    public record DashboardStatusBreakdownDto
    {
        public required string Status { get; init; }
        public int Count { get; init; }
    }

    public record DashboardDepartmentIssueDto
    {
        public int DptId { get; init; }
        public required string DepartmentName { get; init; }
        public int Count { get; init; }
    }

    public record DashboardSummaryDto
    {
        public required DashboardOverviewDto Overview { get; init; }
        public required DashboardSummaryFilterDto Filter { get; init; }
        public required List<DashboardTrendPointDto> Trend { get; init; }
        public required List<DashboardStatusBreakdownDto> StatusBreakdown { get; init; }
        public required List<DashboardDepartmentIssueDto> TopDepartments { get; init; }
    }
}
