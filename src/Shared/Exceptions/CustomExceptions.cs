namespace ItSupportServer.src.Shared.Exceptions
{
    /// <summary>
    /// Base exception for all application exceptions
    /// Pattern: Error enrichment with metadata
    /// Reference: Microsoft REST API Guidelines
    /// </summary>
    public abstract class AppException : Exception
    {
        public string Code { get; }
        public Dictionary<string, object>? Metadata { get; }

        protected AppException(string message, string code, Dictionary<string, object>? metadata = null)
            : base(message)
        {
            Code = code;
            Metadata = metadata;
        }
    }

    /// <summary>
    /// Thrown when a resource is not found (404)
    /// </summary>
    public class NotFoundException : AppException
    {
        public NotFoundException(string resourceName, object resourceId)
            : base(
                $"{resourceName} với ID '{resourceId}' không tồn tại",
                "NOT_FOUND",
                new Dictionary<string, object>
                {
                    ["resourceName"] = resourceName,
                    ["resourceId"] = resourceId
                })
        {
        }

        public NotFoundException(string message)
            : base(message, "NOT_FOUND")
        {
        }
    }

    /// <summary>
    /// Thrown when validation fails (400)
    /// </summary>
    public class ValidationException : AppException
    {
        public Dictionary<string, string[]> Errors { get; }

        public ValidationException(Dictionary<string, string[]> errors)
            : base("Một hoặc nhiều lỗi xác thực xảy ra", "VALIDATION_FAILED")
        {
            Errors = errors;
        }

        public ValidationException(string field, string error)
            : this(new Dictionary<string, string[]>
            {
                [field] = new[] { error }
            })
        {
        }
    }

    /// <summary>
    /// Thrown when authentication fails (401)
    /// </summary>
    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message = "Xác thực thất bại")
            : base(message, "UNAUTHORIZED")
        {
        }
    }

    /// <summary>
    /// Thrown when authorization fails (403)
    /// </summary>
    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message = "Bạn không có quyền truy cập tài nguyên này")
            : base(message, "FORBIDDEN")
        {
        }
    }

    /// <summary>
    /// Thrown when there's a conflict (409) - e.g., duplicate entry
    /// </summary>
    public class ConflictException : AppException
    {
        public ConflictException(string message)
            : base(message, "CONFLICT")
        {
        }

        public ConflictException(string resourceName, string conflictingValue)
            : base(
                $"{resourceName} '{conflictingValue}' đã tồn tại",
                "CONFLICT",
                new Dictionary<string, object>
                {
                    ["resourceName"] = resourceName,
                    ["conflictingValue"] = conflictingValue
                })
        {
        }
    }

    /// <summary>
    /// Thrown for business rule violations (422)
    /// </summary>
    public class BusinessRuleException : AppException
    {
        public BusinessRuleException(string message, string code = "BUSINESS_RULE_VIOLATION")
            : base(message, code)
        {
        }
    }

    public class OtpRequiredException : AppException
    {
        public Guid AccountId { get; }

        public OtpRequiredException(Guid accountId)
            : base("Tài khoản chưa xác minh OTP", "OTP_REQUIRED")
        {
            AccountId = accountId;
        }
    }

    /// <summary>
    /// Enhanced TooManyAttemptsException with Retry-After support
    /// Pattern: RFC 7231 compliant
    /// </summary>
    public class TooManyAttemptsException : AppException
    {
        public int? RetryAfterSeconds { get; }

        public TooManyAttemptsException(string message, int? retryAfterSeconds = null)
            : base(message, "TOO_MANY_ATTEMPTS", retryAfterSeconds.HasValue
                ? new Dictionary<string, object> { ["retryAfter"] = retryAfterSeconds.Value }
                : null)
        {
            RetryAfterSeconds = retryAfterSeconds;
        }
    }

    /// <summary>
    /// Batch operation exception (207 Multi-Status)
    /// Pattern: Google Batch API, Microsoft Graph Batch
    /// </summary>
    public class BatchOperationException : AppException
    {
        public List<BatchItemResult> Results { get; }

        public BatchOperationException(List<BatchItemResult> results)
            : base(
                $"Batch operation completed with {results.Count(r => !r.Success)} failures",
                "BATCH_OPERATION_ERROR",
                new Dictionary<string, object>
                {
                    ["totalItems"] = results.Count,
                    ["successCount"] = results.Count(r => r.Success),
                    ["failureCount"] = results.Count(r => !r.Success)
                })
        {
            Results = results;
        }
    }

    /// <summary>
    /// Result for individual item in batch operation
    /// </summary>
    public class BatchItemResult
    {
        public object ItemId { get; set; } = null!;
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
    }

    public class ExternalServiceException : AppException
    {
        public ExternalServiceException(string message)
            : base(message, "EXTERNAL_SERVICE_ERROR")
        {
        }
    }
}