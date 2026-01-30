using Microsoft.AspNetCore.Authorization;

namespace ItSupportServer.src.Modules.Authorization
{
    /// <summary>
    /// Authorization requirement for permission-based access control
    /// Pattern: Policy-based authorization (ASP.NET Core)
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission ?? throw new ArgumentNullException(nameof(permission));
        }
    }
}
