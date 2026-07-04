using System.ComponentModel.DataAnnotations;

namespace Dia_CoreAPI.DTOs.StockAccount
{
    public class JewStockMasterDto
    {
        [Key]
        public int JewCode { get; set; }
        public string Stock_ID { get; set; }
        public string DesignID { get; set; }


        public string DesignName { get; set; }


        public float DesignPriority { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string JewSize { get; set; }
        public string MetalType { get; set; }
        public string JewelleryCerNo { get; set; }
        public string Color { get; set; }
        public string Purity { get; set; }
        public double PureRatio { get; set; }
        public double GrossWeight { get; set; }
        public double NetWeight { get; set; }
        public double MetalPriceGm { get; set; }
        public string MakingType { get; set; }
        public float MakingRate { get; set; }
        public float MakingCharges { get; set; }
        public float SettingCharges { get; set; }
        public float HandlingChargesPerCarat { get; set; }
        public float HandlingCarat { get; set; }
        public float TotalHandlingCharges { get; set; }
        public string JewelleryCertificate { get; set; }
        public float TotalCertificateCharges { get; set; }
        public float OtherCharges { get; set; }
        public float HallmarkCharges { get; set; }
        public float TotalMetalAmount { get; set; }
        public float TotalDiamondAmount { get; set; }
        public float TotalDiamondCarat { get; set; }
        public float TotalStoneCarat { get; set; }
        public float TotalStoneAmount { get; set; }
        public float TotalMakingCharges { get; set; }

        public string ImagePath { get; set; }
        public string VideoPath { get; set; }
        public string CertPath { get; set; }
        public float FinalAmount { get; set; }

        public string CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public string ModifyBy { get; set; }
        public string ModifyOn { get; set; }
        public int CompanyID { get; set; }

        public float AskingRate { get; set; }
        public double Pcs { get; set; }

        public int TotalDiamondQty { get; set; }
        public int TotalStoneQty { get; set; }
        public string Description { get; set; }
        public byte[] ImageBinary { get; set; }

        public string TrnDate { get; set; }
        public string UseMounting { get; set; }
        public string UseMountingId { get; set; }
        public string MountCategory { get; set; }

    }

    public class JewMetalMasterDto
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter Metal ID")]
        [MaxLength(30, ErrorMessage = "Metal ID Max. 30 Character")]
        public string MetalID { get; set; }

        [Required(ErrorMessage = "Enter Metal Name")]
        [MaxLength(30, ErrorMessage = "Metal Name Max. 30 Character")]
        public string MetalName { get; set; }

        [Required(ErrorMessage = "Enter Metal Priority in Numeric")]
        // [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Shape Priority Only number")]
        public float MetalPriority { get; set; }
        public string MetalType { get; set; }

        public string MetalColor { get; set; }
        public string MetalPurity { get; set; }
        public float Metal24KTRatio { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public string ModifyBy { get; set; }
        public string ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }

    public class JewColorMasterDto
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter JewColor ID")]
        [MaxLength(30, ErrorMessage = "JewColor ID Max. 30 Character")]
        public string JewColorID { get; set; }

        [Required(ErrorMessage = "Enter JewColor Name")]
        [MaxLength(30, ErrorMessage = "Design JewColor Max. 30 Character")]
        public string JewColorName { get; set; }

        [Required(ErrorMessage = "Enter JewColor Priority in Numeric")]
        // [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Shape Priority Only number")]
        public float JewColorPriority { get; set; }

        public string CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public string ModifyBy { get; set; }
        public string ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }

    public class GetJewPurityDto
    {
        public string MetalPurityID { get; set; }
    }

    public class MetalPurityRatioDto
    {
        public double Ratio { get; set; }
    }
    public class JewCategoryDto
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter Category ID")]
        [MaxLength(30, ErrorMessage = "Category ID Max. 30 Character")]
        public string CategoryID { get; set; }

        [Required(ErrorMessage = "Enter Category Name")]
        [MaxLength(30, ErrorMessage = "Category Name Max. 30 Character")]
        public string CategoryName { get; set; }

        [Required(ErrorMessage = "Enter Category Priority in Numeric")]
        // [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Shape Priority Only number")]
        public float CategoryPriority { get; set; }

        public string CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public string ModifyBy { get; set; }
        public string ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }

    public class JewSubCategoryDto
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter SubCategory ID")]
        [MaxLength(30, ErrorMessage = "SubCategory ID Max. 30 Character")]
        public string SubCategoryID { get; set; }

        [Required(ErrorMessage = "Enter SubCategory Name")]
        [MaxLength(30, ErrorMessage = "SubCategory Name Max. 30 Character")]
        public string SubCategoryName { get; set; }

        [Required(ErrorMessage = "Enter SubCategory Priority in Numeric")]
        // [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Shape Priority Only number")]
        public float SubCategoryPriority { get; set; }
        public int CategoryID { get; set; }

        public string CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public string ModifyBy { get; set; }
        public string ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }

    public class AddMountingStockDto
    {
        public List<BasicDataDto> Shape { get; set; }
        public List<BasicDataDto> Color { get; set; }
        public List<BasicDataDto> Size { get; set; }
        public List<BasicDataDto> Clarity { get; set; }
        public List<BasicDataDto> Sieve { get; set; }
        public List<BasicDataDto> Category { get; set; }

        public List<JewCategoryDto> JewCategories { get; set; }

        public List<JewSubCategoryDto> JewSubCategories { get; set; }

        public List<JewMetalMasterDto> JewMetalType { get; set; }

        public List<JewColorMasterDto> JewColor { get; set; }

        public List<string> JewSettingValue { get; set; }

        public string DecimalinInvoiceAmount { get; set; }
    }

    public class SettingMasterDto
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
