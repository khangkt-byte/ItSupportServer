using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Shared.Attributes;
using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Dto;
using ItSupportServer.src.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ItSupportServer.src.Modules.Employee
{
    /// <summary>
    /// API quản lý nhân viên
    /// Pattern: RESTful API
    /// Reference: Microsoft REST API Guidelines
    /// </summary>
    [ApiController]
    [Route("api/employees")]
    [Produces("application/json")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;

        public EmployeesController(IEmployeeService service)
        {
            _service = service;
        }

        // ==================== ADMIN OPERATIONS ====================

        /// <summary>
        /// [ADMIN] Lấy danh sách nhân viên (có phân trang)
        /// </summary>
        [HttpGet]
        [HasPermission(Permissions.EmployeeClaims.View)]
        [ProducesResponseType(typeof(PaginatedResult<ListEmployeeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResult<ListEmployeeDto>>> GetEmployees(
            [FromQuery] QueryParameters parameters)
        {
            var result = await _service.GetEmployeesAsync(parameters);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Lấy thông tin chi tiết nhân viên (bao gồm roles)
        /// </summary>
        [HttpGet("{id}")]
        [HasPermission(Permissions.EmployeeClaims.View)]
        [ProducesResponseType(typeof(DetailEmployeeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DetailEmployeeDto>> GetEmployee(Guid id)
        {
            var result = await _service.GetEmployeeByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Tạo nhân viên mới
        /// </summary>
        [HttpPost]
        [HasPermission(Permissions.EmployeeClaims.Create)]
        [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EmployeeDto>> CreateEmployee([FromBody] CreateEmployeeDto dto)
        {
            var result = await _service.CreateEmployeeAsync(dto);
            return CreatedAtAction(nameof(GetEmployee), new { id = result.EmpId }, result);
        }

        /// <summary>
        /// [ADMIN] Cập nhật thông tin nhân viên
        /// </summary>
        /// <remarks>
        /// **Lưu ý:** Admin không thể tự cập nhật thông tin chính mình.
        /// Để cập nhật thông tin cá nhân, vui lòng sử dụng endpoint `/api/employees/me`
        /// </remarks>
        [HttpPut("{id}")]
        [HasPermission(Permissions.EmployeeClaims.Edit)]
        [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EmployeeDto>> UpdateEmployee(
            [FromRoute] Guid id,
            [FromBody] UpdateEmployeeDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != null && Guid.Parse(currentUserId) == id)
            {
                throw new BusinessRuleException(
                    "Không thể tự chỉnh sửa thông tin thông qua endpoint này. Vui lòng sử dụng /api/employees/me");
            }

            var result = await _service.UpdateEmployeeAsync(id, dto);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Xóa nhân viên
        /// </summary>
        /// <param name="id">Employee ID</param>
        /// <returns>No content (204)</returns>
        /// <remarks>
        /// **Pattern:** RESTful single resource delete
        /// **Reference:** Microsoft REST API Guidelines
        /// 
        /// **Business rules:**
        /// - Không thể xóa Super_Admin (422)
        /// - Không thể xóa nếu nhân viên có nhật ký sự cố (422)
        /// - Cascade delete tài khoản liên kết
        /// </remarks>
        [HttpDelete("{id}")]
        [HasPermission(Permissions.EmployeeClaims.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteEmployee([FromRoute] Guid id)
        {
            await _service.DeleteEmployeeAsync(id);
            return NoContent();
        }

        /// <summary>
        /// [ADMIN] Xóa nhiều nhân viên
        /// </summary>
        /// <param name="empIds">Danh sách employee IDs cần xóa</param>
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
        /// - Không thể xóa Super_Admin (422)
        /// - Không thể xóa nếu nhân viên có nhật ký sự cố (422)
        /// - Transaction rollback nếu ANY item fails
        /// - Cascade delete tài khoản liên kết
        /// 
        /// **Response:**
        /// ```json
        /// {
        ///   "success": true,
        ///   "deletedCount": 3,
        ///   "totalRequested": 3,
        ///   "message": "Đã xóa 3 nhân viên thành công"
        /// }
        /// ```
        /// </remarks>
        [HttpDelete]
        [HasPermission(Permissions.EmployeeClaims.Delete)]
        [ProducesResponseType(typeof(BulkDeleteResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BulkDeleteResultDto>> DeleteEmployees(
            [FromBody] List<Guid> empIds,
            [FromQuery] bool softDelete = true)
        {
            var result = await _service.DeleteEmployeesAsync(empIds, softDelete);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Gán roles cho nhân viên
        /// </summary>
        /// <remarks>
        /// **Lưu ý:** Admin không thể tự thay đổi roles của chính mình
        /// </remarks>
        [HttpPost("{id}/roles")]
        [HasPermission(Permissions.AccountClaims.SetAccessControl)]
        [ProducesResponseType(typeof(DetailEmployeeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DetailEmployeeDto>> AssignRoles(
            [FromRoute] Guid id,
            [FromBody] List<int> roleIds)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != null && Guid.Parse(currentUserId) == id)
            {
                throw new BusinessRuleException("Không thể tự thay đổi roles của chính mình");
            }

            var result = await _service.AssignRolesToEmployeeAsync(id, roleIds);
            return Ok(result);
        }

        // ==================== SELF-SERVICE OPERATIONS ====================

        /// <summary>
        /// [SELF] Lấy thông tin profile của mình
        /// </summary>
        /// <remarks>
        /// Endpoint này cho phép nhân viên xem thông tin cá nhân của chính mình.
        /// Không yêu cầu permission đặc biệt, chỉ cần đăng nhập.
        /// </remarks>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProfileDto>> GetMyProfile()
        {
            var empId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _service.GetProfileAsync(empId);
            return Ok(result);
        }

        /// <summary>
        /// [SELF] Cập nhật profile của mình (self-service)
        /// </summary>
        /// <remarks>
        /// Endpoint này cho phép nhân viên cập nhật một số thông tin cá nhân:
        /// - Họ tên (FullName)
        /// - Số điện thoại (PhoneNumber)
        /// - Email
        /// 
        /// Các thông tin khác như chức vụ, phòng ban chỉ admin mới có thể cập nhật.
        /// </remarks>
        [HttpPut("me")]
        [Authorize]
        [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProfileDto>> UpdateMyProfile([FromBody] UpdateProfileDto dto)
        {
            var empId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _service.UpdateProfileAsync(empId, dto);
            return Ok(result);
        }
    }
}
