using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ItSupportServer.src.Shared.Base;
using System.Security.Claims;
using static ItSupportServer.src.Shared.Base.BaseEnum;

namespace ItSupportServer.src.Modules.User.Employee
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = RoleUser.GroupAdmin)]
    public class EmployeeController(IEmployeeService service, IConfiguration configuration) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetEmployees(
           [FromQuery] string? query,
           [FromQuery] int page = 1,
           [FromQuery] int pageSize = 10,
           [FromQuery] SortOBJ? sort = null
            )
        {
            var result = await service.GetEmployees(query, page, pageSize, sort);
            return this.MyStatusCode(result);
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetEmployee([FromRoute] string Id)
        {
            var result = await service.GetEmployee(Id);
            return this.MyStatusCode(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromForm] CreateEmployeeDto dto)
        {
            var result = await service.CreateEmployee(dto);
            return this.MyStatusCode(result);
        }
        [HttpPut("{Id}")]
        public async Task<IActionResult> UpdateEmployee([FromRoute] string Id, [FromForm] UpdateEmployeeDto dto)
        {
            var idUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Id == idUser)
            {
                return BadRequest("Không thể tự chỉnh sửa tài khoản hiện tại");
            }
            var result = await service.UpdateEmployee(Id, dto);
            return this.MyStatusCode(result);
        }


        [HttpPatch("{Id}/status")]
        public async Task<IActionResult> ChangeStatus([FromRoute] string Id, [FromBody] UsersEnum.STATUS_EMP status)
        {
            var idUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Id == idUser)
            {
                return BadRequest("Không thể tự chỉnh sửa tài khoản hiện tại");
            }
            var result = await service.ChangeStatus(Id, status);
            return this.MyStatusCode(result);
        }

        [Authorize(Roles = $"{RoleUser.Supper_Admin}")]
        [HttpPatch("role/{Id}")]
        public async Task<IActionResult> ChhangeRole([FromRoute] string Id, [FromBody] ROLE newRole)
        {
            var idUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Id == idUser)
            {
                return BadRequest("Không thể tự chỉnh sửa tài khoản hiện tại");
            }

            var result = await service.ChangeRole(Id, newRole);

            return this.MyStatusCode(result);
        }

    }
}
