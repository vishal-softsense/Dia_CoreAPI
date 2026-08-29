using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction
{
    using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;

    public class MixConsignmentReturnPageDataDto
    {
        public List<BasicDataDto> Type { get; set; } = new();
    }

    public class MixConsignmentReturnFilterRequest
    {
        public string? InvNo { get; set; }
        public string? BillNo { get; set; }
        public string? Type { get; set; }
        public string? Party { get; set; }
        public string? LotNo { get; set; }
        public bool ClearMemo { get; set; }
    }

    public class MixConsignmentReturnRowDto
    {
        public string? Date { get; set; }
        public string? MemoNo { get; set; }
        public string? Party { get; set; }
        public string? Broker { get; set; }
        public string? Item { get; set; }
        public float carats { get; set; }
        public float Pcs { get; set; }
        public float Price { get; set; }
        public float Sale { get; set; }
        [JsonPropertyName("return")]
        public float Return { get; set; }
        public float Balance { get; set; }
        [JsonPropertyName("spCode")]
        public float SPCode { get; set; }
        [JsonPropertyName("sptCode")]
        public float SPTCode { get; set; }
        [JsonPropertyName("itemCode")]
        public float ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? MNo_BillNo { get; set; }
        public float CurCode { get; set; }
        public string? StockType { get; set; }
    }

    /// <summary>Raw SP row — includes Jew fields used for StockType mapping (MVC controller).</summary>
    public class MixConsignmentReturnRawRow
    {
        public string? Date { get; set; }
        public string? MemoNo { get; set; }
        public string? Party { get; set; }
        public string? Broker { get; set; }
        public string? Item { get; set; }
        public float carats { get; set; }
        public float Pcs { get; set; }
        public float Price { get; set; }
        public float Sale { get; set; }
        public float Return { get; set; }
        public float Balance { get; set; }
        public float SPCode { get; set; }
        public float SPTCode { get; set; }
        public float ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? MNo_BillNo { get; set; }
        public float CurCode { get; set; }
        public string? StockType { get; set; }
        public string? JewItem { get; set; }
        public string? JewItemName { get; set; }
    }

    public class MixConsignmentReturnItemRequest
    {
        public bool Chk { get; set; }
        public float Sale { get; set; }
        [JsonPropertyName("return")]
        public float Return { get; set; }
        public float NReturn { get; set; }
        public float Balance { get; set; }
        public float Price { get; set; }
        [JsonPropertyName("spCode")]
        public float SPCode { get; set; }
        [JsonPropertyName("sptCode")]
        public float SPTCode { get; set; }
        public string? LotNo { get; set; }
        public string? ItemName { get; set; }
        public float CurCode { get; set; }
        public string? StockType { get; set; }
    }

    public class MixConsignmentReturnSaveRequest
    {
        public DateTime RtnDate { get; set; }
        public List<MixConsignmentReturnItemRequest> Items { get; set; } = new();
    }
}
