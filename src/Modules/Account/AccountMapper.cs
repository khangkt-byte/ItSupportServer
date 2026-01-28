using ItSupportServer.Data.Models;
using Riok.Mapperly.Abstractions;

namespace ItSupportServer.src.Modules.Account
{
    [Mapper]
    public partial class AccountMapper
    {
        // ===== Entity → DTO =====

        [MapperIgnoreSource(nameof(Accounts.Id))]
        [MapperIgnoreSource(nameof(Accounts.Password))]
        [MapperIgnoreSource(nameof(Accounts.DeletedAt))]
        [MapperIgnoreSource(nameof(Accounts.Employee))]
        [MapperIgnoreSource(nameof(Accounts.AccountRoles))]
        [MapperIgnoreSource(nameof(Accounts.AccountClaims))]
        public partial AccountDto MapToAccountDto(Accounts account);

        // ===== Projections =====

        [MapProperty(nameof(Accounts.Employee.FullName), nameof(AccountDto.EmployeeName))]
        [MapProperty(nameof(Accounts.Employee.EmpCode), nameof(AccountDto.EmployeeCode))]
        [MapProperty(nameof(Accounts.Employee.Email), nameof(AccountDto.Email))]
        [MapProperty(nameof(Accounts.Employee.Position), nameof(AccountDto.Position))]
        public partial IQueryable<AccountDto> ProjectToAccountDto(IQueryable<Accounts> query);
    }
}
