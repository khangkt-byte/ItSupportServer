using Microsoft.AspNetCore.Mvc;
using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Attributes;
using ItSupportServer.src.Modules.Authorization;

namespace ItSupportServer.src.Modules.Role
{
    [Route("api/roles")]
    [ApiController]
    public class RolesController(IRolesService service) : ControllerBase
    {
        [HttpGet]
        [HasPermission(Permissions.Roles.View)]
        public async Task<IActionResult> GetRolesAsync(
            [FromQuery] string? query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] SortOBJ? sort = null
            )
        {
            var result = await service.GetRolesAsync(query, page, pageSize, sort);
            return this.MyStatusCode(result);
        }

        [HttpGet("{id}")]
        [HasPermission(Permissions.Roles.View)]
        public async Task<IActionResult> GetRoleAsync([FromRoute] int id)
        {
            var result = await service.GetRoleAsync(id);
            return this.MyStatusCode(result);
        }

        [HttpPost]
        [HasPermission(Permissions.Roles.Create)]
        public async Task<IActionResult> CreateRoleAsync([FromBody] CreateRoleDto dto)
        {
            var result = await service.CreateRoleAsync(dto);
            return this.MyStatusCode(result);
        }

        [HttpPut("{id}")]
        [HasPermission(Permissions.Roles.Edit)]
        public async Task<IActionResult> UpdateRoleAsync([FromRoute] int id, [FromBody] UpdateRoleDto dto)
        {
            dto.RoleId = id;
            var result = await service.UpdateRoleAsync(dto);
            return this.MyStatusCode(result);
        }

        [HttpDelete("{id}")]
        [HasPermission(Permissions.Roles.Delete)]
        public async Task<IActionResult> DeleteRoleAsync([FromRoute] int id)
        {
            var result = await service.DeleteRoleAsync(id);
            return this.MyStatusCode(result);
        }

        [HttpGet("claims")]
        [HasPermission(Permissions.Roles.View)]
        public async Task<IActionResult> GetAllClaimsAsync()
        {
            var result = await service.GetAllClaimsAsync();
            return this.MyStatusCode(result);
        }

        [HttpPost("set-role")]
        [HasPermission(Permissions.Roles.SetRole)]
        public async Task<IActionResult> SetRoleAsync([FromBody] AccountRoleDto dto)
        {
            var result = await service.SetRoleAsync(dto);
            return this.MyStatusCode(result);
        }
    }
}
