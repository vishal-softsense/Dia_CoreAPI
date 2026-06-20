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

        public string ShapeGroupName { get; set; }
        public float ShapeGroupPriority { get; set; }
        public string PricingShapeID { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public string ModifyBy { get; set; }
        public string ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }

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

        public string ColorGroupName { get; set; }
        public float ColorGroupPriority { get; set; }
        public string PricingColorID { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public string ModifyBy { get; set; }
        public string ModifyOn { get; set; }
        public int CompanyID { get; set; }
    }
}
