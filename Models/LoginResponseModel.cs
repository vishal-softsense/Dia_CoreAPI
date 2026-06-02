namespace Dia_CoreAPI.Models
{
    public class LoginResponseModel
    {
        public int UserID { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public int UserTypeId { get; set; }
        public int CompanyID { get; set; }
        public string ProfileImage { get; set; }
    }
}
