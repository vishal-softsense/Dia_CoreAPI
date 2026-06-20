using Dia_CoreAPI.DTOs.Masters;
using System.ComponentModel.DataAnnotations;

namespace Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction
{
    public class InvList
    {
        public long code { get; set; }
        public DateTime Date { get; set; }
        public string InvNo { get; set; }
        //public string Invno { get; set; }
        public string BillNo { get; set; }
        public string Inv { get; set; }
        public string Party { get; set; }
        public string InvType { get; set; }
        public string Subtype { get; set; }
        public string Category { get; set; }
        public float Carats { get; set; }
        public float Amount { get; set; }
        public string Symbol { get; set; }
        public float MasterCurrencyAmount { get; set; }
        public float StockCurrencyAmount { get; set; }
        public string Status { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime RoundDate { get; set; }
        public string per { get; set; }
        public string PaymentStatus { get; set; }
        public string MemoInStatus { get; set; }

    }
    public class PurchaseParcelPageData
    {
        public List<BasicData> Shape { get; set; }
        public List<BasicData> Color { get; set; }
        public List<BasicData> Size { get; set; }
        public List<BasicData> Clarity { get; set; }
        public List<BasicData> Sieve { get; set; }
        public List<BasicData> ProductTypeList { get; set; }
        public List<BasicData> ItemGroup { get; set; }
        public List<BasicData> Type { get; set; }
        public List<BasicData> SubType { get; set; }
        public List<BasicData> Category { get; set; }
        public decimal BaseRate { get; set; }
        public decimal SBaseRate { get; set; }
        public string DecimalinInvoiceAmount { get; set; }
        public List<BasicData> CurrencyList { get; set; }
        public List<BasicData> BrokerList { get; set; }
    }
    public class AddSingleLotParcelRequest
    {
        public AddSingleLotParcel Item { get; set; }
        public List<ImportExcelParcel> ParcelList { get; set; }
    }
    public class AddSingleLotParcel
    {
        public string LotNo { get; set; }
        public string ItemName { get; set; }
        public string ExportNo { get; set; }
        public float Carat { get; set; }
        public int Pcs { get; set; }
        public float MRP { get; set; }
        public float Rate { get; set; }
        public float Back { get; set; }
        public float Less1 { get; set; }
        public float Less2 { get; set; }
        public float Less3 { get; set; }
        public float Amount { get; set; }
        public float SBaseRate { get; set; }
        public float CurCode { get; set; }
        public float MemoTransCode { get; set; }
        public float MemoTranCode { get; set; }
        public float RtnCarats { get; set; }
        public float IssueCarats { get; set; }
        public float Reje { get; set; }
        public float SelecCarat { get; set; }
        public float RejeP { get; set; }
        public float PurchaseBack { get; set; }
        public float Weight { get; set; }
        public float SRate { get; set; }
        public string ShapeId { get; set; }
        public string ColorId { get; set; }
        public string SizeId { get; set; }
        public string SieveId { get; set; }
        public string ClarityId { get; set; }
        public float ReturnCarats { get; set; }
        public float SpCarats { get; set; }
        public string SuppLotNo { get; set; }
        public string StockType { get; set; }
        public int editIndex { get; set; } = -1;        
    }
    public class ImportExcelParcel
    {
        public int SrNo { get; set; }
        public string LotNo { get; set; }
        public string ItemName { get; set; }
        public string MfgSeriesID { get; set; }
        public string MfgPacketID { get; set; }
        public string PShape { get; set; }
        public string PSize { get; set; }
        public string PSieve { get; set; }
        public string PColor { get; set; }
        public string PClarity { get; set; }
        public int Pcs { get; set; }
        public float MRP { get; set; }
        public float PurchaseBack { get; set; }
        public float PurchaseRate { get; set; }
        public float SRate { get; set; }        
        public float PurAmount { get; set; }
        public string Cut { get; set; }
        public float Weight { get; set; }
        public string Polish { get; set; }
        public string Symmetry { get; set; }
        public string FL { get; set; }
        public string SuppLotNo { get; set; }
        public float AskingRate { get; set; }
        public float ActualRate { get; set; }
        public string RMK { get; set; }
        public double Less1 { get; set; }
        public double Less2 { get; set; }
        public double Less3 { get; set; }
        public string GroupId { get; set; }
        public string StoneType { get; set; }
        public string StockType { get; set; }        
        public float IssueCarats { get; set; }
        public float Rejection { get; set; }
        public float ReturnCarats { get; set; }
        public float SpCarats { get; set; }
        public float SelecCarat { get; set; }
        public float MemoTransCode { get; set; }
        public float MemoTranCode { get; set; }        
        public bool Chk
        { get; set; }
        public string Status { get; set; }
        public string ExportNo
        { get; set; }
    }
    public class PurchaseParcelGridView
    {
        public double TotalCarats { get; set; }
        public double TotalMRPAmount { get; set; }
        public double TotalAmountWithoutless { get; set; }
        public double TotalAmount { get; set; }
        public string Alert { get; set; }
        public List<ImportExcelParcel> ExcelData { get; set; }
        public bool IsEdit { get; set; }
    }

