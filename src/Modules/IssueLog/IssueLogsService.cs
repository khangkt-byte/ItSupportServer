using ItSupportServer.Data;
using ItSupportServer.src.Shared.Base;
using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.src.Modules.IssueLog
{
    public class IssueLogsService(AppDbContext db) : IIssueLogsService
    {
        public async Task<BaseResult<PaginatedResult<List<IssueLogDto>>>> GetAllIssueLogsAsync()
        {
			try
			{
				var issueLogs = db.IssueLogs
								   .Where(il => il.DeletedAt == null)
								   .Select(il => new IssueLogDto
								   {
									   IssLogId = il.IssLogId,
									   OperatorId = il.OperatorId,
									   RequesterId = il.RequesterId,
									   DptId = il.DptId,
									   AreaId = il.AreaId,
									   IssueDescription = il.IssueDescription,
									   Cause = il.Cause,
									   Resolution = il.Resolution,
									   PermanentFix = il.PermanentFix,
									   Notes = il.Notes,
									   DateReported = il.DateReported,
									   Status = il.Status
								   })
								   .AsNoTracking();

				//if (!issueLogs.Any())
				//{
				//	issueLogs = issueLogs.Where(il => IssueLogs);
    //            }

				var result = await Pagination<IssueLogDto>.PaginationAsync(issueLogs, 1, int.MaxValue, null);
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
										  OperatorId = il.OperatorId,
										  RequesterId = il.RequesterId,
										  DptId = il.DptId,
										  AreaId = il.AreaId,
										  IssueDescription = il.IssueDescription,
										  Cause = il.Cause,
										  Resolution = il.Resolution,
										  PermanentFix = il.PermanentFix,
										  Notes = il.Notes,
										  DateReported = il.DateReported,
										  Status = il.Status
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

		public async Task<BaseResult<IssueLogDto>> CreateIssueLogAsync(IssueLogsCreateDto createDto)
		{
			try
			{
				throw new NotImplementedException();
            }
			catch (Exception ex)
			{
				return BaseResult<IssueLogDto>.Fail($"Lỗi hệ thống: {ex.Message}", 500);
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

		public async Task<BaseResult<bool>> DeleteIssuesLogAsync(List<Guid> issLogIds, bool softDelete = true)
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
