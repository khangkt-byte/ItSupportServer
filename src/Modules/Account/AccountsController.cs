using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Shared.Attributes;
using ItSupportServer.src.Shared.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ItSupportServer.src.Modules.Account
{
    /// <summary>
    /// API quản lý tài khoản (authentication & authorization)
    /// </summary>
    [ApiController]
    [Route("api/accounts")]
    [Produces("application/json")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _service;

        public AccountsController(IAccountService service)
        {
            _service = service;
        }

        // ==================== ADMIN OPERATIONS ====================

        /// <summary>
        /// [ADMIN] Lấy danh sách tài khoản (có phân trang)
        /// </summary>
        /// <param name="parameters">Query parameters (page, pageSize, sortBy, search)</param>
        /// <returns>Paginated list of accounts</returns>
        [HttpGet]
        [HasPermission(Permissions.AccountClaims.View)]
        [ProducesResponseType(typeof(PaginatedResult<ListAccountDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PaginatedResult<ListAccountDto>>> GetAccounts(
            [FromQuery] QueryParameters parameters)
        {
            var result = await _service.GetAccountsAsync(parameters);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Lấy chi tiết tài khoản (bao gồm roles)
        /// </summary>
        /// <param name="id">Account ID</param>
        /// <returns>Account details with roles</returns>
        [HttpGet("{id}")]
        [HasPermission(Permissions.AccountClaims.View)]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccountDto>> GetAccount([FromRoute] Guid id)
        {
            var result = await _service.GetAccountByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Tạo tài khoản cho nhân viên
        /// </summary>
        /// <param name="dto">Create account request</param>
        /// <returns>Created account with assigned roles</returns>
        /// <remarks>
        /// **Lưu ý:**
        /// - Nhân viên phải tồn tại và chưa có tài khoản
        /// - Username phải unique
        /// - Mật khẩu sẽ được hash tự động
        /// </remarks>
        [HttpPost]
        [HasPermission(Permissions.AccountClaims.Create)]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AccountDto>> CreateAccount([FromBody] CreateAccountDto dto)
        {
            var result = await _service.CreateAccountAsync(dto);
            return CreatedAtAction(nameof(GetAccount), new { id = result.AccountId }, result);
        }

        /// <summary>
        /// [ADMIN] Cập nhật tài khoản (username, lock status)
        /// </summary>
        /// <param name="id">Account ID</param>
        /// <param name="dto">Update account request</param>
        /// <returns>Updated account</returns>
        [HttpPut("{id}")]
        [HasPermission(Permissions.AccountClaims.Edit)]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AccountDto>> UpdateAccount(
            [FromRoute] Guid id,
            [FromBody] UpdateAccountDto dto)
        {
            var result = await _service.UpdateAccountAsync(id, dto);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Xóa nhiều tài khoản (soft delete)
        /// </summary>
        /// <param name="accountIds">List of account IDs to delete</param>
        /// <returns>Success status</returns>
        /// <remarks>
        /// **Lưu ý:** Không thể xóa tài khoản Super Admin
        /// </remarks>
        [HttpDelete]
        [HasPermission(Permissions.AccountClaims.Delete)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<bool>> DeleteAccounts([FromBody] List<Guid> accountIds)
        {
            var result = await _service.DeleteAccountsAsync(accountIds);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Reset mật khẩu cho tài khoản
        /// </summary>
        /// <param name="id">Account ID</param>
        /// <returns>Temporary password (phải gửi cho user ngay)</returns>
        /// <remarks>
        /// **QUAN TRỌNG:**
        /// - Mật khẩu tạm thời sẽ được tạo ngẫu nhiên
        /// - Admin phải gửi mật khẩu cho nhân viên ngay
        /// - Nhân viên nên đổi mật khẩu sau khi đăng nhập
        /// </remarks>
        [HttpPost("{id}/reset-password")]
        [HasPermission(Permissions.AccountClaims.ResetPassword)]
        [ProducesResponseType(typeof(ResetPasswordResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResetPasswordResultDto>> ResetPassword([FromRoute] Guid id)
        {
            var result = await _service.ResetPasswordAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Khóa tài khoản
        /// </summary>
        /// <param name="id">Account ID</param>
        /// <returns>Account with locked status</returns>
        /// <remarks>
        /// Tài khoản sẽ bị khóa trong 30 phút (hoặc vô thời hạn nếu admin set)
        /// </remarks>
        [HttpPost("{id}/lock")]
        [HasPermission(Permissions.AccountClaims.Lock)]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccountDto>> LockAccount([FromRoute] Guid id)
        {
            var result = await _service.LockAccountAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Mở khóa tài khoản
        /// </summary>
        /// <param name="id">Account ID</param>
        /// <returns>Account with unlocked status</returns>
        /// <remarks>
        /// Reset số lần đăng nhập thất bại về 0
        /// </remarks>
        [HttpPost("{id}/unlock")]
        [HasPermission(Permissions.AccountClaims.Lock)]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccountDto>> UnlockAccount([FromRoute] Guid id)
        {
            var result = await _service.UnlockAccountAsync(id);
            return Ok(result);
        }

        // ==================== SELF-SERVICE OPERATIONS ====================

        /// <summary>
        /// [SELF] Đổi mật khẩu của chính mình
        /// </summary>
        /// <param name="dto">Change password request</param>
        /// <returns>Success message</returns>
        /// <remarks>
        /// **Yêu cầu:**
        /// - Phải nhập đúng mật khẩu hiện tại
        /// - Mật khẩu mới phải khác mật khẩu cũ
        /// - Mật khẩu xác nhận phải khớp với mật khẩu mới
        /// </remarks>
        [HttpPost("me/change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> ChangeMyPassword([FromBody] ChangePasswordDto dto)
        {
            var accountId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _service.ChangePasswordAsync(accountId, dto);
            return Ok(new { message = "Mật khẩu đã được thay đổi thành công" });
        }

        /// <summary>
        /// [SELF] Xem lịch sử đăng nhập của mình
        /// </summary>
        /// <returns>Login history entries</returns>
        /// <remarks>
        /// **Thông tin bao gồm:**
        /// - Thời gian đăng nhập
        /// - Địa chỉ IP
        /// - Trình duyệt/thiết bị
        /// - Trạng thái (thành công/thất bại)
        /// </remarks>
        [HttpGet("me/login-history")]
        [Authorize]
        [ProducesResponseType(typeof(List<LoginHistoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<LoginHistoryDto>>> GetMyLoginHistory()
        {
            var accountId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _service.GetLoginHistoryAsync(accountId);
            return Ok(result);
        }
    }
}
