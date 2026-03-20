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

            var passwordResetRetentionCutoff = DateTime.UtcNow.AddHours(-24);
            var refreshTokenRetentionCutoff = DateTime.UtcNow.AddDays(-7);

            var usedResetTokens = await db.PasswordResetTokens
                .Where(t => t.UsedAt != null && t.UsedAt < passwordResetRetentionCutoff)
                .ToListAsync(cancellationToken);

            var expiredUnusedResetTokens = await db.PasswordResetTokens
                .Where(t => t.UsedAt == null && t.ExpiresAt < passwordResetRetentionCutoff)
                .ToListAsync(cancellationToken);

            db.PasswordResetTokens.RemoveRange(usedResetTokens);
            db.PasswordResetTokens.RemoveRange(expiredUnusedResetTokens);

            // ✅ Cleanup revoked refresh tokens
            var revokedRefreshTokens = await db.AccountTokens
                .Where(t => t.RevokedAt != null && t.RevokedAt < refreshTokenRetentionCutoff)
                .ToListAsync(cancellationToken);

            db.AccountTokens.RemoveRange(revokedRefreshTokens);

            var removedResetTokens = usedResetTokens.Count + expiredUnusedResetTokens.Count;
            var totalRemoved = removedResetTokens + revokedRefreshTokens.Count;

            if (totalRemoved > 0)
            {
                await db.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Cleaned up {UsedResetTokens} used reset tokens + {ExpiredResetTokens} expired reset tokens + {RefreshTokens} refresh tokens",
                    usedResetTokens.Count,
                    expiredUnusedResetTokens.Count,
                    revokedRefreshTokens.Count);
            }
        }
    }
}