    public class LotString
    {
        public string LotNo { get; set; }
        public float SBaserate { get; set; }
        public float CurCode { get; set; }
    }
    public class LotNoDataPPur
    {
        public string LotNo { get; set; }
        public string ItemName { get; set; }
        public string Carats { get; set; }
        public string Pcs { get; set; }
        public string PurchaseMRP { get; set; }
        public string PurchaseBack { get; set; }
        public string PurchaseRate { get; set; }
        public string PurchaseAmount { get; set; }
        public string ShapeId { get; set; }
        public string ColorId { get; set; }
        public string SizeId { get; set; }
        public string SieveId { get; set; }
        public string ClarityId { get; set; }
        public string StockType { get; set; }
        public string Amount { get; set; }

    }

    public class PurchaseParcelRequest
    {
        public SalePurs SalePur { get; set; }

        public List<ImportExcelpurAdd> Items { get; set; }
    }
    public class SalePurs
    {
        [Key]
        public int Code { get; set; }
        //[DataType(DataType.DateTime), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime TrnDate { get; set; }
        public string Mode { get; set; }
        public bool chkGrading { get; set; }
        public bool chkoldsale { get; set; }
        public bool chkoldpur { get; set; }
        public short DueDays { get; set; }
        public DateTime DueDate { get; set; }
        public string InvNo { get; set; }
        public string BillNo { get; set; }
        public string SalePurCode { get; set; }
        public string InvType { get; set; }
        public string SubType { get; set; }
        public int BankID { get; set; }
        public string BankAddress1 { get; set; }
        public int Address { get; set; }
        public string Category { get; set; }
        public int CurCode { get; set; }
        public float BaseRate { get; set; }
        public float SBaseRate { get; set; }
        public long PartyCode { get; set; }
        public long? AdatCode { get; set; }
        public float AdatPer { get; set; }
        public float AdatAmount { get; set; }
        public long? BrokerCode { get; set; }
        public float BrokerPer { get; set; }
        public float BrokerAmount { get; set; }
        public string InvDisc { get; set; }
        public float TotCts { get; set; }
        public int TotPcs { get; set; }
        public float PlusMinusAmount { get; set; }
        public float Amount { get; set; }
        public float RoundAmount { get; set; }
        public float FinalAmount { get; set; }
        public float VatPer { get; set; }
        public float VatAmount { get; set; }

        public float CGSTPer { get; set; }
        public float CGSTAmt { get; set; }

        public float SGSTPer { get; set; }
        public float SGSTAmt { get; set; }

        public float IGSTPer { get; set; }

        public string PShape { get; set; }
        public string PSize { get; set; }
        public string PSieve { get; set; }
        public string PColor { get; set; }
        public string PClarity { get; set; }
        public float IGSTAmt { get; set; }

        public float FinalAmtMCur { get; set; }

        public float NetMAmount { get; set; }
        public float FinalMAmount { get; set; }

        public float FinalMPayRecAmount { get; set; }

        public string TrnType { get; set; }
        public long SellerCode { get; set; }
        public string Term { get; set; }
        public long CountryCode { get; set; }

        public float TotAmount { get; set; }
        public float NetAmount { get; set; }
        public string Description { get; set; }
        public bool OutStanding { get; set; }
        public bool StockEffected { get; set; }
        public bool Brokerage { get; set; }
        public bool Cheque { get; set; }
        public int UserCode { get; set; }
        public string CreatedBy { get; set; }
        public string ModifyBy { get; set; }
        public float MasterCurrencyRate { get; set; }
        public float StockCurrencyRate { get; set; }
        public float StockCurrencyFinalAmount { get; set; }
        public long ExpPartyCode { get; set; }
        public float BMasterCurrencyRate { get; set; }
        public float AMasterCurrencyRate { get; set; }
        public float InterestPer { get; set; }
        public DateTime RoundDate { get; set; }
        public int DivideDays { get; set; }
        public long GenerateBillCode { get; set; }
        public string InvDescription { get; set; }
        public string BankName { get; set; }
        public float CFMasterCurrencyOSAmount { get; set; }
        public float CFStockCurrencyOSAmount { get; set; }
        public float CFMasterCurrencyOSBrokerageAmount { get; set; }
        public float CFStockCurrencyOSBrokerageAmount { get; set; }
        public float CFMasterCurrencyExDiffAmount { get; set; }
        public string ShipVia { get; set; }
        public string DeliverTo { get; set; }
        public string Tracking { get; set; }
        public string SalesPerson { get; set; }
        public float LabCharges { get; set; }
        public float FreightCharges { get; set; }
        public int CompanyID { get; set; }

