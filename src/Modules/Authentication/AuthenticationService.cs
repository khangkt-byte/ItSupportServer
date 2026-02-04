using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ItSupportServer.Data.Models;
using ItSupportServer.src.Shared.Exceptions;
using ItSupportServer.src.Shared.Helpers;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using ItSupportServer.src.Shared.Extensions;
using ItSupportServer.src.Modules.Account;

namespace ItSupportServer.src.Modules.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IValidator<LoginDto> _loginValidator;
        private readonly IValidator<OtpDto> _otpValidator;
        private readonly IAccountService _accountService;

        public AuthenticationService(
            AppDbContext db,
            IConfiguration configuration,
            IMemoryCache cache,
            ILogger<AuthenticationService> logger,
            IValidator<LoginDto> loginValidator,
            IValidator<OtpDto> otpValidator,
            IAccountService accountService)
        {
            _db = db;
            _configuration = configuration;
            _cache = cache;
            _logger = logger;
            _loginValidator = loginValidator;
            _otpValidator = otpValidator;
            _accountService = accountService;
        }

        public async Task<TokenResponseDto> LoginAsync(LoginDto dto)
        {
            _logger.LogInformation("Login attempt for: {Identifier}", dto.Identifier);

            // ✅ FIX: Sử dụng extension method để tránh xung đột
            var validationResult = await _loginValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();  // ✅ Throws custom ValidationException

            var user = await _db.Accounts
                .Include(u => u.Employee)
                .Include(u => u.AccountRoles)
                    .ThenInclude(ar => ar.Role)
                .FirstOrDefaultAsync(u =>
                    u.Username == dto.Identifier ||
                    u.Employee.Email == dto.Identifier);

            // ✅ Consistent error message (không để lộ user có tồn tại hay không)
            if (user is null)
            {
                _logger.LogWarning("Login failed: Invalid credentials for {Identifier}", dto.Identifier);
                throw new UnauthorizedException("Tên đăng nhập hoặc mật khẩu không đúng");
            }

            if (user.DeletedAt != null)
            {
                _logger.LogWarning("Login blocked: Account deleted {AccountId}", user.AccountId);
                throw new ForbiddenException("Tài khoản đã bị vô hiệu hóa");
            }

            // ✅ Check if account is locked
            if (await _accountService.IsAccountLockedAsync(user.AccountId))
            {
                _logger.LogWarning("Login blocked: Account locked {AccountId}", user.AccountId);
                throw new ForbiddenException("Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.");
            }

            // Check OTP verification
            if (!string.IsNullOrEmpty(user.Otp) && user.ExpiredOtp != null)
            {
                _logger.LogInformation("OTP verification required for {AccountId}", user.AccountId);
                throw new OtpRequiredException(user.AccountId);
            }

            // VALIDATE PASSWORD
            // ❌ OLD (Microsoft.AspNetCore.Identity)
            // var result = new PasswordHasher<Accounts>().VerifyHashedPassword(
            //     user, user.Password, dto.Password);

            // if (result == PasswordVerificationResult.Failed)
            // {
            //     // ...
            // }

            // ✅ NEW (BCrypt)
            if (!PasswordHelper.VerifyPassword(dto.Password, user.Password))
            {
                _logger.LogWarning("Login failed: Invalid password for {Identifier}", dto.Identifier);
                
                // Record failed login attempt
                await RecordFailedLoginAsync(user.AccountId);
                
                // ✅ Record failed login
                await _accountService.RecordLoginAttemptAsync(user.AccountId, success: false);

                throw new UnauthorizedException("Tên đăng nhập hoặc mật khẩu không đúng");
            }

            // ✅ Check if password needs rehashing (security best practice)
            if (PasswordHelper.NeedsRehash(user.Password))
            {
                user.Password = PasswordHelper.HashPassword(dto.Password);
                await _db.SaveChangesAsync();
            }

            // ✅ Record successful login
            await _accountService.RecordLoginAttemptAsync(user.AccountId, success: true);

            // ✅ Revoke old refresh tokens (optional, for better security)
            await RevokeOldRefreshTokensAsync(user.AccountId);

            _logger.LogInformation("Login successful for {AccountId}", user.AccountId);

            return await CreateTokenResponseAsync(user);
        }

        public async Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto req)
        {
            _logger.LogInformation("Refresh token attempt");

            var tokenRecord = await _db.AccountTokens
                .Include(t => t.Account)
                    .ThenInclude(a => a.AccountRoles)
                        .ThenInclude(ar => ar.Role)
                .FirstOrDefaultAsync(t => t.AccountTokenId.ToString() == req.RefreshToken);

            if (tokenRecord is null)
            {
                _logger.LogWarning("Refresh token not found");
                throw new UnauthorizedException("Refresh token không hợp lệ");
            }

            // ✅ Check if token is revoked
            if (tokenRecord.RevokedAt != null)
            {
                _logger.LogWarning("Refresh token was revoked");
                throw new UnauthorizedException("Refresh token đã bị thu hồi");
            }

            // ✅ Check expiration
            if (tokenRecord.ExpiryTime < DateTime.UtcNow)
            {
                _logger.LogInformation("Refresh token expired, removing");
                _db.AccountTokens.Remove(tokenRecord);
                await _db.SaveChangesAsync();
                throw new UnauthorizedException("Refresh token đã hết hạn");
            }

            var user = tokenRecord.Account;

            // ✅ Check account status
            if (user.DeletedAt != null)
            {
                throw new ForbiddenException("Tài khoản đã bị khóa");
            }

            _logger.LogInformation("Refresh token successful for {AccountId}", user.AccountId);

            // ✅ Rotate refresh token (security best practice)
            return await CreateTokenResponseAsync(user, shouldRotateRefreshToken: true, oldTokenId: tokenRecord.AccountTokenId);
        }

        public async Task<bool> LogoutAsync(Guid accountId, string refreshToken)
        {
            _logger.LogInformation("Logout for {AccountId}", accountId);

            if (Guid.TryParse(refreshToken, out var tokenId))
            {
                var token = await _db.AccountTokens.FindAsync(tokenId);
                if (token != null && token.AccountId == accountId)
                {
                    // ✅ Soft delete / revoke token
                    token.RevokedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }
            }

            _logger.LogInformation("Logout successful for {AccountId}", accountId);
            return true;
        }

        public async Task<OtpResponseDto> ConfirmOtpAsync(OtpDto dto)
        {
            _logger.LogInformation("OTP confirmation for {AccountId}", dto.AccountId);

            // ✅ FIX: Sử dụng extension method
            var validationResult = await _otpValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            var user = await _db.Accounts
                .Include(u => u.Employee)
                .Include(u => u.AccountRoles)
                    .ThenInclude(ar => ar.Role)
                .FirstOrDefaultAsync(u => u.AccountId == dto.AccountId);

            if (user is null)
            {
                throw new NotFoundException("Tài khoản", dto.AccountId);
            }

            // ✅ Check OTP attempts (prevent brute force)
            var cacheKey = $"OTP_Attempts_{dto.AccountId}";
            var attempts = _cache.Get<int>(cacheKey);
            
            if (attempts >= 5)
            {
                _logger.LogWarning("Too many OTP attempts for {AccountId}", dto.AccountId);
                throw new TooManyAttemptsException("Quá nhiều lần nhập OTP sai. Vui lòng yêu cầu OTP mới.");
            }

            // ✅ Verify OTP (should be hashed in production)
            if (user.Otp != dto.Otp || user.ExpiredOtp < DateTime.UtcNow)
            {
                // Increment failed attempts
                _cache.Set(cacheKey, attempts + 1, TimeSpan.FromMinutes(15));
                
                _logger.LogWarning("Invalid or expired OTP for {AccountId}", dto.AccountId);
                throw new UnauthorizedException("Mã OTP không đúng hoặc đã hết hạn");
            }

            // ✅ Clear OTP
            user.Otp = null;
            user.ExpiredOtp = null;
            await _db.SaveChangesAsync();

            // Clear cache
            _cache.Remove(cacheKey);

            _logger.LogInformation("OTP confirmed successfully for {AccountId}", dto.AccountId);

            return new OtpResponseDto
            {
                Token = await CreateTokenResponseAsync(user)
            };
        }

        public async Task<OtpSentResponseDto> RefreshOtpAsync(string email)
        {
            _logger.LogInformation("OTP refresh request for {Email}", email);

            var user = await _db.Accounts
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Employee.Email == email);

            if (user is null)
            {
                throw new NotFoundException("Email không tồn tại");
            }

            if (string.IsNullOrEmpty(user.Otp) && user.ExpiredOtp == null)
            {
                throw new BusinessRuleException("Tài khoản đã được xác minh");
            }

            // ✅ Rate limiting
            var rateLimitKey = $"OTP_RateLimit_{email}";
            if (_cache.TryGetValue(rateLimitKey, out _))
            {
                throw new TooManyAttemptsException("Vui lòng đợi ít nhất 60 giây trước khi yêu cầu OTP mới");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Generate new OTP
                var newOtp = RandomString.GenerateRandomNumericString(6);
                user.Otp = newOtp;  // ✅ TODO: Hash this in production
                user.ExpiredOtp = DateTime.UtcNow.AddMinutes(5);
                
                await _db.SaveChangesAsync();

                // Send email
                var emailSent = await SendMail.SendMailAsync(
                    _configuration,
                    email,
                    "Xác thực email",
                    "Mã OTP của bạn (hết hạn sau 5 phút):",
                    newOtp);

                if (!emailSent)
                {
                    await transaction.RollbackAsync();
                    throw new ExternalServiceException("Không thể gửi email. Vui lòng thử lại.");
                }

                await transaction.CommitAsync();

                // Set rate limit
                _cache.Set(rateLimitKey, true, TimeSpan.FromSeconds(60));

                _logger.LogInformation("OTP sent successfully to {Email}", email);

                return new OtpSentResponseDto
                {
                    AccountId = user.AccountId,
                    Message = "Mã OTP đã được gửi đến email của bạn"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ForgotPasswordAsync(string emailOrUsername)
        {
            _logger.LogInformation("Forgot password request for {Identifier}", emailOrUsername);

            // ✅ Rate limiting
            var rateLimitKey = $"ForgotPassword_{emailOrUsername}";
            if (_cache.TryGetValue(rateLimitKey, out _))
            {
                throw new TooManyAttemptsException("Vui lòng đợi ít nhất 5 phút trước khi yêu cầu lại");
            }

            var user = await _db.Accounts
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u =>
                    u.Employee.Email == emailOrUsername ||
                    u.Username == emailOrUsername);

            // ✅ Security: Không nên reveal user có tồn tại hay không
            // Luôn return success để prevent user enumeration
            if (user is null)
            {
                _logger.LogWarning("Forgot password: User not found {Identifier}", emailOrUsername);
                // Fake delay to prevent timing attacks
                await Task.Delay(Random.Shared.Next(100, 500));
                return true; // Pretend success
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // ✅ Use PasswordHelper
                var newPassword = PasswordHelper.GenerateRandomPassword(12);
                user.Password = PasswordHelper.HashPassword(newPassword);

                await _db.SaveChangesAsync();

                var emailSent = await SendMail.SendMailAsync(
                    _configuration,
                    user.Employee.Email,
                    "Đặt lại mật khẩu",
                    "Mật khẩu tạm thời của bạn:",
                    newPassword + "\n\nVui lòng đổi mật khẩu sau khi đăng nhập.");

                if (!emailSent)
                {
                    await transaction.RollbackAsync();
                    throw new ExternalServiceException("Không thể gửi email");
                }

                await transaction.CommitAsync();

                // Set rate limit
                _cache.Set(rateLimitKey, true, TimeSpan.FromMinutes(5));

                _logger.LogInformation("Password reset successful for {AccountId}", user.AccountId);

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        #region Private Helper Methods

        private async Task<TokenResponseDto> CreateTokenResponseAsync(
            Accounts user,
            bool shouldRotateRefreshToken = false,
            Guid? oldTokenId = null)
        {
            // Generate new access token
            var accessToken = await CreateAccessTokenAsync(user);

            // Generate or reuse refresh token
            string refreshToken;

            if (shouldRotateRefreshToken && oldTokenId.HasValue)
            {
                // ✅ Rotate refresh token (revoke old, create new)
                var oldToken = await _db.AccountTokens.FindAsync(oldTokenId.Value);
                if (oldToken != null)
                {
                    oldToken.RevokedAt = DateTime.UtcNow;
                }
                refreshToken = await GenerateAndSaveRefreshTokenAsync(user.AccountId);
            }
            else
            {
                refreshToken = await GenerateAndSaveRefreshTokenAsync(user.AccountId);
            }

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        private async Task<string> CreateAccessTokenAsync(Accounts user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.NameIdentifier, user.AccountId.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // ✅ Unique token ID
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()) // ✅ Issued at
            };

            // ✅ Add roles
            foreach (var role in user.AccountRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Role.Name));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["AppSettings:Token"]!));
            
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration["AppSettings:Issuer"],
                audience: _configuration["AppSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),  // ✅ 30 minutes (not 10 months!)
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(Guid accountId)
        {
            var refreshToken = new AccountTokens
            {
                AccountId = accountId,
                ExpiryTime = DateTime.UtcNow.AddDays(7)
                // CreatedAt tự động set bởi AuditInterceptor
            };

            await _db.AccountTokens.AddAsync(refreshToken);
            await _db.SaveChangesAsync();

            return refreshToken.AccountTokenId.ToString();
        }

        private async Task RevokeOldRefreshTokensAsync(Guid accountId)
        {
            // ✅ Optional: Keep only last N refresh tokens per user
            var oldTokens = await _db.AccountTokens
                .Where(t => t.AccountId == accountId && t.RevokedAt == null)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(5) // Keep latest 5 tokens
                .ToListAsync();

            foreach (var token in oldTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }

            if (oldTokens.Any())
            {
                await _db.SaveChangesAsync();
            }
        }

        private async Task RecordFailedLoginAsync(Guid accountId)
        {
            var account = await _db.Accounts.FindAsync(accountId);
            if (account is null) return;

            account.FailedLoginAttempts++;

            // Auto-lock after 5 failed attempts
            if (account.FailedLoginAttempts >= 5)
            {
                account.IsLocked = true;
                account.LockedUntil = DateTime.UtcNow.AddMinutes(30);
                _logger.LogWarning("Account {AccountId} locked due to {Attempts} failed attempts",
                    accountId, account.FailedLoginAttempts);
            }

            await _db.SaveChangesAsync();
        }

        #endregion
    }
}
