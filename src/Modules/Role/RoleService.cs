using FluentValidation;
using ItSupportServer.Data.Models;
using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Exceptions;
using ItSupportServer.src.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ItSupportServer.src.Modules.Role
{
    public class RoleService : IRoleService
    {
        private readonly AppDbContext _db;
        private readonly RoleMapper _mapper;
        private readonly ILogger<RoleService> _logger;
        private readonly IValidator<CreateRoleDto> _createValidator;
        private readonly IValidator<UpdateRoleDto> _updateValidator;
        private readonly IValidator<AssignRolesDto> _assignRolesValidator;

        public RoleService(
            AppDbContext db,
            RoleMapper mapper,
            ILogger<RoleService> logger,
            IValidator<CreateRoleDto> createValidator,
            IValidator<UpdateRoleDto> updateValidator,
            IValidator<AssignRolesDto> assignRolesValidator)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _assignRolesValidator = assignRolesValidator;
        }

        public async Task<PaginatedResult<RoleDto>> GetRolesAsync(QueryParameters parameters)
        {
            _logger.LogInformation("Fetching roles with search: {Search}, page: {Page}",
                parameters.Search, parameters.Page);

            var query = _mapper.ProjectToRoleDto(_db.Roles
                .Include(r => r.RoleClaims)
                    .ThenInclude(rc => rc.Claim)
                .Where(r => r.DeletedAt == null)
                .AsNoTracking());

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                query = query.Where(r => r.Name.Contains(parameters.Search));
            }

            var result = await query.ToPaginatedResultAsync(parameters, defaultSortField: "Name");

            _logger.LogInformation("Retrieved {Count} roles", result.TotalCount);

            return result;
        }

        public async Task<RoleDto> GetRoleByIdAsync(int roleId)
        {
            _logger.LogInformation("Fetching role {RoleId}", roleId);

            var role = await _mapper.ProjectToRoleDto(_db.Roles
                .Include(r => r.RoleClaims)
                    .ThenInclude(rc => rc.Claim)
                .Where(r => r.RoleId == roleId && r.DeletedAt == null)
                .AsNoTracking())
                .FirstOrDefaultAsync();

            if (role is null)
            {
                _logger.LogWarning("Role {RoleId} not found", roleId);
                throw new NotFoundException("Vai trò", roleId);
            }

            return role;
        }

        public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto)
        {
            _logger.LogInformation("Creating new role: {Name}", dto.Name);

            var validationResult = await _createValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            // Check duplicate name
            var nameExists = await _db.Roles
                .Where(r => r.Name == dto.Name && r.DeletedAt == null)
                .AnyAsync();

            if (nameExists)
            {
                _logger.LogWarning("Role with name {Name} already exists", dto.Name);
                throw new ConflictException("Vai trò", dto.Name);
            }

            // Validate claim IDs exist
            if (dto.ClaimIds != null && dto.ClaimIds.Any())
            {
                var existingClaims = await _db.Claims
                    .Where(c => dto.ClaimIds.Contains(c.ClaimId))
                    .Select(c => c.ClaimId)
                    .ToListAsync();

                var missingClaims = dto.ClaimIds.Except(existingClaims).ToList();
                if (missingClaims.Any())
                {
                    throw new NotFoundException($"Claims không tồn tại: {string.Join(", ", missingClaims)}");
                }
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var newRole = _mapper.MapToRole(dto);
                // RoleId auto-increment by database
                // CreatedAt set by interceptor

                await _db.Roles.AddAsync(newRole);
                await _db.SaveChangesAsync();

                // Add role-claim relationships
                if (dto.ClaimIds != null && dto.ClaimIds.Any())
                {
                    var roleClaims = dto.ClaimIds
                        .Distinct()
                        .Select(claimId => new RoleClaims
                        {
                            RoleId = newRole.RoleId,
                            ClaimId = claimId
                        });

                    await _db.RoleClaims.AddRangeAsync(roleClaims);
                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                _logger.LogInformation("Successfully created role {RoleId}", newRole.RoleId);

                return await GetRoleByIdAsync(newRole.RoleId);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<RoleDto> UpdateRoleAsync(int roleId, UpdateRoleDto dto)
        {
            _logger.LogInformation("Updating role {RoleId}", roleId);

            var validationResult = await _updateValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            var role = await _db.Roles.FindAsync(roleId);

            if (role is null || role.DeletedAt != null)
            {
                _logger.LogWarning("Role {RoleId} not found", roleId);
                throw new NotFoundException("Vai trò", roleId);
            }

            bool hasChanges = false;

            // Update Name
            if (dto.Name != null && role.Name != dto.Name)
            {
                var nameExists = await _db.Roles
                    .Where(r => r.Name == dto.Name && r.RoleId != roleId && r.DeletedAt == null)
                    .AnyAsync();

                if (nameExists)
                {
                    throw new ConflictException("Vai trò", dto.Name);
                }

                role.Name = dto.Name;
                hasChanges = true;
            }

            // Update Description
            if (dto.Description != null && role.Description != dto.Description)
            {
                role.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description;
                hasChanges = true;
            }

            // Update Claims
            if (dto.ClaimIds != null)
            {
                // Validate claim IDs
                var existingClaims = await _db.Claims
                    .Where(c => dto.ClaimIds.Contains(c.ClaimId))
                    .Select(c => c.ClaimId)
                    .ToListAsync();

                var missingClaims = dto.ClaimIds.Except(existingClaims).ToList();
                if (missingClaims.Any())
                {
                    throw new NotFoundException($"Claims không tồn tại: {string.Join(", ", missingClaims)}");
                }

                // Get current claims
                var currentClaimIds = await _db.RoleClaims
                    .Where(rc => rc.RoleId == roleId)
                    .Select(rc => rc.ClaimId)
                    .ToListAsync();

                var selected = dto.ClaimIds.Distinct().ToList();

                var toAdd = selected.Except(currentClaimIds).ToList();
                var toRemove = currentClaimIds.Except(selected).ToList();

                if (toAdd.Any() || toRemove.Any())
                {
                    hasChanges = true;

                    // Add new claims
                    if (toAdd.Any())
                    {
                        var newRoleClaims = toAdd.Select(claimId => new RoleClaims
                        {
                            RoleId = roleId,
                            ClaimId = claimId
                        });

                        await _db.RoleClaims.AddRangeAsync(newRoleClaims);
                    }

                    // Remove old claims
                    if (toRemove.Any())
                    {
                        var removeRoleClaims = await _db.RoleClaims
                            .Where(rc => rc.RoleId == roleId && toRemove.Contains(rc.ClaimId))
                            .ToListAsync();

                        _db.RoleClaims.RemoveRange(removeRoleClaims);
                    }
                }
            }

            if (hasChanges)
            {
                // UpdatedAt set automatically by interceptor
                await _db.SaveChangesAsync();
                _logger.LogInformation("Successfully updated role {RoleId}", roleId);
            }
            else
            {
                _logger.LogInformation("No changes detected for role {RoleId}", roleId);
            }

            return await GetRoleByIdAsync(roleId);
        }

        public async Task<bool> DeleteRoleAsync(int roleId, bool softDelete = true)
        {
            return await DeleteRolesAsync([roleId], softDelete);
        }

        public async Task<bool> DeleteRolesAsync(List<int> roleIds, bool softDelete = true)
        {
            _logger.LogInformation("Deleting {Count} roles (soft: {SoftDelete})",
                roleIds?.Count ?? 0, softDelete);

            ArgumentNullException.ThrowIfNull(roleIds);

            if (roleIds.Count == 0)
            {
                throw new Shared.Exceptions.ValidationException("roleIds",
                    "Vui lòng chọn vai trò để xóa");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var existing = await _db.Roles
                    .Where(r => roleIds.Contains(r.RoleId) && r.DeletedAt == null)
                    .ToListAsync();

                if (existing.Count == 0)
                {
                    throw new NotFoundException("Không tìm thấy vai trò để xóa");
                }

                // Protect system roles (RoleId < 5 assumed to be system roles)
                if (existing.Any(r => r.RoleId < 5))
                {
                    throw new BusinessRuleException("Không thể xóa vai trò hệ thống");
                }

                // Check if roles are in use
                var rolesInUse = await _db.AccountRoles
                    .Where(ar => roleIds.Contains(ar.RoleId))
                    .Select(ar => ar.RoleId)
                    .Distinct()
                    .ToListAsync();

                if (rolesInUse.Any())
                {
                    var usedRoleNames = existing
                        .Where(r => rolesInUse.Contains(r.RoleId))
                        .Select(r => r.Name)
                        .ToList();

                    _logger.LogWarning("Cannot delete roles {Roles} - in use",
                        string.Join(", ", usedRoleNames));

                    throw new BusinessRuleException(
                        $"Không thể xóa vai trò {string.Join(", ", usedRoleNames)} vì đang được sử dụng");
                }

                if (softDelete)
                {
                    foreach (var item in existing)
                    {
                        item.DeletedAt = DateTime.UtcNow;
                    }
                    _db.Roles.UpdateRange(existing);
                }
                else
                {
                    // Remove role-claim relationships first
                    var roleClaims = await _db.RoleClaims
                        .Where(rc => roleIds.Contains(rc.RoleId))
                        .ToListAsync();

                    _db.RoleClaims.RemoveRange(roleClaims);
                    _db.Roles.RemoveRange(existing);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully deleted {Count} roles", existing.Count);

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<ClaimDto>> GetAllClaimsAsync()
        {
            _logger.LogInformation("Fetching all claims");

            var claims = await _db.Claims
                .OrderBy(c => c.Category)
                    .ThenBy(c => c.Claim)
                .Select(c => new ClaimDto
                {
                    ClaimId = c.ClaimId,
                    Claim = c.Claim,
                    Category = c.Category
                })
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} claims", claims.Count);

            return claims;
        }

        public async Task<AccountRolesDto> AssignRolesToAccountAsync(AssignRolesDto dto)
        {
            _logger.LogInformation("Assigning roles to account {AccountId}", dto.AccountId);

            var validationResult = await _assignRolesValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            var account = await _db.Accounts
                .Include(a => a.AccountRoles)
                    .ThenInclude(ar => ar.Role)
                .FirstOrDefaultAsync(a => a.AccountId == dto.AccountId && a.DeletedAt == null);

            if (account is null)
            {
                throw new NotFoundException("Tài khoản", dto.AccountId);
            }

            // Validate all roles exist
            var existingRoles = await _db.Roles
                .Where(r => dto.RoleIds.Contains(r.RoleId) && r.DeletedAt == null)
                .Select(r => r.RoleId)
                .ToListAsync();

            var missingRoles = dto.RoleIds.Except(existingRoles).ToList();
            if (missingRoles.Any())
            {
                throw new NotFoundException($"Roles không tồn tại: {string.Join(", ", missingRoles)}");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // Get current role IDs
                var currentRoleIds = account.AccountRoles
                    .Select(ar => ar.RoleId)
                    .ToList();

                var selected = dto.RoleIds.Distinct().ToList();

                var toAdd = selected.Except(currentRoleIds).ToList();
                var toRemove = currentRoleIds.Except(selected).ToList();

                // Add new roles
                if (toAdd.Any())
                {
                    var newAccountRoles = toAdd.Select(roleId => new AccountRoles
                    {
                        AccountId = dto.AccountId,
                        RoleId = roleId
                    });

                    await _db.AccountRoles.AddRangeAsync(newAccountRoles);
                }

                // Remove old roles
                if (toRemove.Any())
                {
                    var removeAccountRoles = await _db.AccountRoles
                        .Where(ar => ar.AccountId == dto.AccountId && toRemove.Contains(ar.RoleId))
                        .ToListAsync();

                    _db.AccountRoles.RemoveRange(removeAccountRoles);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // ✅ Invalidate permission cache
                _authorizationService.InvalidatePermissionCache(dto.AccountId);

                _logger.LogInformation("Successfully assigned {Count} roles to account {AccountId}",
                    dto.RoleIds.Count, dto.AccountId);

                // Return updated account with roles
                var updatedRoles = await _db.Roles
                    .Where(r => dto.RoleIds.Contains(r.RoleId))
                    .Select(r => new RoleDto
                    {
                        RoleId = r.RoleId,
                        Name = r.Name,
                        Description = r.Description,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt
                    })
                    .ToListAsync();

                return new AccountRolesDto
                {
                    AccountId = dto.AccountId,
                    Username = account.Username,
                    Roles = updatedRoles
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
