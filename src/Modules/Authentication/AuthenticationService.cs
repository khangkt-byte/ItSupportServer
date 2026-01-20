using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using ItSupportServer.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static ItSupportServer.src.Shared.Base.BaseEnum;
using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Helper;

namespace ItSupportServer.src.Modules.Authentication
{
    public class AuthenticationService(AppDbContext db, IConfiguration configuration, IMapper mapper, IMemoryCache _cache) : IAuthenticationService
    {

        //tạo token
        private async Task<string> CreateToken(Accounts user)
        {
            var userRoles = await db.Users.FindAsync(user.UserId);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, userRoles.Position),

            };

            //vì dùng user id để tìm role của user đó nên không cần lặp qua roles nữa
            //foreach (var role in user.TaiKhoanRoles)
            //{
            //    claims.Add(new Claim(ClaimTypes.Role, role.RoleId));
            //}
            //system.identity.tokens.jwt tai thu vien ve
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
        .FirstOrDefaultAsync(t => t.Id.ToString() == refreshToken);

            if (token is null) return null;

            // load user kèm role 
            return await db.Accounts
                .Include(u => u.AccountRoles)
                .ThenInclude(tr => tr.Role)
                .FirstOrDefaultAsync(u => u.IdUser == token.IdAccount);
        }

        //tạo vào lưu token mới
        private async Task<string> GenerateAndSaveRefreshToken(Guid IdAccount)
        {
            var token = await db.AccountTokens.AddAsync(new AccountTokens
            {
                IdAccount = IdAccount,
                ExpiryTime = DateTime.UtcNow.AddDays(2)
            });

            await db.SaveChangesAsync();

            return token.Entity.Id.ToString();
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
                    RefreshToken = await GenerateAndSaveRefreshToken(user.UserId)
                };
            }

        }

        public async Task<BaseResult<TokenResponseDto>?> LoginAsync(LoginDto dto)
        {
            try
            {
                var user = await db.Accounts
                    .Include(u => u.User)
                    .Include(u => u.AccountRoles)
                    .ThenInclude(ar => ar.Role)
                    .FirstOrDefaultAsync(u => u.UserName == dto.UserNameOrEmail || u.User.Email == dto.UserNameOrEmail);

                if (user is null) return BaseResult<TokenResponseDto>.Fail("Tài khoản hoặc mật khẩu không đúng", 400);

                if (user.User.Status is false) return BaseResult<TokenResponseDto>.Fail("Tài khoản đã bị khóa", 400);

                if (!string.IsNullOrEmpty(user.Otp) && user.ExpriesOtp != null) return BaseResult<TokenResponseDto>.Fail("Tài khoản chưa xác minh email", 403, new TokenResponseDto { UserId = user.IdUser });

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
                    .FirstOrDefaultAsync(t => t.Id.ToString() == req.RefreshToken);
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

        #region Authentication GG
        public async Task<GoogleResponse> GetGoogleResponse(GoogleAuthDto dto)
        {
            GoogleJsonWebSignature.Payload payload;
            try
            {
                var idtk = configuration.GetValue<string>("Authentication:Google:ClientId")!;
                payload = await GoogleJsonWebSignature.ValidateAsync(
                    dto.IdToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[]
                        {
                            configuration.GetValue<string>("Authentication:Google:ClientId")!
                        }
                    }
                    );

                if (!payload.EmailVerified) return null;
                var res = new GoogleResponse();
                res.GoogleSub = payload.Subject;
                res.Email = payload.Email.Trim().ToLowerInvariant();
                res.EmailVerified = payload.EmailVerified;
                res.Picture = payload.Picture;
                return res;
            }
            catch
            {
                return null;
            }
        }

        public async Task<BaseResult<TokenResponseDto>>? LoginWithGG(GoogleAuthDto dto)
        {
            try
            {
                var res = await GetGoogleResponse(dto);
                if (res is null) return BaseResult<TokenResponseDto>.Fail("Xác thực Google không thành công", 400);

                var user = await db.Accounts.Include(u => u.User).FirstOrDefaultAsync(u => u.GoogleId == res.GoogleSub);
                if (user is null)
                {
                    return BaseResult<TokenResponseDto>.Fail("Tài khoản Google chưa được đăng ký", 400);
                }
                else
                {
                    return BaseResult<TokenResponseDto>.Ok(await CreateTokenResponseAsync(user, false, null));
                }
            }
            catch (Exception e)
            {
                return BaseResult<TokenResponseDto>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }

        public async Task<BaseResult<TokenResponseDto>> RegisterGGAsync(GoogleAuthDto dto)
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var res = await GetGoogleResponse(dto);
                if (res is null) return BaseResult<TokenResponseDto>.Fail("Xác thực Google không thành công", 400);
                var existingUser = await db.Accounts
                    .Include(u => u.User)
                    .FirstOrDefaultAsync(u => u.User.Email == res.Email || u.GoogleId == res.GoogleSub);

                if (existingUser is not null)
                {
                    if (existingUser.GoogleId == res.GoogleSub)
                    {
                        return BaseResult<TokenResponseDto>.Ok(await CreateTokenResponseAsync(existingUser, false, null));
                    }
                    return BaseResult<TokenResponseDto>.Fail($"Tài khoản đã tồn tại email: {res.Email}! Bạn có muốn liên kết tài khoản google này với {res.Email} không", 400, new TokenResponseDto { UserId = existingUser.IdUser });
                }
                else
                {
                    var UserCode = await db.Users.Where(u => u.Position == ROLE.Customer.ToString()).CountAsync() + 1;
                    var NewUser = new Users
                    {
                        Id = Guid.NewGuid(),
                        Email = res.Email,
                        EmployeeCode = $"KH{UserCode.ToString().PadLeft(3, '0')}",
                        Name = res.Email.Split('@')[0],
                        UrlImage = res.Picture,
                        Status = true,
                        Position = ROLE.Customer.ToString(),
                        Gender = "Other",
                        CreatedAt = DateTime.UtcNow
                    };
                    await db.Users.AddAsync(NewUser);
                    await db.SaveChangesAsync();
                    var NewAccount = new Accounts
                    {
                        UserId = NewUser.Id,
                        Username = res.Email,
                        GoogleId = res.GoogleSub,
                        CreatedAt = DateTime.UtcNow
                    };
                    await db.Accounts.AddAsync(NewAccount);
                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return BaseResult<TokenResponseDto>.Ok(await CreateTokenResponseAsync(NewAccount, false, null));
                }
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                return BaseResult<TokenResponseDto>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }

        public async Task<BaseResult<TokenResponseDto>> LinkGGAccoung(GoogleAuthDto dto, Guid UserId)
        {
            try
            {
                var res = await GetGoogleResponse(dto);
                if (res is null) return BaseResult<TokenResponseDto>.Fail("Xác thực Google không thành công", 400);
                var existingUser = await db.Accounts
                    .Include(u => u.User)
                    .FirstOrDefaultAsync(u => u.IdUser == UserId);
                if (existingUser is null)
                    return BaseResult<TokenResponseDto>.Fail("Tài khoản không tồn tại", 404);
                if (res.GoogleSub == existingUser.GoogleId)
                    return BaseResult<TokenResponseDto>.Fail("Tài khoản Google đã được liên kết trước đó", 400);

                existingUser.GoogleId = res.GoogleSub;
                db.Accounts.Update(existingUser);
                await db.SaveChangesAsync();
                return BaseResult<TokenResponseDto>.Ok(await CreateTokenResponseAsync(existingUser, false, null));

            }
            catch (Exception e)
            {
                return BaseResult<TokenResponseDto>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }

        #endregion

        #region Otp
        public async Task<BaseResult<TokenResponseDto>?> ConfirmOtp(OtpDto dto)
        {
            try
            {
                var IsUserExit = await db.Accounts
                    .Include(u => u.User)
                    .FirstOrDefaultAsync(u => u.IdUser == dto.UserId);

                if (IsUserExit is null) return BaseResult<TokenResponseDto>.Fail("Người dùng không tồn tại", 400);

                if (IsUserExit.Otp != dto.Otp || IsUserExit.ExpriesOtp < DateTime.UtcNow)
                {
                    return BaseResult<TokenResponseDto>.Fail("Mã xác minh không đúng hoặc đã hết hạn", 400);
                }

                if (IsUserExit.Otp == dto.Otp && IsUserExit.ExpriesOtp >= DateTime.UtcNow)
                {
                    IsUserExit.Otp = null;
                    IsUserExit.ExpriesOtp = null;
                    IsUserExit.User.Status = true;
                    db.Accounts.Update(IsUserExit);
                    await db.SaveChangesAsync();
                    _cache.Remove($"User_Status_{dto.UserId}");
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
                    .Include(u => u.User)
                    .FirstOrDefaultAsync(u => u.User.Email == email);
                if (IsUserExit is null) return BaseResult<TokenResponseDto>.Fail("Người dùng không tồn tại", 400);

                if (string.IsNullOrEmpty(IsUserExit.Otp) || IsUserExit.ExpriesOtp == null)
                {
                    return BaseResult<TokenResponseDto>.Fail("Tài khoản đã xác minh email", 400);
                }

                var newOtp = RandomString.GenerateRandomNumericString(6);
                IsUserExit.Otp = newOtp;
                IsUserExit.ExpriesOtp = DateTime.UtcNow.AddMinutes(5);
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

                return BaseResult<TokenResponseDto>.Ok(new TokenResponseDto { UserId = IsUserExit.IdUser }, 200, "Gửi mã xác minh thành công");
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
                    .Include(u => u.User)
                    .FirstOrDefaultAsync(u => u.User.Email == EmailOrUserName || u.UserName == EmailOrUserName);
                if (IsUserExit is null) return BaseResult<bool>.Fail("Người dùng không tồn tại", 400, false);

                var newPassword = RandomString.GenerateRandomString(8);
                var hashedPassword = new PasswordHasher<Accounts>().HashPassword(IsUserExit, newPassword);
                IsUserExit.Password = hashedPassword;

                db.Accounts.Update(IsUserExit);
                await db.SaveChangesAsync();

                var SendEmail = await SendMail.SendMailAsync(configuration, IsUserExit.User.Email, "Quên mật khẩu", "Mật khẩu mới của bạn là: ", newPassword);
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
