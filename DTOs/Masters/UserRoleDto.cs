using System.ComponentModel.DataAnnotations;

namespace Dia_CoreAPI.DTOs.Masters
{
    public class UserRoleDto
    {
        public int UserTypeID { get; set; }
        
        [Required(ErrorMessage = "Enter UserType")]
        public string UserType { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Enter Description")]
        public string Description { get; set; } = string.Empty;
        
        public int UnderUser { get; set; }
        public int CompanyID { get; set; }
    }

    public class UserRoleViewDto
    {
        public int UserTypeID { get; set; }
        public string UserType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int UnderUserTypeID { get; set; }
    }
}
