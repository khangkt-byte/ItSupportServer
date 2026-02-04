using FluentValidation;
using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Shared.Attributes;
using ItSupportServer.src.Shared.Base;
using Microsoft.AspNetCore.Mvc;

namespace ItSupportServer.src.Modules.Area
{
    [Route("api/areas")]
    [ApiController]
    public class AreasController(IAreasService service) : ControllerBase
    {
        [HttpGet]
        [HasPermission(Permissions.Areas.View)]
        public async Task<IActionResult> GetAreasAsync(
            [FromQuery] string? query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] SortOBJ? sort = null)
        {
            var result = await service.GetAreasAsync(query, page, pageSize, sort);
            return this.MyStatusCode(result);
        }

        [HttpGet("{areaId}")]
        [HasPermission(Permissions.Areas.View)]
        public async Task<IActionResult> GetAreaByIdAsync([FromRoute] int areaId)
        {
            var result = await service.GetAreaByIdAsync(areaId);
            return this.MyStatusCode(result);
        }

        [HttpPost]
        [HasPermission(Permissions.Areas.Create)]
        public async Task<IActionResult> CreateAreaAsync(
            [FromBody] CreateAreaDto dto,
            [FromServices] IValidator<CreateAreaDto> validator)
        {
            var validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var result = await service.CreateAreaAsync(dto);
            return this.MyStatusCode(result);
        }

        [HttpPut]
        [HasPermission(Permissions.Areas.Edit)]
        public async Task<IActionResult> UpdateAreaAsync(
            [FromBody] UpdateAreaDto dto,
            [FromServices] IValidator<UpdateAreaDto> validator)
        {
            var validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var result = await service.UpdateAreaAsync(dto);
            return this.MyStatusCode(result);
        }

        [HttpDelete]
        [HasPermission(Permissions.Areas.Delete)]
        public async Task<IActionResult> DeleteAreasAsync([FromBody] List<int> areaId)
        {
            var result = await service.DeleteAreasAsync(areaId);
            return this.MyStatusCode(result);
        }
    }
}
