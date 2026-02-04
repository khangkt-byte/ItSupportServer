using ItSupportServer.Data;
using ItSupportServer.Data.Models;
using ItSupportServer.src.Shared.Base;
using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.src.Modules.IssueLog
{
    public class IssueLogsService(AppDbContext db) : IIssueLogsService
    {
        public async Task<BaseResult<PaginatedResult<List<IssueLogDto>>>> GetIssueLogsAsync(string? query, int page, int pageSize, SortOBJ? sort)
        {
            try
            {
                var issueLogs = db.IssueLogs
                                   .Where(il => il.DeletedAt == null)
                                   .Select(il => new IssueLogDto
                                   {
                                       IssLogId = il.IssLogId,
                                       Requester = il.Requester,
                                       DptId = il.DptId,
                                       AreaId = il.AreaId,
                                       IssueDescription = il.IssueDescription,
                                       Cause = il.Cause,
                                       Resolution = il.Resolution,
                                       PermanentFix = il.PermanentFix,
                                       Notes = il.Notes,
                                       DateReported = il.DateReported,
                                       Status = il.Status,

                                       OperatorNames = il.Operators
                                                        .Select(ile => ile.Employee.FullName)
                                                        .ToList()
                                   })
                                   .AsNoTracking();

                //if (!issueLogs.Any())
                //{
                //	issueLogs = issueLogs.Where(il => IssueLogs);
                //}

                var result = await Pagination<IssueLogDto>.PaginationAsync(issueLogs, page, pageSize, sort);
                return BaseResult<PaginatedResult<List<IssueLogDto>>>.Ok(result);
            }
            catch (Exception ex)
            {
                return BaseResult<PaginatedResult<List<IssueLogDto>>>.Fail($"Lỗi hệ thống: {ex.Message}", 500);
            }
        }

        public async Task<BaseResult<IssueLogDto>> GetIssueLogByIdAsync(Guid issLogId)
        {
            try
            {
                var issLog = await db.IssueLogs
                                      .Where(il => il.IssLogId == issLogId && il.DeletedAt == null)
                                      .Select(il => new IssueLogDto
                                      {
                                          IssLogId = il.IssLogId,
                                          Requester = il.Requester,
                                          DptId = il.DptId,
                                          AreaId = il.AreaId,
                                          IssueDescription = il.IssueDescription,
                                          Cause = il.Cause,
                                          Resolution = il.Resolution,
                                          PermanentFix = il.PermanentFix,
                                          Notes = il.Notes,
                                          DateReported = il.DateReported,
                                          Status = il.Status,

                                          OperatorNames = il.Operators
                                                            .Select(ile => ile.Employee.FullName)
                                                            .ToList()
                                      })
                                      .AsNoTracking()
                                      .FirstOrDefaultAsync();

                if (issLog is null)
                    return BaseResult<IssueLogDto>.Fail("Không tìm thấy bản ghi log sự cố.", 404);

                return BaseResult<IssueLogDto>.Ok(issLog);
            }
            catch (Exception ex)
            {
                return BaseResult<IssueLogDto>.Fail($"Lỗi hệ thống: {ex.Message}", 500);
            }
        }

        public async Task<BaseResult<IssueLogsCreateDto>> CreateIssueLogAsync(IssueLogsCreateDto createDto)
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var newIssueLogs = new IssueLogs
                {
                    IssLogId = Guid.CreateVersion7(),
                    Requester = createDto.Requester,
                    DptId = createDto.DptId,
                    AreaId = createDto.AreaId,
                    IssueDescription = createDto.IssueDescription,
                    Cause = createDto.Cause,
                    Resolution = createDto.Resolution,
                    PermanentFix = createDto.PermanentFix,
                    Notes = createDto.Notes,
                    DateReported = createDto.DateReported,
                    Status = createDto.Status,
                    CreatedAt = DateTime.UtcNow
                };

                var newOperators = createDto.OperatorId.Select(opId => new IssueLogEmployees
                {
                    IssLogId = newIssueLogs.IssLogId,
                    EmpId = opId
                });

                await db.IssueLogs.AddAsync(newIssueLogs);
                await db.IssueLogEmployees.AddRangeAsync(newOperators);
                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return BaseResult<IssueLogsCreateDto>.Ok(createDto);
            }
            catch (Exception ex)
            {
                return BaseResult<IssueLogsCreateDto>.Fail($"Lỗi hệ thống: {ex.Message}", 500);
            }
        }

        public async Task<BaseResult<IssueLogsUpdateDto>> UpdateIssueLogAsync(Guid issLogId, IssueLogsUpdateDto updateDto)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                return BaseResult<IssueLogsUpdateDto>.Fail($"Lỗi hệ thống: {ex.Message}", 500);
            }
        }

        public async Task<BaseResult<bool>> DeleteIssueLogsAsync(List<Guid> issLogIds, bool softDelete = true)
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                if (!issLogIds.Any())
                    return BaseResult<bool>.Fail("Vui lòng chọn log sự cố để xóa.", 400);

                var existing = await db.IssueLogs
                                       .Where(il => issLogIds.Contains(il.IssLogId) && il.DeletedAt == null)
                                       .ToListAsync();

                if (!existing.Any())
                    return BaseResult<bool>.Fail("Không tìm thấy log sự cố để xóa.", 404);

                if (softDelete)
                {
                    foreach (var item in existing)
                    {
                        item.DeletedAt = DateTime.UtcNow;
                    }

                    db.IssueLogs.UpdateRange(existing);
                }
                else
                {
                    db.IssueLogs.RemoveRange(existing);
                }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return BaseResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BaseResult<bool>.Fail($"Lỗi hệ thống: {ex.Message}", 500);
            }
        }

    }
}
