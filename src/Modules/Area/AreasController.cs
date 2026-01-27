using FluentValidation;
using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Shared.Attributes;
using ItSupportServer.src.Shared.Base;
using Microsoft.AspNetCore.Mvc;

namespace ItSupportServer.src.Modules.Area
{
    [ApiController]
    [Route("api/[controller]")]
    public class AreasController : ControllerBase
    {
        private readonly IAreasService _service;

        public AreasController(IAreasService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get paginated list of areas
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResult<List<AreaDto>>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        public async Task<ActionResult<PaginatedResult<List<AreaDto>>>> GetAreas(
            [FromQuery] string? query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] SortOBJ? sort = null)
        {
            var result = await _service.GetAreasAsync(query, page, pageSize, sort);
            return Ok(result);
        }

        /// <summary>
        /// Get area by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AreaDto), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        public async Task<ActionResult<AreaDto>> GetArea(int id)
        {
            var result = await _service.GetAreaByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Create new area
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(AreaDto), 201)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 409)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        public async Task<ActionResult<AreaDto>> CreateArea([FromBody] CreateAreaDto dto)
        {
            var result = await _service.CreateAreaAsync(dto);
            return CreatedAtAction(nameof(GetArea), new { id = result.AreaId }, result);
        }

        /// <summary>
        /// Update existing area
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(AreaDto), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        public async Task<ActionResult<AreaDto>> UpdateArea(int id, [FromBody] UpdateAreaDto dto)
        {
            dto.AreaId = id;
            var result = await _service.UpdateAreaAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Delete areas
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 422)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        public async Task<ActionResult<bool>> DeleteAreas(
            [FromBody] List<int> areaIds,
            [FromQuery] bool softDelete = true)
        {
            var result = await _service.DeleteAreasAsync(areaIds, softDelete);
            return Ok(result);
        }
    }
}
