using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Shared.Attributes;
using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ItSupportServer.src.Modules.Employee
{
    /// <summary>
    /// API quản lý nhân viên
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

        /// <summary>
        /// Lấy danh sách nhân viên (có phân trang)
        /// </summary>
        [HttpGet]
        [HasPermission(Permissions.EmployeeClaims.View)]
        [ProducesResponseType(typeof(PaginatedResult<ListEmployeeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PaginatedResult<ListEmployeeDto>>> GetEmployees(
            [FromQuery] QueryParameters parameters)
        {
            var result = await _service.GetEmployeesAsync(parameters);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết nhân viên (bao gồm roles)
        /// </summary>
        [HttpGet("{id}")]
        [HasPermission(Permissions.EmployeeClaims.View)]
        [ProducesResponseType(typeof(DetailEmployeeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DetailEmployeeDto>> GetEmployee(Guid id)
        {
            var result = await _service.GetEmployeeByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Tạo nhân viên mới
        /// </summary>
        [HttpPost]
        [HasPermission(Permissions.EmployeeClaims.Create)]
        [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<EmployeeDto>> CreateEmployee([FromBody] CreateEmployeeDto dto)
        {
            var result = await _service.CreateEmployeeAsync(dto);
            return CreatedAtAction(nameof(GetEmployee), new { id = result.EmpId }, result);
        }

        /// <summary>
        /// Cập nhật thông tin nhân viên
        /// </summary>
        /// <remarks>
        /// **Lưu ý:** Không thể tự cập nhật chính mình. Vui lòng sử dụng API profile.
        /// </remarks>
        [HttpPut("{id}")]
        [HasPermission(Permissions.EmployeeClaims.Edit)]
        [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<EmployeeDto>> UpdateEmployee(
            [FromRoute] Guid id,
            [FromBody] UpdateEmployeeDto dto)
        {
            // Prevent self-edit
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != null && Guid.Parse(currentUserId) == id)
            {
                throw new BusinessRuleException(
                    "Không thể tự chỉnh sửa tài khoản của mình. Vui lòng sử dụng API /api/employees/profile.");
            }

            var result = await _service.UpdateEmployeeAsync(id, dto);
            return Ok(result);
        }

        /// <summary>
        /// Xóa nhiều nhân viên
        /// </summary>
        [HttpDelete]
        [HasPermission(Permissions.EmployeeClaims.Delete)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<bool>> DeleteEmployees(
            [FromBody] List<Guid> empIds,
            [FromQuery] bool softDelete = true)
        {
            var result = await _service.DeleteEmployeesAsync(empIds, softDelete);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin profile của user hiện tại
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProfileDto>> GetProfile()
        {
            var empId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _service.GetProfileAsync(empId);
            return Ok(result);
        }

        /// <summary>
        /// Cập nhật profile của user hiện tại (self-service)
        /// </summary>
        [HttpPut("profile")]
        [Authorize]
        [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ProfileDto>> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var empId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _service.UpdateProfileAsync(empId, dto);
            return Ok(result);
        }

        /// <summary>
        /// Gán roles cho nhân viên
        /// </summary>
        [HttpPost("{id}/roles")]
        [HasPermission(Permissions.RoleClaims.SetRole)]
        [ProducesResponseType(typeof(DetailEmployeeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DetailEmployeeDto>> AssignRoles(
            [FromRoute] Guid id,
            [FromBody] List<int> roleIds)
        {
            // Prevent self-role-change
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != null && Guid.Parse(currentUserId) == id)
            {
                throw new BusinessRuleException("Không thể tự thay đổi roles của mình");
            }

            var result = await _service.AssignRolesToEmployeeAsync(id, roleIds);
            return Ok(result);
        }
    }
}
