using System.Text.Json;

namespace ItSupportServer.src.Shared.Helper
{
    public class HandleJson
    {
        public static Dictionary<string, object>? StringToObj(string? diaChi)
        {
            return diaChi == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(diaChi);
        }

        public static string ObjToString(Dictionary<string, object> obj)
        {
            return JsonSerializer.Serialize(obj);
        }
    }
}
