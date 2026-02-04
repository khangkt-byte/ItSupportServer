using ItSupportServer.Data.Models;
using ItSupportServer.src.Modules.Role;
using Riok.Mapperly.Abstractions;

namespace ItSupportServer.src.Modules.Employee
{
    [Mapper]
    public partial class EmployeesMapper
    {
        public partial CreateEmployeeDto ToEmployeeDto(Employees emp);

        public partial IQueryable<CreateEmployeeDto> ProjectToCreateEmployeeDto(IQueryable<Employees> query);
        public partial IQueryable<UpdateEmployeeDto> ProjectToUpdateEmployeeDtoFull(IQueryable<Employees> query);
    }
}
