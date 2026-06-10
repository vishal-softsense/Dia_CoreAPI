namespace Dia_CoreAPI.DTOs.Masters
{
    public class UserPermissionListDto
    {
        public string FormID { get; set; } = string.Empty;
        public string FormName { get; set; } = string.Empty;
        public bool Select { get; set; }
        public bool Write { get; set; }
        public bool Edit { get; set; }
        public bool Delete { get; set; }
        public bool Navigate { get; set; }
        public bool All { get; set; }
    }

    public class UserPermissionSaveDto
    {
        public int UserTypeID { get; set; }
        public string FormID { get; set; } = string.Empty;
        public bool SelectData { get; set; }
        public bool InsertData { get; set; }
        public bool EditData { get; set; }
        public bool DeleteData { get; set; }
        public bool NavigateData { get; set; }
    }
}
