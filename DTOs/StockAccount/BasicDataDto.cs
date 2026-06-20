namespace Dia_CoreAPI.DTOs.StockAccount
{
    public class BasicDataDto
    {
        public string ID { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TextName { get; set; } = string.Empty;
    }
}
