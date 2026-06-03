using System.ComponentModel.DataAnnotations;

namespace Dia_CoreAPI.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Company ID Required")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Company Id should be numeric.")]
        public string CompanyId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username Required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password Required")]
        public string Password { get; set; } = string.Empty;
    }
}
