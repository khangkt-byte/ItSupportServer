using Microsoft.AspNetCore.Mvc;
using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Modules.Roles
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController(IRolesService service) : ControllerBase
    {
        [HttpGet]
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
        public async Task<IActionResult> GetRoleAsync([FromRoute] string id)
        {
            var result = await service.GetRoleAsync(id);
            return this.MyStatusCode(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoleAsync([FromBody] CreateRoleDto dto)
        {
            var result = await service.CreateRoleAsync(dto);
            return this.MyStatusCode(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoleAsync([FromRoute] string id, [FromBody] UpdateRoleDto dto)
        {
            dto.Id = id;
            var result = await service.UpdateRoleAsync(dto);
            return this.MyStatusCode(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoleAsync([FromRoute] string id)
        {
            var result = await service.DeleteRoleAsync(id);
            return this.MyStatusCode(result);
        }

        [HttpGet("claims")]
        public async Task<IActionResult> GetAllClaimsAsync()
        {
            var result = await service.GetAllClaimsAsync();
            return this.MyStatusCode(result);
        }

        [HttpPost("set-role")]
        public async Task<IActionResult> SetRoleAsync([FromBody] AccountRoleDto dto)
        {
            var result = await service.SetRoleAsync(dto);
            return this.MyStatusCode(result);
        }
    }
}
