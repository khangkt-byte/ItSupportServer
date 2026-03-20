using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using ItSupportServer.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ItSupportServer.src.Modules.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<DashboardService> _logger;
        private readonly IMemoryCache _cache;

        private static readonly TimeSpan DashboardSummaryCacheTtl = TimeSpan.FromSeconds(30);
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> CacheLocks = new();

        private static readonly HashSet<string> ResolvedStatuses =
        [
            "resolved",
            "closed",
            "done",
            "completed"
        ];

        private const string DefaultTimezone = "UTC";

        public DashboardService(AppDbContext db, ILogger<DashboardService> logger, IMemoryCache cache)
        {
            _db = db;
            _logger = logger;
            _cache = cache;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync(
            DashboardSummaryQueryDto query,
            string roleScope,
            CancellationToken cancellationToken = default)
        {
            var normalized = await NormalizeQueryAsync(query, cancellationToken);
            var cacheKey = BuildCacheKey(normalized, roleScope);

            if (_cache.TryGetValue(cacheKey, out DashboardSummaryDto? cached) && cached != null)
            {
                _logger.LogDebug("Dashboard summary returned from cache. Key={CacheKey}", cacheKey);
                return cached;
            }

            var cacheLock = CacheLocks.GetOrAdd(cacheKey, static _ => new SemaphoreSlim(1, 1));
            await cacheLock.WaitAsync(cancellationToken);
            try
            {
                if (_cache.TryGetValue(cacheKey, out cached) && cached != null)
                {
                    _logger.LogDebug("Dashboard summary returned from cache after lock. Key={CacheKey}", cacheKey);
                    return cached;
                }

                var stopwatch = Stopwatch.StartNew();
                var utcNow = DateTime.UtcNow;
                var today = ConvertUtcNowToDate(utcNow, normalized.TimezoneInfo);

                var issueLogsInPeriodQuery = _db.IssueLogs
                    .AsNoTracking()
                    .Where(x => x.DeletedAt == null && x.DateReported >= normalized.FromDate && x.DateReported <= normalized.ToDate);

                var statusBreakdown = await issueLogsInPeriodQuery
                    .GroupBy(x => x.Status == null || x.Status.Trim() == string.Empty ? "Unknown" : x.Status.Trim())
                    .Select(g => new DashboardStatusBreakdownDto
                    {
                        Status = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync(cancellationToken);

                var topDepartments = await issueLogsInPeriodQuery
                    .GroupBy(x => new { x.DptId, x.Department.Name })
                    .Select(g => new DashboardDepartmentIssueDto
                    {
                        DptId = g.Key.DptId,
                        DepartmentName = g.Key.Name,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToListAsync(cancellationToken);

                var trend = normalized.GroupBy switch
                {
                    DashboardTrendGroupBy.Day => await BuildDailyTrendAsync(issueLogsInPeriodQuery, normalized.FromDate, normalized.ToDate, cancellationToken),
                    DashboardTrendGroupBy.Month => await BuildMonthlyTrendAsync(issueLogsInPeriodQuery, normalized.FromDate, normalized.ToDate, cancellationToken),
                    _ => throw new InvalidOperationException("Unsupported trend group by")
                };

                var totalDepartments = await _db.Departments
                    .AsNoTracking()
                    .CountAsync(x => x.DeletedAt == null, cancellationToken);

                var totalEmployees = await _db.Employees
                    .AsNoTracking()
                    .CountAsync(x => x.DeletedAt == null, cancellationToken);

                var totalAccounts = await _db.Accounts
                    .AsNoTracking()
                    .CountAsync(x => x.DeletedAt == null, cancellationToken);

                var activeSessions = await _db.AccountTokens
                    .AsNoTracking()
                    .CountAsync(x => x.RevokedAt == null && x.ExpiryTime > utcNow, cancellationToken);

                var totalIssueLogs = statusBreakdown.Sum(x => x.Count);
                var resolvedIssueLogs = statusBreakdown
                    .Where(x => ResolvedStatuses.Contains(x.Status.Trim().ToLowerInvariant()))
                    .Sum(x => x.Count);

                var issueLogsToday = normalized.FromDate <= today && normalized.ToDate >= today
                    ? await _db.IssueLogs
                        .AsNoTracking()
                        .CountAsync(x => x.DeletedAt == null && x.DateReported == today, cancellationToken)
                    : 0;

                var result = new DashboardSummaryDto
                {
                    Overview = new DashboardOverviewDto
                    {
                        TotalIssueLogs = totalIssueLogs,
                        IssueLogsToday = issueLogsToday,
                        OpenIssueLogs = totalIssueLogs - resolvedIssueLogs,
                        ResolvedIssueLogs = resolvedIssueLogs,
                        TotalDepartments = totalDepartments,
                        TotalEmployees = totalEmployees,
                        TotalAccounts = totalAccounts,
                        ActiveSessions = activeSessions
                    },
                    Filter = new DashboardSummaryFilterDto
                    {
                        Period = normalized.Period,
                        GroupBy = normalized.GroupBy,
                        Timezone = normalized.TimezoneInfo.Id,
                        FromDate = normalized.FromDate,
                        ToDate = normalized.ToDate,
                        MonthOffset = normalized.MonthOffset
                    },
                    Trend = trend,
                    StatusBreakdown = statusBreakdown,
                    TopDepartments = topDepartments
                };

                _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = DashboardSummaryCacheTtl
                });

                stopwatch.Stop();
                _logger.LogInformation(
                    "Dashboard summary generated in {ElapsedMs}ms. Period={Period}, GroupBy={GroupBy}, FromDate={FromDate}, ToDate={ToDate}, Timezone={Timezone}, RoleScope={RoleScope}",
                    stopwatch.ElapsedMilliseconds,
                    normalized.Period,
                    normalized.GroupBy,
                    normalized.FromDate,
                    normalized.ToDate,
                    normalized.TimezoneInfo.Id,
                    roleScope);

                return result;
            }
            finally
            {
                cacheLock.Release();
            }
        }

        private static async Task<List<DashboardTrendPointDto>> BuildDailyTrendAsync(
            IQueryable<Data.Models.Entities.IssueLogs> issueLogsInPeriodQuery,
            DateOnly fromDate,
            DateOnly toDate,
            CancellationToken cancellationToken)
        {
            var trendRaw = await issueLogsInPeriodQuery
                .GroupBy(x => x.DateReported)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Date, x => x.Count, cancellationToken);

            var totalDays = toDate.DayNumber - fromDate.DayNumber + 1;
            return Enumerable.Range(0, totalDays)
                .Select(offset =>
                {
                    var date = fromDate.AddDays(offset);
                    return new DashboardTrendPointDto
                    {
                        PeriodStart = date,
                        Label = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        Count = trendRaw.GetValueOrDefault(date, 0)
                    };
                })
                .ToList();
        }

        private static async Task<List<DashboardTrendPointDto>> BuildMonthlyTrendAsync(
            IQueryable<Data.Models.Entities.IssueLogs> issueLogsInPeriodQuery,
            DateOnly fromDate,
            DateOnly toDate,
            CancellationToken cancellationToken)
        {
            var monthRaw = await issueLogsInPeriodQuery
                .GroupBy(x => new { x.DateReported.Year, x.DateReported.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToDictionaryAsync(x => (x.Year, x.Month), x => x.Count, cancellationToken);

            var monthStarts = GetMonthStarts(fromDate, toDate);

            return monthStarts
                .Select(monthStart =>
                {
                    var key = (monthStart.Year, monthStart.Month);
                    return new DashboardTrendPointDto
                    {
                        PeriodStart = monthStart,
                        Label = monthStart.ToString("yyyy-MM", CultureInfo.InvariantCulture),
                        Count = monthRaw.GetValueOrDefault(key, 0)
                    };
                })
                .ToList();
        }

        private static List<DateOnly> GetMonthStarts(DateOnly fromDate, DateOnly toDate)
        {
            var current = new DateOnly(fromDate.Year, fromDate.Month, 1);
            var end = new DateOnly(toDate.Year, toDate.Month, 1);

            var result = new List<DateOnly>();
            while (current <= end)
            {
                result.Add(current);
                current = current.AddMonths(1);
            }

            return result;
        }

        private static DateOnly ConvertUtcNowToDate(DateTime utcNow, TimeZoneInfo timezone)
        {
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timezone);
            return DateOnly.FromDateTime(localNow);
        }

        private static string BuildCacheKey(NormalizedDashboardQuery query, string roleScope)
        {
            var builder = new StringBuilder("Dashboard:Summary:");
            builder.Append(query.Period).Append(':')
                .Append(query.GroupBy).Append(':')
                .Append(query.FromDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture)).Append(':')
                .Append(query.ToDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture)).Append(':')
                .Append(query.TimezoneInfo.Id).Append(':')
                .Append(query.MonthOffset).Append(':')
                .Append(roleScope);

            return builder.ToString();
        }

        private async Task<NormalizedDashboardQuery> NormalizeQueryAsync(
            DashboardSummaryQueryDto query,
            CancellationToken cancellationToken)
        {
            var timezone = ResolveTimezone(query.Timezone);
            var utcNow = DateTime.UtcNow;
            var localToday = ConvertUtcNowToDate(utcNow, timezone);

            return query.Period switch
            {
                DashboardPeriod.Day => new NormalizedDashboardQuery(
                    DashboardPeriod.Day,
                    query.GroupBy,
                    localToday,
                    localToday,
                    timezone,
                    0),

                DashboardPeriod.Month => NormalizeMonthQuery(query, localToday, timezone),

                DashboardPeriod.Range => NormalizeRangeQuery(query, timezone),

                DashboardPeriod.AllTime => await NormalizeAllTimeQueryAsync(query, localToday, timezone, cancellationToken),

                _ => throw new InvalidOperationException("Unsupported dashboard period")
            };
        }

        private static NormalizedDashboardQuery NormalizeMonthQuery(
            DashboardSummaryQueryDto query,
            DateOnly localToday,
            TimeZoneInfo timezone)
        {
            var effectiveOffset = Math.Abs(query.MonthOffset);
            var targetMonthDate = localToday.AddMonths(-effectiveOffset);
            var fromDate = new DateOnly(targetMonthDate.Year, targetMonthDate.Month, 1);
            var toDate = fromDate.AddMonths(1).AddDays(-1);

            return new NormalizedDashboardQuery(
                DashboardPeriod.Month,
                query.GroupBy,
                fromDate,
                toDate,
                timezone,
                effectiveOffset);
        }

        private static NormalizedDashboardQuery NormalizeRangeQuery(
            DashboardSummaryQueryDto query,
            TimeZoneInfo timezone)
        {
            if (!query.FromDate.HasValue || !query.ToDate.HasValue)
            {
                throw new InvalidOperationException("FromDate and ToDate are required when period is range");
            }

            var fromDate = query.FromDate.Value;
            var toDate = query.ToDate.Value;

            if (fromDate > toDate)
            {
                throw new InvalidOperationException("FromDate cannot be greater than ToDate");
            }

            return new NormalizedDashboardQuery(
                DashboardPeriod.Range,
                query.GroupBy,
                fromDate,
                toDate,
                timezone,
                0);
        }

        private async Task<NormalizedDashboardQuery> NormalizeAllTimeQueryAsync(
            DashboardSummaryQueryDto query,
            DateOnly localToday,
            TimeZoneInfo timezone,
            CancellationToken cancellationToken)
        {
            var firstIssueDate = await _db.IssueLogs
                .AsNoTracking()
                .Where(x => x.DeletedAt == null)
                .Select(x => (DateOnly?)x.DateReported)
                .MinAsync(cancellationToken)
                ?? localToday;

            return new NormalizedDashboardQuery(
                DashboardPeriod.AllTime,
                query.GroupBy,
                firstIssueDate,
                localToday,
                timezone,
                0);
        }

        private static TimeZoneInfo ResolveTimezone(string? timezoneInput)
        {
            var timezoneId = string.IsNullOrWhiteSpace(timezoneInput)
                ? DefaultTimezone
                : timezoneInput.Trim();

            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                if (timezoneId.Equals("Asia/Ho_Chi_Minh", StringComparison.OrdinalIgnoreCase) ||
                    timezoneId.Equals("Asia/Bangkok", StringComparison.OrdinalIgnoreCase))
                {
                    return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                }

                throw new InvalidOperationException($"Unsupported timezone: {timezoneId}");
            }
            catch (InvalidTimeZoneException)
            {
                throw new InvalidOperationException($"Invalid timezone: {timezoneId}");
            }
        }

        private sealed record NormalizedDashboardQuery(
            DashboardPeriod Period,
            DashboardTrendGroupBy GroupBy,
            DateOnly FromDate,
            DateOnly ToDate,
            TimeZoneInfo TimezoneInfo,
            int MonthOffset);
    }
}