        public string PartyName { get; set; }
        public string BrokerName { get; set; }

        public string AdatName { get; set; }

        public string ShapeId { get; set; }


        public string ColorId { get; set; }


        public string SizeId { get; set; }


        public string ClarityId { get; set; }

        public string SieveId { get; set; }

        public string FromEmail { get; set; }
        public string ToEmail { get; set; }

        public float GoldRate { get; set; }
        public string DiamondCostType { get; set; }
        public float LabourCost { get; set; }

        public string Attendee { get; set; }




    }
    public class ImportExcelpurAdd
    {
        public int SrNo { get; set; }
        public string LotNo { get; set; }
        public string ItemName { get; set; }
        public string MfgSeriesID { get; set; }
        public string MfgPacketID { get; set; }
        public string PShape { get; set; }
        public string PSize { get; set; }
        public string PSieve { get; set; }
        public string PColor { get; set; }
        public string PClarity { get; set; }
        public float Weight { get; set; }
        public int Pcs { get; set; }
        public float MRP { get; set; }
        public float PurchaseBack { get; set; }
        public float PurchaseRate { get; set; }
        //public float PurchaseAmount { get; set; }
        public float PurAmount { get; set; }
        public string Cut { get; set; }
        public string Polish { get; set; }
        public string Symmetry { get; set; }
        public string FL { get; set; }
        public float LW { get; set; }
        public float Length { get; set; }
        public float Width { get; set; }
        public float Depth { get; set; }
        public float TotDep { get; set; }
        public float TablePer { get; set; }
        public string Culet { get; set; }
        public string GMin { get; set; }
        public string GMax { get; set; }
        public float Min { get; set; }
        public float Max { get; set; }
        public float GirdlePer { get; set; }
        public string Brown { get; set; }
        public string Milky { get; set; }
        public string HA { get; set; }
        public string Natts { get; set; }
        public string EyeClean { get; set; }
        public string OIT { get; set; }
        public string OIG { get; set; }
        public string OIC { get; set; }
        public string OIP { get; set; }
        public string TabInc { get; set; }
        public string IncPattern { get; set; }
        public string LAB { get; set; }
        public string CertificateNo { get; set; }
        public string InscriptionText { get; set; } //41
        public float CH { get; set; }
        public float CA { get; set; }
        public float PA { get; set; }
        public float PH { get; set; }
        public float StrLn { get; set; }
        public float LrHalf { get; set; }
        public string Luster { get; set; }
        public string KeyToSymbols { get; set; }
        public string LabComments { get; set; }
        public string ReportDate { get; set; }
        public string MfgComments { get; set; }
        public string MemberRemarks { get; set; }
        public string IGICut { get; set; }
        public string HRDCut { get; set; }
        // public string StoneId { get; set; }
        public string SuppLotNo { get; set; }
        public string GirdleCondition { get; set; }
        public double AskingBack { get; set; }
        public float AskingRate { get; set; }
        public float ActualBack { get; set; }
        public float ActualRate { get; set; }
        public string Green { get; set; }
        public string FLColor { get; set; }
        public string CuletCondition { get; set; }
        public string Shade { get; set; }
        public string CrownInclusion { get; set; }
        public string BIT { get; set; }
        public string BIS { get; set; }
        public string FancyColor { get; set; }
        public string FancyColorIntensity { get; set; }
        public string FancyColorOvertone { get; set; }
        public string Proportion { get; set; }
        public string RMK { get; set; }
        public string ItemGroup { get; set; }
        public string Treatment { get; set; }
        public string RFID { get; set; }
        public float BottomBack { get; set; }
        public float BottomRate { get; set; }
        public float B2BBack { get; set; }
        public float B2BRate { get; set; }
        public float B2CBack { get; set; }
        public float B2CRate { get; set; }
        public double Less1 { get; set; }
        public double Less2 { get; set; }
        public double Less3 { get; set; }
        public bool SinglePcs { get; set; }
        public string Status { get; set; }
        public string StockType { get; set; }
        public bool Chk { get; set; }

        public float MemoTransCode
        { get; set; }
        public float ReturnCarats
        { get; set; }
        public float SpCarats { get; set; }
        public float RtnCarats { get; set; }
        public float IssueCarats { get; set; }
        public float Rejection { get; set; }
        //public float Carats { get; set; }
        // public float PMRPDown { get; set; }

    }
}
