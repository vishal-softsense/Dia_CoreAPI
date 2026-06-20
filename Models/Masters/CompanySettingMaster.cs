using System.ComponentModel.DataAnnotations;

namespace Dia_CoreAPI.Models.Masters
{
    public class CompanySettingMaster
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Enter Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter Value")]
        public string Value { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? COMMENT { get; set; }

        public int CompanyID { get; set; }
    }
}
