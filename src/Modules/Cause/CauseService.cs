using FluentValidation;
using ItSupportServer.Data.Models;
using ItSupportServer.src.Shared.Base;
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
                    c.IssueName.Contains(parameters.Search));
            }

            var result = await query.ToPaginatedResultAsync(parameters, defaultSortField: "Name");

            // ✅ Batch calculate usage counts (1 query instead of N)
            var causeIds = result.Items.Select(c => c.CauseId).ToList();

            var usageCounts = await _db.IssueLogs
                .Where(il => causeIds.Contains(il.CauseId!.Value) && il.DeletedAt == null)
                .GroupBy(il => il.CauseId!.Value)
                .Select(g => new { CauseId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CauseId, x => x.Count);

            // ✅ FIX: Create new instance (PaginatedResult is class, not record)
            var itemsWithUsage = result.Items.Select(c => c with
            {
                UsageCount = usageCounts.GetValueOrDefault(c.CauseId, 0)
            }).ToList();

            // ✅ FIX: Create new PaginatedResult instead of using 'with'
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

            // ✅ Calculate usage count (single query)
            var usageCount = await _db.IssueLogs
                .CountAsync(il => il.CauseId == causeId && il.DeletedAt == null);

            return cause with { UsageCount = usageCount };
        }

        public async Task<List<CauseDto>> GetCausesByIssueIdAsync(long issId)
        {
            _logger.LogInformation("Fetching causes for issue {IssId}", issId);

            // Verify issue exists
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

            // ✅ Batch calculate usage counts
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

            // Verify issue exists
            var issueExists = await _db.Issues
                .AnyAsync(i => i.IssId == dto.IssId && i.DeletedAt == null);

            if (!issueExists)
            {
                throw new NotFoundException("Vấn đề", dto.IssId);
            }

            // Check duplicate name for same issue
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
            // CauseId auto-increment
            // CreatedAt set by interceptor

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

            // Update Name
            if (dto.Name != null && cause.Name != dto.Name)
            {
                // Check duplicate for same issue
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

            // Update Description
            if (dto.Description != null && cause.Description != dto.Description)
            {
                cause.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description;
                hasChanges = true;
            }

            if (hasChanges)
            {
                // UpdatedAt set by interceptor
                await _db.SaveChangesAsync();
                _logger.LogInformation("Successfully updated cause {CauseId}", causeId);
            }
            else
            {
                _logger.LogInformation("No changes detected for cause {CauseId}", causeId);
            }

            return await GetCauseByIdAsync(causeId);
        }

        public async Task<bool> DeleteCausesAsync(List<long> causeIds, bool softDelete = true)
        {
            _logger.LogInformation("Deleting {Count} causes (soft: {SoftDelete})",
                causeIds?.Count ?? 0, softDelete);

            ArgumentNullException.ThrowIfNull(causeIds);

            if (causeIds.Count == 0)
            {
                throw new Shared.Exceptions.ValidationException("causeIds",
                    "Vui lòng chọn nguyên nhân để xóa");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var existing = await _db.Causes
                    .Include(c => c.Issues)
                    .Where(c => causeIds.Contains(c.CauseId) && c.DeletedAt == null)
                    .ToListAsync();

                if (existing.Count == 0)
                {
                    throw new NotFoundException("Không tìm thấy nguyên nhân để xóa");
                }

                // Check if causes are referenced in issue logs
                var causesInLogs = await _db.IssueLogs
                    .Where(il => causeIds.Contains(il.CauseId!.Value) && il.DeletedAt == null)
                    .Select(il => il.CauseId!.Value)
                    .Distinct()
                    .ToListAsync();

                if (causesInLogs.Any())
                {
                    var usedCauseNames = existing
                        .Where(c => causesInLogs.Contains(c.CauseId))
                        .Select(c => c.Name)
                        .ToList();

                    _logger.LogWarning("Cannot delete causes {Causes} - referenced in logs",
                        string.Join(", ", usedCauseNames));

                    throw new BusinessRuleException(
                        $"Không thể xóa nguyên nhân {string.Join(", ", usedCauseNames)} vì đang được sử dụng trong nhật ký");
                }

                if (softDelete)
                {
                    foreach (var item in existing)
                    {
                        item.DeletedAt = DateTime.UtcNow;
                    }
                    _db.Causes.UpdateRange(existing);
                }
                else
                {
                    _db.Causes.RemoveRange(existing);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully deleted {Count} causes", existing.Count);

                return true;
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

            // ✅ FIX: Cannot use nested COUNT in OrderBy with EF Core
            // Load data first, then calculate usage count
            var causes = await query
                .OrderBy(c => c.Name)  // Sort by name first
                .Take(20)  // Take more than needed for sorting
                .Select(c => new
                {
                    c.CauseId,
                    c.IssId,
                    c.Name,
                    c.Description
                })
                .ToListAsync();

            // ✅ Calculate usage counts in-memory
            var causeIds = causes.Select(c => c.CauseId).ToList();
            
            var usageCounts = await _db.IssueLogs
                .Where(il => causeIds.Contains(il.CauseId!.Value) && il.DeletedAt == null)
                .GroupBy(il => il.CauseId!.Value)
                .Select(g => new { CauseId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CauseId, x => x.Count);

            // ✅ Build suggestions with usage counts, then sort by usage
            var suggestions = causes
                .Select(c => new CauseSuggestionDto
                {
                    CauseId = c.CauseId,
                    IssId = c.IssId,
                    Name = c.Name,
                    Description = c.Description,
                    UsageCount = usageCounts.GetValueOrDefault(c.CauseId, 0)
                })
                .OrderByDescending(s => s.UsageCount)  // Most used first
                .ThenBy(s => s.Name)
                .Take(10)
                .ToList();

            _logger.LogInformation("Retrieved {Count} cause suggestions", suggestions.Count);

            return suggestions;
        }
    }
}