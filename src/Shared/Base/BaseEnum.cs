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
            Supper_Admin = 0,
            [EnumMember(Value = "Quản lý")]
            Admin = 1,
            [EnumMember(Value = "Nhân viên")]
            Employee = 2,
        }


        public static string GroupRoleEmployee()
        {
            return $"{RoleUser.Supper_Admin},{RoleUser.Admin},{RoleUser.Employee}";
        }
        public static string GroupRoleAdmin()
        {
            return $"{RoleUser.Supper_Admin},{RoleUser.Admin}";

        }
    }
    public static class RoleUser
    {
        public const string Supper_Admin = "Supper_Admin";
        public const string Admin = "Admin";
        public const string Employee = "Employee";
        public const string GroupAdmin = "Supper_Admin,Admin";
        public const string GroupEmployee = "Supper_Admin,Admin,Employee";
    }
}
