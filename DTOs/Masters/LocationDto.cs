using System.ComponentModel.DataAnnotations;

namespace Dia_CoreAPI.DTOs.Masters
{

    #region Area Master
    public class AreaMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter Area ID")]
        [MaxLength(30, ErrorMessage = "Area ID Max. 30 Character")]
        public string AreaID { get; set; }

        [Required(ErrorMessage = "Enter Area Name in Alphabets")]
        [MaxLength(30, ErrorMessage = "Area Name Max. 30 Character")]
        public string AreaName { get; set; }

        [Required(ErrorMessage = "Enter Area Priority in Numeric")]
        public float AreaPriority { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion

    #region City Master
    public class CityMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter City ID")]
        [MaxLength(30, ErrorMessage = "City ID Max. 30 Character")]
        public string CityID { get; set; }

        [Required(ErrorMessage = "Enter City Name in Alphabets")]
        [MaxLength(30, ErrorMessage = "City Name Max. 30 Character")]
        public string CityName { get; set; }

        [Required(ErrorMessage = "Enter City Priority in Numeric")]
        public float CityPriority { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion

    #region State Master
    public class StateMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter State ID")]
        [MaxLength(30, ErrorMessage = "State ID Max. 30 Character")]
        public string StateID { get; set; }

        [Required(ErrorMessage = "Enter State Name in Alphabets")]
        [MaxLength(30, ErrorMessage = "State Name Max. 30 Character")]
        public string StateName { get; set; }

        [Required(ErrorMessage = "Enter State Priority in Numeric")]
        public float StatePriority { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion

    #region Country Master
    public class CountryMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter Country ID")]
        [MaxLength(30, ErrorMessage = "Country ID Max. 30 Character")]
        public string CountryID { get; set; }

        [Required(ErrorMessage = "Enter Country Name in Alphabets")]
        [MaxLength(30, ErrorMessage = "Country Name Max. 30 Character")]
        public string CountryName { get; set; }

        [Required(ErrorMessage = "Enter Country Priority in Numeric")]
        public float CountryPriority { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion
}
