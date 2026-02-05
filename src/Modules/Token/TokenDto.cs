namespace ItSupportServer.src.Modules.Token
{
    /// <summary>
    /// Data Transfer Objects for Token module
    /// Pattern: Request/Response DTOs
    /// Reference: Microsoft REST API Guidelines
    /// </summary>

    /// <summary>
    /// CSRF token response (200 OK)
    /// Used by: GET /api/csrf/token
    /// </summary>
    public record CsrfTokenResponse
    {
        /// <summary>
        /// HMAC-signed CSRF token
        /// Format: {randomToken}.{hmacSignature}
        /// Length: Variable (Base64 encoded)
        /// </summary>
        /// <example>dGVzdFRva2VuMTIzNDU2Nzg5MA==.aEJzY2NnYXRlZEhhc2g=</example>
        public required string CsrfToken { get; init; }
    }

    /// <summary>
    /// Rate limit response (429 Too Many Requests)
    /// Returned when global rate limiter is exceeded
    /// Reference: IETF RFC 6585 - Additional HTTP Status Codes
    /// </summary>
    public record RateLimitResponse
    {
        /// <summary>
        /// Error code
        /// </summary>
        /// <example>TOO_MANY_REQUESTS</example>
        public required string Error { get; init; }

        /// <summary>
        /// Human-readable error message
        /// </summary>
        /// <example>Rate limit exceeded. Please try again later.</example>
        public required string Message { get; init; }

        /// <summary>
        /// Seconds to wait before retrying
        /// </summary>
        /// <example>60</example>
        public double? RetryAfter { get; init; }
    }
}