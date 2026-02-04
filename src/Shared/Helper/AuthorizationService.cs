using Microsoft.EntityFrameworkCore;
using ItSupportServer.Data;
using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Shared.Helper
{
    public class AuthorizationService(AppDbContext db)
    {
        public async Task<BaseResult<bool>> RoleHasClaimAsync(string EmployeeId, string claimType)
        {
            var hasPermission = await db.Accounts
                .AsNoTracking()
                .Where(a => a.AccountId == EmployeeId)
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
            var hasPermission = await db.Accounts
                .AsNoTracking()
                .Where(a => a.AccountId == EmployeeId)
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
