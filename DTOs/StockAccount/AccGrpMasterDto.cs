using System;

namespace Dia_CoreAPI.DTOs.StockAccount
{
    public class AccGrpMasterListDto
    {
        public int AccGrpCode { get; set; }
        public string AccGrpName { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string ODrCr { get; set; } = string.Empty;
        public string UnderGroupName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public float OBalance { get; set; }
        public string Country { get; set; } = string.Empty;
        public string KAM { get; set; } = string.Empty;
    }

    public class AccGrpMasterDto
    {
        public int AccGrpCode { get; set; }
        public string AccGrpName { get; set; } = string.Empty;
        public string AG { get; set; } = "A";
        public string SName { get; set; } = string.Empty;
        public int UnderGroup { get; set; }
        public double UGrCode { get; set; }
        public string ODrCr { get; set; } = "D";
        public double AccTypeCode { get; set; }
        public string UGrpName { get; set; } = string.Empty;
        public int AccType { get; set; }
        public string AccEffCode { get; set; } = string.Empty;
        public string AccEffSide { get; set; } = string.Empty;
        public int Currency { get; set; }
        public float BaseRate { get; set; }
        public float MasterCurrencyRate { get; set; }
        public float StockCurrencyRate { get; set; }
        public float SBaseRate { get; set; }
        public float OBalance { get; set; }
        public double CurCode { get; set; }
        public double Interest { get; set; }
        public double Discount { get; set; }
        public double InterestPer { get; set; }
        public double DiscountPer { get; set; }
        public double CreditLimit { get; set; }
        public int DueDays { get; set; }
        public int GracePeriod { get; set; }
        public string Term { get; set; } = string.Empty;
        public string TermName { get; set; } = string.Empty;
        public double TermPer { get; set; }
        public string LocalLangName { get; set; } = string.Empty;
        public string LocalLangContactPerson { get; set; } = string.Empty;
        public string LocalLangAddress { get; set; } = string.Empty;
        public string WebUser { get; set; } = string.Empty;
        public string WebsiteUserID { get; set; } = string.Empty;
        public string Address1 { get; set; } = string.Empty;
        public string Address2 { get; set; } = string.Empty;
        public string Address3 { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string AddressLine2 { get; set; } = string.Empty;
        public string AddressLine3 { get; set; } = string.Empty;
        public string Zipcode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string CityCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string AreaCode { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string OfficePhone { get; set; } = string.Empty;
        public string HomePhone { get; set; } = string.Empty;
        public string Fax { get; set; } = string.Empty;
        public string QQID { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Skype { get; set; } = string.Empty;
        public string SkypeId { get; set; } = string.Empty;
        public string Through { get; set; } = string.Empty;
        public string Broker { get; set; } = string.Empty;
        public string BrokerName { get; set; } = string.Empty;
        public float BrokerCode { get; set; }
        public double BrokerPer { get; set; }
        public string ContactPerson { get; set; } = string.Empty;
        public string Agent { get; set; } = string.Empty;
        public string Executive { get; set; } = string.Empty;
        public string RefPartyName1 { get; set; } = string.Empty;
        public string RefParty1 { get; set; } = string.Empty;
        public string RefParty2 { get; set; } = string.Empty;
        public string RefPhone1 { get; set; } = string.Empty;
        public string RefComments1 { get; set; } = string.Empty;
        public string RefPartyName2 { get; set; } = string.Empty;
        public string RefPhone2 { get; set; } = string.Empty;
        public string RefComments2 { get; set; } = string.Empty;
        public string BankACNO { get; set; } = string.Empty;
        public string BankAccNo { get; set; } = string.Empty;
        public string BankACDetail { get; set; } = string.Empty;
        public string BankAccDetail { get; set; } = string.Empty;
        public string BankRouting { get; set; } = string.Empty;
        public string BankSwift { get; set; } = string.Empty;
        public string BankSwiftCode { get; set; } = string.Empty;
        public string PanNO { get; set; } = string.Empty;
        public string PAN { get; set; } = string.Empty;
        public string GSTno { get; set; } = string.Empty;
        public string TINNO { get; set; } = string.Empty;
        public string CINNO { get; set; } = string.Empty;
        public string GRNO { get; set; } = string.Empty;
        public string CSTNO { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public bool Compliance { get; set; }
        public bool Active { get; set; }
        public bool EmailInventory { get; set; }
        public string ShippingName { get; set; } = string.Empty;
        public string ShippingAddress1 { get; set; } = string.Empty;
        public string ShippingAddress2 { get; set; } = string.Empty;
        public string ShippingAddress3 { get; set; } = string.Empty;
        public string ShippingZipcode { get; set; } = string.Empty;
        public string ShipName { get; set; } = string.Empty;
        public string ShipStreet { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingState { get; set; } = string.Empty;
        public string ShippingCountry { get; set; } = string.Empty;
        public string ShippingArea { get; set; } = string.Empty;
        public string ShipCityCode { get; set; } = string.Empty;
        public string ShipStateCode { get; set; } = string.Empty;
        public string ShipCountryCode { get; set; } = string.Empty;
        public string ShipAreaCode { get; set; } = string.Empty;
        public string ShippingPhone { get; set; } = string.Empty;
        public string ShippingFax { get; set; } = string.Empty;
        public string Markup { get; set; } = string.Empty;
        public string KAM { get; set; } = string.Empty;
    }
}
