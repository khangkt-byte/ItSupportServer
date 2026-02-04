using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ItSupportServer.src.Modules.Authorization
{
    /// <summary>
    /// Dynamic permission policy provider
    /// Pattern: Convention-based policy naming
    /// Usage: [HasPermission("Department.View")] → Creates policy automatically
    /// Reference: ASP.NET Core Authorization documentation
    /// </summary>
    public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
            : base(options)
        {
        }

        /// <summary>
        /// Dynamically create policies based on permission names
        /// Pattern: Fallback policy provider
        /// </summary>
        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // ✅ Try to get existing policy first
            var policy = await base.GetPolicyAsync(policyName);
            if (policy != null)
            {
                return policy;
            }

            // ✅ Create dynamic policy for permission strings
            return new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();
        }
    }
}