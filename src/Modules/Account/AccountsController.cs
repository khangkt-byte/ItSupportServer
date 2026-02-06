using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Shared.Attributes;
using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Dto;
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
        /// [ADMIN] Xóa tài khoản đơn lẻ (soft delete)
        /// </summary>
        /// <param name="id">Account ID</param>
        /// <returns>No content (204)</returns>
        /// <remarks>
        /// **Pattern:** RESTful single resource delete
        /// **Reference:** Microsoft REST API Guidelines
        /// 
        /// **Business rules (422):**
        /// - Không thể xóa tài khoản Super Admin
        /// - Không thể tự xóa tài khoản của chính mình
        /// - Không thể xóa tài khoản có dữ liệu quan trọng liên quan
        /// 
        /// **Not Found (404):**
        /// - Account không tồn tại
        /// 
        /// **Example:**
        /// ```
        /// DELETE /api/accounts/guid-here
        /// ```
        /// 
        /// **Response:** 204 No Content (theo chuẩn REST)
        /// </remarks>
        [HttpDelete("{id}")]
        [HasPermission(Permissions.AccountClaims.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAccount([FromRoute] Guid id)
        {
            await _service.DeleteAccountAsync(id);
            return NoContent();
        }

        /// <summary>
        /// [ADMIN] Xóa nhiều tài khoản (soft delete)
        /// </summary>
        /// <param name="accountIds">Danh sách account IDs cần xóa</param>
        /// <returns>Kết quả xóa hàng loạt</returns>
        /// <remarks>
        /// **Strategy:** All-or-nothing (transaction-based)
        /// - Nếu TẤT CẢ thành công → 200 OK với summary
        /// - Nếu BẤT KỲ lỗi nào → Rollback, throw error (4xx/5xx)
        /// 
        /// **Pattern:** Microsoft Dynamics 365 bulk operations
        /// **Reference:** 
        /// - https://learn.microsoft.com/power-apps/developer/data-platform/delete-data-bulk
        /// - https://learn.microsoft.com/azure/architecture/best-practices/api-design
        /// 
        /// **Business rules (422):**
        /// - Không thể xóa tài khoản Super Admin
        /// - Không thể tự xóa tài khoản của chính mình
        /// - Transaction rollback nếu ANY account fails validation
        /// 
        /// **Not Found (404):**
        /// - Nếu ANY account ID không tồn tại (strict validation)
        /// 
        /// **Example:**
        /// ```json
        /// ["guid-1", "guid-2", "guid-3"]
        /// ```
        /// 
        /// **Response:**
        /// ```json
        /// {
        ///   "success": true,
        ///   "deletedCount": 3,
        ///   "totalRequested": 3,
        ///   "message": "Đã xóa 3 tài khoản thành công"
        /// }
        /// ```
        /// </remarks>
        [HttpDelete]
        [HasPermission(Permissions.AccountClaims.Delete)]
        [ProducesResponseType(typeof(BulkDeleteResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BulkDeleteResultDto>> DeleteAccounts(
            [FromBody] List<Guid> accountIds)
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

        // ✅ ADD THIS ENDPOINT - MUST BE BEFORE {id} ROUTE
        /// <summary>
        /// [SELF] Lấy danh sách permissions của mình
        /// </summary>
        /// <returns>List of permission names</returns>
        /// <remarks>
        /// **Purpose:** Frontend uses this for permission-based UI rendering
        /// 
        /// **Pattern:** Permission-based UI optimization
        /// **Reference:** Auth0 RBAC, Azure AD App Roles
        /// 
        /// **Caching Strategy:**
        /// - Backend: Permissions query is fast (indexed joins)
        /// - Frontend: Should cache result for 5-10 minutes
        /// - Invalidate cache on: Login, Logout, Role change
        /// 
        /// **Security:**
        /// - Only returns permissions for authenticated user (cannot query others)
        /// - Does NOT bypass authorization checks (backend still validates)
        /// - Used for UX optimization (hiding unavailable features)
        /// 
        /// **Example Response:**
        /// ```json
        /// [
        ///   "Admin",
        ///   "Account.View",
        ///   "Account.Create",
        ///   "Account.Edit",
        ///   "Employee.View",
        ///   "IssueLog.View"
        /// ]
        /// ```
        /// 
        /// **Frontend Usage:**
        /// ```typescript
        /// const permissions = await authApi.getMyPermissions();
        /// const canCreateAccount = permissions.includes('Account.Create');
        /// 
        /// {canCreateAccount && <CreateAccountButton />}
        /// ```
        /// </remarks>
        [HttpGet("my-permissions")]
        [Authorize]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<string>>> GetMyPermissions()
        {
            var accountId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var permissions = await _service.GetPermissionsAsync(accountId);
            return Ok(permissions);
        }

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
