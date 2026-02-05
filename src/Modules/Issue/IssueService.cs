using FluentValidation;
using ItSupportServer.Data.Models;
using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Exceptions;
using ItSupportServer.src.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ItSupportServer.src.Modules.Issue
{
    /// <summary>
    /// Issue service implementation
    /// Pattern: Domain-Driven Design (DDD) service layer
    /// Security: Input validation, referential integrity checks
    /// Reference: ServiceNow Knowledge Base Management
    /// </summary>
    public class IssueService : IIssueService
    {
        private readonly AppDbContext _db;
        private readonly IssueMapper _mapper;
        private readonly ILogger<IssueService> _logger;
        private readonly IValidator<CreateIssueDto> _createValidator;
        private readonly IValidator<UpdateIssueDto> _updateValidator;

        public IssueService(
            AppDbContext db,
            IssueMapper mapper,
            ILogger<IssueService> logger,
            IValidator<CreateIssueDto> createValidator,
            IValidator<UpdateIssueDto> updateValidator)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<PaginatedResult<IssueDto>> GetIssuesAsync(QueryParameters parameters)
        {
            _logger.LogInformation("Fetching issues with search: {Search}, page: {Page}",
                parameters.Search, parameters.Page);

            var query = _mapper.ProjectToIssueDto(_db.Issues
                .Include(i => i.Causes)
                .Where(i => i.DeletedAt == null)
                .AsNoTracking());

            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                query = query.Where(i =>
                    i.Name.Contains(parameters.Search) ||
                    (i.Description != null && i.Description.Contains(parameters.Search)) ||
                    (i.Category != null && i.Category.Contains(parameters.Search)));
            }

            var result = await query.ToPaginatedResultAsync(parameters, defaultSortField: "Name");

            _logger.LogInformation("Retrieved {Count} issues", result.TotalCount);

            return result;
        }

        public async Task<IssueDto> GetIssueByIdAsync(long issId)
        {
            _logger.LogInformation("Fetching issue {IssId}", issId);

            var issue = await _mapper.ProjectToIssueDto(_db.Issues
                .Include(i => i.Causes)
                .Where(i => i.IssId == issId && i.DeletedAt == null)
                .AsNoTracking())
                .FirstOrDefaultAsync();

            if (issue is null)
            {
                _logger.LogWarning("Issue {IssId} not found", issId);
                throw new NotFoundException("Vấn đề", issId);
            }

            // Calculate usage count separately (performance optimization)
            // Reference: Avoid SELECT N+1 problem
            var usageCount = await _db.IssueLogs
                .Where(il => il.IssueId == issId && il.DeletedAt == null)
                .CountAsync();

            return issue with { UsageCount = usageCount };
        }

        public async Task<IssueDto> CreateIssueAsync(CreateIssueDto dto)
        {
            _logger.LogInformation("Creating new issue: {Name}", dto.Name);

            var validationResult = await _createValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            var nameExists = await _db.Issues
                .Where(i => i.Name == dto.Name && i.DeletedAt == null)
                .AnyAsync();

            if (nameExists)
            {
                _logger.LogWarning("Issue with name '{Name}' already exists", dto.Name);
                throw new ConflictException("Vấn đề", dto.Name);
            }

            var newIssue = _mapper.MapToIssue(dto);
            // IssId auto-increment
            // CreatedAt set by interceptor

            await _db.Issues.AddAsync(newIssue);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Successfully created issue {IssId}", newIssue.IssId);

            return await GetIssueByIdAsync(newIssue.IssId);
        }

        public async Task<IssueDto> UpdateIssueAsync(long issId, UpdateIssueDto dto)
        {
            _logger.LogInformation("Updating issue {IssId}", issId);

            var validationResult = await _updateValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            var issue = await _db.Issues.FindAsync(issId);

            if (issue is null || issue.DeletedAt != null)
            {
                _logger.LogWarning("Issue {IssId} not found", issId);
                throw new NotFoundException("Vấn đề", issId);
            }

            bool hasChanges = false;

            // Update Name
            if (dto.Name != null && issue.Name != dto.Name)
            {
                var nameExists = await _db.Issues
                    .Where(i => i.Name == dto.Name && i.IssId != issId && i.DeletedAt == null)
                    .AnyAsync();

                if (nameExists)
                {
                    throw new ConflictException("Vấn đề", dto.Name);
                }

                issue.Name = dto.Name.Trim();
                hasChanges = true;
            }

            // Update Description
            if (dto.Description != null && issue.Description != dto.Description)
            {
                issue.Description = string.IsNullOrWhiteSpace(dto.Description) 
                    ? null 
                    : dto.Description.Trim();
                hasChanges = true;
            }

            if (dto.Category != null && issue.Category != dto.Category)
            {
                issue.Category = string.IsNullOrWhiteSpace(dto.Category) 
                    ? null 
                    : dto.Category.Trim();
                hasChanges = true;
            }

            if (dto.Severity.HasValue && issue.Severity != dto.Severity.Value)
            {
                issue.Severity = dto.Severity.Value;
                hasChanges = true;
            }

            if (hasChanges)
            {
                await _db.SaveChangesAsync();
                _logger.LogInformation("Successfully updated issue {IssId}", issId);
            }
            else
            {
                _logger.LogInformation("No changes detected for issue {IssId}", issId);
            }

            return await GetIssueByIdAsync(issId);
        }

        /// <summary>
        /// Delete single issue
        /// Pattern: RESTful single resource delete (returns void, throws on error)
        /// Reference: Microsoft REST API Guidelines - DELETE returns 204 No Content
        /// Business Rules: 
        /// - Cannot delete if issue has Causes
        /// - Cannot delete if issue is referenced in IssueLogs
        /// </summary>
        public async Task DeleteIssueAsync(long issId, bool softDelete = true)
        {
            _logger.LogInformation("Deleting issue {IssId} | SoftDelete: {SoftDelete}",
                issId, softDelete);

            var issue = await _db.Issues
                .Include(i => i.Causes)
                .FirstOrDefaultAsync(i => i.IssId == issId && i.DeletedAt == null);

            if (issue == null)
            {
                _logger.LogWarning("Issue {IssId} not found", issId);
                throw new NotFoundException("Vấn đề", issId);
            }

            // ✅ Business rule 1: Check for Causes
            // Pattern: Prevent orphaned child records (referential integrity)
            // Reference: Database Design Best Practices
            if (issue.Causes != null && issue.Causes.Any(c => c.DeletedAt == null))
            {
                var activeCauseCount = issue.Causes.Count(c => c.DeletedAt == null);
                _logger.LogWarning("Issue {IssId}:{Name} has {Count} causes",
                    issue.IssId, issue.Name, activeCauseCount);

                throw new BusinessRuleException(
                    $"Không thể xóa vấn đề '{issue.Name}' vì có {activeCauseCount} nguyên nhân liên quan. " +
                    $"Vui lòng xóa các nguyên nhân trước.",
                    "ISSUE_HAS_CAUSES");
            }

            // ✅ Business rule 2: Check IssueLogs references
            var hasIssueLogs = await _db.IssueLogs
                .AnyAsync(il => il.IssueId == issId && il.DeletedAt == null);

            if (hasIssueLogs)
            {
                var issueLogCount = await _db.IssueLogs
                    .CountAsync(il => il.IssueId == issId && il.DeletedAt == null);

                _logger.LogWarning("Issue {IssId}:{Name} referenced in {Count} logs",
                    issue.IssId, issue.Name, issueLogCount);

                throw new BusinessRuleException(
                    $"Không thể xóa vấn đề '{issue.Name}' vì đang được sử dụng trong {issueLogCount} nhật ký",
                    "ISSUE_IN_USE");
            }

            // ✅ Perform delete
            if (softDelete)
            {
                issue.DeletedAt = DateTime.UtcNow;
                _db.Issues.Update(issue);

                await _db.SaveChangesAsync();

                _logger.LogInformation("Soft deleted issue {IssId}:{Name}",
                    issue.IssId, issue.Name);
            }
            else
            {
                // Hard delete with transaction
                using var transaction = await _db.Database.BeginTransactionAsync();

                try
                {
                    _db.Issues.Remove(issue);

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Hard deleted issue {IssId}:{Name}",
                        issue.IssId, issue.Name);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        /// <summary>
        /// Delete multiple issues with all-or-nothing transaction
        /// Pattern: Microsoft Dynamics 365 bulk operations
        /// Strategy: Validate ALL → Delete ALL → Return summary
        /// Reference: https://learn.microsoft.com/en-us/power-apps/developer/data-platform/bulk-operations
        /// </summary>
        public async Task<BulkDeleteResultDto> DeleteIssuesAsync(List<long> issIds, bool softDelete = true)
        {
            _logger.LogInformation("Batch delete started | Count: {Count} | SoftDelete: {SoftDelete}",
                issIds?.Count ?? 0, softDelete);

            // ✅ Input validation
            ArgumentNullException.ThrowIfNull(issIds);

            if (issIds.Count == 0)
            {
                throw new Shared.Exceptions.ValidationException("issIds",
                    "Vui lòng chọn ít nhất một vấn đề để xóa");
            }

            // ✅ Remove duplicates
            var uniqueIds = issIds.Distinct().ToList();

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // ✅ Step 1: Validate ALL items BEFORE any deletion
                var existing = await _db.Issues
                    .Include(i => i.Causes)
                    .Where(i => uniqueIds.Contains(i.IssId) && i.DeletedAt == null)
                    .ToListAsync();

                var notFoundIds = uniqueIds.Except(existing.Select(i => i.IssId)).ToList();
                if (notFoundIds.Any())
                {
                    _logger.LogWarning("Issues not found: {Ids}", string.Join(", ", notFoundIds));
                    throw new NotFoundException($"Vấn đề không tồn tại: {string.Join(", ", notFoundIds)}");
                }

                // ✅ Step 2: Check business rules for ALL items

                // Rule 1: Check for Causes (referential integrity)
                var issuesWithCauses = existing
                    .Where(i => i.Causes != null && i.Causes.Any(c => c.DeletedAt == null))
                    .ToList();

                if (issuesWithCauses.Any())
                {
                    var errorDetails = issuesWithCauses
                        .Select(i => $"{i.Name} ({i.Causes!.Count(c => c.DeletedAt == null)} nguyên nhân)")
                        .ToList();

                    _logger.LogWarning("Issues with causes: {Issues}",
                        string.Join(", ", issuesWithCauses.Select(i => $"{i.IssId}:{i.Name}")));

                    throw new BusinessRuleException(
                        $"Không thể xóa vấn đề {string.Join(", ", errorDetails)} vì có nguyên nhân liên quan. " +
                        $"Vui lòng xóa các nguyên nhân trước.",
                        "ISSUE_HAS_CAUSES");
                }

                // Rule 2: Check IssueLogs references
                var issuesInLogs = await _db.IssueLogs
                    .Where(il => il.IssueId.HasValue &&
                           uniqueIds.Contains(il.IssueId.Value) &&
                           il.DeletedAt == null)
                    .Select(il => il.IssueId!.Value)
                    .Distinct()
                    .ToListAsync();

                if (issuesInLogs.Any())
                {
                    var usedIssues = existing
                        .Where(i => issuesInLogs.Contains(i.IssId))
                        .ToList();

                    var errorDetails = new List<string>();
                    foreach (var issue in usedIssues)
                    {
                        var count = await _db.IssueLogs
                            .CountAsync(il => il.IssueId == issue.IssId && il.DeletedAt == null);
                        errorDetails.Add($"{issue.Name} ({count} nhật ký)");
                    }

                    _logger.LogWarning("Issues referenced in logs: {Issues}",
                        string.Join(", ", usedIssues.Select(i => $"{i.IssId}:{i.Name}")));

                    throw new BusinessRuleException(
                        $"Không thể xóa vấn đề {string.Join(", ", errorDetails)} vì đang được sử dụng trong nhật ký",
                        "ISSUE_IN_USE");
                }

                // ✅ Step 3: All checks passed → Delete ALL
                foreach (var issue in existing)
                {
                    if (softDelete)
                        issue.DeletedAt = DateTime.UtcNow;
                    else
                        _db.Issues.Remove(issue);

                    _logger.LogInformation("Deleted issue {IssId}:{Name}", issue.IssId, issue.Name);
                }

                if (softDelete)
                {
                    _db.Issues.UpdateRange(existing);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Batch delete SUCCESS | Count: {Count}", existing.Count);

                return new BulkDeleteResultDto
                {
                    Success = true,
                    DeletedCount = existing.Count,
                    TotalRequested = issIds.Count,
                    Message = $"Đã xóa {existing.Count} vấn đề thành công"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<IssueSuggestionDto>> GetIssueSuggestionsAsync(string? search = null)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Fetching issue suggestions with search: {Search}", search);

            var query = _db.Issues
                .Where(i => i.DeletedAt == null)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(i =>
                    i.Name.Contains(search) ||
                    (i.Description != null && i.Description.Contains(search)));
            }

            // ✅ USE MAPPERLY PROJECTION: Type-safe, no anonymous types
            // Pattern: KISS with Mapperly type safety
            // Performance: Single database query with LEFT JOIN + GROUP BY
            // Benefits:
            // - Compile-time type checking (no runtime errors)
            // - Refactoring safety (rename detection)
            // - Code reusability (method in mapper)
            // - Testability (can mock mapper)
            // References:
            // - Mapperly: https://github.com/riok/mapperly
            // - EF Core: Query projection patterns
            // - Clean Code: Single Responsibility Principle
            var suggestions = await _mapper
                .ProjectToIssueSuggestion(query, _db.IssueLogs)
                .OrderByDescending(x => x.UsageCount)
                .ThenBy(x => x.Name)
                .Take(10)
                .ToListAsync();

            stopwatch.Stop();

            _logger.LogInformation(
                "Retrieved {Count} issue suggestions in {Duration}ms",
                suggestions.Count, stopwatch.ElapsedMilliseconds);

            // Performance monitoring
            if (stopwatch.ElapsedMilliseconds > 100)
            {
                _logger.LogWarning(
                    "Performance alert: Issue suggestions exceeded 100ms threshold ({Duration}ms). " +
                    "Consider adding response caching (5min TTL).",
                    stopwatch.ElapsedMilliseconds);
            }

            return suggestions;
        }
    }
}
