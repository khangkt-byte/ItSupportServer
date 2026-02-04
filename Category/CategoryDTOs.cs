using FastFood.MenuItem;
using System.ComponentModel.DataAnnotations;

namespace FastFood.Category
{
    public class CategoryDTO
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Tên là bắt buộc")]
        [MaxLength(50, ErrorMessage = "Tên không được quá 50 ký tự")]
        [RegularExpression(@"^[\p{L}\p{M}\p{N} _-]+$",
        ErrorMessage = "Tên chỉ được chứa chữ cái có dấu, số, khoảng trắng, gạch ngang (-) và gạch dưới (_)")]
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? UrlImage { get; set; }
        public bool IsActive { get; set; }
        public List<MenuItemDTO>? MenuItems { get; set; }
    }

    public class CreateCategoryDTO
    {
        [Required(ErrorMessage = "Tên là bắt buộc")]
        [MaxLength(50, ErrorMessage = "Tên không được quá 50 ký tự")]
        [RegularExpression(@"^[\p{L}\p{M}\p{N} _-]+$",
        ErrorMessage = "Tên chỉ được chứa chữ cái có dấu, số, khoảng trắng, gạch ngang (-) và gạch dưới (_)")]
        public string Name { get; set; }
        public string? Description { get; set; }
        public IFormFile? Image { get; set; }
    }

    public class UpdateCategoryDTO
    {
        [Required(ErrorMessage = "Tên là bắt buộc")]
        [MaxLength(50, ErrorMessage = "Tên không được quá 50 ký tự")]
        [RegularExpression(@"^[\p{L}\p{M}\p{N} _-]+$",
        ErrorMessage = "Tên chỉ được chứa chữ cái có dấu, số, khoảng trắng, gạch ngang (-) và gạch dưới (_)")]
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? UrlImage { get; set; }
        public IFormFile? NewImage { get; set; }
    }
}
