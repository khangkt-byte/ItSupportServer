using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Shared.Base;
using Microsoft.AspNetCore.Mvc;
using ItSupportServer.src.Shared.Attributes;
using ItSupportServer.src.Shared.Dto;

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
        /// [ADMIN] Xóa phòng ban
        /// </summary>
        /// <param name="dptId">Department ID</param>
        /// <returns>No content (204)</returns>
        /// <remarks>
        /// **Pattern:** RESTful single resource delete
        /// **Reference:** Microsoft REST API Guidelines
        /// 
        /// **Business rules:**
        /// - Không thể xóa nếu phòng ban có nhân viên (422)
        /// - Không thể xóa nếu phòng ban có nhật ký sự cố (422)
        /// </remarks>
        [HttpDelete("{dptId}")]
        [HasPermission(Permissions.DepartmentClaims.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteDepartment([FromRoute] int dptId)
        {
            await _service.DeleteDepartmentAsync(dptId);
            return NoContent();
        }

        /// <summary>
        /// [ADMIN] Xóa nhiều phòng ban
        /// </summary>
        /// <param name="dptIds">Danh sách department IDs cần xóa</param>
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
        /// - Không thể xóa phòng ban có nhân viên (422)
        /// - Không thể xóa phòng ban có nhật ký sự cố (422)
        /// - Transaction rollback nếu ANY item fails
        /// 
        /// **Response:**
        /// ```json
        /// {
        ///   "success": true,
        ///   "deletedCount": 3,
        ///   "totalRequested": 3,
        ///   "message": "Đã xóa 3 phòng ban thành công"
        /// }
        /// ```
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