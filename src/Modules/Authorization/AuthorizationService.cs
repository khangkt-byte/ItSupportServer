using ItSupportServer.Data.Models;
using ItSupportServer.src.Modules.User;
using ItSupportServer.src.Shared.Base;
using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.src.Modules.Authorization
{
    public class AuthorizationService(AppDbContext db)
    {
        public async Task<BaseResult<bool>> RoleHasClaimAsync(string empId, string claimType)
        {
            if (!Guid.TryParse(empId, out Guid accId))
            {
                return BaseResult<bool>.Fail("Id không hợp lệ.", 400);
            }

            var hasPermission = await db.Accounts
                .AsNoTracking()
                .Where(a => a.AccountId == accId)
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

        public async Task<BaseResult<bool>> RoleHasListClaimAsync(string empId, string[] claimTypes)
        {
            if (!Guid.TryParse(empId, out Guid AccountId))
            {
                return BaseResult<bool>.Fail("Id không hợp lệ.", 400);
            }

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
