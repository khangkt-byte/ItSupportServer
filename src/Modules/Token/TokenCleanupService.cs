using ItSupportServer.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.src.Modules.Token
{
    /// <summary>
    /// Background service to cleanup expired tokens
    /// Pattern: Hosted Service (ASP.NET Core)
    /// Schedule: Runs every 6 hours
    /// Reference: Microsoft BackgroundService pattern
    /// </summary>
    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenCleanupService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(6);

        public TokenCleanupService(
            IServiceProvider serviceProvider,
            ILogger<TokenCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Token cleanup service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredTokensAsync(stoppingToken);
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in token cleanup service");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Retry after 5 min
                }
            }

            _logger.LogInformation("Token cleanup service stopped");
        }

        private async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var cutoffDate = DateTime.UtcNow.AddDays(-7); // Keep expired tokens for 7 days (audit trail)

            // ✅ Cleanup password reset tokens
            var expiredResetTokens = await db.PasswordResetTokens
                .Where(t => t.ExpiresAt < cutoffDate)
                .ToListAsync(cancellationToken);

            db.PasswordResetTokens.RemoveRange(expiredResetTokens);

            // ✅ Cleanup revoked refresh tokens
            var revokedRefreshTokens = await db.AccountTokens
                .Where(t => t.RevokedAt != null && t.RevokedAt < cutoffDate)
                .ToListAsync(cancellationToken);

            db.AccountTokens.RemoveRange(revokedRefreshTokens);

            var totalRemoved = expiredResetTokens.Count + revokedRefreshTokens.Count;

            if (totalRemoved > 0)
            {
                await db.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation(
                    "Cleaned up {ResetTokens} password reset tokens + {RefreshTokens} refresh tokens",
                    expiredResetTokens.Count, revokedRefreshTokens.Count);
            }
        }
    }
}