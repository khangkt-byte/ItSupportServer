using FluentValidation;
using ItSupportServer.Data.Models;
using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Exceptions;
using ItSupportServer.src.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ItSupportServer.src.Modules.IssueLog
{
    public class IssueLogService : IIssueLogService
    {
        private readonly AppDbContext _db;
        private readonly IssueLogMapper _mapper;  // ✅ Use mapper
        private readonly ILogger<IssueLogService> _logger;
        private readonly IValidator<CreateIssueLogDto> _createValidator;
        private readonly IValidator<UpdateIssueLogDto> _updateValidator;

        public IssueLogService(
            AppDbContext db,
            IssueLogMapper mapper,  // ✅ Inject mapper
            ILogger<IssueLogService> logger,
            IValidator<CreateIssueLogDto> createValidator,
            IValidator<UpdateIssueLogDto> updateValidator)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<PaginatedResult<List<IssueLogDto>>> GetIssueLogsAsync(
            string? query, int page, int pageSize, SortOBJ? sort)
        {
            _logger.LogInformation("Fetching issue logs with query: {Query}, page: {Page}", query, page);

            // ✅ Use mapper projection
            var issueLogsQuery = _mapper.ProjectToIssueLogDto(_db.IssueLogs
                .Where(il => il.DeletedAt == null)
                .AsNoTracking());

            if (!string.IsNullOrWhiteSpace(query))
            {
                issueLogsQuery = issueLogsQuery.Where(il =>
                    il.Operator.Contains(query) ||
                    (il.Requester != null && il.Requester.Contains(query)) ||
                    il.Department.Contains(query) ||
                    il.Area.Contains(query) ||
                    (il.IssueDescription != null && il.IssueDescription.Contains(query)) ||
                    (il.Resolution != null && il.Resolution.Contains(query)));
            }

            var result = await Pagination<IssueLogDto>.ToPaginatedResultAsync(issueLogsQuery, page, pageSize, sort);
            
            _logger.LogInformation("Retrieved {Count} issue logs", result.TotalItems);
            
            return result;
        }

        public async Task<IssueLogDto> GetIssueLogByIdAsync(Guid issLogId)
        {
            _logger.LogInformation("Fetching issue log {IssLogId}", issLogId);

            var issLog = await _mapper.ProjectToIssueLogDto(_db.IssueLogs
                .Where(il => il.IssLogId == issLogId && il.DeletedAt == null)
                .AsNoTracking())
                .FirstOrDefaultAsync();

            if (issLog is null)
            {
                _logger.LogWarning("Issue log {IssLogId} not found", issLogId);
                throw new NotFoundException("Nhật ký sự cố", issLogId);
            }

            return issLog;
        }

        public async Task<IssueLogDto> CreateIssueLogAsync(CreateIssueLogDto dto)
        {
            _logger.LogInformation("Creating new issue log");

            var validationResult = await _createValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            using var transaction = await _db.Database.BeginTransactionAsync();
            
            try
            {
                // ✅ Use mapper instead of manual mapping
                var newIssueLog = _mapper.MapToIssueLog(dto);
                newIssueLog.IssLogId = Guid.CreateVersion7();
                // CreatedAt set automatically by interceptor

                await _db.IssueLogs.AddAsync(newIssueLog);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully created issue log {IssLogId}", newIssueLog.IssLogId);

                return _mapper.MapToIssueLogDto(newIssueLog);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IssueLogDto> UpdateIssueLogAsync(Guid issLogId, UpdateIssueLogDto dto)
        {
            _logger.LogInformation("Updating issue log {IssLogId}", issLogId);

            var validationResult = await _updateValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            var issueLog = await _db.IssueLogs.FindAsync(issLogId);

            if (issueLog is null || issueLog.DeletedAt != null)
            {
                _logger.LogWarning("Issue log {IssLogId} not found", issLogId);
                throw new NotFoundException("Nhật ký sự cố", issLogId);
            }

            // ✅ Partial update logic
            bool hasChanges = false;

            if (dto.Operator != null && issueLog.Operator != dto.Operator)
            {
                issueLog.Operator = dto.Operator;
                hasChanges = true;
            }

            if (dto.Requester != null && issueLog.Requester != dto.Requester)
            {
                issueLog.Requester = dto.Requester;
                hasChanges = true;
            }

            if (dto.Department != null && issueLog.Department != dto.Department)
            {
                issueLog.Department = dto.Department;
                hasChanges = true;
            }

            if (dto.Area != null && issueLog.Area != dto.Area)
            {
                issueLog.Area = dto.Area;
                hasChanges = true;
            }

            if (dto.IssueDescription != null && issueLog.IssueDescription != dto.IssueDescription)
            {
                issueLog.IssueDescription = dto.IssueDescription;
                hasChanges = true;
            }

            if (dto.Cause != null && issueLog.Cause != dto.Cause)
            {
                issueLog.Cause = string.IsNullOrWhiteSpace(dto.Cause) ? null : dto.Cause;
                hasChanges = true;
            }

            if (dto.Resolution != null && issueLog.Resolution != dto.Resolution)
            {
                issueLog.Resolution = string.IsNullOrWhiteSpace(dto.Resolution) ? null : dto.Resolution;
                hasChanges = true;
            }

            if (dto.PermanentFix != null && issueLog.PermanentFix != dto.PermanentFix)
            {
                issueLog.PermanentFix = string.IsNullOrWhiteSpace(dto.PermanentFix) ? null : dto.PermanentFix;
                hasChanges = true;
            }

            if (dto.Notes != null && issueLog.Notes != dto.Notes)
            {
                issueLog.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes;
                hasChanges = true;
            }

            if (dto.DateReported.HasValue && issueLog.DateReported != dto.DateReported.Value)
            {
                issueLog.DateReported = dto.DateReported.Value;
                hasChanges = true;
            }

            if (dto.Status != null && issueLog.Status != dto.Status)
            {
                issueLog.Status = string.IsNullOrWhiteSpace(dto.Status) ? null : dto.Status;
                hasChanges = true;
            }

            if (hasChanges)
            {
                // UpdatedAt set automatically by interceptor
                await _db.SaveChangesAsync();
                _logger.LogInformation("Successfully updated issue log {IssLogId}", issLogId);
            }
            else
            {
                _logger.LogInformation("No changes detected for issue log {IssLogId}", issLogId);
            }

            return _mapper.MapToIssueLogDto(issueLog);
        }

        public async Task<bool> DeleteIssueLogsAsync(List<Guid> issLogIds, bool softDelete = true)
        {
            _logger.LogInformation("Deleting {Count} issue logs (soft: {SoftDelete})", 
                issLogIds?.Count ?? 0, softDelete);

            ArgumentNullException.ThrowIfNull(issLogIds);

            if (issLogIds.Count == 0)
            {
                throw new Shared.Exceptions.ValidationException("issLogIds", 
                    "Vui lòng chọn nhật ký sự cố để xóa");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            
            try
            {
                var existing = await _db.IssueLogs
                    .Where(il => issLogIds.Contains(il.IssLogId) && il.DeletedAt == null)
                    .ToListAsync();

                if (existing.Count == 0)
                {
                    throw new NotFoundException("Không tìm thấy nhật ký sự cố để xóa");
                }

                if (softDelete)
                {
                    foreach (var item in existing)
                    {
                        item.DeletedAt = DateTime.UtcNow;
                    }
                    _db.IssueLogs.UpdateRange(existing);
                }
                else
                {
                    _db.IssueLogs.RemoveRange(existing);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully deleted {Count} issue logs", existing.Count);

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
