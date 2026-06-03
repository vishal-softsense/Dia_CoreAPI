namespace Dia_CoreAPI.Models
{
    public class LoginResponseModel
    {
        public int UserID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int UserTypeId { get; set; }
        public int CompanyID { get; set; }
        public string ProfileImage { get; set; } = string.Empty;
    }
}