using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ItSupportServer.src.Shared.Base;
using System.Security.Claims;
using static ItSupportServer.src.Shared.Base.BaseEnum;
using ItSupportServer.src.Modules.User;

namespace ItSupportServer.src.Modules.Employee
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = RoleUser.GroupAdmin)]
    public class EmployeeController(IEmployeeService service, IConfiguration configuration) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetEmployeesAsync(
           [FromQuery] string? query,
           [FromQuery] int page = 1,
           [FromQuery] int pageSize = 10,
           [FromQuery] SortOBJ? sort = null
            )
        {
            var result = await service.GetEmployeesAsync(query, page, pageSize, sort);
            return this.MyStatusCode(result);
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetEmployeeAsync([FromRoute] string Id)
        {
            var result = await service.GetEmployeeAsync(Id);
            return this.MyStatusCode(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployeeAsync([FromForm] CreateEmployeeDto dto)
        {
            var result = await service.CreateEmployeeAsync(dto);
            return this.MyStatusCode(result);
        }
        [HttpPut("{Id}")]
        public async Task<IActionResult> UpdateEmployeeAsync([FromRoute] string Id, [FromForm] UpdateEmployeeDto dto)
        {
            var idUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Id == idUser)
            {
                return BadRequest("Không thể tự chỉnh sửa tài khoản hiện tại");
            }
            var result = await service.UpdateEmployeeAsync(Id, dto);
            return this.MyStatusCode(result);
        }


        [HttpPatch("{Id}/status")]
        public async Task<IActionResult> ChangeStatusAsync([FromRoute] string Id, [FromBody] UsersEnum.STATUS_EMP status)
        {
            var idUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Id == idUser)
            {
                return BadRequest("Không thể tự chỉnh sửa tài khoản hiện tại");
            }
            var result = await service.ChangeStatusAsync(Id, status);
            return this.MyStatusCode(result);
        }

        [Authorize(Roles = $"{RoleUser.Super_Admin}")]
        [HttpPatch("role/{Id}")]
        public async Task<IActionResult> ChangeRoleAsync([FromRoute] string Id, [FromBody] ROLE newRole)
        {
            var idUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Id == idUser)
            {
                return BadRequest("Không thể tự chỉnh sửa tài khoản hiện tại");
            }

            var result = await service.ChangeRoleAsync(Id, newRole);

            return this.MyStatusCode(result);
        }

    }
}
