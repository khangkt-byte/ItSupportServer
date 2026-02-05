using FluentValidation;
using ItSupportServer.Data.Models;
using ItSupportServer.Data.Models.Entities;
using ItSupportServer.src.Modules.Authorization;
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
        private readonly IAuthorizationService _authorizationService;

        public RoleService(
            AppDbContext db,
            RoleMapper mapper,
            ILogger<RoleService> logger,
            IValidator<CreateRoleDto> createValidator,
            IValidator<UpdateRoleDto> updateValidator,
            IValidator<AssignRolesDto> assignRolesValidator,
            IAuthorizationService authorizationService)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _assignRolesValidator = assignRolesValidator;
            _authorizationService = authorizationService;
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
            var affectedAccountIds = new List<Guid>();

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

                    // ✅ Get affected accounts for cache invalidation
                    affectedAccountIds = await _db.AccountRoles
                        .Where(ar => ar.RoleId == roleId)
                        .Select(ar => ar.AccountId)
                        .ToListAsync();

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

                // ✅ Invalidate permission cache for all affected accounts
                if (affectedAccountIds.Any())
                {
                    _authorizationService.InvalidatePermissionCache(affectedAccountIds);
                    _logger.LogInformation(
                        "Invalidated permission cache for {Count} accounts affected by role {RoleId} update",
                        affectedAccountIds.Count, roleId);
                }

                _logger.LogInformation("Successfully updated role {RoleId}", roleId);
            }
            else
            {
                _logger.LogInformation("No changes detected for role {RoleId}", roleId);
            }

            return await GetRoleByIdAsync(roleId);
        }

        public async Task DeleteRoleAsync(int roleId, bool softDelete = true)  // ✅ Changed to Task (void)
        {
            _logger.LogInformation("Deleting role {RoleId} | SoftDelete: {SoftDelete}",
                roleId, softDelete);

            var role = await _db.Roles
                .Include(r => r.AccountRoles)
                .FirstOrDefaultAsync(r => r.RoleId == roleId && r.DeletedAt == null);

            if (role == null)
            {
                _logger.LogWarning("Role {RoleId} not found", roleId);
                throw new NotFoundException("Vai trò", roleId);
            }

            // ✅ Check business rules
            if (role.AccountRoles.Any())
            {
                _logger.LogWarning("Role {RoleId}:{Name} in use by {Count} accounts",
                    role.RoleId, role.Name, role.AccountRoles.Count);
                throw new BusinessRuleException(
                    $"Không thể xóa vai trò {role.Name} vì đang được sử dụng bởi {role.AccountRoles.Count} tài khoản",
                    "ROLE_IN_USE");
            }

            // ✅ Perform delete
            if (softDelete)
            {
                role.DeletedAt = DateTime.UtcNow;
                _db.Roles.Update(role);

                await _db.SaveChangesAsync();

                _logger.LogInformation("Soft deleted role {RoleId}:{Name}",
                    role.RoleId, role.Name);
            }
            else
            {
                // Hard delete with transaction
                using var transaction = await _db.Database.BeginTransactionAsync();

                try
                {
                    // Rely on DB cascade or manual cascade
                    _db.Roles.Remove(role);

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Hard deleted role {RoleId}:{Name}",
                        role.RoleId, role.Name);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            // ✅ No return - throw exception on error
        }

        public async Task<BulkDeleteResultDto> DeleteRolesAsync(List<int> roleIds, bool softDelete = true)
        {
            _logger.LogInformation("Batch delete started | Count: {Count} | SoftDelete: {SoftDelete}",
        roleIds?.Count ?? 0, softDelete);

            // ✅ Add input validation
            ArgumentNullException.ThrowIfNull(roleIds);

            if (roleIds.Count == 0)
            {
                throw new Shared.Exceptions.ValidationException("roleIds", "Vui lòng chọn ít nhất một vai trò để xóa");
            }

            // ✅ Remove duplicates
            var uniqueIds = roleIds.Distinct().ToList();

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // ✅ Step 1: Validate ALL items BEFORE any deletion
                var existing = await _db.Roles
                    .Where(r => uniqueIds.Contains(r.RoleId) && r.DeletedAt == null)
                    .Include(r => r.AccountRoles)
                    .ToListAsync();

                var notFoundIds = roleIds.Except(existing.Select(r => r.RoleId)).ToList();
                if (notFoundIds.Any())
                {
                    _logger.LogWarning("Roles not found: {Ids}", string.Join(", ", notFoundIds));
                    throw new NotFoundException($"Vai trò không tồn tại: {string.Join(", ", notFoundIds)}");
                }

                // ✅ Step 2: Check business rules for ALL items
                var rolesInUse = existing.Where(r => r.AccountRoles.Any()).ToList();
                if (rolesInUse.Any())
                {
                    _logger.LogWarning("Roles in use: {Roles}",
                        string.Join(", ", rolesInUse.Select(r => $"{r.RoleId}:{r.Name}")));
                    throw new BusinessRuleException(
                        $"Không thể xóa vai trò {string.Join(", ", rolesInUse.Select(r => r.Name))} vì đang được sử dụng",
                        "ROLE_IN_USE");
                }

                // ✅ Step 3: All checks passed → Delete ALL
                foreach (var role in existing)
                {
                    if (softDelete)
                        role.DeletedAt = DateTime.UtcNow;
                    else
                        _db.Roles.Remove(role);

                    _logger.LogInformation("Deleted role {RoleId}:{Name}", role.RoleId, role.Name);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Batch delete SUCCESS | Count: {Count}", existing.Count);

                return new BulkDeleteResultDto
                {
                    Success = true,
                    DeletedCount = existing.Count,
                    TotalRequested = roleIds.Count,
                    Message = $"Đã xóa {existing.Count} vai trò thành công"
                };
            }
            catch
            {
                await transaction.RollbackAsync();  // ✅ Rollback on ANY error
                throw;
            }
        }

        public async Task<List<ClaimDto>> GetAllClaimsAsync()
        {
            _logger.LogInformation("Fetching all claims");

            var claims = await _mapper.ProjectToClaimDto(_db.Claims
                .OrderBy(c => c.Category)
                    .ThenBy(c => c.Claim)
                .AsNoTracking())
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

                // ✅ Invalidate permission cache (already has field now)
                _authorizationService.InvalidatePermissionCache(dto.AccountId);
                _logger.LogInformation(
                    "Invalidated permission cache for account {AccountId} after role assignment",
                    dto.AccountId);

                _logger.LogInformation("Successfully assigned {Count} roles to account {AccountId}",
                    dto.RoleIds.Count, dto.AccountId);

                // Return updated account with roles
                var updatedRoles = await _mapper.ProjectToRoleDto(_db.Roles
                    .Where(r => dto.RoleIds.Contains(r.RoleId))
                    .AsNoTracking())
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
