using System.ComponentModel.DataAnnotations;

namespace ItSupportServer.src.Modules.Role
{
    public record CreateRoleDto(
        string Name,
        string? Description,
        List<int>? ClaimIds
        );

    public record UpdateRoleDto(
        int RoleId,
        string Name,
        string? Description,
        List<int>? ClaimIds
        );

    public record RolesDto
    {
        public int RoleId { get; init; }
        public string Name { get; init; }
        public string? Description { get; init; }
        public List<ClaimDto>? Claims { get; init; }
    };

    public record ClaimDto(
        int ClaimId,
        string Claim,
        string? Category
        );

    public record AccountRoleDto(
        Guid AccountId,
        List<int> RoleId
        );

    public record AccountRoleResponseDto(
        Guid AccountId,
        string? Username,
        List<int> RoleId
        );
}
