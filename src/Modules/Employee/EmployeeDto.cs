using ItSupportServer.src.Modules.Role;
using ItSupportServer.src.Shared.Helpers;
using System.ComponentModel.DataAnnotations;
using static ItSupportServer.src.Modules.User.UserEnum;

namespace ItSupportServer.src.Modules.Employee
{
    /// <summary>
    /// Employee response DTO
    /// </summary>
    public record EmployeeDto
    {
        public Guid EmpId { get; init; }
        public string? EmpCode { get; init; }
        public required string FullName { get; init; }
        public string? PhoneNumber { get; init; }
        public string? Email { get; init; }
        public int DptId { get; init; }
        public int AreaId { get; init; }
        public string? Position { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    /// <summary>
    /// List view of employee (minimal info)
    /// </summary>
    public record ListEmployeeDto
    {
        public Guid EmpId { get; init; }
        public string? EmpCode { get; init; }
        public required string FullName { get; init; }
        public string? Email { get; init; }
        public string? PhoneNumber { get; init; }
        public string? Position { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    /// <summary>
    /// Detailed employee info with roles
    /// </summary>
    public record DetailEmployeeDto
    {
        public Guid EmpId { get; init; }
        public string? EmpCode { get; init; }
        public required string FullName { get; init; }
        public string? PhoneNumber { get; init; }
        public string? Email { get; init; }
        public int DptId { get; init; }
        public int AreaId { get; init; }
        public string? Position { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public List<RoleDto>? Roles { get; init; }
    }

    /// <summary>
    /// User profile DTO (self-service view)
    /// </summary>
    public record ProfileDto
    {
        public Guid EmpId { get; init; }
        public string? EmpCode { get; init; }
        public required string FullName { get; init; }
        public string? PhoneNumber { get; init; }
        public string? Email { get; init; }
        public int DptId { get; init; }
        public int AreaId { get; init; }
        public string? Position { get; init; }
        public string? Username { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    /// <summary>
    /// Create employee request DTO
    /// </summary>
    public record CreateEmployeeDto
    {
        public string? EmpCode { get; init; }
        public required string FullName { get; init; }
        public string? PhoneNumber { get; init; }
        public string? Email { get; init; }
        public int DptId { get; init; }
        public int AreaId { get; init; }
        public string? Position { get; init; }
    }

    /// <summary>
    /// Update employee request DTO (partial updates supported)
    /// </summary>
    public record UpdateEmployeeDto
    {
        public string? EmpCode { get; init; }
        public string? FullName { get; init; }
        public string? PhoneNumber { get; init; }
        public string? Email { get; init; }
        public int? DptId { get; init; }
        public int? AreaId { get; init; }
        public string? Position { get; init; }
    }

    /// <summary>
    /// Update user profile DTO (self-service)
    /// </summary>
    public record UpdateProfileDto
    {
        public required string FullName { get; init; }
        public string? PhoneNumber { get; init; }
        public string? Email { get; init; }
    }
}