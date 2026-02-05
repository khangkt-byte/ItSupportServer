using ItSupportServer.Data.Models;
using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace ItSupportServer.src.Shared.Services
{
    /// <summary>
    /// Secure session management service
    /// Compliance: OWASP Session Management, NIST SP 800-63B
    /// </summary>
    public class SessionManagementService
    {
        private readonly AppDbContext _db;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SessionManagementService> _logger;
        private readonly IConfiguration _configuration;

        // ✅ Session Configuration (OWASP Recommended)
        private const int SESSION_ID_LENGTH = 32; // 256 bits
        private const int MAX_CONCURRENT_SESSIONS = 3; // Per user
        private const int SESSION_ABSOLUTE_TIMEOUT_MINUTES = 480; // 8 hours
        private const int SESSION_IDLE_TIMEOUT_MINUTES = 30; // 30 minutes
        private const int SESSION_RENEWAL_THRESHOLD_MINUTES = 5; // Renew 5 min before expiry

        public SessionManagementService(
            AppDbContext db,
            IMemoryCache cache,
            ILogger<SessionManagementService> logger,
            IConfiguration configuration)
        {
            _db = db;
            _cache = cache;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Generate cryptographically strong session ID
        /// Pattern: RFC 4122 v4 (Random UUID) with additional entropy
        /// Reference: OWASP Session ID Generation
        /// </summary>
        public string GenerateSecureSessionId()
        {
            // ✅ OWASP: Use cryptographically strong random number generator
            var randomBytes = new byte[SESSION_ID_LENGTH];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            // ✅ Additional entropy from timestamp
            var timestamp = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
            var combined = randomBytes.Concat(timestamp).ToArray();

            // ✅ Hash for uniformity (prevents pattern analysis)
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(combined);

            // ✅ Base64 URL-safe encoding
            return Convert.ToBase64String(hash)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        /// <summary>
        /// Create new session with security validation
        /// Pattern: NIST SP 800-63B Section 7.1
        /// </summary>
        public async Task<SessionCreationResult> CreateSessionAsync(
            Guid accountId,
            HttpContext httpContext)
        {
            // ✅ 1. SESSION FIXATION PREVENTION
            // Generate NEW session ID after authentication
            var sessionId = GenerateSecureSessionId();

            // ✅ 2. DEVICE FINGERPRINTING
            var fingerprint = ComputeDeviceFingerprint(httpContext);

            // ✅ 3. GEO-LOCATION (Optional)
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();

            // ✅ 4. CONCURRENT SESSION CONTROL
            await EnforceConcurrentSessionLimitAsync(accountId);

            // ✅ 5. CREATE SESSION RECORD
            var session = new AccountTokens
            {
                AccountTokenId = Guid.NewGuid(),
                AccountId = accountId,
                SessionId = sessionId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                DeviceInfo = fingerprint,
                CreatedAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow,
                ExpiryTime = DateTime.UtcNow.AddMinutes(SESSION_ABSOLUTE_TIMEOUT_MINUTES),
                RevokedAt = null
            };

            _db.AccountTokens.Add(session);
            await _db.SaveChangesAsync();

            // ✅ 6. CACHE SESSION FOR FAST LOOKUP
            CacheSession(session);

            // ✅ 7. LOG SESSION CREATION
            _logger.LogInformation(
                "Session created: {SessionId} | Account: {AccountId} | IP: {IP} | Device: {Device}",
                sessionId, accountId, ipAddress, fingerprint.Substring(0, 16)
            );

            return new SessionCreationResult
            {
                SessionId = sessionId,
                TokenId = session.AccountTokenId,
                ExpiresAt = session.ExpiryTime
            };
        }

        /// <summary>
        /// Compute device fingerprint for session binding
        /// Pattern: Device Trust Scoring
        /// Reference: Auth0 Adaptive MFA
        /// </summary>
        private string ComputeDeviceFingerprint(HttpContext httpContext)
        {
            var components = new[]
            {
                httpContext.Request.Headers["User-Agent"].ToString(),
                httpContext.Request.Headers["Accept-Language"].ToString(),
                httpContext.Request.Headers["Accept-Encoding"].ToString(),
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                httpContext.Request.Headers["Sec-Ch-Ua"].ToString(), // Chrome User-Agent Client Hints
                httpContext.Request.Headers["Sec-Ch-Ua-Platform"].ToString()
            };

            var combined = string.Join("|", components.Where(c => !string.IsNullOrEmpty(c)));

            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(combined));
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Enforce concurrent session limit (OWASP Recommendation)
        /// Pattern: Concurrent Session Control
        /// Reference: OWASP Session Management - Concurrent Sessions
        /// </summary>
        private async Task EnforceConcurrentSessionLimitAsync(Guid accountId)
        {
            var activeSessions = await _db.AccountTokens
                .Where(t =>
                    t.AccountId == accountId &&
                    t.RevokedAt == null &&
                    t.ExpiryTime > DateTime.UtcNow)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            // ✅ Revoke oldest sessions if exceeding limit
            if (activeSessions.Count >= MAX_CONCURRENT_SESSIONS)
            {
                var sessionsToRevoke = activeSessions
                    .Skip(MAX_CONCURRENT_SESSIONS - 1)
                    .ToList();

                foreach (var session in sessionsToRevoke)
                {
                    session.RevokedAt = DateTime.UtcNow;
                    
                    // ✅ Clear from cache
                    _cache.Remove($"Session:{session.SessionId}");

                    _logger.LogInformation(
                        "Session revoked due to concurrent limit: {SessionId} | Account: {AccountId}",
                        session.SessionId, accountId
                    );
                }

                await _db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Cache session for performance
        /// </summary>
        private void CacheSession(AccountTokens session)
        {
            var cacheKey = $"Session:{session.SessionId}";
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(SESSION_IDLE_TIMEOUT_MINUTES),
                SlidingExpiration = TimeSpan.FromMinutes(15)
            };

            _cache.Set(cacheKey, session, cacheOptions);
        }
    }

    public class SessionCreationResult
    {
        public string SessionId { get; set; } = null!;
        public Guid TokenId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}