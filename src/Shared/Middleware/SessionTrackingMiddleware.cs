using ItSupportServer.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.src.Shared.Middleware
{
    /// <summary>
    /// [OBSOLETE] Legacy session tracking middleware — superseded by UnifiedSessionMiddleware.
    /// DO NOT register this in Program.cs. Kept for reference only.
    /// The old jti-based DB lookup (AccountTokenId == jti) is BROKEN because jti is a random
    /// UUID never stored in AccountTokens. UnifiedSessionMiddleware correctly uses sid → SessionId.
    /// </summary>
    [Obsolete("Use UnifiedSessionMiddleware (UseUnifiedSessionTracking()) instead. " +
              "This middleware has a broken DB lookup and will be removed in a future version.")]
    public class SessionTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SessionTrackingMiddleware> _logger;

        public SessionTrackingMiddleware(RequestDelegate next, ILogger<SessionTrackingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // ✅ Update last accessed time for authenticated requests
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var accountId = context.User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var tokenId = context.User.FindFirst("jti")?.Value;

                if (accountId != null && tokenId != null)
                {
                    try
                    {
                        var db = context.RequestServices.GetRequiredService<AppDbContext>();

                        var token = await db.AccountTokens
                            .FirstOrDefaultAsync(t =>
                                t.AccountId.ToString() == accountId &&
                                t.AccountTokenId.ToString() == tokenId);

                        if (token != null && token.RevokedAt == null)
                        {
                            token.LastAccessedAt = DateTime.UtcNow;
                            await db.SaveChangesAsync();

                            _logger.LogDebug(
                                "Session activity tracked for AccountId: {AccountId}, TokenId: {TokenId}",
                                accountId, tokenId);
                        }
                    }
                    catch (Exception ex)
                    {
                        // ✅ Don't fail request if session tracking fails
                        _logger.LogWarning(ex, "Failed to track session activity for AccountId: {AccountId}", accountId);
                    }
                }
            }

            await _next(context);
        }
    }

    // ✅ CRITICAL: Extension method for UseSessionTracking()
    public static class SessionTrackingMiddlewareExtensions
    {
        /// <summary>
        /// Adds session tracking middleware to update LastAccessedAt timestamp
        /// </summary>
        public static IApplicationBuilder UseSessionTracking(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SessionTrackingMiddleware>();
        }
    }
}