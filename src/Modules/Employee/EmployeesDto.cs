using ItSupportServer.src.Modules.Role;
using ItSupportServer.src.Shared.Helpers;
using System.ComponentModel.DataAnnotations;
using static ItSupportServer.src.Modules.User.UsersEnum;

namespace ItSupportServer.src.Modules.Employee
{
    public record CreateEmployeeDto(
        string? EmpCode,
        string FullName,
        string? PhoneNumber,
        string? Email,
        int DptId,
        int AreaId,
        string? Position,
        string Password,

        [DataType(DataType.Date, ErrorMessage = "Ngày sinh không đúng định dạng.")]
        [CustomValidation(typeof(MyValidate), nameof(MyValidate.ValidateBirthdayStaff))]
        DateTime? Birthday,
        GENDER? Gender,
        IFormFile? UrlImage
        );

    public record UpdateEmployeeDto(
        string? EmpCode,
        string FullName,
        string PhoneNumber,
        string Email,
        int DptId,
        int AreaId,
        [Required(ErrorMessage = "Ngày sinh là bắt buộc.")]
        [DataType(DataType.Date, ErrorMessage = "Ngày sinh không đúng định dạng.")]
        [CustomValidation(typeof(MyValidate), nameof(MyValidate.ValidateBirthdayStaff))]
        DateTime Birthday,
        GENDER? Gender,
        string Position,
        string? UrlImage,
        IFormFile? NewImage
        );

    public record ListEmployeeDto(
        Guid EmpId,
        string EmpCode,
        string FullName,
        string Email,
        string? PhoneNumber,
        string Position,
        string? UrlImage,
        bool? Status,
        DateTime CreatedAt
        );

    public record updateProfileDto(
        string Name,
        string? PhoneNumber,
        string? Address,
        GENDER? Gender,
        string Email,
        [DataType(DataType.Date, ErrorMessage = "Ngày sinh không đúng định dạng.")]
        DateTime? Birthday,
        IFormFile? Img
        );

    public record DetailUserDto(
        Guid EmpId,
        string EmpCode,
        string FullName,
        DateTime? Birthday,
        string? Gender,
        string? PhoneNumber,
        string? Email,
        int DptId,
        int AreaId,
        string? Position,
        string? UrlImage,
        bool? Status,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        List<RolesDto>? Roles
        );
}