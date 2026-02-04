using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ItSupportServer.src.Shared.Base
{
    public class BaseEnum
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum ROLE
        {
            [EnumMember(Value = "Quản lý hệ thống")]
            Super_Admin = 0,
            [EnumMember(Value = "Quản lý")]
            Admin = 1,
            [EnumMember(Value = "Nhân viên")]
            Employee = 2,
        }


        //public static string GroupRoleEmployee()
        //{
        //    return $"{RoleUser.Super_Admin},{RoleUser.Admin},{RoleUser.Employee}";
        //}
        //public static string GroupRoleAdmin()
        //{
        //    return $"{RoleUser.Super_Admin},{RoleUser.Admin}";
        //}
    }
    public static class RoleUser
    {
        public const string Super_Admin = "Super_Admin";
        public const string Admin = "Admin";
        public const string Employee = "Employee";
        public const string GroupAdmin = "Super_Admin,Admin";
        public const string GroupEmployee = "Super_Admin,Admin,Employee";
    }

    public static class Permissions
    {
        public static class Departments
        {
            public const string View = "Department.View";
            public const string Create = "Department.Create";
            public const string Edit = "Department.Edit";
            public const string Delete = "Department.Delete";
        }

        public static class Areas
        {
            public const string View = "Area.View";
            public const string Create = "Area.Create";
            public const string Edit = "Area.Edit";
            public const string Delete = "Area.Delete";
        }

        public static class Employees
        {
            public const string View = "Employee.View";
            public const string Create = "Employee.Create";
            public const string Edit = "Employee.Edit";
            public const string Delete = "Employee.Delete";
        }

        public static class Roles
        {
            public const string View = "Role.View";
            public const string Create = "Role.Create";
            public const string Edit = "Role.Edit";
            public const string Delete = "Role.Delete";
            public const string SetRole = "Role.SetRole";
        }

        public static class Devices
        {
            public const string View = "Device.View";
            public const string Create = "Device.Create";
            public const string Edit = "Device.Edit";
            public const string Delete = "Device.Delete";
        }

        public static class DeviceTypes
        {
            public const string View = "DeviceType.View";
            public const string Create = "DeviceType.Create";
            public const string Edit = "DeviceType.Edit";
            public const string Delete = "DeviceType.Delete";
        }

        public static class Issues
        {
            public const string View = "Issue.View";
            public const string Create = "Issue.Create";
            public const string Edit = "Issue.Edit";
            public const string Delete = "Issue.Delete";
        }

        public static class IssueLogs
        {
            public const string View = "IssueLog.View";
            public const string Create = "IssueLog.Create";
            public const string Edit = "IssueLog.Edit";
            public const string Delete = "IssueLog.Delete";
        }
    }

}
