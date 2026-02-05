using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Shared.Base;
using Microsoft.AspNetCore.Mvc;
using ItSupportServer.src.Shared.Attributes;

namespace ItSupportServer.src.Modules.Department
{
    /// <summary>
    /// API quản lý phòng ban
    /// Pattern: RESTful API, CRUD operations
    /// Reference: Microsoft Graph, GitHub API
    /// </summary>
    [ApiController]
    [Route("api/departments")]
    [Produces("application/json")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _service;

        public DepartmentsController(IDepartmentService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lấy danh sách phòng ban (có phân trang)
        /// </summary>
        [HttpGet]
        [HasPermission(Permissions.DepartmentClaims.View)]
        [ProducesResponseType(typeof(PaginatedResult<DepartmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResult<DepartmentDto>>> GetDepartments(
            [FromQuery] QueryParameters parameters)
        {
            var result = await _service.GetDepartmentsAsync(parameters);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin phòng ban theo ID
        /// </summary>
        [HttpGet("{id}")]
        [HasPermission(Permissions.DepartmentClaims.View)]
        [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DepartmentDto>> GetDepartment(int id)
        {
            var result = await _service.GetDepartmentByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Lấy gợi ý phòng ban cho autocomplete/dropdown
        /// </summary>
        [HttpGet("suggestions")]
        [HasPermission(Permissions.DepartmentClaims.View)]
        [ProducesResponseType(typeof(List<DepartmentSuggestionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<DepartmentSuggestionDto>>> GetSuggestions(
            [FromQuery] string? search = null)
        {
            var result = await _service.GetDepartmentSuggestionsAsync(search);
            return Ok(result);
        }

        /// <summary>
        /// Tạo phòng ban mới
        /// </summary>
        [HttpPost]
        [HasPermission(Permissions.DepartmentClaims.Create)]
        [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DepartmentDto>> CreateDepartment([FromBody] CreateDepartmentDto dto)
        {
            var result = await _service.CreateDepartmentAsync(dto);
            return CreatedAtAction(nameof(GetDepartment), new { id = result.DptId }, result);
        }

        /// <summary>
        /// Cập nhật phòng ban
        /// </summary>
        [HttpPut("{id}")]
        [HasPermission(Permissions.DepartmentClaims.Edit)]
        [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DepartmentDto>> UpdateDepartment(
            [FromRoute] int id,
            [FromBody] UpdateDepartmentDto dto)
        {
            var result = await _service.UpdateDepartmentAsync(id, dto);
            return Ok(result);
        }

        /// <summary>
        /// Xóa phòng ban
        /// </summary>
        /// <param name="id">Department ID</param>
        /// <returns>No content (204)</returns>
        /// <remarks>
        /// **Business rules:**
        /// - Không thể xóa nếu phòng ban có nhân viên (422)
        /// - Không thể xóa nếu phòng ban có issue logs (422)
        /// </remarks>
        [HttpDelete("{id}")]
        [HasPermission(Permissions.DepartmentClaims.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteDepartment([FromRoute] int id)
        {
            await _service.DeleteDepartmentAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Xóa nhiều phòng ban
        /// </summary>
        /// <param name="dptIds">Danh sách department IDs cần xóa</param>
        /// <param name="softDelete">Soft delete (mặc định: true)</param>
        /// <returns>Kết quả xóa hàng loạt</returns>
        /// <remarks>
        /// **Strategy:** All-or-nothing (transaction-based)
        /// - Nếu TẤT CẢ thành công → 200 OK với summary
        /// - Nếu BẤT KỲ lỗi nào → Rollback, throw error (4xx/5xx)
        /// 
        /// **Business rules:**
        /// - Không thể xóa phòng ban đang có nhân viên (422)
        /// - Không thể xóa phòng ban đang có issue logs (422)
        /// - Transaction rollback nếu ANY item fails
        /// </remarks>
        [HttpDelete]
        [HasPermission(Permissions.DepartmentClaims.Delete)]
        [ProducesResponseType(typeof(BulkDeleteResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BulkDeleteResultDto>> DeleteDepartments(
            [FromBody] List<int> dptIds,
            [FromQuery] bool softDelete = true)
        {
            var result = await _service.DeleteDepartmentsAsync(dptIds, softDelete);
            return Ok(result);
        }
    }
}