namespace ItSupportServer.src.Shared.Exceptions
{
    /// <summary>
    /// Centralized error code catalog
    /// Pattern: Auth0 Error Codes, Stripe Error Types
    /// Reference: 
    /// - https://auth0.com/docs/troubleshoot/customer-support/error-codes
    /// - https://stripe.com/docs/api/errors/handling
    /// </summary>
    public static class ErrorCodes
    {
        // ===== Validation (400) =====
        public const string VALIDATION_FAILED = "VALIDATION_FAILED";
        public const string INVALID_ARGUMENT = "INVALID_ARGUMENT";
        public const string INVALID_FORMAT = "INVALID_FORMAT";
        public const string REQUIRED_FIELD_MISSING = "REQUIRED_FIELD_MISSING";

        // ===== Authentication (401) =====
        public const string UNAUTHORIZED = "UNAUTHORIZED";
        public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";
        public const string TOKEN_EXPIRED = "TOKEN_EXPIRED";
        public const string TOKEN_INVALID = "TOKEN_INVALID";

        // ===== Authorization (403) =====
        public const string FORBIDDEN = "FORBIDDEN";
        public const string INSUFFICIENT_PERMISSIONS = "INSUFFICIENT_PERMISSIONS";
        public const string ACCOUNT_LOCKED = "ACCOUNT_LOCKED";
        public const string ACCOUNT_DISABLED = "ACCOUNT_DISABLED";
        public const string OTP_REQUIRED = "OTP_REQUIRED";

        // ===== Not Found (404) =====
        public const string NOT_FOUND = "NOT_FOUND";
        public const string RESOURCE_NOT_FOUND = "RESOURCE_NOT_FOUND";

        // ===== Conflict (409) =====
        public const string CONFLICT = "CONFLICT";
        public const string DUPLICATE_ENTRY = "DUPLICATE_ENTRY";
        public const string USERNAME_EXISTS = "USERNAME_EXISTS";
        public const string EMAIL_EXISTS = "EMAIL_EXISTS";

        // ===== Business Rules (422) =====
        public const string BUSINESS_RULE_VIOLATION = "BUSINESS_RULE_VIOLATION";
        public const string ROLE_IN_USE = "ROLE_IN_USE";
        public const string CANNOT_DELETE_SUPER_ADMIN = "CANNOT_DELETE_SUPER_ADMIN";
        public const string CANNOT_MODIFY_SELF = "CANNOT_MODIFY_SELF";
        public const string CAUSE_NOT_BELONGS_TO_ISSUE = "CAUSE_NOT_BELONGS_TO_ISSUE";

        // ===== Rate Limiting (429) =====
        public const string TOO_MANY_ATTEMPTS = "TOO_MANY_ATTEMPTS";
        public const string RATE_LIMIT_EXCEEDED = "RATE_LIMIT_EXCEEDED";

        // ===== External Services (502) =====
        public const string EXTERNAL_SERVICE_ERROR = "EXTERNAL_SERVICE_ERROR";
        public const string EMAIL_SERVICE_UNAVAILABLE = "EMAIL_SERVICE_UNAVAILABLE";

        // ===== Server Errors (500) =====
        public const string INTERNAL_ERROR = "INTERNAL_ERROR";
        public const string DATABASE_ERROR = "DATABASE_ERROR";

        // ===== Batch Operations (207) =====
        public const string BATCH_OPERATION_ERROR = "BATCH_OPERATION_ERROR";
        public const string PARTIAL_SUCCESS = "PARTIAL_SUCCESS";
    }
}