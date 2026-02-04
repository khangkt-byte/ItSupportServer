using Microsoft.EntityFrameworkCore;
using ItSupportServer.src.Shared.Base;
using ItSupportServer.Data.Models;

namespace ItSupportServer.src.Modules.Authorization
{
    public class AuthorizationService(AppDbContext db)
    {
        public async Task<BaseResult<bool>> RoleHasClaimAsync(string EmployeeId, string claimType)
        {
            var AccountId = Guid.Parse(EmployeeId);
            var hasPermission = await db.Accounts
                .AsNoTracking()
                .Where(a => a.AccountId == AccountId)
                .AnyAsync(a =>
                    a.AccountClaims.Any(ac =>
                        ac.Claim.Claim == claimType || ac.Claim.Claim == "Admin") ||
                    a.AccountRoles.Any(ar =>
                        ar.Role.RoleClaims.Any(rc =>
                            rc.Claim.Claim == claimType || rc.Claim.Claim == "Admin"))
    );

            return hasPermission
                ? BaseResult<bool>.Ok(true)
                : BaseResult<bool>.Fail("Bạn không có quyền truy cập chức năng này", 403);
        }

        public async Task<BaseResult<bool>> RoleHasListClaimAsync(string EmployeeId, string[] claimTypes)
        {
            var AccountId = Guid.Parse(EmployeeId);
            var hasPermission = await db.Accounts
                .AsNoTracking()
                .Where(a => a.AccountId == AccountId)
                .AnyAsync(a =>
                    a.AccountClaims.Any(ac =>
                        claimTypes.Contains(ac.Claim.Claim) || ac.Claim.Claim == "Admin") ||
                    a.AccountRoles.Any(ar =>
                        ar.Role.RoleClaims.Any(rc =>
                            claimTypes.Contains(rc.Claim.Claim) || rc.Claim.Claim == "Admin")));

            return hasPermission
                ? BaseResult<bool>.Ok(true)
                : BaseResult<bool>.Fail("Bạn không có quyền truy cập chức năng này", 403);
        }
    }
}
