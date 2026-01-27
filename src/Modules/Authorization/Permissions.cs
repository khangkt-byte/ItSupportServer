namespace ItSupportServer.src.Modules.Authorization
{
    public static class Permissions
    {
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
            //public const string ResetPassword = "Account.ResetPassword";
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
        }
    }
}
