using System.ComponentModel.DataAnnotations;

namespace Dia_CoreAPI.DTOs
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Company ID Required")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Company Id shound be numeric.")]
        public string CompanyId { get; set; }

        [Required(ErrorMessage = "Username Required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password Required")]
        public string Password { get; set; }
    }
}
