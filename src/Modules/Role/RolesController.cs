using Microsoft.AspNetCore.Mvc;
using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Attributes;
using ItSupportServer.src.Modules.Authorization;
using FluentValidation;

namespace ItSupportServer.src.Modules.Role
{
    [Route("api/roles")]
    [ApiController]
    public class RolesController(IRoleService service) : ControllerBase
    {
        [HttpGet]
        [HasPermission(Permissions.RoleClaims.View)]
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
        [HasPermission(Permissions.RoleClaims.View)]
        public async Task<IActionResult> GetRoleAsync([FromRoute] int id)
        {
            var result = await service.GetRoleAsync(id);
            return this.MyStatusCode(result);
        }

        [HttpPost]
        [HasPermission(Permissions.RoleClaims.Create)]
        public async Task<IActionResult> CreateRoleAsync(
            [FromBody] CreateRoleDto dto,
            [FromServices] IValidator<CreateRoleDto> validator)
        {
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var result = await service.CreateRoleAsync(dto);
            return this.MyStatusCode(result);
        }

        [HttpPut("{id}")]
        [HasPermission(Permissions.RoleClaims.Edit)]
        public async Task<IActionResult> UpdateRoleAsync(
            [FromRoute] int id,
            [FromBody] UpdateRoleDto dto,
            [FromServices] IValidator<UpdateRoleDto> validator)
        {
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            dto.RoleId = id;
            var result = await service.UpdateRoleAsync(dto);
            return this.MyStatusCode(result);
        }

        [HttpDelete("{id}")]
        [HasPermission(Permissions.RoleClaims.Delete)]
        public async Task<IActionResult> DeleteRoleAsync([FromRoute] int id)
        {
            var result = await service.DeleteRoleAsync(id);
            return this.MyStatusCode(result);
        }

        [HttpGet("claims")]
        [HasPermission(Permissions.RoleClaims.View)]
        public async Task<IActionResult> GetAllClaimsAsync()
        {
            var result = await service.GetAllClaimsAsync();
            return this.MyStatusCode(result);
        }

        [HttpPost("set-role")]
        [HasPermission(Permissions.RoleClaims.SetRole)]
        public async Task<IActionResult> SetRoleAsync(
            [FromBody] AccountRoleDto dto,
            [FromServices] IValidator<AccountRoleDto> validator)
        {
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var result = await service.SetRoleAsync(dto);
            return this.MyStatusCode(result);
        }
    }
}
