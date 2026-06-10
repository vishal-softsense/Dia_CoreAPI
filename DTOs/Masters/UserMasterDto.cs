using System;
using System.ComponentModel.DataAnnotations;

namespace Dia_CoreAPI.DTOs.Masters
{
    public class UserMasterDto
    {
        public int UserID { get; set; }
        
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; } = string.Empty;

        // Password might be empty during an edit if they don't want to change it
        public string Password { get; set; } = string.Empty;
        
        public string UserPassword { get; set; } = string.Empty;
        
        public string ConfirmPassword { get; set; } = string.Empty;
        
        public string UserConfirmPassword { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter a valid professional email address")]
        public string Email { get; set; } = string.Empty;
        
        public string FirstName { get; set; } = string.Empty;
        
        public string LastName { get; set; } = string.Empty;
        
        public string MobileNo { get; set; } = string.Empty;
        
        public string BirthDate { get; set; } = string.Empty;
        
        public string Birthday { get; set; } = string.Empty;
        
        public string Address1 { get; set; } = string.Empty;
        
        public string Address { get; set; } = string.Empty;
        
        public string Landmark { get; set; } = string.Empty;
        
        public string City { get; set; } = string.Empty;
        
        public string State { get; set; } = string.Empty;
        
        public string Country { get; set; } = string.Empty;
        
        public string Twitter { get; set; } = string.Empty;
        
        public string Facebook { get; set; } = string.Empty;
        
        public string Instagram { get; set; } = string.Empty;
        
        public string Linkedin { get; set; } = string.Empty;
        
        public int UserTypeId { get; set; }
        
        public int CompanyID { get; set; }
    }

    public class UserListDto
    {
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public DateTime? LastLogin { get; set; }
        public string AccountStatus { get; set; } = string.Empty;
    }
}
