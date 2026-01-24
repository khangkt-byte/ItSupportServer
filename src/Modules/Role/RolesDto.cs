using System.ComponentModel.DataAnnotations;

namespace ItSupportServer.src.Modules.Role
{
    public class CreateRoleDto
    {
        [Required(ErrorMessage = "Tên vai trò là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Tên không được quá 200 ký tự")]
        [RegularExpression(@"^[\p{L}\p{M}\p{N} _-]+$",
        ErrorMessage = "Tên chỉ được chứa chữ cái có dấu, số, khoảng trắng, gạch ngang (-) và gạch dưới (_)")]
        public string Name { get; set; }

        public List<int>? ClaimIds { get; set; } = new List<int>();
    }

    public class UpdateRoleDto
    {
        public int RoleId { get; set; }
        [Required(ErrorMessage = "Tên vai trò là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Tên không được quá 200 ký tự")]
        [RegularExpression(@"^[\p{L}\p{M}\p{N} _-]+$",
        ErrorMessage = "Tên chỉ được chứa chữ cái có dấu, số, khoảng trắng, gạch ngang (-) và gạch dưới (_)")]
        public string Name { get; set; }

        public List<int>? ClaimIds { get; set; } = new List<int>();
    }

    public class RolesDto
    {
        public int RoleId { get; set; }
        public string Name { get; set; }

        public List<ClaimDto>? Claims { get; set; } = new List<ClaimDto>();
    }

    public class ClaimDto
    {
        public int ClaimId { get; set; }
        public string Claim { get; set; }
        public string? Category { get; set; }
    }

    public class AccountRoleDto
    {
        [Required(ErrorMessage = "Mã tài khoản là bắt buộc")]
        public required Guid AccountId { get; set; }
        [Required(ErrorMessage = "Mã vai trò là bắt buộc")]
        public List<int> RoleId { get; set; } = new List<int>();
    }

    public class AccountRoleResponseDto
    {
        public Guid AccountId { get; set; }
        public string? Username { get; set; }
        public List<int> RoleId { get; set; } = new List<int>();
    }
}
