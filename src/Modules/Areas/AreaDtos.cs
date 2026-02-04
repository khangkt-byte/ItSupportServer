using System.ComponentModel.DataAnnotations;

namespace ItSupportServer.src.Modules.Areas
{
    public class AreaCreateDto
    {
        [Required(ErrorMessage = "Tên là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Tên không được quá 200 ký tự")]
        [RegularExpression(@"^[\p{L}\p{M}\p{N} _-]+$",
        ErrorMessage = "Tên chỉ được chứa chữ cái có dấu, số, khoảng trắng, gạch ngang (-) và gạch dưới (_)")]
        public string Name { get; set; }
        public string? Description { get; set; }
    }

    public class AreaUpdateDto
    {
        [RegularExpression(@"^[\p{L}\p{M}\p{N} _-]+$",
        ErrorMessage = "Tên chỉ được chứa chữ cái có dấu, số, khoảng trắng, gạch ngang (-) và gạch dưới (_)")]
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
