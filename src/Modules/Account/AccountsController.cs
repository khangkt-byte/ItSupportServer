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
    /// Pattern: RESTful API, RBAC
    /// Security: JWT + Permission-based authorization
    /// Reference: Microsoft Identity Platform, Auth0 Management API
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
        /// <remarks>
        /// **Supports:**
        /// - Pagination: page, pageSize
        /// - Sorting: sortBy, sortDirection
        /// - Search: username, employee name, email
        /// 
        /// **Example:**
        /// ```
        /// GET /api/accounts?page=1&amp;pageSize=20&amp;search=admin&amp;sortBy=CreatedAt
        /// ```
        /// </remarks>
        [HttpGet]
        [HasPermission(Permissions.AccountClaims.View)]
        [ProducesResponseType(typeof(PaginatedResult<ListAccountDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResult<ListAccountDto>>> GetAccounts(
            [FromQuery] QueryParameters parameters)
        {
            var result = await _service.GetAccountsAsync(parameters);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Lấy chi tiết tài khoản (bao gồm roles)
        /// </summary>
        /// <param name="id">Account ID (GUID)</param>
        /// <returns>Account details with roles and employee info</returns>
        [HttpGet("{id}")]
        [HasPermission(Permissions.AccountClaims.View)]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
        /// **Validation (400):**
        /// - Username: 3-32 chars, alphanumeric + underscore
        /// - Password: 8-128 chars, complexity requirements
        /// - EmpId: required
        /// 
        /// **Not Found (404):**
        /// - Employee ID không tồn tại
        /// - Role IDs (nếu có) không tồn tại
        /// 
        /// **Conflict (409):**
        /// - Username đã tồn tại
        /// - Employee đã có tài khoản
        /// 
        /// **Example:**
        /// ```json
        /// {
        ///   "empId": "guid-here",
        ///   "username": "admin",
        ///   "password": "Admin@123",
        ///   "roleIds": [1, 2]
        /// }
        /// ```
        /// </remarks>
        [HttpPost]
        [HasPermission(Permissions.AccountClaims.Create)]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
        /// <remarks>
        /// **Partial update:** Chỉ các fields có giá trị sẽ được update
        /// 
        /// **Validation (400):**
        /// - Username format (nếu update)
        /// 
        /// **Not Found (404):**
        /// - Account không tồn tại
        /// 
        /// **Conflict (409):**
        /// - Username mới đã tồn tại (nếu update username)
        /// </remarks>
        [HttpPut("{id}")]
        [HasPermission(Permissions.AccountClaims.Edit)]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
        /// **Business rules (422):**
        /// - Không thể xóa tài khoản Super Admin
        /// - Không thể tự xóa tài khoản của chính mình
        /// 
        /// **Not Found (404):**
        /// - Nếu ANY account ID không tồn tại (transaction rollback)
        /// 
        /// **Example:**
        /// ```json
        /// ["guid-1", "guid-2", "guid-3"]
        /// ```
        /// </remarks>
        [HttpDelete]
        [HasPermission(Permissions.AccountClaims.Delete)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
        /// - Mật khẩu tạm thời sẽ được tạo ngẫu nhiên (12 chars, complexity compliant)
        /// - Admin phải gửi mật khẩu cho nhân viên ngay (qua email hoặc tin nhắn)
        /// - Nhân viên nên đổi mật khẩu sau khi đăng nhập
        /// - Response chỉ trả về 1 lần, không lưu plain text
        /// 
        /// **Not Found (404):**
        /// - Account không tồn tại
        /// 
        /// **Business Rules (422):**
        /// - Không thể reset mật khẩu Super Admin (security)
        /// - Không thể reset mật khẩu chính mình (dùng change-password)
        /// 
        /// **Example response:**
        /// ```json
        /// {
        ///   "accountId": "guid-here",
        ///   "temporaryPassword": "Abc@1234Xyz"
        /// }
        /// ```
        /// </remarks>
        [HttpPost("{id}/reset-password")]
        [HasPermission(Permissions.AccountClaims.ResetPassword)]
        [ProducesResponseType(typeof(ResetPasswordResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
        /// **Behavior:**
        /// - Tài khoản sẽ bị khóa vô thời hạn (LockedUntil = null)
        /// - Hoặc khóa trong 30 phút (tùy logic)
        /// - Revoke tất cả refresh tokens hiện tại
        /// 
        /// **Business Rules (422):**
        /// - Không thể khóa tài khoản Super Admin
        /// - Không thể tự khóa chính mình
        /// - Không thể khóa tài khoản đã bị xóa
        /// 
        /// **Example:**
        /// ```
        /// POST /api/accounts/guid-here/lock
        /// ```
        /// </remarks>
        [HttpPost("{id}/lock")]
        [HasPermission(Permissions.AccountClaims.Lock)]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
        /// **Behavior:**
        /// - Set IsLocked = false
        /// - Set LockedUntil = null
        /// - Reset FailedLoginAttempts = 0
        /// 
        /// **Example:**
        /// ```
        /// POST /api/accounts/guid-here/unlock
        /// ```
        /// </remarks>
        [HttpPost("{id}/unlock")]
        [HasPermission(Permissions.AccountClaims.Lock)]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
        /// **Validation (400):**
        /// - CurrentPassword, NewPassword, ConfirmPassword: required
        /// - NewPassword: 8-128 chars, complexity requirements
        /// - ConfirmPassword must match NewPassword
        /// 
        /// **Unauthorized (401):**
        /// - Mật khẩu hiện tại không đúng
        /// - Token expired/invalid
        /// 
        /// **Business Rules (422):**
        /// - Mật khẩu mới phải khác mật khẩu cũ
        /// - Không thể dùng mật khẩu đã sử dụng gần đây (nếu có history)
        /// 
        /// **Example:**
        /// ```json
        /// {
        ///   "currentPassword": "OldPass@123",
        ///   "newPassword": "NewPass@456",
        ///   "confirmPassword": "NewPass@456"
        /// }
        /// ```
        /// </remarks>
        [HttpPost("me/change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
        /// - User Agent (trình duyệt/thiết bị)
        /// - Trạng thái (thành công/thất bại)
        /// - Location (nếu có IP geolocation)
        /// 
        /// **Unauthorized (401):**
        /// - Token invalid/expired
        /// 
        /// **Not Found (404):**
        /// - Account đã bị xóa (edge case)
        /// 
        /// **Example response:**
        /// ```json
        /// [
        ///   {
        ///     "timestamp": "2025-02-03T10:30:00Z",
        ///     "ipAddress": "192.168.1.100",
        ///     "userAgent": "Chrome/120.0",
        ///     "success": true
        ///   }
        /// ]
        /// ```
        /// </remarks>
        [HttpGet("me/login-history")]
        [Authorize]
        [ProducesResponseType(typeof(List<LoginHistoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<LoginHistoryDto>>> GetMyLoginHistory()
        {
            var accountId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _service.GetLoginHistoryAsync(accountId);
            return Ok(result);
        }
    }
}
