using ItSupportServer.src.Modules.Role;
using ItSupportServer.src.Shared.Helpers;
using System.ComponentModel.DataAnnotations;
using static ItSupportServer.src.Modules.User.UserEnum;

namespace ItSupportServer.src.Modules.Employee
{
    public record CreateEmployeeDto(
        Guid EmpId,
        string? EmpCode,
        string FullName,
        string? PhoneNumber,
        string? Email,
        int DptId,
        int AreaId,
        string? Position,
        DateTime CreatedAt
        //string Password,

        //[DataType(DataType.Date, ErrorMessage = "Ngày sinh không đúng định dạng.")]
        //[CustomValidation(typeof(MyValidate), nameof(MyValidate.ValidateBirthdayStaff))]
        //DateTime? Birthday,
        //GENDER? Gender,
        //IFormFile? UrlImage
        );

    public record UpdateEmployeeDto(
        string? EmpCode,
        string FullName,
        string? PhoneNumber,
        string? Email,
        int DptId,
        int AreaId,
        //[Required(ErrorMessage = "Ngày sinh là bắt buộc.")]
        //[DataType(DataType.Date, ErrorMessage = "Ngày sinh không đúng định dạng.")]
        //[CustomValidation(typeof(MyValidate), nameof(MyValidate.ValidateBirthdayStaff))]
        //DateTime Birthday,
        //GENDER? Gender,
        string? Position,
        DateTime? UpdatedAt
        //string? UrlImage,
        //IFormFile? NewImage
        );

    public record ListEmployeeDto(
        Guid EmpId,
        string? EmpCode,
        string FullName,
        string? Email,
        string? PhoneNumber,
        string? Position,
        //string? UrlImage,
        //bool? Status,
        DateTime CreatedAt
        );

    public record UpdateProfileDto(
        string FullName,
        string? PhoneNumber,
        //GENDER? Gender,
        string? Email,
        DateTime? UpdatedAt
        //[DataType(DataType.Date, ErrorMessage = "Ngày sinh không đúng định dạng.")]
        //DateTime? Birthday,
        //IFormFile? Img
        );

    public record DetailUserDto
    {
        public Guid EmpId { get; init; }
        public string? EmpCode { get; init; }
        public string FullName { get; init; }
        //DateTime? Birthday,
        //string? Gender,
        public string? PhoneNumber { get; init; }
        public string? Email { get; init; }
        public int DptId { get; init; }
        public int AreaId { get; init; }
        string? Position { get; init; }
        //string? UrlImage,
        //bool? Status,
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public List<RolesDto>? Roles { get; init; }
    };
}