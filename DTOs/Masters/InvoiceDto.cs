using System.ComponentModel.DataAnnotations;

namespace Dia_CoreAPI.DTOs.Masters
{
    #region Term Master
    public class TermMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter Term ID")]
        [MaxLength(30, ErrorMessage = "Term ID Max. 30 Character")]
        public string TermID { get; set; }

        [Required(ErrorMessage = "Enter Term Name")]
        [MaxLength(30, ErrorMessage = "Term Name Max. 30 Character")]
        public string TermName { get; set; }

        [Required(ErrorMessage = "Enter Term Priority in Numeric")]
        public float TermPriority { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion

    #region Type Master
    public class TypeMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter Type ID")]
        [MaxLength(30, ErrorMessage = "Type ID Max. 30 Character")]
        public string TypeID { get; set; }

        [Required(ErrorMessage = "Enter Type Name")]
        [MaxLength(30, ErrorMessage = "Type Name Max. 30 Character")]
        public string TypeName { get; set; }

        [Required(ErrorMessage = "Enter Type Priority in Numeric")]
        public float TypePriority { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion

    #region SubType Master
    public class SubTypeMaster
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Enter SubType ID")]
        [MaxLength(30, ErrorMessage = "SubType ID Max. 30 Character")]
        public string SubTypeID { get; set; }

        [Required(ErrorMessage = "Enter SubType Name")]
        [MaxLength(30, ErrorMessage = "SubType Name Max. 30 Character")]
        public string SubTypeName { get; set; }

        [Required(ErrorMessage = "Enter SubType Priority in Numeric")]
        public float SubTypePriority { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion

    #region Currency Master
    public class CurrencyMaster
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public string? Major { get; set; }
        public string? Minor { get; set; }
        public string Symbol { get; set; }
        public string MasterCurrencyOperator { get; set; }
        public string StockCurrencyOperator { get; set; }
        public float MasterCurrencyRate { get; set; }
        public float StockCurrencyRate { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifyBy { get; set; }
        public string? ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
    #endregion
}
