using System.Reflection;

namespace ItSupportServer.src.Modules.Authorization
{
    /// <summary>
    /// Permission constants for the application
    /// Pattern: Static constants with reflection-based discovery
    /// Usage: [HasPermission(Permissions.DepartmentClaims.View)]
    /// Reference: IdentityServer4, Auth0, Stripe SDK patterns
    /// </summary>
    public static class Permissions
    {
        // ===== SYSTEM PERMISSIONS =====

        /// <summary>
        /// Super admin permission - bypasses all checks
        /// Security: Use sparingly, only for system administrators
        /// </summary>
        public const string AdminClaim = "Admin";

        // ===== MODULE PERMISSIONS =====

        public static class DepartmentClaims
        {
            public const string View = "Department.View";
            public const string Create = "Department.Create";
            public const string Edit = "Department.Edit";
            public const string Delete = "Department.Delete";
        }

        public static class AreaClaims
        {
            public const string View = "Area.View";
            public const string Create = "Area.Create";
            public const string Edit = "Area.Edit";
            public const string Delete = "Area.Delete";
        }

        public static class EmployeeClaims
        {
            public const string View = "Employee.View";
            public const string Create = "Employee.Create";
            public const string Edit = "Employee.Edit";
            public const string Delete = "Employee.Delete";
        }

        public static class AccountClaims
        {
            public const string View = "Account.View";
            public const string Create = "Account.Create";
            public const string Edit = "Account.Edit";
            public const string Delete = "Account.Delete";
            public const string ResetPassword = "Account.ResetPassword";
            public const string Lock = "Account.Lock";
        }

        public static class RoleClaims
        {
            public const string View = "Role.View";
            public const string Create = "Role.Create";
            public const string Edit = "Role.Edit";
            public const string Delete = "Role.Delete";
            public const string SetRole = "Role.SetRole";
        }

        public static class DeviceClaims
        {
            public const string View = "Device.View";
            public const string Create = "Device.Create";
            public const string Edit = "Device.Edit";
            public const string Delete = "Device.Delete";
        }

        public static class DeviceTypeClaims
        {
            public const string View = "DeviceType.View";
            public const string Create = "DeviceType.Create";
            public const string Edit = "DeviceType.Edit";
            public const string Delete = "DeviceType.Delete";
        }

        public static class IssueClaims
        {
            public const string View = "Issue.View";
            public const string Create = "Issue.Create";
            public const string Edit = "Issue.Edit";
            public const string Delete = "Issue.Delete";
        }

        public static class CauseClaims
        {
            public const string View = "Cause.View";
            public const string Create = "Cause.Create";
            public const string Edit = "Cause.Edit";
            public const string Delete = "Cause.Delete";
        }

        public static class IssueLogClaims
        {
            public const string View = "IssueLog.View";
            public const string Create = "IssueLog.Create";
            public const string Edit = "IssueLog.Edit";
            public const string Delete = "IssueLog.Delete";
            public const string Import = "IssueLog.Import";
            public const string Export = "IssueLog.Export";
        }

        // ===== REFLECTION-BASED HELPERS (Auto-Discovery) =====

        private static List<string>? _cachedAllPermissions;
        private static Dictionary<string, List<string>>? _cachedByModule;

        /// <summary>
        /// Tự động lấy tất cả permissions (cho Admin Panel)
        /// Pattern: Reflection-based discovery (IdentityServer4 pattern)
        /// Performance: ~2ms first call, then cached
        /// Maintainability: ✅ Auto-updates when adding new permissions
        /// </summary>
        public static List<string> GetAllPermissions()
        {
            if (_cachedAllPermissions != null)
                return _cachedAllPermissions;

            _cachedAllPermissions = GetPermissionsByModule()
                .SelectMany(kvp => kvp.Value)
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            return _cachedAllPermissions;
        }

        /// <summary>
        /// Tự động nhóm permissions theo Module cho UI
        /// Pattern: Reflection-based categorization
        /// Use case: Admin panel permission management, role editor
        /// Performance: Cached after first call
        /// </summary>
        public static Dictionary<string, List<string>> GetPermissionsByModule()
        {
            if (_cachedByModule != null)
                return _cachedByModule;

            var result = new Dictionary<string, List<string>>();
            var type = typeof(Permissions);

            // 1. Get system-level claims (direct constants in Permissions class)
            var systemClaims = type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                .Select(f => f.GetRawConstantValue()?.ToString())
                .Where(v => !string.IsNullOrEmpty(v))
                .ToList();

            if (systemClaims.Count > 0)
            {
                result["System"] = systemClaims!;
            }

            // 2. Get module-level claims (nested classes like DepartmentClaims)
            var moduleTypes = type.GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

            foreach (var moduleType in moduleTypes)
            {
                var moduleName = moduleType.Name.Replace("Claims", ""); // "DepartmentClaims" → "Department"

                var claims = moduleType
                    .GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                    .Select(f => f.GetRawConstantValue()?.ToString())
                    .Where(v => !string.IsNullOrEmpty(v))
                    .OrderBy(v => v)
                    .ToList();

                if (claims.Count > 0)
                {
                    result[moduleName] = claims!;
                }
            }

            _cachedByModule = result;
            return result;
        }

        /// <summary>
        /// Get permissions for specific module
        /// </summary>
        public static List<string> GetModulePermissions(string moduleName)
        {
            var allPermissions = GetPermissionsByModule();
            return allPermissions.TryGetValue(moduleName, out var permissions)
                ? permissions
                : new List<string>();
        }

        /// <summary>
        /// Check if a permission string is valid
        /// </summary>
        public static bool IsValidPermission(string permission)
        {
            return GetAllPermissions().Contains(permission);
        }

        /// <summary>
        /// Clear cached reflection results (for unit tests)
        /// </summary>
        public static void ClearCache()
        {
            _cachedAllPermissions = null;
            _cachedByModule = null;
        }
    }
}
