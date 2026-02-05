using ItSupportServer.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

public class TokenManagementService
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TokenManagementService> _logger;

    // ✅ 1. ACTIVE SESSION TRACKING
    public async Task<bool> IsSessionActive(Guid accountId, string tokenId)
    {
        // ✅ Check if token is in active session list
        var cacheKey = $"ActiveSession_{accountId}_{tokenId}";
        
        if (_cache.TryGetValue(cacheKey, out bool isActive))
        {
            return isActive;
        }

        // ✅ Check database
        var session = await _db.AccountTokens
            .FirstOrDefaultAsync(t => 
                t.AccountId == accountId && 
                t.AccountTokenId.ToString() == tokenId &&
                t.RevokedAt == null &&
                t.ExpiryTime > DateTime.UtcNow);

        var sessionActive = session != null;
        
        // ✅ Cache result for 5 minutes
        _cache.Set(cacheKey, sessionActive, TimeSpan.FromMinutes(5));

        return sessionActive;
    }

    // ✅ 2. CONCURRENT SESSION CONTROL
    public async Task<int> GetActiveSessionCount(Guid accountId)
    {
        return await _db.AccountTokens
            .Where(t => 
                t.AccountId == accountId &&
                t.RevokedAt == null &&
                t.ExpiryTime > DateTime.UtcNow)
            .CountAsync();
    }

    public async Task EnforceConcurrentSessionLimit(Guid accountId, int maxSessions = 3)
    {
        var activeSessions = await _db.AccountTokens
            .Where(t => 
                t.AccountId == accountId &&
                t.RevokedAt == null &&
                t.ExpiryTime > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        // ✅ Revoke oldest sessions if exceeding limit
        if (activeSessions.Count >= maxSessions)
        {
            var sessionsToRevoke = activeSessions.Skip(maxSessions - 1);
            
            foreach (var session in sessionsToRevoke)
            {
                session.RevokedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Revoked {Count} old sessions for account {AccountId}",
                sessionsToRevoke.Count(), accountId);
        }
    }

    // ✅ 3. SESSION TERMINATION
    public async Task TerminateAllSessions(Guid accountId, string? exceptTokenId = null)
    {
        var sessions = await _db.AccountTokens
            .Where(t => 
                t.AccountId == accountId &&
                t.RevokedAt == null)
            .ToListAsync();

        foreach (var session in sessions)
        {
            if (exceptTokenId == null || session.AccountTokenId.ToString() != exceptTokenId)
            {
                session.RevokedAt = DateTime.UtcNow;
                
                // ✅ Clear cache
                _cache.Remove($"ActiveSession_{accountId}_{session.AccountTokenId}");
            }
        }

        await _db.SaveChangesAsync();

        _logger.LogWarning(
            "Terminated all sessions for account {AccountId}",
            accountId);
    }

    // ✅ 4. IDLE SESSION TIMEOUT
    public async Task RevokeIdleSessions(TimeSpan idleTimeout)
    {
        var cutoffTime = DateTime.UtcNow.Subtract(idleTimeout);

        var idleSessions = await _db.AccountTokens
            .Where(t => 
                t.RevokedAt == null &&
                t.LastAccessedAt < cutoffTime)
            .ToListAsync();

        foreach (var session in idleSessions)
        {
            session.RevokedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Revoked {Count} idle sessions",
            idleSessions.Count);
    }

    // ✅ 5. SUSPICIOUS ACTIVITY DETECTION
    public async Task<bool> DetectSuspiciousActivity(Guid accountId, HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers["User-Agent"].ToString();

        // ✅ Get recent sessions
        var recentSessions = await _db.AccountTokens
            .Where(t => 
                t.AccountId == accountId &&
                t.CreatedAt > DateTime.UtcNow.AddHours(-24))
            .ToListAsync();

        // ✅ Check for multiple IPs
        var uniqueIps = recentSessions
            .Select(s => s.IpAddress)
            .Distinct()
            .Count();

        if (uniqueIps > 5) // More than 5 different IPs in 24h
        {
            _logger.LogWarning(
                "Suspicious activity detected: Multiple IPs for account {AccountId}",
                accountId);
            return true;
        }

        // ✅ Check for rapid session creation
        var recentCount = recentSessions
            .Count(s => s.CreatedAt > DateTime.UtcNow.AddMinutes(-15));

        if (recentCount > 10) // More than 10 logins in 15 min
        {
            _logger.LogWarning(
                "Suspicious activity detected: Rapid logins for account {AccountId}",
                accountId);
            return true;
        }

        return false;
    }
}