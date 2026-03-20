using ItSupportServer.Data.Models.Entities;
using ItSupportServer.src.Modules.Role;
using Riok.Mapperly.Abstractions;

namespace ItSupportServer.src.Modules.Account
{
    [Mapper]
    public partial class AccountMapper
    {
        [UseMapper]
        private static readonly RoleMapper rolesMapper = new();

        // ===== Projections for EF Core (IQueryable) =====

        [MapperIgnoreSource(nameof(Accounts.Id))]
        [MapperIgnoreSource(nameof(Accounts.Password))]
        [MapperIgnoreSource(nameof(Accounts.CurrentPoints))]
        [MapperIgnoreSource(nameof(Accounts.LifetimePoints))]
        [MapperIgnoreSource(nameof(Accounts.Otp))]
        [MapperIgnoreSource(nameof(Accounts.ExpiredOtp))]
        [MapperIgnoreSource(nameof(Accounts.DeletedAt))]
        [MapperIgnoreSource(nameof(Accounts.AccountTokens))]
        [MapperIgnoreSource(nameof(Accounts.PasswordResetTokens))]
        [MapProperty(nameof(Accounts.Employee.EmpCode), nameof(AccountDto.EmpCode))]
        [MapProperty(nameof(Accounts.Employee.FullName), nameof(AccountDto.EmpName))]
        [MapProperty(nameof(Accounts.Employee.Email), nameof(AccountDto.Email))]
        [MapProperty(nameof(Accounts.Employee.FullName), nameof(AccountDto.Position))]
        [MapPropertyFromSource(nameof(AccountDto.Roles), Use = nameof(MapRoles))]
        [MapPropertyFromSource(nameof(AccountDto.Claims), Use = nameof(MapClaimsForAccount))]
        public partial AccountDto MapToAccountDto(Accounts account);

        /// <summary>
        /// Project to AccountDto with Employee and Roles
        /// Use this for GetAccountByIdAsync (needs roles)
        /// </summary>
        public partial IQueryable<AccountDto> ProjectToAccountDto(IQueryable<Accounts> query);
        //{
        //    return query.Select(a => new AccountDto
        //    {
        //        AccountId = a.AccountId,
        //        Username = a.Username,
        //        EmpName = a.Employee.FullName,
        //        EmpCode = a.Employee.EmpCode,
        //        Email = a.Employee.Email,
        //        Position = a.Employee.Position,
        //        IsLocked = a.IsLocked,
        //        FailedLoginAttempts = a.FailedLoginAttempts,
        //        LastLoginAt = a.LastLoginAt,
        //        LockedUntil = a.LockedUntil,
        //        CreatedAt = a.CreatedAt,
        //        UpdatedAt = a.UpdatedAt,
        //        Roles = a.AccountRoles.Select(ar => new RoleDto
        //        {
        //            RoleId = ar.Role.RoleId,
        //            Name = ar.Role.Name,
        //            Description = ar.Role.Description,
        //            CreatedAt = ar.Role.CreatedAt,
        //            UpdatedAt = ar.Role.UpdatedAt
        //        }).ToList()
        //    });
        //}

        private static List<RoleDto>? MapRoles(Accounts account)
        {
            if (account.AccountRoles == null)
                return null;
            return rolesMapper.ProjectToRoleDto(account.AccountRoles
                .Where(ar => ar.Role.DeletedAt == null)
                .Select(ar => ar.Role)
                .AsQueryable())
                .ToList();
        }

        private static List<ClaimDto>? MapClaimsForAccount(Accounts account)
        {
            if (account == null)
                return null;

            var directClaims = account.AccountClaims?
                .Select(ac => ac.Claim)
                .Where(c => c != null)
                .GroupBy(c => c.ClaimId)
                .Select(g => g.First())
                .AsQueryable();

            if (directClaims == null)
                return null;

            return rolesMapper.ProjectToClaimDto(directClaims).ToList();
        }

        [MapProperty(nameof(Accounts.Employee.EmpCode), nameof(ListAccountDto.EmpCode))]
        [MapProperty(nameof(Accounts.Employee.FullName), nameof(ListAccountDto.EmpName))]
        [MapPropertyFromSource(nameof(ListAccountDto.TotalClaims), Use = nameof(CountClaims))]
        [MapperIgnoreSource(nameof(Accounts.Id))]
        [MapperIgnoreSource(nameof(Accounts.Password))]
        [MapperIgnoreSource(nameof(Accounts.FailedLoginAttempts))]
        [MapperIgnoreSource(nameof(Accounts.LockedUntil))]
        [MapperIgnoreSource(nameof(Accounts.CurrentPoints))]
        [MapperIgnoreSource(nameof(Accounts.LifetimePoints))]
        [MapperIgnoreSource(nameof(Accounts.Otp))]
        [MapperIgnoreSource(nameof(Accounts.ExpiredOtp))]
        [MapperIgnoreSource(nameof(Accounts.AccountTokens))]
        [MapperIgnoreSource(nameof(Accounts.PasswordResetTokens))]
        [MapperIgnoreSource(nameof(Accounts.UpdatedAt))]
        [MapperIgnoreSource(nameof(Accounts.DeletedAt))]
        public partial ListAccountDto MapToListAccountDto(Accounts account);

        /// <summary>
        /// Project to ListAccountDto (for paginated list)
        /// Use this for GetAccountsAsync (no roles needed)
        /// </summary>
        public partial IQueryable<ListAccountDto> ProjectToListAccountDto(IQueryable<Accounts> query);

        private static int CountClaims(Accounts account)
        {
            if (account == null) return 0;

            var uniqueClaims = new HashSet<int>();

            if (account.AccountClaims != null)
            {
                foreach (var ac in account.AccountClaims)
                    uniqueClaims.Add(ac.ClaimId);
            }

            if (account.AccountRoles != null)
            {
                foreach (var ar in account.AccountRoles)
                {
                    if (ar.Role is { DeletedAt: null } role && role.RoleClaims != null)
                    {
                        foreach (var rc in role.RoleClaims)
                            uniqueClaims.Add(rc.ClaimId);
                    }
                }
            }

            return uniqueClaims.Count;
        }
    }
}
