namespace Dia_CoreAPI.DTOs.Masters
{
    public class AllMasterDto
    {
        public string Name { get; set; }
        public string Count { get; set; }
        public string URL { get; set; }
        public string Type { get; set; }
    }

    public class MasterCountDto
    {
        public int TotalMasters { get; set; }
        public int TotalCategories { get; set; }
    }

    public class PermissionCheck
    {
        public int UserTypeID { get; set; }
        public int FormID { get; set; }
    }
}