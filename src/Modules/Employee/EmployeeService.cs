//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Caching.Memory;
//using ItSupportServer.Data;
//using ItSupportServer.src.Shared.Base;
//using static ItSupportServer.src.Shared.Base.BaseEnum;
//using ItSupportServer.src.Modules.Role;
//using ItSupportServer.src.Modules.User;
//using static ItSupportServer.src.Modules.User.UsersEnum;

//namespace ItSupportServer.src.Modules.Employee
//{
//    public class EmployeeService(AppDbContext db, BaseCrud<Employees, Guid> crud, IMemoryCache _cache) : IEmployeeService
//    {

//        public async Task<BaseResult<PaginatedResult<List<ListEmployeeDto>>>> GetEmployeesAsync(string? query, int page, int pageSize, SortOBJ? sort)
//        {
//            try
//            {
//                var employees = await db.Employees
//                    .Include(u => u.Account)
//                    .ThenInclude(a => a.AccountRoles)
//                    .Where(u => u.DeletedAt == null && u.Account.AccountRoles.Any()).Select(e => new ListEmployeeDto
//                    {
//                        EmpId = e.Id,
//                        EmpCode = e.EmpCode,
//                        FullName = e.FullName,
//                        Email = e.Email,
//                        PhoneNumber = e.PhoneNumber,
//                        Position = e.Position,
//                        UrlImage = e.UrlImage,
//                        Status = e.Status,
//                        CreatedAt = e.CreatedAt,
//                    }).ToListAsync();
//                if (!string.IsNullOrEmpty(query))
//                {
//                    query = query.ToLower();
//                    employees = employees.Where(e =>
//                        e.FullName.ToLower().Contains(query) ||
//                        e.Email.Contains(query) ||
//                        e.EmpCode.ToLower().Contains(query) ||
//                        e.PhoneNumber!.Contains(query) ||
//                        e.Position.ToLower().Contains(query) ||
//                        e.EmpId.ToString() == query).ToList();
//                }

//                var result = Pagination<ListEmployeeDto>.PaginationList(employees, page, pageSize, sort);

//                return BaseResult<PaginatedResult<List<ListEmployeeDto>>>.Ok(result);
//            }
//            catch (Exception e)
//            {
//                return BaseResult<PaginatedResult<List<ListEmployeeDto>>>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
//            }
//        }

//        public async Task<BaseResult<DetailUserDto>> GetEmployeeAsync(string EmpId)
//        {
//            try
//            {
//                var _empId = Guid.Parse(EmpId);

//                var employee = await db.Employees.Include(u => u.Account)
//                    .ThenInclude(a => a.AccountRoles)
//                    .ThenInclude(ar => ar.Role)
//                    .Where(u => u.DeletedAt == null)
//                    .Select(e => new DetailUserDto
//                    {
//                        EmpId = e.EmpId,
//                        EmpCode = e.EmpCode,
//                        //Username = e.Account != null ? e.Account.Username : null,
//                        FullName = e.FullName,
//                        Email = e.Email,
//                        PhoneNumber = e.PhoneNumber,
//                        Birthday = (DateTime)e.Birthday!,
//                        CreatedAt = e.CreatedAt,
//                        UpdatedAt = e.UpdatedAt,
//                        Position = e.Position,
//                        UrlImage = e.UrlImage,
//                        Status = e.Status,
//                        Roles = e.Account.AccountRoles.Select(ar => new RolesDto
//                        {
//                            RoleId = ar.Role.Id,
//                            Name = ar.Role.Name,
//                        }).ToList()
//                    })
//                    .FirstOrDefaultAsync(e => e.EmpId == _empId);

//                if (employee is null) return BaseResult<DetailUserDto>.Fail("Nhân viên không tồn tại", 404);

//                return BaseResult<DetailUserDto>.Ok(employee);
//            }
//            catch (Exception e)
//            {
//                return BaseResult<DetailUserDto>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
//            }
//        }

//        public async Task<BaseResult<CreateEmployeeDto>> CreateEmployeeAsync(CreateEmployeeDto dto)
//        {
//            using var transaction = await db.Database.BeginTransactionAsync();
//            try
//            {
//                var IsEmailExist = db.Employees.Any(e => e.Email == dto.Email);
//                if (IsEmailExist)
//                {
//                    return BaseResult<CreateEmployeeDto>.Fail("Email đã tồn tại", 400);
//                }

//                //var CountEmployee = await db.Employees.CountAsync() + 1;
//                //employee.EmpCode = $"NV{CountEmployee.ToString().PadLeft(3, '0')}";

