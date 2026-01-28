using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Shared.Attributes;
using ItSupportServer.src.Shared.Base;
using Microsoft.AspNetCore.Mvc;

namespace ItSupportServer.src.Modules.Role
{
    /// <summary>
    /// API quản lý vai trò và phân quyền
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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
        [HttpDelete("{id}")]
        [HasPermission(Permissions.RoleClaims.Delete)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<bool>> DeleteRole([FromRoute] int id)
        {
            var result = await _service.DeleteRoleAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Xóa nhiều vai trò
        /// </summary>
        [HttpDelete]
        [HasPermission(Permissions.RoleClaims.Delete)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<bool>> DeleteRoles([FromBody] List<int> roleIds)
        {
            var result = await _service.DeleteRolesAsync(roleIds);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách tất cả claims có thể gán
        /// </summary>
        [HttpGet("claims")]
        [HasPermission(Permissions.RoleClaims.View)]
        [ProducesResponseType(typeof(List<ClaimDto>), StatusCodes.Status200OK)]
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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccountRolesDto>> AssignRolesToAccount(
            [FromBody] AssignRolesDto dto)
        {
            var result = await _service.AssignRolesToAccountAsync(dto);
            return Ok(result);
        }
    }
}
