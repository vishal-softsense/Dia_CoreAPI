using System.ComponentModel.DataAnnotations;

namespace Dia_CoreAPI.DTOs.Masters
{
    #region Jewellery Category Master
    public class JewCategoryMaster
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
        public float CategoryPriority { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion

    #region Jewellery SubCategory Master
    public class JewSubCategoryMaster
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
        public float SubCategoryPriority { get; set; }

        public int CategoryID { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion

    #region Jewellery Metal Master
    public class JewMetalMaster
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
        public float MetalPriority { get; set; }

        public string? MetalType { get; set; }
        public string? MetalColor { get; set; }
        public string? MetalPurity { get; set; }
        public float Metal24KTRatio { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion

    #region Jewellery Metal Purity Master
    public class JewMetalPurityMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter MetalPurity ID")]
        [MaxLength(30, ErrorMessage = "MetalPurity ID Max. 30 Character")]
        public string MetalPurityID { get; set; }

        [Required(ErrorMessage = "Enter MetalPurity Name")]
        [MaxLength(30, ErrorMessage = "MetalPurity Name Max. 30 Character")]
        public string MetalPurityName { get; set; }

        [Required(ErrorMessage = "Enter MetalPurity Priority in Numeric")]
        public float MetalPurityPriority { get; set; }

        [Required(ErrorMessage = "Enter MetalPurityToPureRatio in Numeric")]
        public double MetalPurityToPureRatio { get; set; }

        public string? Metal { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion

    #region Jewellery Color Master
    public class JewColorMaster
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
        public float JewColorPriority { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion

    #region Jewellery Design Master
    public class JewDesignMaster
    {
        public int ID { get; set; }
        public string? DesignID { get; set; }
        public string? DesignName { get; set; }
        public float DesignPriority { get; set; }
    }
    #endregion

    #region Helper DTOs
    public class GetJewPurity
    {
        public string MetalPurityID { get; set; }
    }
    #endregion
}
