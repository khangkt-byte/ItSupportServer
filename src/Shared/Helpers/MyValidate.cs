using System.ComponentModel.DataAnnotations;

namespace ItSupportServer.src.Shared.Helpers
{
    public class MyValidate
    {
        public static ValidationResult? ValidateBirthdayStaff(DateTime birthday, ValidationContext _)
        {
            var result = new MyValidate().Birthday(birthday, false, _);
            return result;
        }

        public static ValidationResult? ValidateBirthdayCustomer(DateTime? birthday, ValidationContext _)
        {
            if (!birthday.HasValue)
                return ValidationResult.Success;
            var result = new MyValidate().Birthday(birthday.Value, true, _);
            return result;
        }

        private ValidationResult? Birthday(DateTime birthday, bool IsCustomer = false, ValidationContext _ = null)
        {
            // Yêu cầu 16–100 tuổi tính theo UTC “ngày hiện tại”
            var today = DateTime.UtcNow.Date;

            // Không cho phép ngày sinh trong tương lai
            if (birthday.Date > today)
                return new ValidationResult("Ngày sinh không được lớn hơn ngày hiện tại");

            var ageYears = today.Year - birthday.Year;
            if (birthday.Date > today.AddYears(-ageYears)) ageYears--;

            if (IsCustomer)
            {
                if (ageYears < 14)
                    return new ValidationResult("Tuổi phải từ 14 trở lên");
                if (ageYears > 100)
                    return new ValidationResult("Tuổi không được lớn hơn 100");
            }
            else
            {
                if (ageYears < 16)
                    return new ValidationResult("Tuổi phải từ 16 trở lên");
                if (ageYears > 100)
                    return new ValidationResult("Tuổi không được lớn hơn 100");
            }


            return ValidationResult.Success;
        }
    }
}
