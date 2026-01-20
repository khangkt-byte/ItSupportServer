using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ITSupportServer.EF_Core.Data;
using ITSupportServer.src.Shared.Base;
using ITSupportServer.src.Shared.Helper;
using System.Collections.Generic;
using static ITSupportServer.src.Shared.Base.BaseEnum;
using static ITSupportServer.src.Modules.User.UsersEnum;

namespace ITSupportServer.src.Modules.User.Customer
{
    public class CustomerService(AppDbContext db, GitHubImageService git, IConfiguration con, IMapper map, BaseCrud<Users, Guid> crud) : ICustomerService
    {
        public async Task<BaseResult<PaginatedResult<List<ListCustomerDto>>>> GetCustomers(string? query, int page, int pageSize, SortOBJ? sort)
        {
            try
            {
                var customersQuery = db.Users
                    .Include(u => u.Account)
                    .ThenInclude(a => a.AccountRoles)
                    .Where(u => u.DeletedAt == null && u.Account.AccountRoles.Any(r => r.RoleId == EMP_CUS.customer.ToString())).Select(c => new ListCustomerDto
                    {
                        Id = c.Id,
                        Code = c.EmployeeCode,
                        Name = c.Name,
                        Email = c.Email,
                        UserName = c.Account.UserName,
                        ImgUrl = c.UrlImage
                    }).AsQueryable();

                if (!string.IsNullOrEmpty(query))
                {
                    customersQuery = customersQuery
                        .Where(c => c.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                    c.Email.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                    c.UserName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                    c.Code.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                    c.Id == Guid.Parse(query));
                }

                var result = await Pagination<ListCustomerDto>.PaginationAsync(customersQuery, page, pageSize, sort);
                return BaseResult<PaginatedResult<List<ListCustomerDto>>>.Ok(result);
            }
            catch (Exception ex)
            {
                return BaseResult<PaginatedResult<List<ListCustomerDto>>>.Fail($"Lỗi hệ thống: {ex.Message}", 500);
            }
        }

        public async Task<BaseResult<ResultRegisterCustomerDto>> RegisterCustomerdto(RegisterCustomerDto dto)
        {
            using var transaction = db.Database.BeginTransaction();
            try
            {
                var exitAccount = await db.Accounts.Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.User.Email == dto.Email || a.UserName == dto.UserName);
                if (exitAccount is not null)
                {
                    return BaseResult<ResultRegisterCustomerDto>.Fail("Tài khoản đã tồn tại", 400);
                }
                if (dto.UserName is null) dto.UserName = dto.Email;

                var countCustomer = await db.Users
                    .Include(u => u.Account)
                    .ThenInclude(a => a.AccountRoles)
                    .Where(c => c.Account.AccountRoles.Any(r => r.RoleId == EMP_CUS.customer.ToString())).CountAsync() + 1;
                //add new user
                var user = new Users
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    EmployeeCode = $"KH{countCustomer.ToString().PadLeft(3, '0')}",
                    Gender = dto.Gender.ToString(),
                    Position = ROLE.Customer.ToString(),
                };
                await db.Users.AddAsync(user);
                await db.SaveChangesAsync();

                var hashedPass = new PasswordHasher<RegisterCustomerDto>()
                    .HashPassword(dto, dto.Password);
                //add new account
                var code = RandomString.GenerateRandomNumericString(6);
                var account = new Accounts
                {
                    UserId = user.Id,
                    Username = dto.UserName,
                    Password = hashedPass,
                    Otp = code,
                    ExpiredOtp = DateTime.UtcNow.AddMinutes(5),
                };
                var NewAcc = await db.Accounts.AddAsync(account);
                await db.SaveChangesAsync();
                //add role
                await db.AccountRoles.AddAsync(new AccountRoles
                {
                    AccountId = NewAcc.Entity.IdUser,
                    RoleId = EMP_CUS.customer.ToString()
                });
                await db.SaveChangesAsync();
                var Mail = await SendMail.SendMailAsync(con, account.User.Email, "Xác thực email của bạn", "Đây là mã otp của bạn, mã sẽ hết hạn sau 5 phút!", code);

                if (!Mail)
                {
                    await transaction.RollbackAsync();
                    return BaseResult<ResultRegisterCustomerDto>.Fail("Gửi mã xác thực thất bại, vui lòng kiểm tra lại email", 500);
                }

                await transaction.CommitAsync();

                var result = map.Map<ResultRegisterCustomerDto>(dto);
                result.IdUser = user.Id;

                return BaseResult<ResultRegisterCustomerDto>.Ok(result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BaseResult<ResultRegisterCustomerDto>.Fail($"Lỗi hệ thống: {ex.Message}", 500);
            }
        }
    }
}