//                var employee = new Employees
//                {
//                    EmpId = Guid.CreateVersion7(),
//                    EmpCode = dto.EmpCode,
//                    FullName = dto.FullName,
//                    Birthday = dto.Birthday,
//                    Gender = dto.Gender.ToString(),
//                    PhoneNumber = dto.PhoneNumber,
//                    Email = dto.Email,
//                    DptId = dto.DptId,
//                    AreaId = dto.AreaId,
//                    Position = dto.Position,
//                    CreatedAt = DateTime.UtcNow,
//                    Status = true
//                };

//                employee.Birthday = dto.Birthday != null
//                    ? DateTime.SpecifyKind((global::System.DateTime)dto.Birthday, DateTimeKind.Utc)
//                    : null;
//                employee.Position = ROLE.Employee.ToString();

//                //if (dto.UrlImage is not null)
//                //{
//                //    var url = await git.UpdateOneImg(dto.UrlImage, EMP_CUS.user.ToString());
//                //    employee.UrlImage = url.Url;
//                //}

//                var NewEmployee = await db.Employees.AddAsync(employee);
//                await db.SaveChangesAsync();

//                #region NewAccount
//                //var HashPassword = new PasswordHasher<CreateEmployeeDto>()
//                //    .HashPassword(dto, dto.Password);
//                //var NewAccount = await db.Accounts.AddAsync(new Accounts
//                //{
//                //    AccountId = NewEmployee.Entity.Id,
//                //    Username = dto.Email,
//                //    Password = HashPassword

//                //});

//                //await db.AccountRoles.AddAsync(new AccountRoles
//                //{
//                //    AccountId = NewEmployee.Entity.Id,
//                //    RoleId = EMP_CUS.employee.ToString()
//                //});
//                #endregion

//                await db.SaveChangesAsync();
//                await transaction.CommitAsync();
//                return BaseResult<CreateEmployeeDto>.Ok(dto);
//            }
//            catch (Exception e)
//            {
//                await transaction.RollbackAsync();
//                return BaseResult<CreateEmployeeDto>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
//            }
//        }

//        public async Task<BaseResult<Employees>> UpdateEmployeeAsync(string Id, UpdateEmployeeDto dto)
//        {
//            try
//            {
//                var IsEmployeeExist = await db.Employees.FirstOrDefaultAsync(e => e.Id.ToString() == Id && e.Position != ROLE.Super_Admin.ToString());
//                if (IsEmployeeExist is null) return BaseResult<Employees>.Fail("Nhân viên không tồn tại", 404);

//                var IsEmailExist = db.Employees.Any(e => e.Email == dto.Email && e.Id.ToString() != Id);
//                if (IsEmailExist)
//                {
//                    return BaseResult<Employees>.Fail("Email đã tồn tại", 400);
//                }

//                var UpdateEmployee = mapper.Map(dto, IsEmployeeExist);

//                //if (dto.NewImage is not null)
//                //{
//                //    var url = await git.UpdateOneImg(dto.NewImage, EMP_CUS.user.ToString());
//                //    UpdateEmployee.UrlImage = url.Url;
//                //}
//                UpdateEmployee.Birthday = DateTime.SpecifyKind(dto.Birthday, DateTimeKind.Utc);

//                db.Employees.Update(UpdateEmployee);
//                await db.SaveChangesAsync();

//                return BaseResult<Employees>.Ok(UpdateEmployee);
//            }
//            catch (Exception e)
//            {
//                return BaseResult<Employees>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
//            }
//        }

//        public async Task<BaseResult<STATUS_EMP>> ChangeStatusAsync(string idString, STATUS_EMP status)
//        {
//            try
//            {
//                // 1. Parse ID sang Guid trước để Query nhanh hơn (Tận dụng Index SQL)
//                if (!Guid.TryParse(idString, out Guid userId))
//                {
//                    return BaseResult<STATUS_EMP>.Fail("ID không hợp lệ", 400);
//                }

//                var user = await db.Employees
//                    .Include(e => e.Account)
//                    .ThenInclude(a => a.AccountRoles)
//                    .FirstOrDefaultAsync(e => e.Id == userId && e.Position != ROLE.Super_Admin.ToString());

//                if (user is null)
//                    return BaseResult<STATUS_EMP>.Fail("User không tồn tại hoặc là Admin", 404);

//                bool newStatusValue = (status == STATUS_EMP.Active);

//                if (user.Status == newStatusValue)
//                {
//                    string statusStr = newStatusValue ? "Hoạt động" : "Không hoạt động";
//                    return BaseResult<STATUS_EMP>.Fail($"User đã ở trạng thái {statusStr}", 400);
//                }

