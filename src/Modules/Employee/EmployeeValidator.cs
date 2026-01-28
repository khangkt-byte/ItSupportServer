using FluentValidation;

namespace ItSupportServer.src.Modules.Employee
{
    public class CreateEmployeeDtoValidator : AbstractValidator<CreateEmployeeDto>
    {
        public CreateEmployeeDtoValidator()
        {
            RuleFor(x => x.EmpCode)
                .MaximumLength(20).WithMessage("Mã nhân viên không được quá 20 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.EmpCode));

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Họ tên là bắt buộc.")
                .MaximumLength(150).WithMessage("Họ tên không được quá 150 ký tự.")
                .Matches(@"^[\p{L}\p{M}\p{N} _-]+$").WithMessage("Họ tên chỉ được chứa chữ cái có dấu, số, khoảng trắng, gạch ngang (-) và gạch dưới (_).");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(15).WithMessage("Số điện thoại không được quá 15 ký tự.")
                .Matches(@"^(0\d{9,12}|\+?84\d{9,12})$").WithMessage("Số điện thoại không hợp lệ (ví dụ: 0912345678 hoặc +84912345678).")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email không hợp lệ.")
                .MaximumLength(254).WithMessage("Email không được quá 254 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.DptId)
                .GreaterThan(0).WithMessage("Bộ phận là bắt buộc.");

            RuleFor(x => x.AreaId)
                .GreaterThan(0).WithMessage("Khu vực là bắt buộc.");

            RuleFor(x => x.Position)
                .MaximumLength(150).WithMessage("Chức vụ không được quá 150 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Position));

            //RuleFor(x => x.Password)
            //    .NotEmpty().WithMessage("Mật khẩu là bắt buộc.")
            //    .Length(6, 100).WithMessage("Mật khẩu phải có độ dài từ 8 đến 100 ký tự.")
            //    .Matches(@"^[a-zA-Z0-9_@#]+$").WithMessage("Mật khẩu chỉ được chứa chữ cái, @, #, gạch ngang (-) và gạch dưới (_).");

            //RuleFor(x => x.Birthday)
            //    .Must(date => date == null || date <= DateTime.Today).WithMessage("Ngày sinh không được lớn hơn ngày hiện tại.");

        }
    }

    public class UpdateEmployeeDtoValidator : AbstractValidator<UpdateEmployeeDto>
    {
        public UpdateEmployeeDtoValidator()
        {
            RuleFor(x => x.EmpCode)
                .MaximumLength(20).WithMessage("Mã nhân viên không được quá 20 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.EmpCode));

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Họ tên là bắt buộc.")
                .MaximumLength(150).WithMessage("Họ tên không được quá 150 ký tự.")
                .Matches(@"^[\p{L}\p{M}\p{N} _-]+$").WithMessage("Họ tên chỉ được chứa chữ cái có dấu, số, khoảng trắng, gạch ngang (-) và gạch dưới (_).")
                .When(x => x.FullName != null);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(15).WithMessage("Số điện thoại không được quá 15 ký tự.")
                .Matches(@"^(0\d{9,12}|\+?84\d{9,12})$").WithMessage("Số điện thoại không hợp lệ (ví dụ: 0912345678 hoặc +84912345678).")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email không hợp lệ.")
                .MaximumLength(254).WithMessage("Email không được quá 254 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.DptId)
                .GreaterThan(0).WithMessage("Bộ phận là bắt buộc.");

            RuleFor(x => x.AreaId)
                .GreaterThan(0).WithMessage("Khu vực là bắt buộc.");

            RuleFor(x => x.Position)
                .MaximumLength(150).WithMessage("Chức vụ không được quá 150 ký tự.");
        }
    }

    public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
    {
        public UpdateProfileDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Tên là bắt buộc.")
                .MaximumLength(150).WithMessage("Tên không được quá 150 ký tự.")
                .Matches(@"^[\p{L}\p{M}\p{N} _-]+$").WithMessage("Tên chỉ được chứa chữ cái có dấu, số, khoảng trắng, gạch ngang (-) và gạch dưới (_).");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(15).WithMessage("Số điện thoại không được quá 15 ký tự.")
                .Matches(@"^(0\d{9,12}|\+?84\d{9,12})$").WithMessage("Số điện thoại không hợp lệ (ví dụ: 0912345678 hoặc +84912345678).")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email không đúng định dạng.")
                .MaximumLength(254).WithMessage("Email không được quá 254 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));
        }
    }
}
