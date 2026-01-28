using Microsoft.EntityFrameworkCore;
using ItSupportServer.src.Shared.Base;
using ItSupportServer.Data.Models;

namespace ItSupportServer.src.Modules.Role
{
    public class RoleService(AppDbContext db) : IRoleService
    {
        public async Task<BaseResult<PaginatedResult<List<RolesDto>>>> GetRolesAsync(string? query, int page, int pageSize, SortOBJ? sort)
        {
            try
            {
                var roles = db.Roles
                    .Where(r => r.DeletedAt == null)
                    .Select(r => new RolesDto
                    {
                        RoleId = r.RoleId,
                        Name = r.Name,

                        Claims = db.RoleClaims
                            .Where(rc => rc.RoleId == r.RoleId)
                            .OrderBy(o => o.Claim.ClaimId)
                            .Select(rc => new ClaimDto
                            {
                                ClaimId = rc.ClaimId,
                                Claim = rc.Claim.Claim
                            }).ToList()

                    })
                    .AsQueryable();
                if (query is not null) roles = roles.Where(r => r.Name.Contains(query));
                var TotalItems = await roles.CountAsync();

                var result = await Pagination<RolesDto>.PaginationAsync(roles, page, pageSize, sort);

                return BaseResult<PaginatedResult<List<RolesDto>>>.Ok(result);
            }
            catch (Exception e)
            {
                return BaseResult<PaginatedResult<List<RolesDto>>>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }

        public async Task<BaseResult<RolesDto>> GetRoleAsync(int roleId)
        {
            try
            {
                var role = await db.Roles.FindAsync(roleId);
                if (role is null) return BaseResult<RolesDto>.Fail("Vai trò không tồn tại", 404);

                var result = new RolesDto
                {
                    Name = role.Name,
                    Claims = await db.RoleClaims
                        .Where(rc => rc.RoleId == role.RoleId)
                        .OrderBy(o => o.Claim.ClaimId)
                        .Select(rc => new ClaimDto
                        {
                            ClaimId = rc.ClaimId,
                            Claim = rc.Claim.Claim
                        }).ToListAsync()
                };

                return BaseResult<RolesDto>.Ok(result);
            }
            catch (Exception e)
            {
                return BaseResult<RolesDto>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }

        public async Task<BaseResult<CreateRoleDto>> CreateRoleAsync(CreateRoleDto dto)
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var existingRole = await db.Roles
                                           .Where(r => r.Name == dto.Name && r.DeletedAt == null)
                                           .FirstOrDefaultAsync();
                if (existingRole is not null) return BaseResult<CreateRoleDto>.Fail("Vai trò đã tồn tại hoặc tên bị trùng", 400);

                var role = await db.Roles.AddAsync(new Roles()
                {
                    Name = dto.Name,
                });

                var selected = dto.ClaimIds!.Distinct().ToList();

                if (selected.Any())
                {
                    var addedRole = selected.Select(id => new RoleClaims { RoleId = role.Entity.RoleId, ClaimId = id }).ToList();
                    await db.RoleClaims.AddRangeAsync(addedRole);
                }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return BaseResult<CreateRoleDto>.Ok(dto);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                return BaseResult<CreateRoleDto>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }

        public async Task<BaseResult<UpdateRoleDto>> UpdateRoleAsync(UpdateRoleDto dto)
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var role = await db.Roles.FindAsync(dto.RoleId);
                if (role is null) return BaseResult<UpdateRoleDto>.Fail("Vai trò không tồn tại", 404);

                role.Name = dto.Name;

                db.Roles.Update(role);

                var existingRoleClaims = await db.RoleClaims
                                             .Where(rc => rc.RoleId == dto.RoleId)
                                             .Select(c => c.ClaimId)
                                             .ToListAsync();

                var selected = dto.ClaimIds!.Distinct().ToList();

                var toAdd = selected.Except(existingRoleClaims);
                var toRemove = existingRoleClaims.Except(selected);

                if (toAdd.Any())
                {
                    var adds = toAdd.Select(id => new RoleClaims { RoleId = role.RoleId, ClaimId = id }).ToList();
                    await db.RoleClaims.AddRangeAsync(adds);
                }

                if (toRemove.Any())
                {
                    var removes = await db.RoleClaims
                                          .Where(rc => rc.RoleId == dto.RoleId && toRemove.Contains(rc.ClaimId))
                                          .ToListAsync();
                    db.RoleClaims.RemoveRange(removes);
                }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return BaseResult<UpdateRoleDto>.Ok(dto);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                return BaseResult<UpdateRoleDto>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }

        public async Task<BaseResult<bool>> DeleteRoleAsync(int roleId, bool softDelete = true)
        {
            try
            {
                var role = await db.Roles.FindAsync(roleId);
                if (role is null) return BaseResult<bool>.Fail("Không tìm thấy vai trò", 404);

                role.DeletedAt = DateTime.UtcNow;

                db.Roles.Update(role);
                await db.SaveChangesAsync();

                return BaseResult<bool>.Ok(true);
            }
            catch (Exception e)
            {
                return BaseResult<bool>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }

        public async Task<BaseResult<List<ClaimDto>>> GetAllClaimsAsync()
        {
            try
            {
                var claims = await db.Claims
                    .OrderBy(o => o.ClaimId)
                    .Select(c => new ClaimDto
                    {
                        ClaimId = c.ClaimId,
                        Claim = c.Claim,
                        Category = c.Category,
                    }).ToListAsync();
                return BaseResult<List<ClaimDto>>.Ok(claims);
            }
            catch (Exception e)
            {
                return BaseResult<List<ClaimDto>>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }

        public async Task<BaseResult<AccountRoleResponseDto>> SetRoleAsync(AccountRoleDto dto)
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var user = await db.Accounts.FindAsync(dto.AccountId);
                if (user is null) return BaseResult<AccountRoleResponseDto>.Fail("Tài khoản không tồn tại", 404);

                var existingAccountRoles = await db.AccountRoles
                                                  .Where(ar => ar.AccountId == dto.AccountId)
                                                  .Select(r => r.RoleId)
                                                  .ToListAsync();

                var selected = dto.RoleId.Distinct().ToList();

                var toAdd = selected.Except(existingAccountRoles);
                var toRemove = existingAccountRoles.Except(selected);

                if (toAdd.Any())
                {
                    var adds = toAdd.Select(id => new AccountRoles { AccountId = dto.AccountId, RoleId = id }).ToList();
                    await db.AccountRoles.AddRangeAsync(adds);
                }

                if (toRemove.Any())
                {
                    var removes = await db.AccountRoles
                                          .Where(ar => ar.AccountId == dto.AccountId && toRemove.Contains(ar.RoleId))
                                          .ToListAsync();
                    db.AccountRoles.RemoveRange(removes);
                }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return BaseResult<AccountRoleResponseDto>.Ok(new AccountRoleResponseDto()
                {
                    RoleId = dto.RoleId,
                    AccountId = dto.AccountId,
                    Username = user.Username,
                });
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                return BaseResult<AccountRoleResponseDto>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }
    }
}
