using Microsoft.AspNetCore.Authorization;

namespace ItSupportServer.src.Shared.Attributes
{
    /// <summary>
    /// Permission-based authorization attribute
    /// Pattern: Policy-based authorization (ASP.NET Core)
    /// 
    /// USAGE EXAMPLES:
    /// 
    /// Single Permission (AND logic):
    /// [HasPermission(Permissions.DepartmentClaims.View)]
    /// 
    /// Multiple Permissions (OR logic):
    /// [HasPermission(Permissions.DepartmentClaims.View)]
    /// [HasPermission(Permissions.DepartmentClaims.Edit)]
    /// → User needs View OR Edit
    /// 
    /// Multiple Different Attributes (AND logic):
    /// [Authorize(Roles = "Manager")]
    /// [HasPermission(Permissions.DepartmentClaims.View)]
    /// → User needs Manager role AND View permission
    /// 
    /// Reference: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/simple
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Create permission attribute
        /// </summary>
        /// <param name="permission">Permission constant (e.g., "Department.View")</param>
        public HasPermissionAttribute(string permission) : base(permission)
        {
        }
    }
}