using ItSupportServer.Data.Models;
using Riok.Mapperly.Abstractions;

namespace ItSupportServer.src.Modules.Account
{
    [Mapper]
    public partial class AccountMapper
    {
        // ===== Projections for EF Core (IQueryable) =====

        /// <summary>
        /// Project to AccountDto with Employee and Roles
        /// Use this for GetAccountByIdAsync (needs roles)
        /// </summary>
        public IQueryable<AccountDto> ProjectToAccountDto(IQueryable<Accounts> query)
        {
            return query.Select(a => new AccountDto
            {
                AccountId = a.AccountId,
                Username = a.Username,
                EmployeeName = a.Employee.FullName,
                EmployeeCode = a.Employee.EmpCode,
                Email = a.Employee.Email,
                Position = a.Employee.Position,
                IsLocked = a.IsLocked,
                FailedLoginAttempts = a.FailedLoginAttempts,
                LastLoginAt = a.LastLoginAt,
                LockedUntil = a.LockedUntil,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                Roles = a.AccountRoles.Select(ar => new Role.RoleDto
                {
                    RoleId = ar.Role.RoleId,
                    Name = ar.Role.Name,
                    Description = ar.Role.Description,
                    CreatedAt = ar.Role.CreatedAt,
                    UpdatedAt = ar.Role.UpdatedAt
                }).ToList()
            });
        }

        /// <summary>
        /// Project to ListAccountDto (for paginated list)
        /// Use this for GetAccountsAsync (no roles needed)
        /// </summary>
        public IQueryable<ListAccountDto> ProjectToListAccountDto(IQueryable<Accounts> query)
        {
            return query.Select(a => new ListAccountDto
            {
                AccountId = a.AccountId,
                Username = a.Username,
                EmployeeName = a.Employee.FullName,
                EmployeeCode = a.Employee.EmpCode,
                IsLocked = a.IsLocked,
                LastLoginAt = a.LastLoginAt,
                CreatedAt = a.CreatedAt
            });
        }
    }
}
