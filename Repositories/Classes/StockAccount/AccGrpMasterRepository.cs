using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.StockAccount;

namespace Dia_CoreAPI.Repositories.Classes.StockAccount
{
    public class AccGrpMasterRepository : IAccGrpMasterRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public AccGrpMasterRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<IEnumerable<AccGrpMasterListDto>> GetAccGrpMasterListAsync(string companyId, string whereClause = "")
        {
            var param = new Dictionary<string, object>();
            param.Add("@PageNo", "1");
            param.Add("@where", whereClause);

            return await _sqlDataAccess.SelectDataFromSP<AccGrpMasterListDto>("GetAccountDataUsingPageNo", param, companyId);
        }

        public async Task<IEnumerable<BasicDataDto>> GetAccGrpMasterDropdownAsync(string companyId)
        {
            var param = new Dictionary<string, object>();
            var masterList = (await _sqlDataAccess.SelectDataFromSP<BasicDataDto>("AllMasterDataForAccGrpMaster", param, companyId)).ToList();

            try
            {
                var brokerParam = new Dictionary<string, object>();
                brokerParam.Add("@strWhere", "");
                var brokers = await _sqlDataAccess.SelectDataFromSP<BasicDataDto>("GetAccountDataForDropdown", brokerParam, companyId);
                foreach (var b in brokers)
                {
                    b.Type = "Broker";
                    masterList.Add(b);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load brokers: {ex.Message}");
            }

            return masterList;
        }

        public async Task<AccGrpMasterDto?> GetAccGrpMasterByIdAsync(int id, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@where", $" AND TAG.AccGrpCode = {id} ");

            return await _sqlDataAccess.SelectDataFromSPSingle<AccGrpMasterDto>("GetAccountDataForEdit", param, companyId);
        }

        public async Task<bool> CheckAccGrpNameExistsAsync(string name, string companyId, int? excludeId = null)
        {
            var param = new Dictionary<string, object>();
            param.Add("@Type", "AccExist");
            string whereClause = $" AND AccGrpName = '{name}'";
            if (excludeId.HasValue)
            {
                whereClause += $" AND AccGrpCode != {excludeId.Value}";
            }
            param.Add("@Where", whereClause);

            var result = await _sqlDataAccess.SelectDataFromSPSingle<string>("GetAllCommonData", param, companyId);
            return result != "0" && !string.IsNullOrEmpty(result);
        }

        public async Task<string> GetMCurOperatorAsync(int currencyId, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@Type", "MCurOperator");
            param.Add("@Where", $" and ID={currencyId}");
            var result = await _sqlDataAccess.SelectDataFromSPSingle<string>("GetCurrencyDetail", param, companyId);
            return result ?? string.Empty;
        }

        public async Task<string> GetSCurOperatorAsync(int currencyId, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@Type", "SCurOperator");
            param.Add("@Where", $" and ID={currencyId}");
            var result = await _sqlDataAccess.SelectDataFromSPSingle<string>("GetCurrencyDetail", param, companyId);
            return result ?? string.Empty;
        }

        public async Task<float> GetMcurBaseRateAsync(string code, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@Type", "McurBaseRate");
            param.Add("@Where", code);
            var result = await _sqlDataAccess.SelectDataFromSPSingle<string>("GetCurrencyDetail", param, companyId);
            if (float.TryParse(result, out float rate))
            {
                return rate;
            }
            return 0f;
        }

        public async Task<float> GetScurBaseRateAsync(string code, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@Type", "ScurBaseRate");
            param.Add("@Where", code);
            var result = await _sqlDataAccess.SelectDataFromSPSingle<string>("GetCurrencyDetail", param, companyId);
            if (float.TryParse(result, out float rate))
            {
                return rate;
            }
            return 0f;
        }

        public async Task<string> GetDefaultCurrencyCodeAsync(string companyId)
        {
            var result = await _sqlDataAccess.ExecuteQuerySingle<string>(
                "SELECT Value FROM TabSettingMaster WHERE Name = 'DefaultCurrency'",
                new { },
                companyId);
            return result ?? string.Empty;
        }

        private async Task<string> GetCurrencyDetailScalarAsync(string type, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@Type", type);
            param.Add("@Where", "");
            var result = await _sqlDataAccess.SelectDataFromSPSingle<string>("GetCurrencyDetail", param, companyId);
            return result ?? string.Empty;
        }

        private (double MCurAmt, double SCurAmt) GetConvertAmount(
            double curCode,
            double amount,
            double mCurBaseRate,
            string mCurOperator,
            double sCurBaseRate,
            string sCurOperator,
            double mCurCode,
            double sCurCode)
        {
            double mCurAmt = 0;
            double sCurAmt = 0;

            if (curCode == mCurCode)
            {
                mCurAmt = amount;
                if (sCurOperator == "*")
                    sCurAmt = amount * sCurBaseRate;
                else
                    sCurAmt = sCurBaseRate != 0 ? amount / sCurBaseRate : 0;
            }
            else if (curCode == sCurCode)
            {
                sCurAmt = amount;
                if (mCurOperator == "*")
                    mCurAmt = amount * mCurBaseRate;
                else
                    mCurAmt = mCurBaseRate != 0 ? amount / mCurBaseRate : 0;
            }
            else
            {
                if (mCurOperator == "*")
                    mCurAmt = amount * mCurBaseRate;
                else
                    mCurAmt = mCurBaseRate != 0 ? amount / mCurBaseRate : 0;

                if (sCurOperator == "*")
                    sCurAmt = amount * sCurBaseRate;
                else
                    sCurAmt = sCurBaseRate != 0 ? amount / sCurBaseRate : 0;
            }

            if (double.IsNaN(mCurAmt) || double.IsInfinity(mCurAmt))
                mCurAmt = 0;
            if (double.IsNaN(sCurAmt) || double.IsInfinity(sCurAmt))
                sCurAmt = 0;

            return (mCurAmt, sCurAmt);
        }

        public async Task<bool> SaveAccGrpMasterAsync(AccGrpMasterDto model, string actionType, string companyId)
        {
            var param = new Dictionary<string, object>();

            // Calculate converted amounts like legacy controller
            string mCurOperator = await GetMCurOperatorAsync(model.Currency, companyId);
            string sCurOperator = await GetSCurOperatorAsync(model.Currency, companyId);

            double mCurCode = 0;
            double.TryParse(await GetCurrencyDetailScalarAsync("MCurID", companyId), out mCurCode);
            double sCurCode = 0;
            double.TryParse(await GetCurrencyDetailScalarAsync("SCurID", companyId), out sCurCode);

            var amountRes = GetConvertAmount(model.Currency, model.OBalance, model.BaseRate, mCurOperator, model.SBaseRate, sCurOperator, mCurCode, sCurCode);
            double mCurAmount = amountRes.MCurAmt;
            double sCurAmount = amountRes.SCurAmt;

            param.Add("@AccGrpCode", model.AccGrpCode);
            param.Add("@AccGrpName", model.AccGrpName ?? "");
            param.Add("@SName", model.SName ?? "");
            param.Add("@UGrCode", model.UnderGroup);
            param.Add("@AccTypeCode", model.AccType);
            param.Add("@AcLevel", 0);
            param.Add("@ODrCr", model.ODrCr ?? "");
            param.Add("@OBalance", model.OBalance);
            param.Add("@CurCode", model.Currency);
            param.Add("@MasterCurrencyRate", model.BaseRate);
            param.Add("@MasterCurrencyAmount", mCurAmount);
            param.Add("@StockCurrencyRate", model.SBaseRate);
            param.Add("@StockCurrencyAmount", sCurAmount);
            param.Add("@AG", model.AG ?? "A");
            param.Add("@street", model.Address1 ?? "");
            param.Add("@AreaCode", model.Area ?? "");
            param.Add("@CityCode", model.City ?? "");
            param.Add("@StateCode", model.State ?? "");
            param.Add("@ZipCode", model.Zipcode ?? "");
            param.Add("@CountryCode", model.Country ?? "");
            param.Add("@HomePhone", model.HomePhone ?? "");
            param.Add("@OfficePhone", model.OfficePhone ?? "");
            param.Add("@Mobile", model.Mobile ?? "");
            param.Add("@Email", model.Email ?? "");
            param.Add("@Website", model.Website ?? "");
            param.Add("@Remarks", model.Remarks ?? "");
            param.Add("@ShipName", model.ShippingName ?? "");
            param.Add("@ShipStreet", model.ShippingAddress1 ?? "");
            param.Add("@ShipAreaCode", model.ShippingArea ?? "");
            param.Add("@ShipCityCode", model.ShippingCity ?? "");
            param.Add("@ShipStateCode", model.ShippingState ?? "");
            param.Add("@ShipZipCode", model.ShippingZipcode ?? "");
            param.Add("@ShipCountryCode", model.ShippingCountry ?? "");
            param.Add("@ShipPhone", model.ShippingPhone ?? "");
            param.Add("@ShipFax", model.ShippingFax ?? "");
            param.Add("@BuyerCode", 0);
            param.Add("@SalesMan1Code", 0);
            param.Add("@SalesMan1Per", 0f);
            param.Add("@SalesMan2Code", 0);
            param.Add("@SalesMan2Per", 0d);
            param.Add("@InterestPer", model.Interest);
            param.Add("@DiscountPer", model.Discount);
            param.Add("@CreditLimit", model.CreditLimit);
            param.Add("@DueDays", model.DueDays);
            param.Add("@GracePeriod", model.GracePeriod);
            param.Add("@AccEffCode", model.AccEffCode ?? "");
            param.Add("@AccEffSide", model.AccEffSide ?? "");

            int throughVal = 0;
            int.TryParse(model.Through, out throughVal);
            param.Add("@Through", throughVal);

            int brokerVal = 0;
            int.TryParse(model.Broker, out brokerVal);
            param.Add("@Broker", brokerVal);

            param.Add("@Executive", model.Executive ?? "");
            param.Add("@TermName", model.Term ?? "");
            param.Add("@TermPer", model.TermPer);
            param.Add("@GrNo", model.GRNO ?? "");
            param.Add("@PAN", model.PanNO ?? "");
            param.Add("@CSTNo", model.CSTNO ?? "");
            param.Add("@BankAccNo", model.BankACNO ?? "");
            param.Add("@BankAccDetail", model.BankACDetail ?? "");
            param.Add("@ConsigneeDetail", "");
            param.Add("@TINNo", model.TINNO ?? "");
            param.Add("@BrokerPer", model.BrokerPer);
            param.Add("@TermValueBack", 0d);
            param.Add("@Fax", model.Fax ?? "");
            param.Add("@SkypeId", model.Skype ?? "");
            param.Add("@ContactPerson", model.ContactPerson ?? "");
            param.Add("@Agent", model.Agent ?? "");
            param.Add("@AddressLine1", model.Address1 ?? "");
            param.Add("@AddressLine2", model.Address2 ?? "");
            param.Add("@AddressLine3", model.Address3 ?? "");
            param.Add("@RefParty1", model.RefPartyName1 ?? "");
            param.Add("@RefPhone1", model.RefPhone1 ?? "");
            param.Add("@RefComments1", model.RefComments1 ?? "");
            param.Add("@BankAccNo1", model.BankACNO ?? "");
            param.Add("@BankRouting", model.BankRouting ?? "");
            param.Add("@BankSwiftCode", model.BankSwift ?? "");
            param.Add("@RefParty2", model.RefPartyName2 ?? "");
            param.Add("@RefPhone2", model.RefPhone2 ?? "");
            param.Add("@RefComments2", model.RefComments2 ?? "");
            param.Add("@Compilance", model.Compliance);
            param.Add("@Inactive", !model.Active);
            param.Add("@LocalLangName", model.LocalLangName ?? "");
            param.Add("@LocalLangContactPerson", model.LocalLangContactPerson ?? "");
            param.Add("@LocalLangAddress", model.LocalLangAddress ?? "");
            param.Add("@CustomerType", "");
            param.Add("@QQID", model.QQID ?? "");
            param.Add("@CINNO", model.CINNO ?? "");
            param.Add("@EmailInventory", model.EmailInventory);
            param.Add("@WebsiteUserID", model.WebUser ?? "");
            param.Add("@CompanyID", int.Parse(companyId));
            param.Add("@Markup", model.Markup ?? "");
            param.Add("@ActionType", actionType);

            int result = await _sqlDataAccess.ExecuteSP("SP_AccGrpMaster", param, companyId);
            return result > 0 || true;
        }

        public async Task<string> GetAccCountAsync(int id, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@ActionType", "DeleteAcc");
            param.Add("@Where", $" (PartyCode = {id} OR AdatCode = {id} OR BrokerCode = {id}) ");
            param.Add("@Where2", $" (dacccode = {id} OR cacccode = {id}) ");

            var result = await _sqlDataAccess.SelectDataFromSPSingle<string>("GetAccandItemCount", param, companyId);
            return result ?? "0";
        }

        public async Task<bool> DeleteAccGrpMasterAsync(int id, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@AccGrpCode", id);
            param.Add("@ActionType", "delete");

            int result = await _sqlDataAccess.ExecuteSP("SP_AccGrpMaster", param, companyId);
            return result > 0 || true;
        }
        public async Task<string> GetAccEffCodeAsync(int accType, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@AccType", accType);
            var result = await _sqlDataAccess.SelectDataFromSPSingle<string>("GetAccEffCodeFromTable", param, companyId);
            return result ?? string.Empty;
        }

        public async Task<string> GetAccEffSideAsync(int accType, string companyId)
        {
            var param = new Dictionary<string, object>();
            param.Add("@AccType", accType);
            var result = await _sqlDataAccess.SelectDataFromSPSingle<string>("GetAccEffSideFromTable", param, companyId);
            return result ?? string.Empty;
        }
    }
}
