using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static ItSupportServer.src.Shared.Base.BaseEnum;
using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Helpers;
using ItSupportServer.Data.Models;

namespace ItSupportServer.src.Modules.Authentication
{
    public class AuthenticationsService(AppDbContext db, IConfiguration configuration, IMemoryCache _cache) : IAuthenticationsService
    {

        //tạo token
        private async Task<string> CreateToken(Accounts user)
        {
            var userRoles = await db.Employees.FindAsync(user.AccountId);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.AccountId.ToString()),
                new Claim(ClaimTypes.Role, userRoles.Position),

            };

            //vì dùng user id để tìm role của user đó nên không cần lặp qua roles nữa
            //foreach (var role in user.TaiKhoanRoles)
            //{
            //    claims.Add(new Claim(ClaimTypes.Role, role.RoleId));
            //}
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!)
                );
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDes = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddMonths(10),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(tokenDes);
        }

        //kiểm tra token có còn hợp lệ
        private async Task<Accounts?> ValidateRefreshTokenAsync(string refreshToken)
        {
            var token = await db.AccountTokens
        .FirstOrDefaultAsync(t => t.AccountTokenId.ToString() == refreshToken);

            if (token is null) return null;

            // load user kèm role 
            return await db.Accounts
                .Include(u => u.AccountRoles)
                .ThenInclude(tr => tr.Role)
                .FirstOrDefaultAsync(u => u.AccountId == token.AccountId);
        }

        //tạo vào lưu token mới
        private async Task<string> GenerateAndSaveRefreshToken(Guid accountId)
        {
            var token = await db.AccountTokens.AddAsync(new AccountTokens
            {
                AccountId = accountId,
                ExpiryTime = DateTime.UtcNow.AddDays(2)
            });

            await db.SaveChangesAsync();

            return token.Entity.AccountTokenId.ToString();
        }

        //tạo token trả về
        private async Task<TokenResponseDto> CreateTokenResponseAsync(Accounts user, bool isTokenExpry, string? refToken)
        {
            if (isTokenExpry is true)
            {
                return new TokenResponseDto
                {
                    AccessToken = await CreateToken(user),
                    RefreshToken = refToken
                };
            }
            else
            {
                return new TokenResponseDto
                {
                    AccessToken = await CreateToken(user),
                    RefreshToken = await GenerateAndSaveRefreshToken(user.AccountId)
                };
            }

        }

        public async Task<BaseResult<TokenResponseDto>?> LoginAsync(LoginDto dto)
        {
            try
            {
                var user = await db.Accounts
                    .Include(u => u.Employee)
                    .Include(u => u.AccountRoles)
                    .ThenInclude(ar => ar.Role)
                    .FirstOrDefaultAsync(u => u.Username == dto.Identifier || u.Employee.Email == dto.Identifier);

                if (user is null) return BaseResult<TokenResponseDto>.Fail("Tài khoản hoặc mật khẩu không đúng", 400);

                //if (user.Employee.Status is false) return BaseResult<TokenResponseDto>.Fail("Tài khoản đã bị khóa", 400);

                if (!string.IsNullOrEmpty(user.Otp) && user.ExpiredOtp != null) return BaseResult<TokenResponseDto>.Fail("Tài khoản chưa xác minh email", 403, new TokenResponseDto { AccountId = user.AccountId });

                if (new PasswordHasher<Accounts>().VerifyHashedPassword(user, user.Password, dto.Password)
                   == PasswordVerificationResult.Failed)
                {
                    return BaseResult<TokenResponseDto>.Fail("Không đúng mật khẩu", 400);
                }

                return BaseResult<TokenResponseDto>.Ok(await CreateTokenResponseAsync(user, false, null));
            }
            catch (Exception e)
            {
                return BaseResult<TokenResponseDto>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }

        public async Task<BaseResult<TokenResponseDto>?> RefreshTokenAsync(RefreshTokenRequestDto req)
        {
            try
            {
                var user = await ValidateRefreshTokenAsync(req.RefreshToken);
                if (user is null) return BaseResult<TokenResponseDto>.Fail("RefreshToken không hợp lệ", 400);
                var refToken = await db.AccountTokens
                    .FirstOrDefaultAsync(t => t.AccountTokenId.ToString() == req.RefreshToken);
                if (refToken.ExpiryTime < DateTime.UtcNow)
                {
                    db.AccountTokens.Remove(refToken);
                    await db.SaveChangesAsync();
                    return BaseResult<TokenResponseDto>.Ok(await CreateTokenResponseAsync(user, false, null));
                }


                return BaseResult<TokenResponseDto>.Ok(await CreateTokenResponseAsync(user, true, req.RefreshToken));
            }
            catch (Exception e)
            {
                return BaseResult<TokenResponseDto>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }

        #region Otp
        public async Task<BaseResult<TokenResponseDto>?> ConfirmOtp(OtpDto dto)
        {
            try
            {
                var IsUserExit = await db.Accounts
                    .Include(u => u.Employee)
                    .FirstOrDefaultAsync(u => u.AccountId == dto.AccountId);

                if (IsUserExit is null) return BaseResult<TokenResponseDto>.Fail("Người dùng không tồn tại", 400);

                if (IsUserExit.Otp != dto.Otp || IsUserExit.ExpiredOtp < DateTime.UtcNow)
                {
                    return BaseResult<TokenResponseDto>.Fail("Mã xác minh không đúng hoặc đã hết hạn", 400);
                }

                if (IsUserExit.Otp == dto.Otp && IsUserExit.ExpiredOtp >= DateTime.UtcNow)
                {
                    IsUserExit.Otp = null;
                    IsUserExit.ExpiredOtp = null;
                    //IsUserExit.Employee.Status = true;
                    db.Accounts.Update(IsUserExit);
                    await db.SaveChangesAsync();
                    _cache.Remove($"User_Status_{dto.AccountId}");
                    return BaseResult<TokenResponseDto>.Ok(await CreateTokenResponseAsync(IsUserExit, false, null));
                }
                return BaseResult<TokenResponseDto>.Fail("Xác minh không thành công", 400);
            }
            catch (Exception e)
            {
                return BaseResult<TokenResponseDto>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }

        public async Task<BaseResult<TokenResponseDto>?> RefreshOtp(string email)
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var IsUserExit = await db.Accounts
                    .Include(u => u.Employee)
                    .FirstOrDefaultAsync(u => u.Employee.Email == email);
                if (IsUserExit is null) return BaseResult<TokenResponseDto>.Fail("Người dùng không tồn tại", 400);

                if (string.IsNullOrEmpty(IsUserExit.Otp) || IsUserExit.ExpiredOtp == null)
                {
                    return BaseResult<TokenResponseDto>.Fail("Tài khoản đã xác minh email", 400);
                }

                var newOtp = RandomString.GenerateRandomNumericString(6);
                IsUserExit.Otp = newOtp;
                IsUserExit.ExpiredOtp = DateTime.UtcNow.AddMinutes(5);
                db.Accounts.Update(IsUserExit);
                await db.SaveChangesAsync();

                //gửi email
                var emailService = await SendMail.SendMailAsync(configuration, email, "Xác thực email của bạn", "Đây là mã otp của bạn, mã sẽ hết hạn sau 5 phút!", newOtp);
                if (!emailService)
                {
                    await transaction.RollbackAsync();
                    return BaseResult<TokenResponseDto>.Fail("Gửi mã xác minh không thành công, vui lòng thử lại", 500);
                }
                await transaction.CommitAsync();

                return BaseResult<TokenResponseDto>.Ok(new TokenResponseDto { AccountId = IsUserExit.AccountId }, 200, "Gửi mã xác minh thành công");
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                return BaseResult<TokenResponseDto>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }


        #endregion
        public async Task<BaseResult<bool>> ForgotPassword(string EmailOrUserName)
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var IsUserExit = await db.Accounts
                    .Include(u => u.Employee)
                    .FirstOrDefaultAsync(u => u.Employee.Email == EmailOrUserName || u.Username == EmailOrUserName);
                if (IsUserExit is null) return BaseResult<bool>.Fail("Người dùng không tồn tại", 400, false);

                var newPassword = RandomString.GenerateRandomString(8);
                var hashedPassword = new PasswordHasher<Accounts>().HashPassword(IsUserExit, newPassword);
                IsUserExit.Password = hashedPassword;

                db.Accounts.Update(IsUserExit);
                await db.SaveChangesAsync();

                var SendEmail = await SendMail.SendMailAsync(configuration, IsUserExit.Employee.Email, "Quên mật khẩu", "Mật khẩu mới của bạn là: ", newPassword);
                if (!SendEmail)
                {
                    await transaction.RollbackAsync();
                    return BaseResult<bool>.Fail("Gửi mật khẩu mới không thành công, vui lòng thử lại", 500, false);
                }

                await transaction.CommitAsync();
                return BaseResult<bool>.Ok(true, 200, "Gửi mật khẩu mới thành công, vui lòng kiểm tra email");
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                return BaseResult<bool>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }
    }
}
