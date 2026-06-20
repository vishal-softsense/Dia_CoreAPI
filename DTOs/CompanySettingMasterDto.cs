using System.ComponentModel.DataAnnotations;

namespace Dia_CoreAPI.DTOs
{
    public class CompanySettingMasterDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Enter Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Enter Value")]
        public string Value { get; set; }

        public string? Description { get; set; }

        public string? COMMENT { get; set; }

        public int CompanyId { get; set; }        
    }
}
