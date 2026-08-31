using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.FinancialTransaction.Loan;
using Microsoft.Data.SqlClient;
using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Classes.FinancialTransaction.Loan
{
    public class LoanTakenRepository : Interfaces.FinancialTransaction.Loan.ILoanTakenRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;
        private const string FormName = "Loan Taken";
        private const string TrnType = "LT";
        private const string JvType = "LOANT";
        private const string ListSp = "GetSalepurLT";
        private const bool jvDebitIsParty = false;

        public LoanTakenRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<int> CheckUserPermissionAsync(string userTypeId, string actionType, string companyId)
        {
            string where = actionType switch
            {
                "insert" => " InsertData = 1 ",
                "edit" => " EditData = 1 ",
                "delete" => " DeleteData = 1 ",
                "navigate" => " NavigateData = 1 ",
                _ => " SelectData = 1 "
            };

            var parameters = new Dictionary<string, object>
            {
                { "@UserTypeId", "'" + userTypeId + "'" },
                { "@FormName", "'" + FormName + "'" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<PermissionCheck>(
                "GetUserPermission", parameters, companyId);

            return result.Count();
        }

        public async Task<List<LoanListItemDto>> GetListAsync(string companyId)
        {
            string sql = $" EXEC [{ListSp}] @Where =' and trntype=''{TrnType}'''";
            var rows = (await _sqlDataAccess.SelectRawSql<LoanSalepurRow>(sql, companyId)).ToList();

            return rows.Select(row => new LoanListItemDto
            {
                Code = row.Code,
                ConDate = row.ConDate,
                Accgrpname = row.AccGrpName,
                FromToAccount = row.FromToAccount,
                Amount = row.TotAmount,
                Cur = row.Cur
            }).ToList();
        }

        public async Task<LoanPageDataDto> GetPageDataAsync(string companyId)
        {
            var masterData = (await _sqlDataAccess.SelectDataFromSP<BasicDataDto>(
                "AllMasterDataForAccGrpMaster", new Dictionary<string, object>(), companyId)).ToList();

            var defaultCurrencyCode = await _sqlDataAccess.ExecuteQuerySingle<string>(
                "SELECT Value FROM TabSettingMaster WHERE Name = 'DefaultCurrency'",
                new { }, companyId);

            float mBaseRate = 0;
            float sBaseRate = 0;
            if (!string.IsNullOrWhiteSpace(defaultCurrencyCode) && int.TryParse(defaultCurrencyCode, out _))
            {
                mBaseRate = await GetCurrencyRateAsync("McurBaseRate", defaultCurrencyCode, companyId);
                sBaseRate = await GetCurrencyRateAsync("ScurBaseRate", defaultCurrencyCode, companyId);
            }

            return new LoanPageDataDto
            {
                CurrencyList = masterData.Where(x => x.Type == "Currency").ToList(),
                DefaultCurrencyCode = defaultCurrencyCode,
                MBaseRate = mBaseRate,
                SBaseRate = sBaseRate
            };
        }

        public async Task<LoanDetailDto?> GetDetailAsync(int id, string companyId)
        {
            string sql = $" EXEC [{ListSp}] @Where =' AND tsp.Code = {id}'";
            var row = (await _sqlDataAccess.SelectRawSql<LoanSalepurRow>(sql, companyId)).FirstOrDefault();
            if (row == null) return null;

            int party = int.TryParse(row.PartyCode, out var p) ? p : 0;
            int ftParty = int.TryParse(row.ExpPartyCode, out var f) ? f : 0;
            int broker = int.TryParse(row.BrokerCode, out var b) ? b : 0;

            return new LoanDetailDto
            {
                Code = row.Code,
                InvNo = row.InvNo,
                Date = row.TrnDate,
                RenewDate = row.RoundDate,
                Due_Date = row.DueDate,
                DueDays = row.Duedays,
                Party = party,
                PartyName = row.AccGrpName,
                FTParty = ftParty,
                FTPartyName = row.FromToAccount,
                Currency = row.CurCode,
                MBaseRate = (float)row.MasterCurrencyRate,
                SBaseRate = (float)row.StockCurrencyRate,
                Broker = broker,
                BrokerName = row.BrokerName,
                Brokerper = row.Brokerper,
                Amount = (float)row.TotAmount,
                InterestPer = row.InterestPer,
                DivideDays = row.DivideDays,
                OutStanding = row.OutStanding || row.Outstanding
            };
        }

        public async Task<(bool Success, string Message)> AddAsync(LoanSaveRequest request, string userId, string companyId)
        {
            string connectionString = await _sqlDataAccess.GetClientConnectionString(companyId);
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();
            try
            {
                int usercode = int.TryParse(userId, out var u) ? u : 0;
                int companyid = int.TryParse(companyId, out var c) ? c : 0;

                float curCode = request.Currency;
                float mCurBaseRate = request.MBaseRate;
                float sCurBaseRate = request.SBaseRate;

                string mCurOperator = await ExecuteCurrencyDetailScalarAsync(conn, trans, "MCurOperator", " and ID=" + curCode);
                string sCurOperator = await ExecuteCurrencyDetailScalarAsync(conn, trans, "SCurOperator", " and ID=" + curCode);

                var (mCurAmt, sCurAmt) = GetConvertAmount(curCode, request.Amount, mCurBaseRate, mCurOperator, sCurBaseRate, sCurOperator);

                string maxInvNoStr = await ExecuteCommonDataScalarAsync(conn, trans, "Inv", $" And TrnType=''{TrnType}'' ");
                int invNo = (int.TryParse(maxInvNoStr, out var maxInv) ? maxInv : 0) + 1;

                bool outStanding = request.OutStanding;
                bool stockEffected = !outStanding;

                await InsertSalePurAsync(conn, trans, invNo, TrnType, request, mCurBaseRate, sCurBaseRate, mCurAmt, sCurAmt,
                    outStanding, stockEffected, usercode, userId, companyid);

                int maxSPCode = int.Parse(await ExecuteCommonDataScalarAsync(conn, trans, "SPCode"));

                long debitAcc = jvDebitIsParty ? request.Party : request.FTParty;
                long creditAcc = jvDebitIsParty ? request.FTParty : request.Party;

                await InsertJvAsync(conn, trans, request.Date, request.Due_Date, debitAcc, creditAcc,
                    Math.Abs(request.Amount), curCode.ToString(), JvType, maxSPCode,
                    mCurBaseRate, mCurAmt, sCurBaseRate, sCurAmt, usercode, userId, companyId);

                trans.Commit();
                return (true, "Record inserted successfully.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> EditAsync(LoanSaveRequest request, string userId, string companyId)
        {
            string connectionString = await _sqlDataAccess.GetClientConnectionString(companyId);
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();
            try
            {
                int usercode = int.TryParse(userId, out var u) ? u : 0;
                int companyid = int.TryParse(companyId, out var c) ? c : 0;
                int code = request.Code;

                float curCode = request.Currency;
                float mCurBaseRate = request.MBaseRate;
                float sCurBaseRate = request.SBaseRate;

                string mCurOperator = await ExecuteCurrencyDetailScalarAsync(conn, trans, "MCurOperator", " and ID=" + curCode);
                string sCurOperator = await ExecuteCurrencyDetailScalarAsync(conn, trans, "SCurOperator", " and ID=" + curCode);

                var (mCurAmt, sCurAmt) = GetConvertAmount(curCode, request.Amount, mCurBaseRate, mCurOperator, sCurBaseRate, sCurOperator);

                bool outStanding = request.OutStanding;
                bool stockEffected = !outStanding;

                await UpdateSalePurAsync(conn, trans, code, request.InvNo, TrnType, request, mCurBaseRate, sCurBaseRate,
                    mCurAmt, sCurAmt, outStanding, stockEffected, usercode, userId, companyid);

                await DeleteJvAsync(conn, trans, code, JvType);

                long debitAcc = jvDebitIsParty ? request.Party : request.FTParty;
                long creditAcc = jvDebitIsParty ? request.FTParty : request.Party;

                await InsertJvAsync(conn, trans, request.Date, request.Due_Date, debitAcc, creditAcc,
                    Math.Abs(request.Amount), curCode.ToString(), JvType, code,
                    mCurBaseRate, mCurAmt, sCurBaseRate, sCurAmt, usercode, userId, companyId);

                trans.Commit();
                return (true, "Record Update successfully.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId)
        {
            string connectionString = await _sqlDataAccess.GetClientConnectionString(companyId);
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();
            try
            {
                string payRecCountStr;
                using (var cmd = new SqlCommand("GetPayRecCount", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Where", " And InvCode = " + id));
                    payRecCountStr = (await cmd.ExecuteScalarAsync())?.ToString() ?? "0";
                }

                if (int.TryParse(payRecCountStr, out var payRecCount) && payRecCount > 0)
                    return (false, "Here are Payments entries against This Invoice. So Invoice cannot be Deleted.");

                using (var cmd = new SqlCommand("SP_DeleteSalePur", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@code", id));
                    await cmd.ExecuteNonQueryAsync();
                }

                await DeleteJvAsync(conn, trans, id, JvType);

                trans.Commit();
                return (true, "Record Deleted.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return (false, ex.Message);
            }
        }

        private async Task<float> GetCurrencyRateAsync(string type, string curCode, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", type },
                { "@Where", curCode }
            };
            var result = await _sqlDataAccess.SelectDataFromSPSingle<string>("GetCurrencyDetail", param, companyId);
            return float.TryParse(result, out var rate) ? rate : 0f;
        }

        private static async Task InsertSalePurAsync(
            SqlConnection conn, SqlTransaction trans, int invNo, string trnType,
            LoanSaveRequest req, double mCurBaseRate, double sCurBaseRate, double mCurAmt, double sCurAmt,
            bool outStanding, bool stockEffected, int usercode, string userId, int companyId)
        {
            using var cmd = new SqlCommand("SP_InsertSalePur", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@invno", invNo));
            cmd.Parameters.Add(new SqlParameter("@billno", ""));
            cmd.Parameters.Add(new SqlParameter("@trntype", trnType));
            cmd.Parameters.Add(new SqlParameter("@invtype", trnType));
            cmd.Parameters.Add(new SqlParameter("@category", ""));
            cmd.Parameters.Add(new SqlParameter("@trndate", req.Date));
            cmd.Parameters.Add(new SqlParameter("@duedays", req.DueDays));
            cmd.Parameters.Add(new SqlParameter("@duedate", req.Due_Date));
            cmd.Parameters.Add(new SqlParameter("@partycode", req.Party));
            cmd.Parameters.Add(new SqlParameter("@SellerCode", (object)0));
            cmd.Parameters.Add(new SqlParameter("@term", ""));
            cmd.Parameters.Add(new SqlParameter("@countrycode", (object)0));
            cmd.Parameters.Add(new SqlParameter("@BrokerCode", req.Broker == 0 ? DBNull.Value : req.Broker));
            cmd.Parameters.Add(new SqlParameter("@BrokerPer", req.Brokerper));
            cmd.Parameters.Add(new SqlParameter("@BrokerAmount", (object)0));
            cmd.Parameters.Add(new SqlParameter("@adatcode", DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@adatper", (object)0));
            cmd.Parameters.Add(new SqlParameter("@adatamount", (object)0));
            cmd.Parameters.Add(new SqlParameter("@totcts", (object)0));
            cmd.Parameters.Add(new SqlParameter("@totamount", Math.Round(req.Amount, 4)));
            cmd.Parameters.Add(new SqlParameter("@netamount", Math.Round(req.Amount, 4)));
            cmd.Parameters.Add(new SqlParameter("@PlusMinAmount", (object)0));
            cmd.Parameters.Add(new SqlParameter("@RoundAmount", (object)0));
            cmd.Parameters.Add(new SqlParameter("@FinalAmount", Math.Round(mCurAmt, 4)));
            cmd.Parameters.Add(new SqlParameter("@BaseRate", Math.Round(req.MBaseRate, 4)));
            cmd.Parameters.Add(new SqlParameter("@Disc", req.Disc ?? ""));
            cmd.Parameters.Add(new SqlParameter("@CurCode", req.Currency.ToString()));
            cmd.Parameters.Add(new SqlParameter("@Outstanding", outStanding));
            cmd.Parameters.Add(new SqlParameter("@StockEffected", stockEffected));
            cmd.Parameters.Add(new SqlParameter("@Brokerage", false));
            cmd.Parameters.Add(new SqlParameter("@Commission", false));
            cmd.Parameters.Add(new SqlParameter("@Cheque", false));
            cmd.Parameters.Add(new SqlParameter("@usercode", usercode));
            cmd.Parameters.Add(new SqlParameter("@mastercurrencyrate", Math.Round(mCurBaseRate, 4)));
            cmd.Parameters.Add(new SqlParameter("@mastercurrencyfinalamount", Math.Round(mCurAmt, 2)));
            cmd.Parameters.Add(new SqlParameter("@stockcurrencyrate", Math.Round(sCurBaseRate, 2)));
            cmd.Parameters.Add(new SqlParameter("@stockcurrencyfinalamount", Math.Round(sCurAmt, 4)));
            cmd.Parameters.Add(new SqlParameter("@ExpPartyCode", req.FTParty));
            cmd.Parameters.Add(new SqlParameter("@RoundDate", req.RenewDate));
            cmd.Parameters.Add(new SqlParameter("@DivideDays", req.DivideDays));
            cmd.Parameters.Add(new SqlParameter("@InterestPer", req.InterestPer));
            cmd.Parameters.Add(new SqlParameter("@VatPer", (object)0));
            cmd.Parameters.Add(new SqlParameter("@VatAmount", (object)0));
            cmd.Parameters.Add(new SqlParameter("@invdescription", ""));
            cmd.Parameters.Add(new SqlParameter("@bankname", (object)0));
            cmd.Parameters.Add(new SqlParameter("@subtype", ""));
            cmd.Parameters.Add(new SqlParameter("@ShipVia", ""));
            cmd.Parameters.Add(new SqlParameter("@deliverto", ""));
            cmd.Parameters.Add(new SqlParameter("@Tracking", ""));
            cmd.Parameters.Add(new SqlParameter("@SalesPerson", ""));
            cmd.Parameters.Add(new SqlParameter("@Attendee", ""));
            cmd.Parameters.Add(new SqlParameter("@labcharges", (object)0));
            cmd.Parameters.Add(new SqlParameter("@freightcharges", (object)0));
            cmd.Parameters.Add(new SqlParameter("@duedays1", (object)0));
            cmd.Parameters.Add(new SqlParameter("@duedays2", (object)0));
            cmd.Parameters.Add(new SqlParameter("@CGSTPer", (object)0));
            cmd.Parameters.Add(new SqlParameter("@CGSTAmount", (object)0));
            cmd.Parameters.Add(new SqlParameter("@SGSTPer", (object)0));
            cmd.Parameters.Add(new SqlParameter("@SGSTAmount", (object)0));
            cmd.Parameters.Add(new SqlParameter("@IGSTPer", (object)0));
            cmd.Parameters.Add(new SqlParameter("@IGSTAmount", (object)0));
            cmd.Parameters.Add(new SqlParameter("@CreatedBy", userId ?? ""));
            cmd.Parameters.Add(new SqlParameter("@CompanyID", companyId));
            cmd.Parameters.Add(new SqlParameter("@netMamount", Math.Round(mCurAmt, 4)));
            cmd.Parameters.Add(new SqlParameter("@FinalPayRecAmount", Math.Round(mCurAmt, 4)));
            cmd.Parameters.Add(new SqlParameter("@Address", (object)0));
            cmd.Parameters.Add(new SqlParameter("@ActionType", "insert"));
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task UpdateSalePurAsync(
            SqlConnection conn, SqlTransaction trans, int code, int invNo, string trnType,
            LoanSaveRequest req, double mCurBaseRate, double sCurBaseRate, double mCurAmt, double sCurAmt,
            bool outStanding, bool stockEffected, int usercode, string userId, int companyId)
        {
            using var cmd = new SqlCommand("SP_UpdateSalePur", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@code", code));
            cmd.Parameters.Add(new SqlParameter("@invno", invNo));
            cmd.Parameters.Add(new SqlParameter("@ModifyBy", userId ?? ""));
            cmd.Parameters.Add(new SqlParameter("@BillNo", ""));
            cmd.Parameters.Add(new SqlParameter("@TrnType", trnType));
            cmd.Parameters.Add(new SqlParameter("@invtype", trnType));
            cmd.Parameters.Add(new SqlParameter("@SubType", ""));
            cmd.Parameters.Add(new SqlParameter("@BankID", (object)0));
            cmd.Parameters.Add(new SqlParameter("@Category", ""));
            cmd.Parameters.Add(new SqlParameter("@Trndate", req.Date));
            cmd.Parameters.Add(new SqlParameter("@RoundDate", req.RenewDate));
            cmd.Parameters.Add(new SqlParameter("@duedays", req.DueDays));
            cmd.Parameters.Add(new SqlParameter("@duedate", req.Due_Date));
            cmd.Parameters.Add(new SqlParameter("@DivideDays", req.DivideDays));
            cmd.Parameters.Add(new SqlParameter("@partycode", req.Party));
            cmd.Parameters.Add(new SqlParameter("@SellerCode", (object)0));
            cmd.Parameters.Add(new SqlParameter("@countrycode", (object)0));
            cmd.Parameters.Add(new SqlParameter("@brokercode", req.Broker == 0 ? DBNull.Value : req.Broker));
            cmd.Parameters.Add(new SqlParameter("@brokerper", req.Brokerper));
            cmd.Parameters.Add(new SqlParameter("@BrokerAmount", (object)0));
            cmd.Parameters.Add(new SqlParameter("@Adatcode", DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@Adatper", (object)0));
            cmd.Parameters.Add(new SqlParameter("@AdatAmount", (object)0));
            cmd.Parameters.Add(new SqlParameter("@totcts", (object)0));
            cmd.Parameters.Add(new SqlParameter("@totamount", Math.Round(req.Amount, 2)));
            cmd.Parameters.Add(new SqlParameter("@netamount", Math.Round(req.Amount, 2)));
            cmd.Parameters.Add(new SqlParameter("@NetMAmount", Math.Round(mCurAmt, 4)));
            cmd.Parameters.Add(new SqlParameter("@plusminamount", (object)0));
            cmd.Parameters.Add(new SqlParameter("@RoundAmount", (object)0));
            cmd.Parameters.Add(new SqlParameter("@finalamount", Math.Round(req.Amount, 2)));
            cmd.Parameters.Add(new SqlParameter("@FinalPayRecAmount", Math.Round(mCurAmt, 4)));
            cmd.Parameters.Add(new SqlParameter("@baserate", Math.Round(req.MBaseRate, 2)));
            cmd.Parameters.Add(new SqlParameter("@ExpPartyCode", req.FTParty));
            cmd.Parameters.Add(new SqlParameter("@ShipVia", ""));
            cmd.Parameters.Add(new SqlParameter("@Tracking", ""));
            cmd.Parameters.Add(new SqlParameter("@SalesPerson", ""));
            cmd.Parameters.Add(new SqlParameter("@Attendee", ""));
            cmd.Parameters.Add(new SqlParameter("@disc", req.Disc ?? ""));
            cmd.Parameters.Add(new SqlParameter("@CurCode", req.Currency.ToString()));
            cmd.Parameters.Add(new SqlParameter("@OutStanding", outStanding));
            cmd.Parameters.Add(new SqlParameter("@StockEffected", stockEffected));
            cmd.Parameters.Add(new SqlParameter("@Brokerage", false));
            cmd.Parameters.Add(new SqlParameter("@Commission", false));
            cmd.Parameters.Add(new SqlParameter("@Cheque", false));
            cmd.Parameters.Add(new SqlParameter("@UserCode", usercode));
            cmd.Parameters.Add(new SqlParameter("@term", ""));
            cmd.Parameters.Add(new SqlParameter("@InterestPer", req.InterestPer));
            cmd.Parameters.Add(new SqlParameter("@MasterCurrencyRate", Math.Round(mCurBaseRate, 2)));
            cmd.Parameters.Add(new SqlParameter("@MasterCurrencyFinalAmount", Math.Round(mCurAmt, 2)));
            cmd.Parameters.Add(new SqlParameter("@StockCurrencyRate", Math.Round(sCurBaseRate, 2)));
            cmd.Parameters.Add(new SqlParameter("@StockCurrencyFinalAmount", Math.Round(sCurAmt, 2)));
            cmd.Parameters.Add(new SqlParameter("@VatPer", (object)0));
            cmd.Parameters.Add(new SqlParameter("@VatAmount", (object)0));
            cmd.Parameters.Add(new SqlParameter("@CGSTPer", (object)0));
            cmd.Parameters.Add(new SqlParameter("@CGSTAmount", (object)0));
            cmd.Parameters.Add(new SqlParameter("@SGSTPer", (object)0));
            cmd.Parameters.Add(new SqlParameter("@SGSTAmount", (object)0));
            cmd.Parameters.Add(new SqlParameter("@IGSTPer", (object)0));
            cmd.Parameters.Add(new SqlParameter("@IGSTAmount", (object)0));
            cmd.Parameters.Add(new SqlParameter("@CompanyID", companyId));
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task InsertJvAsync(
            SqlConnection conn, SqlTransaction trans,
            DateTime jvDate, DateTime dueDate, long debitAcc, long creditAcc,
            float amount, string curCode, string jvType, int vCode,
            double mCurBaseRate, double mCurAmt, double sCurBaseRate, double sCurAmt,
            int usercode, string userId, string companyId)
        {
            using var cmd = new SqlCommand("SP_insertjv", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@dacccode", debitAcc));
            cmd.Parameters.Add(new SqlParameter("@cacccode", creditAcc));
            cmd.Parameters.Add(new SqlParameter("@amount", amount));
            cmd.Parameters.Add(new SqlParameter("@JVdate", jvDate));
            cmd.Parameters.Add(new SqlParameter("@duedate", dueDate));
            cmd.Parameters.Add(new SqlParameter("@curcode", curCode));
            cmd.Parameters.Add(new SqlParameter("@vtype", jvType));
            cmd.Parameters.Add(new SqlParameter("@vcode", vCode));
            cmd.Parameters.Add(new SqlParameter("@disc", ""));
            cmd.Parameters.Add(new SqlParameter("@Adjustvtype", (object)0));
            cmd.Parameters.Add(new SqlParameter("@Adjustvcode", (object)0));
            cmd.Parameters.Add(new SqlParameter("@BaseRate", Math.Round(mCurBaseRate, 4)));
            cmd.Parameters.Add(new SqlParameter("@MasterCurrencyRate", Math.Round(mCurBaseRate, 4)));
            cmd.Parameters.Add(new SqlParameter("@MasterCurrencyAmount", Math.Round(mCurAmt, 4)));
            cmd.Parameters.Add(new SqlParameter("@StockCurrencyRate", Math.Round(sCurBaseRate, 4)));
            cmd.Parameters.Add(new SqlParameter("@StockCurrencyAmount", Math.Round(sCurAmt, 4)));
            cmd.Parameters.Add(new SqlParameter("@GCode", (object)0));
            cmd.Parameters.Add(new SqlParameter("@UserCode", usercode));
            cmd.Parameters.Add(new SqlParameter("@CreatedBy", userId ?? ""));
            cmd.Parameters.Add(new SqlParameter("@Companyid", companyId));
            cmd.Parameters.Add(new SqlParameter("@AgainstAccCode", (object)0));
            cmd.Parameters.Add(new SqlParameter("@ActionType", "insert"));
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task DeleteJvAsync(SqlConnection conn, SqlTransaction trans, int vCode, string jvType)
        {
            using var cmd = new SqlCommand("SP_DeleteJV", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@vcode", vCode));
            cmd.Parameters.Add(new SqlParameter("@vtype", jvType));
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task<string> ExecuteCommonDataScalarAsync(SqlConnection conn, SqlTransaction trans, string type, string where = "")
        {
            using var cmd = new SqlCommand("GetAllCommonData", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@Type", type));
            cmd.Parameters.Add(new SqlParameter("@Where", where?.Replace("''", "'") ?? ""));
            return (await cmd.ExecuteScalarAsync())?.ToString() ?? string.Empty;
        }

        private static async Task<string> ExecuteCurrencyDetailScalarAsync(SqlConnection conn, SqlTransaction trans, string type, string where = "")
        {
            using var cmd = new SqlCommand("GetCurrencyDetail", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@Type", type));
            cmd.Parameters.Add(new SqlParameter("@Where", where?.Replace("''", "'") ?? ""));
            return (await cmd.ExecuteScalarAsync())?.ToString() ?? "*";
        }

        private static (double MCurAmt, double SCurAmt) GetConvertAmount(
            double curCode, double amount, double mCurBaseRate, string mCurOperator,
            double sCurBaseRate, string sCurOperator)
        {
            double mCurAmt;
            double sCurAmt;

            if (mCurOperator == "*")
                mCurAmt = amount * mCurBaseRate;
            else
                mCurAmt = mCurBaseRate != 0 ? amount / mCurBaseRate : 0;

            if (sCurOperator == "*")
                sCurAmt = amount * sCurBaseRate;
            else
                sCurAmt = sCurBaseRate != 0 ? amount / sCurBaseRate : 0;

            if (double.IsNaN(mCurAmt) || double.IsInfinity(mCurAmt)) mCurAmt = 0;
            if (double.IsNaN(sCurAmt) || double.IsInfinity(sCurAmt)) sCurAmt = 0;
            return (mCurAmt, sCurAmt);
        }
    }
}
