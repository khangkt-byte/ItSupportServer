using FluentValidation;
using ItSupportServer.Data.Models;
using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Dto;
using ItSupportServer.src.Shared.Exceptions;
using ItSupportServer.src.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ItSupportServer.src.Modules.Cause
{
    /// <summary>
    /// Cause service implementation
    /// Pattern: Domain-Driven Design (DDD) service layer
    /// Security: Input validation, FK verification, business rule enforcement
    /// Reference: ServiceNow problem management
    /// </summary>
    public class CauseService : ICauseService
    {
        private readonly AppDbContext _db;
        private readonly CauseMapper _mapper;
        private readonly ILogger<CauseService> _logger;
        private readonly IValidator<CreateCauseDto> _createValidator;
        private readonly IValidator<UpdateCauseDto> _updateValidator;

        public CauseService(
            AppDbContext db,
            CauseMapper mapper,
            ILogger<CauseService> logger,
            IValidator<CreateCauseDto> createValidator,
            IValidator<UpdateCauseDto> updateValidator)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<PaginatedResult<ListCauseDto>> GetCausesAsync(QueryParameters parameters)
        {
            _logger.LogInformation("Fetching causes with search: {Search}, page: {Page}",
                parameters.Search, parameters.Page);

            var query = _mapper.ProjectToListCauseDto(_db.Causes
                .Include(c => c.Issues)
                .Where(c => c.DeletedAt == null)
                .AsNoTracking());

            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                query = query.Where(c =>
                    c.Name.Contains(parameters.Search) ||
                    (c.IssueName != null && c.IssueName.Contains(parameters.Search)));
            }

            var result = await query.ToPaginatedResultAsync(parameters, defaultSortField: "Name");

            // ✅ Batch calculate usage counts (1 query instead of N)
            var causeIds = result.Items.Select(c => c.CauseId).ToList();

            var usageCounts = await _db.IssueLogs
                .Where(il => causeIds.Contains(il.CauseId!.Value) && il.DeletedAt == null)
                .GroupBy(il => il.CauseId!.Value)
                .Select(g => new { CauseId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CauseId, x => x.Count);

            var itemsWithUsage = result.Items.Select(c => c with
            {
                UsageCount = usageCounts.GetValueOrDefault(c.CauseId, 0)
            }).ToList();

            return result with { Items = itemsWithUsage };
        }

        public async Task<CauseDto> GetCauseByIdAsync(long causeId)
        {
            _logger.LogInformation("Fetching cause {CauseId}", causeId);

            var cause = await _mapper.ProjectToCauseDto(_db.Causes
                .Include(c => c.Issues)
                .Where(c => c.CauseId == causeId && c.DeletedAt == null)
                .AsNoTracking())
                .FirstOrDefaultAsync();

            if (cause is null)
            {
                _logger.LogWarning("Cause {CauseId} not found", causeId);
                throw new NotFoundException("Nguyên nhân", causeId);
            }

            var usageCount = await _db.IssueLogs
                .CountAsync(il => il.CauseId == causeId && il.DeletedAt == null);

            return cause with { UsageCount = usageCount };
        }

        public async Task<List<CauseDto>> GetCausesByIssueIdAsync(long issId)
        {
            _logger.LogInformation("Fetching causes for issue {IssId}", issId);

            var issueExists = await _db.Issues
                .AnyAsync(i => i.IssId == issId && i.DeletedAt == null);

            if (!issueExists)
            {
                throw new NotFoundException("Vấn đề", issId);
            }

            var causes = await _mapper.ProjectToCauseDto(_db.Causes
                .Include(c => c.Issues)
                .Where(c => c.IssId == issId && c.DeletedAt == null)
                .AsNoTracking())
                .ToListAsync();

            var causeIds = causes.Select(c => c.CauseId).ToList();

            var usageCounts = await _db.IssueLogs
                .Where(il => causeIds.Contains(il.CauseId!.Value) && il.DeletedAt == null)
                .GroupBy(il => il.CauseId!.Value)
                .Select(g => new { CauseId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CauseId, x => x.Count);

            var causesWithUsage = causes.Select(c => c with
            {
                UsageCount = usageCounts.GetValueOrDefault(c.CauseId, 0)
            }).ToList();

            return causesWithUsage;
        }

        public async Task<CauseDto> CreateCauseAsync(CreateCauseDto dto)
        {
            _logger.LogInformation("Creating new cause for issue {IssId}", dto.IssId);

            var validationResult = await _createValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            var issueExists = await _db.Issues
                .AnyAsync(i => i.IssId == dto.IssId && i.DeletedAt == null);

            if (!issueExists)
            {
                throw new NotFoundException("Vấn đề", dto.IssId);
            }

            var nameExists = await _db.Causes
                .Where(c => c.IssId == dto.IssId && c.Name == dto.Name && c.DeletedAt == null)
                .AnyAsync();

            if (nameExists)
            {
                _logger.LogWarning("Cause with name {Name} already exists for issue {IssId}",
                    dto.Name, dto.IssId);
                throw new ConflictException($"Nguyên nhân '{dto.Name}' đã tồn tại cho vấn đề này");
            }

            var newCause = _mapper.MapToCause(dto);

            await _db.Causes.AddAsync(newCause);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Successfully created cause {CauseId}", newCause.CauseId);

            return await GetCauseByIdAsync(newCause.CauseId);
        }

        public async Task<CauseDto> UpdateCauseAsync(long causeId, UpdateCauseDto dto)
        {
            _logger.LogInformation("Updating cause {CauseId}", causeId);

            var validationResult = await _updateValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            var cause = await _db.Causes
                .Include(c => c.Issues)
                .FirstOrDefaultAsync(c => c.CauseId == causeId);

            if (cause is null || cause.DeletedAt != null)
            {
                _logger.LogWarning("Cause {CauseId} not found", causeId);
                throw new NotFoundException("Nguyên nhân", causeId);
            }

            bool hasChanges = false;

            if (dto.Name != null && cause.Name != dto.Name)
            {
                var nameExists = await _db.Causes
                    .Where(c => c.IssId == cause.IssId && 
                               c.Name == dto.Name && 
                               c.CauseId != causeId && 
                               c.DeletedAt == null)
                    .AnyAsync();

                if (nameExists)
                {
                    throw new ConflictException($"Nguyên nhân '{dto.Name}' đã tồn tại cho vấn đề này");
                }

                cause.Name = dto.Name;
                hasChanges = true;
            }

            if (dto.Description != null && cause.Description != dto.Description)
            {
                cause.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description;
                hasChanges = true;
            }

            if (hasChanges)
            {
                await _db.SaveChangesAsync();
                _logger.LogInformation("Successfully updated cause {CauseId}", causeId);
            }
            else
            {
                _logger.LogInformation("No changes detected for cause {CauseId}", causeId);
            }

            return await GetCauseByIdAsync(causeId);
        }

        /// <summary>
        /// Delete single cause
        /// Pattern: RESTful single resource delete (returns void, throws on error)
        /// Reference: Microsoft REST API Guidelines - DELETE returns 204 No Content
        /// Business Rules: Cannot delete if cause is referenced in IssueLogs
        /// </summary>
        public async Task DeleteCauseAsync(long causeId, bool softDelete = true)
        {
            _logger.LogInformation("Deleting cause {CauseId} | SoftDelete: {SoftDelete}",
                causeId, softDelete);

            var cause = await _db.Causes
                .Include(c => c.Issues)
                .FirstOrDefaultAsync(c => c.CauseId == causeId && c.DeletedAt == null);

            if (cause == null)
            {
                _logger.LogWarning("Cause {CauseId} not found", causeId);
                throw new NotFoundException("Nguyên nhân", causeId);
            }

            // ✅ Business rule: Check if cause is referenced in issue logs
            var isUsedInLogs = await _db.IssueLogs
                .AnyAsync(il => il.CauseId == causeId && il.DeletedAt == null);

            if (isUsedInLogs)
            {
                var usageCount = await _db.IssueLogs
                    .CountAsync(il => il.CauseId == causeId && il.DeletedAt == null);

                _logger.LogWarning("Cause {CauseId}:{Name} in use by {Count} issue logs",
                    cause.CauseId, cause.Name, usageCount);

                throw new BusinessRuleException(
                    $"Không thể xóa nguyên nhân '{cause.Name}' vì đang được sử dụng trong {usageCount} nhật ký sự cố",
                    "CAUSE_IN_USE");
            }

            // ✅ Perform delete
            if (softDelete)
            {
                cause.DeletedAt = DateTime.UtcNow;
                _db.Causes.Update(cause);

                await _db.SaveChangesAsync();

                _logger.LogInformation("Soft deleted cause {CauseId}:{Name}",
                    cause.CauseId, cause.Name);
            }
            else
            {
                // Hard delete with transaction
                using var transaction = await _db.Database.BeginTransactionAsync();

                try
                {
                    _db.Causes.Remove(cause);

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Hard deleted cause {CauseId}:{Name}",
                        cause.CauseId, cause.Name);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        /// <summary>
        /// Delete multiple causes with all-or-nothing transaction
        /// Pattern: Microsoft Dynamics 365 bulk operations
        /// Strategy: Validate ALL → Delete ALL → Return summary
        /// Reference: https://learn.microsoft.com/en-us/power-apps/developer/data-platform/bulk-operations
        /// </summary>
        public async Task<BulkDeleteResultDto> DeleteCausesAsync(List<long> causeIds, bool softDelete = true)
        {
            _logger.LogInformation("Batch delete started | Count: {Count} | SoftDelete: {SoftDelete}",
                causeIds?.Count ?? 0, softDelete);

            // ✅ Input validation
            ArgumentNullException.ThrowIfNull(causeIds);

            if (causeIds.Count == 0)
            {
                throw new Shared.Exceptions.ValidationException("causeIds",
                    "Vui lòng chọn ít nhất một nguyên nhân để xóa");
            }

            // ✅ Remove duplicates
            var uniqueIds = causeIds.Distinct().ToList();

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // ✅ Step 1: Validate ALL items BEFORE any deletion
                var existing = await _db.Causes
                    .Include(c => c.Issues)
                    .Where(c => uniqueIds.Contains(c.CauseId) && c.DeletedAt == null)
                    .ToListAsync();

                var notFoundIds = uniqueIds.Except(existing.Select(c => c.CauseId)).ToList();
                if (notFoundIds.Any())
                {
                    _logger.LogWarning("Causes not found: {Ids}", string.Join(", ", notFoundIds));
                    throw new NotFoundException($"Nguyên nhân không tồn tại: {string.Join(", ", notFoundIds)}");
                }

                // ✅ Step 2: Check business rules for ALL items
                var causesInLogs = await _db.IssueLogs
                    .Where(il => uniqueIds.Contains(il.CauseId!.Value) && il.DeletedAt == null)
                    .Select(il => il.CauseId!.Value)
                    .Distinct()
                    .ToListAsync();

                if (causesInLogs.Any())
                {
                    var usedCauses = existing
                        .Where(c => causesInLogs.Contains(c.CauseId))
                        .ToList();

                    var errorDetails = new List<string>();
                    foreach (var cause in usedCauses)
                    {
                        var count = await _db.IssueLogs
                            .CountAsync(il => il.CauseId == cause.CauseId && il.DeletedAt == null);
                        errorDetails.Add($"{cause.Name} ({count} nhật ký)");
                    }

                    _logger.LogWarning("Causes in use: {Causes}",
                        string.Join(", ", usedCauses.Select(c => $"{c.CauseId}:{c.Name}")));

                    throw new BusinessRuleException(
                        $"Không thể xóa nguyên nhân {string.Join(", ", errorDetails)} vì đang được sử dụng trong nhật ký",
                        "CAUSE_IN_USE");
                }

                // ✅ Step 3: All checks passed → Delete ALL
                foreach (var cause in existing)
                {
                    if (softDelete)
                        cause.DeletedAt = DateTime.UtcNow;
                    else
                        _db.Causes.Remove(cause);

                    _logger.LogInformation("Deleted cause {CauseId}:{Name}", cause.CauseId, cause.Name);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Batch delete SUCCESS | Count: {Count}", existing.Count);

                return new BulkDeleteResultDto
                {
                    Success = true,
                    DeletedCount = existing.Count,
                    TotalRequested = causeIds.Count,
                    Message = $"Đã xóa {existing.Count} nguyên nhân thành công"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<CauseSuggestionDto>> GetCauseSuggestionsAsync(long issId, string? search = null)
        {
            _logger.LogInformation("Fetching cause suggestions for issue {IssId}, search: {Search}",
                issId, search);

            var query = _db.Causes
                .Where(c => c.IssId == issId && c.DeletedAt == null)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Name.Contains(search));
            }

            var causes = await query
                .OrderBy(c => c.Name)
                .Take(20)
                .Select(c => new
                {
                    c.CauseId,
                    c.IssId,
                    c.Name,
                    c.Description
                })
                .ToListAsync();

            var causeIds = causes.Select(c => c.CauseId).ToList();
            
            var usageCounts = await _db.IssueLogs
                .Where(il => causeIds.Contains(il.CauseId!.Value) && il.DeletedAt == null)
                .GroupBy(il => il.CauseId!.Value)
                .Select(g => new { CauseId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CauseId, x => x.Count);

            var suggestions = causes
                .Select(c => new CauseSuggestionDto
                {
                    CauseId = c.CauseId,
                    IssId = c.IssId,
                    Name = c.Name,
                    Description = c.Description,
                    UsageCount = usageCounts.GetValueOrDefault(c.CauseId, 0)
                })
                .OrderByDescending(s => s.UsageCount)
                .ThenBy(s => s.Name)
                .Take(10)
                .ToList();

            _logger.LogInformation("Retrieved {Count} cause suggestions", suggestions.Count);

            return suggestions;
        }
    }
}