using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Shared.Attributes;
using ItSupportServer.src.Shared.Base;
using Microsoft.AspNetCore.Mvc;

namespace ItSupportServer.src.Modules.Role
{
    /// <summary>
    /// API quản lý vai trò và phân quyền
    /// Pattern: RESTful API, RBAC
    /// Reference: Microsoft Graph, GitHub API
    /// </summary>
    [ApiController]
    [Route("api/roles")]
    [Produces("application/json")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _service;

        public RolesController(IRoleService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lấy danh sách vai trò (có phân trang)
        /// </summary>
        [HttpGet]
        [HasPermission(Permissions.RoleClaims.View)]
        [ProducesResponseType(typeof(PaginatedResult<RoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResult<RoleDto>>> GetRoles(
            [FromQuery] QueryParameters parameters)
        {
            var result = await _service.GetRolesAsync(parameters);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin vai trò theo ID (bao gồm claims)
        /// </summary>
        [HttpGet("{id}")]
        [HasPermission(Permissions.RoleClaims.View)]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RoleDto>> GetRole(int id)
        {
            var result = await _service.GetRoleByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Tạo vai trò mới
        /// </summary>
        [HttpPost]
        [HasPermission(Permissions.RoleClaims.Create)]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleDto dto)
        {
            var result = await _service.CreateRoleAsync(dto);
            return CreatedAtAction(nameof(GetRole), new { id = result.RoleId }, result);
        }

        /// <summary>
        /// Cập nhật vai trò
        /// </summary>
        [HttpPut("{id}")]
        [HasPermission(Permissions.RoleClaims.Edit)]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RoleDto>> UpdateRole(
            [FromRoute] int id,
            [FromBody] UpdateRoleDto dto)
        {
            var result = await _service.UpdateRoleAsync(id, dto);
            return Ok(result);
        }

        /// <summary>
        /// Xóa vai trò
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <returns>No content (204)</returns>
        /// <remarks>
        /// **Pattern:** RESTful single resource delete
        /// **Reference:** Microsoft REST API Guidelines
        /// 
        /// **Business rules:**
        /// - Không thể xóa nếu role đang được sử dụng (422)
        /// - Không thể xóa system roles (422)
        /// </remarks>
        [HttpDelete("{id}")]
        [HasPermission(Permissions.RoleClaims.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteRole([FromRoute] int id)
        {
            await _service.DeleteRoleAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Xóa nhiều vai trò
        /// </summary>
        /// <param name="roleIds">Danh sách role IDs cần xóa</param>
        /// <param name="softDelete">Soft delete (mặc định: true)</param>
        /// <returns>Kết quả xóa hàng loạt</returns>
        /// <remarks>
        /// **Strategy:** All-or-nothing (transaction-based)
        /// - Nếu TẤT CẢ thành công → 200 OK với summary
        /// - Nếu BẤT KỲ lỗi nào → Rollback, throw error (4xx/5xx)
        /// 
        /// **Pattern:** Microsoft Dynamics 365 standard tables
        /// **Reference:** https://learn.microsoft.com/en-us/power-apps/developer/data-platform/bulk-operations
        /// 
        /// **Business rules:**
        /// - Không thể xóa role đang được sử dụng (422)
        /// - Không thể xóa system roles (422)
        /// - Transaction rollback nếu ANY item fails
        /// 
        /// **Response:**
        /// ```json
        /// {
        ///   "success": true,
        ///   "deletedCount": 3,
        ///   "totalRequested": 3,
        ///   "message": "Đã xóa 3 vai trò thành công"
        /// }
        /// ```
        /// </remarks>
        [HttpDelete]
        [HasPermission(Permissions.RoleClaims.Delete)]
        [ProducesResponseType(typeof(BulkDeleteResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BulkDeleteResultDto>> DeleteRoles(
            [FromBody] List<int> roleIds,
            [FromQuery] bool softDelete = true)
        {
            var result = await _service.DeleteRolesAsync(roleIds, softDelete);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách tất cả claims có thể gán
        /// </summary>
        [HttpGet("claims")]
        [HasPermission(Permissions.RoleClaims.View)]
        [ProducesResponseType(typeof(List<ClaimDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ClaimDto>>> GetAllClaims()
        {
            var result = await _service.GetAllClaimsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Gán roles cho tài khoản
        /// </summary>
        [HttpPost("assign")]
        [HasPermission(Permissions.RoleClaims.SetRole)]
        [ProducesResponseType(typeof(AccountRolesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AccountRolesDto>> AssignRolesToAccount(
            [FromBody] AssignRolesDto dto)
        {
            var result = await _service.AssignRolesToAccountAsync(dto);
            return Ok(result);
        }
    }
}
