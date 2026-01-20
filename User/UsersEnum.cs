using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ITSupportServer.src.Modules.User
{
    public class UsersEnum
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum GENDER
        {
            [EnumMember(Value = "Nam")]
            Nam,
            [EnumMember(Value = "Nữ")]
            Nu,
            [EnumMember(Value = "Khác")]
            Other
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum STATUS_EMP
        {
            [EnumMember()]
            Active,
            [EnumMember()]
            InActive
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum EMP_CUS
        {
            [EnumMember(Value = "employee")]
            employee,
            [EnumMember(Value = "customer")]
            customer,
            [EnumMember(Value = "user")]
            user
        }

    }
}
