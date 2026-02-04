using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ItSupportServer.src.Shared.Base
{
    public class BaseEnum
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum STATUS
        {
            [EnumMember(Value = "Đợi duyệt")]
            Waiting = 0,
            [EnumMember(Value = "Chờ xử lý")]
            Pending = 1,
            [EnumMember(Value = "Xác minh thanh toán")]
            Confirm = 2,
            [EnumMember(Value = "Hoàn thành")]
            Success = 3,
            [EnumMember(Value = "Hủy")]
            Cancel = 4
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum METHOD_PAYMENT
        {
            [EnumMember(Value = "Tiền mặt")]
            Cash = 1,
            [EnumMember(Value = "Chuyển khoản")]
            BankTransfer = 2,
            [EnumMember(Value = "Ví điện tử")]
            EWallet = 3
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum ROLE
        {
            [EnumMember(Value = "Quản lý hệ thống")]
            Supper_Admin = 0,
            [EnumMember(Value = "Quản lý")]
            Admin = 1,
            [EnumMember(Value = "Nhân viên")]
            Employee = 2,
            [EnumMember(Value = "Khách hàng")]
            Customer = 3
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
