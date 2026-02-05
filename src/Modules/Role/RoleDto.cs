using System.ComponentModel.DataAnnotations;

namespace ItSupportServer.src.Modules.Role
{
    /// <summary>
    /// Role response DTO
    /// </summary>
    public record RoleDto
    {
        public int RoleId { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public List<ClaimDto>? Claims { get; init; }
    }

    /// <summary>
    /// Claim information DTO
    /// </summary>
    public record ClaimDto
    {
        public int ClaimId { get; init; }
        public required string Claim { get; init; }
        public string? Category { get; init; }
    }

    /// <summary>
    /// Create role request DTO
    /// </summary>
    public record CreateRoleDto
    {
        public required string Name { get; init; }
        public string? Description { get; init; }

        /// <summary>
        /// List of claim IDs to assign to this role
        /// </summary>
        public List<int>? ClaimIds { get; init; }
    }

    /// <summary>
    /// Update role request DTO (partial updates supported)
    /// </summary>
    public record UpdateRoleDto
    {
        public string? Name { get; init; }
        public string? Description { get; init; }

        /// <summary>
        /// List of claim IDs (null = don't update, empty = clear all)
        /// </summary>
        public List<int>? ClaimIds { get; init; }
    }

    /// <summary>
    /// Assign roles to account request DTO
    /// </summary>
    public record AssignRolesDto
    {
        public required Guid AccountId { get; init; }
        public required List<int> RoleIds { get; init; } 
    }

    /// <summary>
    /// Account with assigned roles response DTO
    /// </summary>
    public record AccountRolesDto
    {
        public Guid AccountId { get; init; }
        public required string Username { get; init; }
        public List<RoleDto> Roles { get; init; } = [];
    }
}