//                // 4. Cập nhật Database
//                user.Status = newStatusValue;

//                await db.SaveChangesAsync();

//                _cache.Remove($"User_Status_{idString}");

//                return BaseResult<STATUS_EMP>.Ok(status);
//            }
//            catch (Exception e)
//            {
//                return BaseResult<STATUS_EMP>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
//            }
//        }

//        public async Task<BaseResult<ProfileDto>> GetProfileAsync(string EmpId)
//        {
//            try
//            {
//                var _empId = Guid.Parse(EmpId);

//                var employee = await db.Employees.Include(e => e.Account)
//                    .Select(e => new ProfileDto
//                    {
//                        EmpId = e.Id,
//                        EmpCode = e.EmpCode,
//                        FullName = e.FullName,
//                        Email = e.Email,
//                        PhoneNumber = e.PhoneNumber,
//                        Birthday = (DateTime?)e.Birthday,
//                        CreatedAt = e.CreatedAt,
//                        Position = e.Position,
//                        UpdatedAt = e.UpdatedAt,
//                        UrlImage = e.UrlImage,
//                        Status = e.Status,
//                        Username = e.Account != null ? e.Account.Username : null,
//                    })
//                    .FirstOrDefaultAsync(e => e.EmpId == _empId);
//                if (employee is null) return BaseResult<ProfileDto>.Fail("User không tồn tại", 404);

//                return BaseResult<ProfileDto>.Ok(employee);
//            }
//            catch (Exception e)
//            {
//                return BaseResult<ProfileDto>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
//            }
//        }

//        public async Task<BaseResult<ProfileDto>> UpdateProfileAsync(string Id, updateProfileDto dto)
//        {
//            try
//            {
//                Guid UserId = Guid.Parse(Id);
//                var user = await db.Employees.FindAsync(UserId);
//                if (user is null) return BaseResult<ProfileDto>.Fail("User không tồn tại", 404);

//                mapper.Map(dto, user);

//                //if (dto.Img is not null)
//                //{
//                //    var url = await git.UpdateOneImg(dto.Img, EMP_CUS.user.ToString());
//                //    user.UrlImage = url.Url;
//                //}
//                if (dto.Email != user.Email)
//                {
//                    var IsEmailExist = db.Employees.Any(e => e.Email == dto.Email && e.EmpId != UserId);
//                    if (IsEmailExist)
//                    {
//                        return BaseResult<ProfileDto>.Fail("Email đã tồn tại", 400);
//                    }
//                }
//                user.Gender = dto.Gender.ToString();
//                if (dto.Birthday is not null)
//                {
//                    var today = DateTime.UtcNow;
//                    var ageYears = today.Year - dto.Birthday.Value.Year;

//                    if (ageYears < 16)
//                        return BaseResult<ProfileDto>.Fail("Nhân viên phải từ 16 tuổi trở lên", 400);
//                    user.Birthday = DateTime.SpecifyKind((DateTime)dto.Birthday!, DateTimeKind.Utc);
//                }
//                db.Employees.Update(user);
//                await db.SaveChangesAsync();

//                return BaseResult<ProfileDto>.Ok(new ProfileDto
//                {
//                    EmpId = user.EmpId,
//                    EmpCode = user.EmpCode,
//                    FullName = user.FullName,
//                    Email = user.Email,
//                    PhoneNumber = user.PhoneNumber,
//                    Birthday = (DateTime)user.Birthday!,
//                    CreatedAt = user.CreatedAt,
//                    Position = user.Position,
//                    UpdatedAt = user.UpdatedAt,
//                    UrlImage = user.UrlImage,
//                    Status = user.Status,
//                    Username = user.Account != null ? user.Account.Username : null,
//                });
//            }
//            catch (Exception e)
//            {
//                return BaseResult<ProfileDto>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
//            }
//        }

//        public async Task<BaseResult<string>>? ChangeRoleAsync(string EmpId, ROLE newRole)
//        {
//            try
//            {
//                var user = await db.Employees.FindAsync(EmpId);
//                if (user is null) return BaseResult<string>.Fail("User không tồn tại", 404);
//                user.Position = newRole.ToString();
//                db.Employees.Update(user);
//                await db.SaveChangesAsync();
//                _cache.Remove($"User_Status_{EmpId}");
//                return BaseResult<string>.Ok("Đổi vai trò thành công");
//            }
//            catch (Exception e)
//            {
//                return BaseResult<string>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
//            }
//        }
//    }
//}
