using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ItSupportServer.src.Modules.Authorization
{
    /// <summary>
    /// Custom policy provider for dynamic permission policies
    /// Pattern: Convention-based policy creation
    /// Purpose: Auto-create policies from permission strings
    /// </summary>
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return _fallbackPolicyProvider.GetDefaultPolicyAsync();
        }

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            return _fallbackPolicyProvider.GetFallbackPolicyAsync();
        }

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // If policy name looks like a permission (e.g., "Employee.View")
            // create a policy with PermissionRequirement
            if (!string.IsNullOrEmpty(policyName) && policyName.Contains('.'))
            {
                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new PermissionRequirement(policyName))
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            // Otherwise use default policy provider
            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}
