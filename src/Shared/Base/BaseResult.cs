using Microsoft.AspNetCore.Mvc;

namespace ItSupportServer.src.Shared.Base
{
    public class BaseResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public T? Item { get; set; }

        public static BaseResult<T> Ok(T item, int statusCode = 200, string message = "Success") =>
            new BaseResult<T> { Success = true, Message = message, StatusCode = statusCode, Item = item };

        public static BaseResult<T> Fail(string message, int statusCode = 400, T? data = default) =>
            new BaseResult<T> { Success = false, Message = message, StatusCode = statusCode, Item = data };
    }

    public static class MyStatusCodeBase
    {
        public static ActionResult MyStatusCode<T>(this ControllerBase controller, BaseResult<T> result)
        {
            return controller.StatusCode(result.StatusCode, new
            {
                success = result.Success,
                message = result.Message,
                item = result.Item
            });
        }
    }
}
