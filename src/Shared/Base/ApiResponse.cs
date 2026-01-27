using System.Text.Json.Serialization;

namespace ItSupportServer.src.Shared.Base
{
    /// <summary>
    /// Standard API response wrapper following RFC 7807 and Microsoft REST API Guidelines
    /// </summary>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Human-readable message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Response data payload
        /// </summary>
        [JsonPropertyName("data")]
        public T? Data { get; set; }

        /// <summary>
        /// Timestamp of the response (ISO 8601 format)
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Unique identifier for request tracking
        /// </summary>
        public string? TraceId { get; set; }

        /// <summary>
        /// Detailed error information (only for failures)
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ErrorDetails? Error { get; set; }

        // Factory methods
        public static ApiResponse<T> Ok(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> Fail(string message, string? errorCode = null, Dictionary<string, string[]>? validationErrors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Error = new ErrorDetails
                {
                    Code = errorCode ?? "OPERATION_FAILED",
                    ValidationErrors = validationErrors
                }
            };
        }

        public static ApiResponse<T> NotFound(string message = "Resource not found")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Error = new ErrorDetails { Code = "NOT_FOUND" }
            };
        }

        public static ApiResponse<T> Unauthorized(string message = "Unauthorized access")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Error = new ErrorDetails { Code = "UNAUTHORIZED" }
            };
        }

        public static ApiResponse<T> Forbidden(string message = "Forbidden")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Error = new ErrorDetails { Code = "FORBIDDEN" }
            };
        }

        public static ApiResponse<T> BadRequest(string message, Dictionary<string, string[]>? validationErrors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Error = new ErrorDetails
                {
                    Code = "BAD_REQUEST",
                    ValidationErrors = validationErrors
                }
            };
        }
    }

    /// <summary>
    /// Detailed error information following RFC 7807 Problem Details
    /// </summary>
    public class ErrorDetails
    {
        /// <summary>
        /// Machine-readable error code
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Validation errors (field-level)
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string[]>? ValidationErrors { get; set; }

        /// <summary>
        /// Additional error metadata
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Extension methods for converting ApiResponse to IActionResult
    /// </summary>
    public static class ApiResponseExtensions
    {
        public static IActionResult ToActionResult<T>(this ApiResponse<T> response)
        {
            var statusCode = response.Success ? 200 : DetermineStatusCode(response.Error?.Code);
            
            return new ObjectResult(response)
            {
                StatusCode = statusCode
            };
        }

        private static int DetermineStatusCode(string? errorCode)
        {
            return errorCode switch
            {
                "NOT_FOUND" => 404,
                "UNAUTHORIZED" => 401,
                "FORBIDDEN" => 403,
                "BAD_REQUEST" => 400,
                "VALIDATION_FAILED" => 422,
                _ => 500
            };
        }
    }
}