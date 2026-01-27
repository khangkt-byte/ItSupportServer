namespace ItSupportServer.src.Shared.Exceptions
{
    /// <summary>
    /// Base exception for all application exceptions
    /// </summary>
    public abstract class ApplicationException : Exception
    {
        public string Code { get; }
        public Dictionary<string, object>? Metadata { get; }

        protected ApplicationException(string message, string code, Dictionary<string, object>? metadata = null)
            : base(message)
        {
            Code = code;
            Metadata = metadata;
        }
    }

    /// <summary>
    /// Thrown when a resource is not found (404)
    /// </summary>
    public class NotFoundException : ApplicationException
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
    public class ValidationException : ApplicationException
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
    public class UnauthorizedException : ApplicationException
    {
        public UnauthorizedException(string message = "Xác thực thất bại")
            : base(message, "UNAUTHORIZED")
        {
        }
    }

    /// <summary>
    /// Thrown when authorization fails (403)
    /// </summary>
    public class ForbiddenException : ApplicationException
    {
        public ForbiddenException(string message = "Bạn không có quyền truy cập tài nguyên này")
            : base(message, "FORBIDDEN")
        {
        }
    }

    /// <summary>
    /// Thrown when there's a conflict (409) - e.g., duplicate entry
    /// </summary>
    public class ConflictException : ApplicationException
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
    public class BusinessRuleException : ApplicationException
    {
        public BusinessRuleException(string message, string code = "BUSINESS_RULE_VIOLATION")
            : base(message, code)
        {
        }
    }
}