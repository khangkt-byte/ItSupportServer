using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ItSupportServer.src.Shared.Base;
using System.Security.Claims;
using static ItSupportServer.src.Shared.Base.BaseEnum;
using ItSupportServer.src.Shared.Attributes;
using ItSupportServer.src.Modules.Authorization;
using FluentValidation;

namespace ItSupportServer.src.Modules.Employee
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = RoleUser.GroupAdmin)]
    public class EmployeesController(IEmployeesService service, IConfiguration configuration) : ControllerBase
    {
        [HttpGet]
        [HasPermission(Permissions.Employees.View)]
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
        [HasPermission(Permissions.Employees.View)]
        public async Task<IActionResult> GetEmployeeAsync([FromRoute] string Id)
        {
            var result = await service.GetEmployeeAsync(Id);
            return this.MyStatusCode(result);
        }

        [HttpPost]
        [HasPermission(Permissions.Employees.Create)]
        public async Task<IActionResult> CreateEmployeeAsync(
            [FromForm] CreateEmployeeDto dto,
            [FromServices] IValidator<CreateEmployeeDto> validator)
        {
            var validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var result = await service.CreateEmployeeAsync(dto);
            return this.MyStatusCode(result);
        }

        [HttpPut("{Id}")]
        [HasPermission(Permissions.Employees.Edit)]
        public async Task<IActionResult> UpdateEmployeeAsync(
            [FromRoute] string Id,
            [FromForm] UpdateEmployeeDto dto,
            [FromServices] IValidator<UpdateEmployeeDto> validator)
        {
            var validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var idUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Id == idUser)
            {
                return BadRequest("Không thể tự chỉnh sửa tài khoản hiện tại");
            }
            var result = await service.UpdateEmployeeAsync(Id, dto);
            return this.MyStatusCode(result);
        }


        //[HttpPatch("{Id}/status")]
        //[HasPermission(Permissions.Employees.Edit)]
        //public async Task<IActionResult> ChangeStatusAsync([FromRoute] string Id, [FromBody] UsersEnum.STATUS_EMP status)
        //{
        //    var idUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    if (Id == idUser)
        //    {
        //        return BadRequest("Không thể tự chỉnh sửa tài khoản hiện tại");
        //    }
        //    var result = await service.ChangeStatusAsync(Id, status);
        //    return this.MyStatusCode(result);
        //}

        //[Authorize(Roles = $"{RoleUser.Super_Admin}")]
        [HttpPatch("role/{Id}")]
        [HasPermission(Permissions.Employees.Edit)]
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
