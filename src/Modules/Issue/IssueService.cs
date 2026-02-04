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

        public async Task<bool> DeleteIssuesAsync(List<long> issIds, bool softDelete = true)
        {
            _logger.LogInformation("Deleting {Count} issues (soft: {SoftDelete})",
                issIds?.Count ?? 0, softDelete);

            ArgumentNullException.ThrowIfNull(issIds);

            if (issIds.Count == 0)
            {
                throw new Shared.Exceptions.ValidationException("issIds",
                    "Vui lòng chọn vấn đề để xóa");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var existing = await _db.Issues
                    .Where(i => issIds.Contains(i.IssId) && i.DeletedAt == null)
                    .ToListAsync();

                if (existing.Count == 0)
                {
                    throw new NotFoundException("Không tìm thấy vấn đề để xóa");
                }

                // ✅ FIX: Referential Integrity Check 1 - Causes
                // Pattern: Prevent orphaned child records
                // Reference: Database Design Best Practices
                // Handle nullable IssId in Causes entity
                var issuesWithCauses = await _db.Causes
                    .Where(c => 
                        issIds.Contains(c.IssId) &&  // ✅ Use .Value after null check
                        c.DeletedAt == null)
                    .Select(c => c.IssId)  // ✅ Non-null assertion after HasValue check
                    .Distinct()
                    .ToListAsync();

                if (issuesWithCauses.Any())
                {
                    var usedIssueNames = existing
                        .Where(i => issuesWithCauses.Contains(i.IssId))
                        .Select(i => i.Name)
                        .ToList();

                    _logger.LogWarning("Cannot delete issues {Issues} - have causes",
                        string.Join(", ", usedIssueNames));

                    throw new BusinessRuleException(
                        $"Không thể xóa vấn đề {string.Join(", ", usedIssueNames)} vì có nguyên nhân liên quan. " +
                        $"Vui lòng xóa các nguyên nhân trước.");
                }

                // ✅ Referential integrity - Issue Logs
                var issuesInLogs = await _db.IssueLogs
                    .Where(il => 
                        il.IssueId.HasValue &&
                        issIds.Contains(il.IssueId.Value) &&
                        il.DeletedAt == null)
                    .Select(il => il.IssueId!.Value)
                    .Distinct()
                    .ToListAsync();

                if (issuesInLogs.Any())
                {
                    var usedIssueNames = existing
                        .Where(i => issuesInLogs.Contains(i.IssId))
                        .Select(i => i.Name)
                        .ToList();

                    _logger.LogWarning("Cannot delete issues {Issues} - referenced in logs",
                        string.Join(", ", usedIssueNames));

                    throw new BusinessRuleException(
                        $"Không thể xóa vấn đề {string.Join(", ", usedIssueNames)} vì đang được sử dụng trong {issuesInLogs.Count} nhật ký");
                }

                if (softDelete)
                {
                    foreach (var item in existing)
                    {
                        item.DeletedAt = DateTime.UtcNow;
                    }
                    _db.Issues.UpdateRange(existing);
                }
                else
                {
                    _db.Issues.RemoveRange(existing);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully deleted {Count} issues", existing.Count);

                return true;
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
