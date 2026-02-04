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
        /// [ADMIN] Lấy danh sách tài khoản
        /// </summary>
        [HttpGet]
        [HasPermission(Permissions.AccountClaims.View)]
        [ProducesResponseType(typeof(PaginatedResult<AccountDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<AccountDto>>> GetAccounts(
            [FromQuery] QueryParameters parameters)
        {
            var result = await _service.GetAccountsAsync(parameters);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Lấy chi tiết tài khoản
        /// </summary>
        [HttpGet("{id}")]
        [HasPermission(Permissions.AccountClaims.View)]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<AccountDto>> GetAccount(Guid id)
        {
            var result = await _service.GetAccountByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Tạo tài khoản cho nhân viên
        /// </summary>
        [HttpPost]
        [HasPermission(Permissions.AccountClaims.Create)]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<AccountDto>> CreateAccount([FromBody] CreateAccountDto dto)
        {
            var result = await _service.CreateAccountAsync(dto);
            return CreatedAtAction(nameof(GetAccount), new { id = result.AccountId }, result);
        }

        /// <summary>
        /// [ADMIN] Cập nhật tài khoản (username, status, etc.)
        /// </summary>
        [HttpPut("{id}")]
        [HasPermission(Permissions.AccountClaims.Edit)]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<AccountDto>> UpdateAccount(
            Guid id, [FromBody] UpdateAccountDto dto)
        {
            var result = await _service.UpdateAccountAsync(id, dto);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Xóa/vô hiệu hóa tài khoản
        /// </summary>
        [HttpDelete]
        [HasPermission(Permissions.AccountClaims.Delete)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> DeleteAccounts([FromBody] List<Guid> accountIds)
        {
            var result = await _service.DeleteAccountsAsync(accountIds);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Reset mật khẩu cho nhân viên
        /// </summary>
        [HttpPost("{id}/reset-password")]
        [HasPermission(Permissions.AccountClaims.ResetPassword)]
        [ProducesResponseType(typeof(ResetPasswordResultDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResetPasswordResultDto>> ResetPassword(Guid id)
        {
            var result = await _service.ResetPasswordAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Khóa/mở khóa tài khoản
        /// </summary>
        [HttpPost("{id}/toggle-lock")]
        [HasPermission(Permissions.AccountClaims.Lock)]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<AccountDto>> ToggleLock(Guid id)
        {
            var result = await _service.ToggleLockAsync(id);
            return Ok(result);
        }

        // ==================== SELF-SERVICE OPERATIONS ====================
        
        /// <summary>
        /// [SELF] Đổi mật khẩu của chính mình
        /// </summary>
        [HttpPost("me/change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> ChangeMyPassword([FromBody] ChangePasswordDto dto)
        {
            var accountId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _service.ChangePasswordAsync(accountId, dto);
            return Ok(new { message = "Mật khẩu đã được thay đổi thành công" });
        }

        /// <summary>
        /// [SELF] Xem lịch sử đăng nhập
        /// </summary>
        [HttpGet("me/login-history")]
        [Authorize]
        [ProducesResponseType(typeof(List<LoginHistoryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<LoginHistoryDto>>> GetMyLoginHistory()
        {
            var accountId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _service.GetLoginHistoryAsync(accountId);
            return Ok(result);
        }
    }
}
