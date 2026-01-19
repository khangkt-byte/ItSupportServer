using System.ComponentModel.DataAnnotations;

namespace ITSupportServer.AreasController
{
    public class AreasDto
    {
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
    }

    public class AreasUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
