using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Shared.Attributes;
using ItSupportServer.src.Shared.Base;
using Microsoft.AspNetCore.Mvc;

namespace ItSupportServer.src.Modules.Area
{
    /// <summary>
    /// API quản lý khu vực
    /// Pattern: RESTful API
    /// Reference: Microsoft REST API Guidelines
    /// </summary>
    [ApiController]
    [Route("api/areas")]
    [Produces("application/json")]
    public class AreasController : ControllerBase
    {
        private readonly IAreaService _service;

        public AreasController(IAreaService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lấy danh sách khu vực (có phân trang)
        /// </summary>
        /// <param name="parameters">Query parameters (page, pageSize, sortBy, search)</param>
        /// <returns>Paginated list of areas</returns>
        [HttpGet]
        [HasPermission(Permissions.AreaClaims.View)]
        [ProducesResponseType(typeof(PaginatedResult<AreaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResult<AreaDto>>> GetAreas(
            [FromQuery] QueryParameters parameters)
        {
            var result = await _service.GetAreasAsync(parameters);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin khu vực theo ID
        /// </summary>
        /// <param name="id">Area ID</param>
        /// <returns>Area details</returns>
        [HttpGet("{id}")]
        [HasPermission(Permissions.AreaClaims.View)]
        [ProducesResponseType(typeof(AreaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AreaDto>> GetArea(int id)
        {
            var result = await _service.GetAreaByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Tạo khu vực mới
        /// </summary>
        /// <param name="dto">Create area DTO</param>
        /// <returns>Created area</returns>
        [HttpPost]
        [HasPermission(Permissions.AreaClaims.Create)]
        [ProducesResponseType(typeof(AreaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AreaDto>> CreateArea([FromBody] CreateAreaDto dto)
        {
            var result = await _service.CreateAreaAsync(dto);
            return CreatedAtAction(nameof(GetArea), new { id = result.AreaId }, result);
        }

        /// <summary>
        /// Cập nhật khu vực
        /// </summary>
        /// <param name="id">Area ID</param>
        /// <param name="dto">Update area DTO</param>
        /// <returns>Updated area</returns>
        [HttpPut("{id}")]
        [HasPermission(Permissions.AreaClaims.Edit)]
        [ProducesResponseType(typeof(AreaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AreaDto>> UpdateArea(
            [FromRoute] int id,
            [FromBody] UpdateAreaDto dto)
        {
            var result = await _service.UpdateAreaAsync(id, dto);
            return Ok(result);
        }

        /// <summary>
        /// Xóa khu vực
        /// </summary>
        /// <param name="id">Area ID</param>
        /// <returns>No content (204)</returns>
        /// <remarks>
        /// **Pattern:** RESTful single resource delete
        /// **Reference:** Microsoft REST API Guidelines
        /// 
        /// **Business rules:**
        /// - Không thể xóa nếu khu vực đang được sử dụng bởi nhân viên (422)
        /// </remarks>
        [HttpDelete("{id}")]
        [HasPermission(Permissions.AreaClaims.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteArea([FromRoute] int id)
        {
            await _service.DeleteAreaAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Xóa nhiều khu vực
        /// </summary>
        /// <param name="areaIds">Danh sách area IDs cần xóa</param>
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
        /// - Không thể xóa khu vực đang được sử dụng bởi nhân viên (422)
        /// - Transaction rollback nếu ANY item fails
        /// 
        /// **Response:**
        /// ```json
        /// {
        ///   "success": true,
        ///   "deletedCount": 3,
        ///   "totalRequested": 3,
        ///   "message": "Đã xóa 3 khu vực thành công"
        /// }
        /// ```
        /// </remarks>
        [HttpDelete]
        [HasPermission(Permissions.AreaClaims.Delete)]
        [ProducesResponseType(typeof(BulkDeleteResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BulkDeleteResultDto>> DeleteAreas(
            [FromBody] List<int> areaIds,
            [FromQuery] bool softDelete = true)
        {
            var result = await _service.DeleteAreasAsync(areaIds, softDelete);
            return Ok(result);
        }
    }
}
