using ItSupportServer.Data.Models;
using ItSupportServer.src.Modules.Employee;
using ItSupportServer.src.Modules.User;
using ItSupportServer.src.Shared.Base;
using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.src.Modules.Account
{
    public class AccountsService(AppDbContext db) : IAccountsService
    {
        public async Task<BaseResult<ProfileDto>> GetProfileAsync(string EmpId)
        {
            try
            {
                var _empId = Guid.Parse(EmpId);

                var employee = await db.Employees.Include(e => e.Account)
                    .Select(e => new ProfileDto
                    {
                        EmpId = e.Id,
                        EmpCode = e.EmpCode,
                        FullName = e.FullName,
                        Email = e.Email,
                        PhoneNumber = e.PhoneNumber,
                        //Birthday = (DateTime?)e.Birthday,
                        CreatedAt = e.CreatedAt,
                        Position = e.Position,
                        UpdatedAt = e.UpdatedAt,
                        //UrlImage = e.UrlImage,
                        //Status = e.Status,
                        Username = e.Account != null ? e.Account.Username : null,
                    })
                    .FirstOrDefaultAsync(e => e.EmpId == _empId);
                if (employee is null) return BaseResult<ProfileDto>.Fail("User không tồn tại", 404);

                return BaseResult<ProfileDto>.Ok(employee);
            }
            catch (Exception e)
            {
                return BaseResult<ProfileDto>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }

        public async Task<BaseResult<ProfileDto>> UpdateProfileAsync(string Id, updateProfileDto dto)
        {
            try
            {
                Guid UserId = Guid.Parse(Id);
                var user = await db.Employees.FindAsync(UserId);
                if (user is null) return BaseResult<ProfileDto>.Fail("User không tồn tại", 404);

                mapper.Map(dto, user);

                //if (dto.Img is not null)
                //{
                //    var url = await git.UpdateOneImg(dto.Img, EMP_CUS.user.ToString());
                //    user.UrlImage = url.Url;
                //}
                if (dto.Email != user.Email)
                {
                    var IsEmailExist = db.Employees.Any(e => e.Email == dto.Email && e.EmpId != UserId);
                    if (IsEmailExist)
                    {
                        return BaseResult<ProfileDto>.Fail("Email đã tồn tại", 400);
                    }
                }
                //user.Gender = dto.Gender.ToString();
                //if (dto.Birthday is not null)
                //{
                //    var today = DateTime.UtcNow;
                //    var ageYears = today.Year - dto.Birthday.Value.Year;

                //    if (ageYears < 16)
                //        return BaseResult<ProfileDto>.Fail("Nhân viên phải từ 16 tuổi trở lên", 400);
                //    user.Birthday = DateTime.SpecifyKind((DateTime)dto.Birthday!, DateTimeKind.Utc);
                //}
                db.Employees.Update(user);
                await db.SaveChangesAsync();

                return BaseResult<ProfileDto>.Ok(new ProfileDto
                {
                    EmpId = user.EmpId,
                    EmpCode = user.EmpCode,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    //Birthday = (DateTime)user.Birthday!,
                    CreatedAt = user.CreatedAt,
                    Position = user.Position,
                    UpdatedAt = user.UpdatedAt,
                    //UrlImage = user.UrlImage,
                    //Status = user.Status,
                    Username = user.Account != null ? user.Account.Username : null,
                });
            }
            catch (Exception e)
            {
                return BaseResult<ProfileDto>.Fail($"Lỗi Hệ thống: {e.Message}", 500);
            }
        }
    }
}
