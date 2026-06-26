using System.ComponentModel.DataAnnotations;

namespace Dia_CoreAPI.DTOs.Masters
{
    #region Brown Master
    public class BrownMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter Brown ID")]
        [MaxLength(30, ErrorMessage = "Brown ID Max. 30 Character")]
        public string BrownID { get; set; }

        [Required(ErrorMessage = "Enter Brown Name")]
        [MaxLength(30, ErrorMessage = "Brown Name Max. 30 Character")]
        public string BrownName { get; set; }

        [Required(ErrorMessage = "Enter Brown Priority in Numeric")]
        public float BrownPriority { get; set; }

        public string? BrownGroupName { get; set; }
        public float BrownGroupPriority { get; set; }
        public string? PricingBrownID { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion

    #region Green Master
    public class GreenMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter Green ID")]
        [MaxLength(30, ErrorMessage = "Green ID Max. 30 Character")]
        public string GreenID { get; set; }

        [Required(ErrorMessage = "Enter Green Name")]
        [MaxLength(30, ErrorMessage = "Green Name Max. 30 Character")]
        public string GreenName { get; set; }

        [Required(ErrorMessage = "Enter Green Priority in Numeric")]
        public float GreenPriority { get; set; }

        public string? GreenGroupName { get; set; }
        public float GreenGroupPriority { get; set; }
        public string? PricingGreenID { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion

    #region Milky Master
    public class MilkyMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter Milky ID")]
        [MaxLength(30, ErrorMessage = "Milky ID Max. 30 Character")]
        public string MilkyID { get; set; }

        [Required(ErrorMessage = "Enter Milky Name")]
        [MaxLength(30, ErrorMessage = "Milky Name Max. 30 Character")]
        public string MilkyName { get; set; }

        [Required(ErrorMessage = "Enter Milky Priority in Numeric")]
        public float MilkyPriority { get; set; }

        public string? MilkyGroupName { get; set; }
        public float MilkyGroupPriority { get; set; }
        public string? PricingMilkyID { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion

    #region Luster Master
    public class LusterMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter Luster ID")]
        [MaxLength(30, ErrorMessage = "Luster ID Max. 30 Character")]
        public string LusterID { get; set; }

        [Required(ErrorMessage = "Enter Luster Name")]
        [MaxLength(30, ErrorMessage = "Luster Name Max. 30 Character")]
        public string LusterName { get; set; }

        [Required(ErrorMessage = "Enter Luster Priority in Numeric")]
        public float LusterPriority { get; set; }

        public string? LusterGroupName { get; set; }
        public float LusterGroupPriority { get; set; }
        public string? PricingLusterID { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion

    #region Natts Master
    public class NattsMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter Natts ID")]
        [MaxLength(30, ErrorMessage = "Natts ID Max. 30 Character")]
        public string NattsID { get; set; }

        [Required(ErrorMessage = "Enter Natts Name")]
        [MaxLength(30, ErrorMessage = "Natts Name Max. 30 Character")]
        public string NattsName { get; set; }

        [Required(ErrorMessage = "Enter Natts Priority in Numeric")]
        public float NattsPriority { get; set; }

        public string? NattsGroupName { get; set; }
        public float NattsGroupPriority { get; set; }
        public string? PricingNattsID { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion

    #region HA Master
    public class HAMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter HA ID")]
        [MaxLength(30, ErrorMessage = "HA ID Max. 30 Character")]
        public string HAID { get; set; }

        [Required(ErrorMessage = "Enter HA Name")]
        [MaxLength(30, ErrorMessage = "HA Name Max. 30 Character")]
        public string HAName { get; set; }

        [Required(ErrorMessage = "Enter HA Priority in Numeric")]
        public float HAPriority { get; set; }

        public string? HAGroupName { get; set; }
        public float HAGroupPriority { get; set; }
        public string? PricingHAID { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion

    #region Shade Master
    public class ShadeMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter Shade ID")]
        [MaxLength(30, ErrorMessage = "Shade ID Max. 30 Character")]
        public string ShadeID { get; set; }

        [Required(ErrorMessage = "Enter Shade Name")]
        [MaxLength(30, ErrorMessage = "Shade Name Max. 30 Character")]
        public string ShadeName { get; set; }

        [Required(ErrorMessage = "Enter Shade Priority in Numeric")]
        public float ShadePriority { get; set; }

        public string? ShadeGroupName { get; set; }
        public float ShadeGroupPriority { get; set; }
        public string? PricingShadeID { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion
}
