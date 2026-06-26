using System.ComponentModel.DataAnnotations;

namespace Dia_CoreAPI.DTOs.Masters
{

    public class BasicData
    {
        public string ID { get; set; }

        public string Type { get; set; }
        public int Priority { get; set; }
        public string Name { get; set; }
        public string TextName { get; set; }
        public object CityName { get; internal set; }
    }

    #region ShapeMaster
    public class ShapeMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter Shape ID")]
        [MaxLength(30, ErrorMessage = "Shape ID Max. 30 Character")]
        public string ShapeID { get; set; }

        [Required(ErrorMessage = "Enter Shape Name")]
        [MaxLength(30, ErrorMessage = "Shape Name Max. 30 Character")]
        public string ShapeName { get; set; }

        [Required(ErrorMessage = "Enter Shape Priority in Numeric")]
        // [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Shape Priority Only number")]
        public float ShapePriority { get; set; }

        public string? ShapeGroupName { get; set; }
        public float ShapeGroupPriority { get; set; }
        public string? PricingShapeID { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion

    #region Color Master
    public class ColorMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter Color ID")]
        [MaxLength(30, ErrorMessage = "Color ID Max. 30 Character")]
        public string ColorID { get; set; }

        [Required(ErrorMessage = "Enter Color Name")]
        [MaxLength(30, ErrorMessage = "Color Name Max. 30 Character")]
        public string ColorName { get; set; }

        [Required(ErrorMessage = "Enter Color Priority in Numeric")]
        // [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Color Priority Only number")]
        public float ColorPriority { get; set; }

        public string? ColorGroupName { get; set; }
        public float ColorGroupPriority { get; set; }
        public string? PricingColorID { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion

    #region Size Master 
    public class SizeMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter Size ID")]
        [MaxLength(30, ErrorMessage = "Size ID Max. 30 Character")]
        public string? SizeID { get; set; }

        [Required(ErrorMessage = "Enter Size Name")]
        [MaxLength(30, ErrorMessage = "Size Name Max. 30 Character")]
        public string? SizeName { get; set; }

        [Required(ErrorMessage = "Enter Size Priority in Numeric")]
        //[RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "SizePriority Numbers and Decimal Numbers  allowed")]        
        public float SizePriority { get; set; }
        [Required(ErrorMessage = "Enter FCarat in Numeric")]
        public string? FCarat { get; set; }
        [Required(ErrorMessage = "Enter TCarat in Numeric")]
        public string? TCarat { get; set; }
        [Required(ErrorMessage = "Enter SizeGroup Name")]
        public string? SizeGroupName { get; set; }

        [Required(ErrorMessage = "Enter SizeGroup Priority in Numeric")]
        //[RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "SizeGroupPriority Numbers and Decimal Numbers allowed")]
        public float? SizeGroupPriority { get; set; }
        //public string PricingSizeID { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion 

    #region Clarity Master
    public class ClarityMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter Clarity ID")]
        [MaxLength(30, ErrorMessage = "Clarity ID Max. 30 Character")]
        public string ClarityID { get; set; }

        [Required(ErrorMessage = "Enter Clarity Name")]
        [MaxLength(30, ErrorMessage = "Clarity Name Max. 30 Character")]
        public string ClarityName { get; set; }

        [Required(ErrorMessage = "Enter Clarity Priority in Numeric")]
        public float ClarityPriority { get; set; }

        public string? ClarityGroupName { get; set; }
        public float ClarityGroupPriority { get; set; }
        public string? PricingClarityID { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion 

    #region Sieve Master
    public class SieveMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter Sieve ID")]
        [MaxLength(30, ErrorMessage = "Sieve ID Max. 30 Character")]
        public string SieveID { get; set; }

        [Required(ErrorMessage = "Enter Sieve Name")]
        [MaxLength(30, ErrorMessage = "Sieve Name Max. 30 Character")]
        public string SieveName { get; set; }

        [Required(ErrorMessage = "Enter Sieve Priority in Numeric")]
        public float SievePriority { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion 

    #region LAB Master
    public class LABMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter LAB ID")]
        [MaxLength(30, ErrorMessage = "LAB ID Max. 30 Character")]
        public string CertID { get; set; }

        [Required(ErrorMessage = "Enter LAB Name")]
        [MaxLength(30, ErrorMessage = "LAB Name Max. 30 Character")]
        public string CertName { get; set; }

        [Required(ErrorMessage = "Enter LAB Priority in Numeric")]
        public float CertPriority { get; set; }

        public string? CertGroupName { get; set; }
        public float CertGroupPriority { get; set; }
        public string? PricingCertID { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion 

    #region Prop Master
    public class PropMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter Prop ID")]
        [MaxLength(30, ErrorMessage = "Prop ID Max. 30 Character")]
        public string PropID { get; set; }

        [Required(ErrorMessage = "Enter Prop Name")]
        [MaxLength(30, ErrorMessage = "Prop Name Max. 30 Character")]
        public string PropName { get; set; }

        [Required(ErrorMessage = "Enter Prop Priority in Numeric")]
        public float PropPriority { get; set; }

        public string? PropGroupName { get; set; }
        public float PropGroupPriority { get; set; }
        public string? PricingPropID { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion 

    #region Polish Master
    public class PolishMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter Polish ID")]
        [MaxLength(30, ErrorMessage = "Polish ID Max. 30 Character")]
        public string PolishID { get; set; }

        [Required(ErrorMessage = "Enter Polish Name")]
        [MaxLength(30, ErrorMessage = "Polish Name Max. 30 Character")]
        public string PolishName { get; set; }

        [Required(ErrorMessage = "Enter Polish Priority in Numeric")]
        public float PolishPriority { get; set; }

        public string? PolishGroupName { get; set; }
        public float PolishGroupPriority { get; set; }
        public string? PricingPolishID { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion 

    #region Sym Master
    public class SymMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter Sym ID")]
        [MaxLength(30, ErrorMessage = "Sym ID Max. 30 Character")]
        public string SymID { get; set; }

        [Required(ErrorMessage = "Enter Sym Name")]
        [MaxLength(30, ErrorMessage = "Sym Name Max. 30 Character")]
        public string SymName { get; set; }

        [Required(ErrorMessage = "Enter Sym Priority in Numeric")]
        public float SymPriority { get; set; }

        public string? SymGroupName { get; set; }
        public float SymGroupPriority { get; set; }
        public string? PricingSymID { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion 

    #region FL Master
    public class FLMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter FL ID")]
        [MaxLength(30, ErrorMessage = "FL ID Max. 30 Character")]
        public string FLID { get; set; }

        [Required(ErrorMessage = "Enter FL Name")]
        [MaxLength(30, ErrorMessage = "FL Name Max. 30 Character")]
        public string FLName { get; set; }

        [Required(ErrorMessage = "Enter FL Priority in Numeric")]
        public float FLPriority { get; set; }

        public string? FLGroupName { get; set; }
        public float FLGroupPriority { get; set; }
        public string? PricingFLID { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion 

    #region FLColor Master
    public class FLColorMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter FLColor ID")]
        [MaxLength(30, ErrorMessage = "FLColor ID Max. 30 Character")]
        public string FLColorID { get; set; }

        [Required(ErrorMessage = "Enter FLColor Name")]
        [MaxLength(30, ErrorMessage = "FLColor Name Max. 30 Character")]
        public string FLColorName { get; set; }

        [Required(ErrorMessage = "Enter FLColor Priority in Numeric")]
        public float FLColorPriority { get; set; }

        public string? FLColorGroupName { get; set; }
        public float FLColorGroupPriority { get; set; }
        public string? PricingFLColorID { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion 
}
