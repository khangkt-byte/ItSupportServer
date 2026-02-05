namespace ItSupportServer.src.Shared.Dto
{
    /// <summary>
    /// Session request pattern for anomaly detection
    /// Pattern: Mutable counter for rate limiting
    /// Reference: OWASP Automated Threats
    /// </summary>
    public class SessionRequestPattern
    {
        /// <summary>
        /// Total request count in current window
        /// </summary>
        public int RequestCount { get; set; }

        /// <summary>
        /// Timestamp of last request (UTC)
        /// </summary>
        public DateTime LastRequestTime { get; set; }

        /// <summary>
        /// Reset pattern tracking
        /// </summary>
        public void Reset()
        {
            RequestCount = 0;
            LastRequestTime = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Session risk profile for hijacking detection
    /// Pattern: Mutable state cached in memory
    /// Reference: OWASP Session Management
    /// Compliance: NIST SP 800-63B
    /// </summary>
    public class SessionRiskProfile
    {
        /// <summary>
        /// Session identifier
        /// </summary>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Account identifier
        /// </summary>
        public string AccountId { get; set; } = string.Empty;

        /// <summary>
        /// Initial IP address at session creation
        /// </summary>
        public string InitialIp { get; set; } = string.Empty;

        /// <summary>
        /// Initial User-Agent at session creation
        /// </summary>
        public string InitialUserAgent { get; set; } = string.Empty;

        /// <summary>
        /// Device fingerprint (SHA256 hash)
        /// </summary>
        public string InitialFingerprint { get; set; } = string.Empty;

        /// <summary>
        /// Session creation timestamp (UTC)
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Session validation result (immutable)
    /// Pattern: Result pattern for validation outcomes
    /// Reference: Microsoft - Result Pattern
    /// </summary>
    public record SessionValidationResult
    {
        public bool IsValid { get; init; }
        public string Reason { get; init; } = string.Empty;
        public int SuspicionScore { get; init; }

        /// <summary>
        /// Create successful validation result
        /// </summary>
        public static SessionValidationResult Valid(int score = 0) =>
            new()
            {
                IsValid = true,
                SuspicionScore = score
            };

        /// <summary>
        /// Create failed validation result
        /// </summary>
        public static SessionValidationResult Invalid(string reason) =>
            new()
            {
                IsValid = false,
                Reason = reason
            };
    }
}
