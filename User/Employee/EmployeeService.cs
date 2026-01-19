using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NhaHangApi.EF_Core.Data;
using NhaHangApi.src.Shared.Base;
using NhaHangApi.src.Shared.Helper;
using static NhaHangApi.src.Shared.Base.BaseEnum;
using static NhaHangApi.src.Modules.User.UsersEnum;
using NhaHangApi.src.Modules.Role;

namespace NhaHangApi.src.Modules.User.Employee
{
    public class EmployeeService(AppDbContext db, IMapper mapper, BaseCrud<Users, Guid> crud, GitHubImageService git, IMemoryCache _cache) : IEmployeeService
    {

        public async Task<BaseResult<PaginatedResult<List<ListEmployeeDto>>>> GetEmployees(string? query, int page, int pageSize, SortOBJ? sort)
        {
            try
            {
                var employees = await db.Users
                    .Include(u => u.Account)
                    .ThenInclude(a => a.AccountRoles)
                    .Where(u => u.DeletedAt == null && u.Account.AccountRoles.Any(r => r.RoleId != EMP_CUS.customer.ToString())).Select(e => new ListEmployeeDto
                    {
                        Id = e.Id,
                        EmployeeCode = e.EmployeeCode,
                        UserName = e.Account != null ? e.Account.UserName : null,
                        Name = e.Name,
                        Email = e.Email,
                        PhoneNumber = e.PhoneNumber,
                        Position = e.Position,
                        UrlImage = e.UrlImage,
                        Status = e.Status,
                        CreatedAt = e.CreatedAt,
                    }).ToListAsync();
                if (!string.IsNullOrEmpty(query))
                {
                    query = query.ToLower();
                    employees = employees.Where(e =>
                        e.Name.ToLower().Contains(query) ||
                        e.Email.Contains(query) ||
                        e.EmployeeCode.ToLower().Contains(query) ||
                        e.PhoneNumber!.Contains(query) ||
                        e.Position.ToLower().Contains(query) ||
                        e.Id.ToString() == query).ToList();
                }

                var result = Pagination<ListEmployeeDto>.PaginationList(employees, page, pageSize, sort);

                return BaseResult<PaginatedResult<List<ListEmployeeDto>>>.Ok(result);
            }
            catch (Exception e)
            {
                return BaseResult<PaginatedResult<List<ListEmployeeDto>>>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }

        public async Task<BaseResult<DetailUserDto>> GetEmployee(string Id)
        {
            try
            {
                var UserId = Guid.Parse(Id);
                var employee = await db.Users.Include(u => u.Account)
                    .ThenInclude(a => a.AccountRoles)
                    .ThenInclude(ar => ar.Role)
                    .Where(u => u.DeletedAt == null)
                    .Select(e => new DetailUserDto
                    {
                        Id = e.Id,
                        Code = e.EmployeeCode,
                        UserName = e.Account != null ? e.Account.UserName : null,
                        Name = e.Name,
                        Email = e.Email,
                        PhoneNumber = e.PhoneNumber,
                        Address = e.Address,
                        Birthday = (DateTime)e.Birthday!,
                        CreatedAt = e.CreatedAt,
                        UpdatedAt = e.UpdatedAt,
                        Position = e.Position,
                        UrlImage = e.UrlImage,
                        Status = e.Status,
                        Roles = e.Account.AccountRoles.Select(ar => new RoleDto
                        {
                            Id = ar.Role.Id,
                            Name = ar.Role.Name,
                        }).ToList()
                    })
                    .FirstOrDefaultAsync(e => e.Id == UserId);

                if (employee is null) return BaseResult<DetailUserDto>.Fail("Nhân viên không tồn tại", 404);

                return BaseResult<DetailUserDto>.Ok(employee);
            }
            catch (Exception e)
            {
                return BaseResult<DetailUserDto>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }

        public async Task<BaseResult<CreateEmployeeDto>> CreateEmployee(CreateEmployeeDto dto)
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var IsEmailExist = db.Users.Any(e => e.Email == dto.Email);
                if (IsEmailExist)
                {
                    return BaseResult<CreateEmployeeDto>.Fail("Email đã tồn tại", 400);
                }

                var employee = mapper.Map<Users>(dto);

                employee.Birthday = DateTime.SpecifyKind(dto.Birthday, DateTimeKind.Utc);
                employee.Position = ROLE.Employee.ToString();

                var CountEmployee = await db.Users.CountAsync() + 1;
                employee.EmployeeCode = $"NV{CountEmployee.ToString().PadLeft(3, '0')}";
                if (dto.UrlImage is not null)
                {
                    var url = await git.UpdateOneImg(dto.UrlImage, EMP_CUS.user.ToString());
                    employee.UrlImage = url.Url;
                }

                var NewEmployee = await db.Users.AddAsync(employee);
                await db.SaveChangesAsync();

                var HashPassword = new PasswordHasher<CreateEmployeeDto>()
                    .HashPassword(dto, dto.Password);
                var NewAccount = await db.Accounts.AddAsync(new Account
                {
                    IdUser = NewEmployee.Entity.Id,
                    UserName = dto.Email,
                    Password = HashPassword

                });

                await db.AccountRoles.AddAsync(new AccountRole
                {
                    AccountId = NewEmployee.Entity.Id,
                    RoleId = EMP_CUS.employee.ToString()
                });
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                return BaseResult<CreateEmployeeDto>.Ok(dto);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                return BaseResult<CreateEmployeeDto>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }

        public async Task<BaseResult<Users>> UpdateEmployee(string Id, UpdateEmployeeDto dto)
        {
            try
            {
                var IsEmployeeExist = await db.Users.FirstOrDefaultAsync(e => e.Id.ToString() == Id && e.Position != ROLE.Supper_Admin.ToString());
                if (IsEmployeeExist is null) return BaseResult<Users>.Fail("Nhân viên không tồn tại", 404);

                var IsEmailExist = db.Users.Any(e => e.Email == dto.Email && e.Id.ToString() != Id);
                if (IsEmailExist)
                {
                    return BaseResult<Users>.Fail("Email đã tồn tại", 400);
                }

                var UpdateEmployee = mapper.Map(dto, IsEmployeeExist);

                if (dto.NewImage is not null)
                {
                    var url = await git.UpdateOneImg(dto.NewImage, EMP_CUS.user.ToString());
                    UpdateEmployee.UrlImage = url.Url;
                }
                UpdateEmployee.Birthday = DateTime.SpecifyKind(dto.Birthday, DateTimeKind.Utc);

                db.Users.Update(UpdateEmployee);
                await db.SaveChangesAsync();

                return BaseResult<Users>.Ok(UpdateEmployee);
            }
            catch (Exception e)
            {
                return BaseResult<Users>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }

        public async Task<BaseResult<STATUS_EMP>> ChangeStatus(string idString, STATUS_EMP status)
        {
            try
            {
                // 1. Parse ID sang Guid trước để Query nhanh hơn (Tận dụng Index SQL)
                if (!Guid.TryParse(idString, out Guid userId))
                {
                    return BaseResult<STATUS_EMP>.Fail("ID không hợp lệ", 400);
                }

                var user = await db.Users
                    .Include(e => e.Account)
                    .ThenInclude(a => a.AccountRoles)
                    .FirstOrDefaultAsync(e => e.Id == userId && e.Position != ROLE.Supper_Admin.ToString());

                if (user is null)
                    return BaseResult<STATUS_EMP>.Fail("User không tồn tại hoặc là Admin", 404);

                bool newStatusValue = (status == STATUS_EMP.Active);

                if (user.Status == newStatusValue)
                {
                    string statusStr = newStatusValue ? "Hoạt động" : "Không hoạt động";
                    return BaseResult<STATUS_EMP>.Fail($"User đã ở trạng thái {statusStr}", 400);
                }

                // 4. Cập nhật Database
                user.Status = newStatusValue;

                await db.SaveChangesAsync();

                _cache.Remove($"User_Status_{idString}");

                return BaseResult<STATUS_EMP>.Ok(status);
            }
            catch (Exception e)
            {
                return BaseResult<STATUS_EMP>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }

        public async Task<BaseResult<ProfileDto>> Profile(string Id)
        {
            try
            {
                var UserId = Guid.Parse(Id);
                var employee = await db.Users.Include(e => e.Account)
                    .Select(e => new ProfileDto
                    {
                        Id = e.Id,
                        EmployeeCode = e.EmployeeCode,
                        Name = e.Name,
                        Email = e.Email,
                        PhoneNumber = e.PhoneNumber,
                        Address = e.Address,
                        Birthday = (DateTime)e.Birthday,
                        CreatedAt = e.CreatedAt,
                        Position = e.Position,
                        UpdatedAt = e.UpdatedAt,
                        UrlImage = e.UrlImage,
                        Status = e.Status,
                        UserName = e.Account != null ? e.Account.UserName : null,
                    })
                    .FirstOrDefaultAsync(e => e.Id == UserId);
                if (employee is null) return BaseResult<ProfileDto>.Fail("User không tồn tại", 404);

                return BaseResult<ProfileDto>.Ok(employee);
            }
            catch (Exception e)
            {
                return BaseResult<ProfileDto>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }

        public async Task<BaseResult<ProfileDto>> UpdateProfile(string Id, updateProfileDto dto)
        {
            try
            {
                Guid UserId = Guid.Parse(Id);
                var user = await db.Users.FindAsync(UserId);
                if (user is null) return BaseResult<ProfileDto>.Fail("User không tồn tại", 404);

                mapper.Map(dto, user);
                if (dto.Img is not null)
                {
                    var url = await git.UpdateOneImg(dto.Img, EMP_CUS.user.ToString());
                    user.UrlImage = url.Url;
                }
                if (dto.Email != user.Email)
                {
                    var IsEmailExist = db.Users.Any(e => e.Email == dto.Email && e.Id != UserId);
                    if (IsEmailExist)
                    {
                        return BaseResult<ProfileDto>.Fail("Email đã tồn tại", 400);
                    }
                }
                user.Gender = dto.Gender.ToString();
                if (dto.Birthday is not null)
                {
                    var today = DateTime.UtcNow;
                    var ageYears = today.Year - dto.Birthday.Value.Year;
                    if (user.Position == ROLE.Customer.ToString())
                    {
                        if (ageYears < 14)
                            return BaseResult<ProfileDto>.Fail("Khách hàng phải từ 14 tuổi trở lên", 400);
                        user.Birthday = DateTime.SpecifyKind((DateTime)dto.Birthday!, DateTimeKind.Utc);
                    }
                    else
                    {
                        if (ageYears < 16)
                            return BaseResult<ProfileDto>.Fail("Nhân viên phải từ 16 tuổi trở lên", 400);
                        user.Birthday = DateTime.SpecifyKind((DateTime)dto.Birthday!, DateTimeKind.Utc);
                    }
                }
                db.Users.Update(user);
                await db.SaveChangesAsync();

                return BaseResult<ProfileDto>.Ok(new ProfileDto
                {
                    Id = user.Id,
                    EmployeeCode = user.EmployeeCode,
                    Name = user.Name,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    Birthday = (DateTime)user.Birthday!,
                    CreatedAt = user.CreatedAt,
                    Position = user.Position,
                    UpdatedAt = user.UpdatedAt,
                    UrlImage = user.UrlImage,
                    Status = user.Status,
                    UserName = user.Account != null ? user.Account.UserName : null,
                });
            }
            catch (Exception e)
            {
                return BaseResult<ProfileDto>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }

        public async Task<BaseResult<string>>? ChangeRole(string Id, ROLE newRole)
        {
            try
            {
                var UserId = Guid.Parse(Id);
                var user = await db.Users.FindAsync(UserId);
                if (user is null) return BaseResult<string>.Fail("User không tồn tại", 404);
                user.Position = newRole.ToString();
                db.Users.Update(user);
                await db.SaveChangesAsync();
                _cache.Remove($"User_Status_{UserId}");
                return BaseResult<string>.Ok("Đổi vai trò thành công");
            }
            catch (Exception e)
            {
                return BaseResult<string>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }
    }
}
