using Microsoft.AspNetCore.Authorization;

namespace ItSupportServer.src.Modules.Authorization
{
    // Requirement để chứa thông tin quyền cần kiểm tra
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }
        public PermissionRequirement(string permission) => Permission = permission;
    }
}
