using System.ComponentModel.DataAnnotations;

namespace Dia_CoreAPI.DTOs.Masters
{
    #region Mines Master
    public class MinesMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter Mines ID")]
        [MaxLength(30, ErrorMessage = "Mines ID Max. 30 Character")]
        public string MinesID { get; set; }

        [Required(ErrorMessage = "Enter Mines Name")]
        [MaxLength(30, ErrorMessage = "Mines Name Max. 30 Character")]
        public string MinesName { get; set; }

        [Required(ErrorMessage = "Enter Mines Priority in Numeric")]
        public float MinesPriority { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion

    #region Rough Master
    public class RoughMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter Rough ID")]
        [MaxLength(30, ErrorMessage = "Rough ID Max. 30 Character")]
        public string RoughID { get; set; }

        [Required(ErrorMessage = "Enter Rough Name")]
        [MaxLength(30, ErrorMessage = "Rough Name Max. 30 Character")]
        public string RoughName { get; set; }

        [Required(ErrorMessage = "Enter Rough Priority in Numeric")]
        public float RoughPriority { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion
}
