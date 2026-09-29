using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.FinancialTransaction.Payment;
using Dia_CoreAPI.Repositories.Interfaces.FinancialTransaction.Payment;
using Microsoft.Data.SqlClient;
using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Classes.FinancialTransaction.Payment
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;
        private const string FormName = "Payments";

        public PaymentRepository(ISqlDataAccess sqlDataAccess)
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

        public async Task<List<PaymentListItemDto>> GetListAsync(string companyId)
        {
            // MVC: EXEC [GetPaymentList] @PageNo = '1'
            string sql = " EXEC [GetPaymentList] @PageNo = '1' ";
            var rows = (await _sqlDataAccess.SelectRawSql<PaymentListRow>(sql, companyId)).ToList();

            return rows.Select(MapListItem).ToList();
        }

        public async Task<PaymentDetailDto?> GetDetailAsync(int id, string companyId)
        {
            // MVC: EXEC [GetPaymentList] @Where = ' And GCODE = {id}', @PageNo = '1'
            string sql = $" EXEC [GetPaymentList] @Where = ' And GCODE = {id}', @PageNo = '1'";
            var row = (await _sqlDataAccess.SelectRawSql<PaymentListRow>(sql, companyId)).FirstOrDefault();
            if (row == null) return null;

            bool isIpay = string.Equals(row.Vtype, "IPAY", StringComparison.OrdinalIgnoreCase);
            string mainText = isIpay ? row.Party : row.Account;
            long mainValue = isIpay ? row.trnacc : row.mainacc;
            string partyText = isIpay ? row.Account : row.Party;
            long partyValue = isIpay ? row.mainacc : row.trnacc;
            // MVC LoadEditPaymentDetails: only Adat/Brokerage are special; free-text disc stays Payment
            string adjType = "Payment";
            if (!string.IsNullOrWhiteSpace(row.disc))
            {
                if (string.Equals(row.disc, "Adat", StringComparison.OrdinalIgnoreCase))
                    adjType = "Adat";
                else if (string.Equals(row.disc, "Brokerage", StringComparison.OrdinalIgnoreCase))
                    adjType = "Brokerage";
            }

            // MVC EditPayment selects currency by Symbol name when CurCode is 0
            int curCode = await ResolveCurCodeAsync(row.CurCode, row.Symbol, companyId);
            // Fallback when Symbol/CurCode missing: amount equals stock or master currency amount
            if (curCode == 0 && row.Amount > 0)
            {
                long sCurId = await GetCurrencyIdAsync("SCurID", companyId);
                long mCurId = await GetCurrencyIdAsync("MCurID", companyId);
                if (Math.Abs(row.StockCurrencyAmount - row.Amount) < 0.011 && sCurId > 0)
                    curCode = (int)sCurId;
                else if (Math.Abs(row.MasterCurrencyAmount - row.Amount) < 0.011 && mCurId > 0)
                    curCode = (int)mCurId;
            }
            float mRate = row.MasterCurrencyRate;
            float sRate = row.StockCurrencyRate;
            if (curCode > 0 && (mRate <= 0 || sRate <= 0))
            {
                if (mRate <= 0)
                    mRate = await GetCurrencyRateAsync("McurBaseRate", curCode.ToString(), companyId);
                if (sRate <= 0)
                    sRate = await GetCurrencyRateAsync("ScurBaseRate", curCode.ToString(), companyId);
            }

            // Prefer resolved symbol from master when row.Symbol empty
            string symbol = row.Symbol;
            if (string.IsNullOrWhiteSpace(symbol) && curCode > 0)
            {
                var masterData = (await _sqlDataAccess.SelectDataFromSP<BasicDataDto>(
                    "AllMasterDataForAccGrpMaster", new Dictionary<string, object>(), companyId)).ToList();
                symbol = masterData.FirstOrDefault(x =>
                    string.Equals(x.Type, "Currency", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.ID, curCode.ToString(), StringComparison.OrdinalIgnoreCase))?.Name;
            }

            return new PaymentDetailDto
            {
                GCODE = row.GCODE,
                InvNo = row.InvNo,
                BillNo = row.BillNo,
                Date = row.Date,
                Vtype = row.Vtype,
                Account = row.Account,
                Party = row.Party,
                Amount = row.Amount,
                Symbol = symbol,
                MasterCurrencyAmount = row.MasterCurrencyAmount,
                StockCurrencyAmount = row.StockCurrencyAmount,
                disc = row.disc,
                MasterCurrencyRate = mRate,
                StockCurrencyRate = sRate,
                CurCode = curCode,
                mainacc = row.mainacc,
                trnacc = row.trnacc,
                AdjustmentType = adjType,
                MainAccCode = mainValue,
                MainAccName = mainText,
                PartyAccCode = partyValue,
                PartyAccName = partyText
            };
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId)
        {
            string connectionString = await _sqlDataAccess.GetClientConnectionString(companyId);
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();
            try
            {
                // MVC: GetJvCode(" AND GCODE = " + ID)
                var jvCodes = await GetJvCodesAsync(conn, trans, " AND GCODE = " + id);

                foreach (var row in jvCodes)
                {
                    if (row.Code <= 0) continue;

                    using (var cmd = new SqlCommand("SP_DeletePayRec", conn, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@JVcode", row.Code));
                        await cmd.ExecuteNonQueryAsync();
                    }

                    using (var cmd = new SqlCommand("SP_DeleteJV", conn, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@code", row.Code));
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                trans.Commit();
                return (true, "Payment /Reciept Record Deleted.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return (false, ex.Message);
            }
        }

        public async Task<PaymentPageDataDto> GetPageDataAsync(string companyId)
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

            long mCurId = await GetCurrencyIdAsync("MCurID", companyId);
            long sCurId = await GetCurrencyIdAsync("SCurID", companyId);

            return new PaymentPageDataDto
            {
                CurrencyList = masterData
                    .Where(x => string.Equals(x.Type, "Currency", StringComparison.OrdinalIgnoreCase))
                    .ToList(),
                DefaultCurrencyCode = defaultCurrencyCode,
                MBaseRate = mBaseRate,
                SBaseRate = sBaseRate,
                MCurId = mCurId,
                SCurId = sCurId
            };
        }

        public async Task<List<PaymentJvListItemDto>> GetJvEntriesAsync(string? transDate, string companyId)
        {
            // MVC: EXEC [GetJVPaymentListUsingPageNo] @PageNo = '1'
            string sql = " EXEC [GetJVPaymentListUsingPageNo] @PageNo = '1' ";
            var rows = (await _sqlDataAccess.SelectRawSql<PaymentJvListRow>(sql, companyId)).ToList();

            if (!string.IsNullOrWhiteSpace(transDate))
            {
                if (!DateTime.TryParse(transDate, out DateTime parsedDate))
                    throw new ArgumentException("Invalid date format");

                rows = rows.Where(x => x.Date.Date == parsedDate.Date).ToList();
            }

            return rows.Select(r => new PaymentJvListItemDto
            {
                GCODE = r.GCODE,
                code = r.code,
                Date = r.Date,
                Vtype = r.Vtype,
                DAccount = r.DAccount,
                CAccount = r.CAccount,
                mainacc = r.mainacc,
                trnacc = r.trnacc,
                Amount = r.Amount,
                Symbol = r.Symbol,
                MasterCurrencyAmount = r.MasterCurrencyAmount,
                StockCurrencyAmount = r.StockCurrencyAmount,
                CurCode = r.CurCode,
                disc = r.disc,
                MasterCurrencyRate = r.MasterCurrencyRate,
                StockCurrencyRate = r.StockCurrencyRate
            }).ToList();
        }

        public async Task<List<PaymentPendingInvDto>> GetPendingInvoicesAsync(PaymentPendingInvRequest request, string companyId)
        {
            long mCurId = await GetCurrencyIdAsync("MCurID", companyId);
            long sCurId = await GetCurrencyIdAsync("SCurID", companyId);
            string mode = (request.Mode ?? "Invoice").Trim();
            string trnType = request.TrnType ?? "";
            string party = request.Party ?? "0";

            List<PaymentPendingInvRow> rows;

            if (string.Equals(mode, "Adat", StringComparison.OrdinalIgnoreCase))
            {
                // MVC GetAdatList: both currencies + ADATCODE
                string whereClause =
                    "AND TSP.CurCode IN (''" + sCurId + "'',''" + mCurId + "'') AND TSP.ADATCODE IN(" + party + ")";
                string sql =
                    "Exec [GetAdatandBrokeragePaymentInvList] @ExtrnType ='" + EscapeSql(trnType) +
                    "',@TrnType = 'OnAdat',@Where = '" + whereClause +
                    "', @CurCode = '" + request.Currency +
                    "',@SCurCode = '" + sCurId +
                    "',@MCurCode = '" + mCurId +
                    "',@CurMasterOperator = '*',@MCurRate = '1'";
                rows = (await _sqlDataAccess.SelectRawSql<PaymentPendingInvRow>(sql, companyId)).ToList();
            }
            else if (string.Equals(mode, "Brokerage", StringComparison.OrdinalIgnoreCase))
            {
                // MVC GetBrokerageList: unquoted IN list + BrokerCode
                string whereClause =
                    " AND tsp.CurCode IN (" + sCurId + "," + mCurId + ") AND tsp.BrokerCode IN(" + party + ")";
                string sql =
                    "Exec [GetAdatandBrokeragePaymentInvList]  @ExtrnType = '" + EscapeSql(trnType) +
                    "',@TrnType = 'OnBrokerage',@Where = '" + whereClause +
                    "', @CurCode = '" + request.Currency +
                    "',@SCurCode = '" + sCurId +
                    "',@MCurCode = '" + mCurId +
                    "',@CurMasterOperator = '*',@MCurRate = '1'";
                rows = (await _sqlDataAccess.SelectRawSql<PaymentPendingInvRow>(sql, companyId)).ToList();
            }
            else
            {
                // MVC get-inv: GetPendingPaymentInvList
                string where;
                if (request.Currency == sCurId)
                    where = " AND TSP.CurCode IN (" + sCurId + ")";
                else
                    where = " AND TSP.CurCode IN (''" + sCurId + "'',''" + mCurId + "'')";

                string sql =
                    " EXEC [GetPendingPaymentInvList] @TrnType = '" + EscapeSql(trnType) +
                    "' , @Party = '" + EscapeSql(party) + "',@where='" + where + "' ";
                rows = (await _sqlDataAccess.SelectRawSql<PaymentPendingInvRow>(sql, companyId)).ToList();
            }

            return rows.Select(r => new PaymentPendingInvDto
            {
                Date = r.Date,
                InvNo = r.InvNo,
                InvCode = r.InvCode,
                DueDate = r.DueDate,
                BillNo = r.BillNo,
                TotCts = r.TotCts,
                Symbol = r.Symbol,
                SCurAmt = r.SCurAmt,
                BaseRate = r.BaseRate,
                MCurAmt = r.MCurAmt,
                AdUSDAmount = r.AdUSDAmount,
                PayINRRate = r.PayINRRate,
                AdINRCurAmt = r.AdINRCurAmt,
                RemUSDAmount = r.RemUSDAmount,
                RemINRCurAmt = r.RemINRCurAmt,
                TrnType = r.TrnType
            }).ToList();
        }

        public async Task<(bool Success, string Message)> AddAsync(PaymentSaveRequest request, string userId, string companyId)
        {
            string connectionString = await _sqlDataAccess.GetClientConnectionString(companyId);
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();
            try
            {
                bool isJV = request.isJVMode;
                string discType = request.PayType ?? "Payment";
                var lines = request.Lines ?? new List<PaymentLineDto>();
                int usercode = int.TryParse(userId, out var u) ? u : 0;

                string mOp = request.MOp ?? "*";
                string sOp = request.SOp ?? "*";

                if (request.Paid > 0)
                {
                    string gcodeStr = await ExecuteCommonDataScalarAsync(conn, trans, "Gcode", "");
                    int maxGcode = (int.TryParse(gcodeStr, out var g) ? g : 0) + 1;

                    string vNo = await ExecuteCommonDataScalarAsync(conn, trans, "VouNo", "And vtype like '%PAY'");
                    string vouNo = ((int.TryParse(vNo, out var vn) ? vn : 0) + 1).ToString();

                    var (mCurNetAmt, sCurNetAmt) = GetConvertAmount(
                        request.CurCode, request.Paid, request.BaseRate, mOp, request.SRate, sOp);

                    string vType = isJV ? "JVPAY" : "IPAY";

                    await InsertJvAsync(conn, trans,
                        request.TransDate, request.TransDate,
                        request.trnacc, request.mainacc,
                        Math.Abs(request.Paid), request.CurCode.ToString(),
                        vType, 0, request.Desc ?? "",
                        Math.Round(request.BaseRate, 4), Math.Round(mCurNetAmt, 4),
                        Math.Round(request.SRate, 4), Math.Round(sCurNetAmt, 4),
                        maxGcode, usercode, userId, companyId, "insert");

                    if (!isJV)
                    {
                        string jvCodeStr = await ExecuteCommonDataScalarAsync(conn, trans, "JVCode",
                            " and Gcode ='" + maxGcode + "' and dacccode='" + request.trnacc + "'");
                        long jvCode = long.TryParse(jvCodeStr, out var jc) ? jc : 0;

                        foreach (var payment in lines)
                        {
                            if (payment.AdSCurAmt > 0 && request.SRate == 1)
                            {
                                await InsertPayRecAsync(conn, trans, request.TransDate,
                                    Convert.ToInt64(payment.InvCode), jvCode,
                                    Math.Abs(payment.AdSCurAmt), request.CurCode.ToString(),
                                    Math.Round(request.BaseRate, 4), discType,
                                    Math.Round(request.BaseRate, 4), Math.Round(payment.AdMCurAmt, 4),
                                    Math.Round(request.SRate, 4), Math.Round(payment.AdSCurAmt, 4),
                                    usercode, userId, companyId);
                            }
                            else if (payment.AdMCurAmt > 0 && request.BaseRate == 1)
                            {
                                await InsertPayRecAsync(conn, trans, request.TransDate,
                                    Convert.ToInt64(payment.InvCode), jvCode,
                                    Math.Abs(payment.AdMCurAmt), request.CurCode.ToString(),
                                    Math.Round(request.BaseRate, 4), discType,
                                    Math.Round(request.BaseRate, 4), Math.Round(payment.AdMCurAmt, 4),
                                    Math.Round(request.SRate, 4), Math.Round(payment.AdSCurAmt, 4),
                                    usercode, userId, companyId);
                            }
                        }
                    }

                    _ = vouNo;
                }

                if (request.Rec > 0)
                {
                    string gcodeStr = await ExecuteCommonDataScalarAsync(conn, trans, "Gcode", "");
                    int maxGcode = (int.TryParse(gcodeStr, out var g) ? g : 0) + 1;

                    string vNo = await ExecuteCommonDataScalarAsync(conn, trans, "VouNo", "And vtype like '%REC'");
                    string vouNo = ((int.TryParse(vNo, out var vn) ? vn : 0) + 1).ToString();

                    var (mCurNetAmt, sCurNetAmt) = GetConvertAmount(
                        request.CurCode, request.Rec, request.BaseRate, mOp, request.SRate, sOp);

                    string vType = isJV ? "JVREC" : "IREC";

                    await InsertJvAsync(conn, trans,
                        request.TransDate, request.TransDate,
                        request.mainacc, request.trnacc,
                        Math.Abs(request.Rec), request.CurCode.ToString(),
                        vType, 0, request.Desc ?? "",
                        Math.Round(request.BaseRate, 4), Math.Round(mCurNetAmt, 4),
                        Math.Round(request.SRate, 4), Math.Round(sCurNetAmt, 4),
                        maxGcode, usercode, userId, companyId, "insert");

                    if (!isJV)
                    {
                        string jvCodeStr = await ExecuteCommonDataScalarAsync(conn, trans, "JVCode",
                            " and Gcode ='" + maxGcode + "' and cacccode='" + request.trnacc + "'");
                        long jvCode = long.TryParse(jvCodeStr, out var jc) ? jc : 0;

                        foreach (var payment in lines)
                        {
                            if (payment.AdMCurAmt > 0 && request.BaseRate == 1)
                            {
                                await InsertPayRecAsync(conn, trans, request.TransDate,
                                    Convert.ToInt64(payment.InvCode), jvCode,
                                    Math.Abs(payment.AdMCurAmt), request.CurCode.ToString(),
                                    Math.Round(request.BaseRate, 4), discType,
                                    Math.Round(request.BaseRate, 4), Math.Round(payment.AdMCurAmt, 4),
                                    Math.Round(request.SRate, 4), Math.Round(payment.AdSCurAmt, 4),
                                    usercode, userId, companyId);
                            }
                            else if (payment.AdSCurAmt > 0 && request.SRate == 1)
                            {
                                await InsertPayRecAsync(conn, trans, request.TransDate,
                                    Convert.ToInt64(payment.InvCode), jvCode,
                                    Math.Abs(payment.AdSCurAmt), request.CurCode.ToString(),
                                    Math.Round(request.BaseRate, 4), discType,
                                    Math.Round(request.BaseRate, 4), Math.Round(payment.AdMCurAmt, 4),
                                    Math.Round(request.SRate, 4), Math.Round(payment.AdSCurAmt, 4),
                                    usercode, userId, companyId);
                            }
                        }
                    }

                    _ = vouNo;
                }

                trans.Commit();
                return (true, "Payment saved successfully.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return (false, ex.Message);
            }
        }

        /// <summary>MVC: POST /data/get-payment-details</summary>
        public async Task<List<PaymentPendingInvDto>> GetEditDetailsAsync(string gcode, string adjustmentType, string companyId)
        {
            string adjType = string.IsNullOrWhiteSpace(adjustmentType) ? "Payment" : adjustmentType;
            string sqlHeader = $" EXEC [GetPaymentList] @Where = ' And GCODE = {EscapeSql(gcode)}', @PageNo = '1'";
            var header = (await _sqlDataAccess.SelectRawSql<PaymentListRow>(sqlHeader, companyId)).FirstOrDefault();
            if (header == null)
                throw new InvalidOperationException("Payment record not found.");

            string trnType = string.Equals(header.Vtype, "IPAY", StringComparison.OrdinalIgnoreCase) ? "PUR" : "SALE";
            string party = string.Equals(header.Vtype, "IPAY", StringComparison.OrdinalIgnoreCase)
                ? header.mainacc.ToString()
                : header.trnacc.ToString();

            long mCurId = await GetCurrencyIdAsync("MCurID", companyId);
            long sCurId = await GetCurrencyIdAsync("SCurID", companyId);
            if (sCurId <= 0) sCurId = 7;
            if (mCurId <= 0) mCurId = 1;

            int headerCurCode = await ResolveCurCodeAsync(header.CurCode, header.Symbol, companyId);
            float masterRate = header.MasterCurrencyRate > 0 ? header.MasterCurrencyRate : 1f;
            if (header.MasterCurrencyRate <= 0 && headerCurCode > 0)
                masterRate = await GetCurrencyRateAsync("McurBaseRate", headerCurCode.ToString(), companyId);
            if (masterRate <= 0) masterRate = 1f;

            string mOp = await _sqlDataAccess.SelectDataFromSPSingle<string>(
                "GetCurrencyDetail",
                new Dictionary<string, object>
                {
                    { "@Type", "MCurOperator" },
                    { "@Where", " AND ID=" + headerCurCode }
                },
                companyId) ?? "*";
            if (string.IsNullOrEmpty(mOp)) mOp = "*";

            var finalResult = new List<PaymentPendingInvDto>();

            var existing = (await _sqlDataAccess.SelectDataFromSP<PaymentPendingInvRow>(
                "GetPaymentDetails_Edit_Dynamic",
                new Dictionary<string, object>
                {
                    { "@GCode", gcode },
                    { "@AdjustmentType", adjType }
                },
                companyId)).ToList();

            foreach (var allocated in existing)
            {
                finalResult.Add(MapPending(allocated));
            }

            if (string.Equals(adjType, "Adat", StringComparison.OrdinalIgnoreCase))
            {
                string whereClause = " AND tsp.CurCode IN (" + sCurId + "," + mCurId + ") AND tsp.AdatCode = " + party;
                string sql =
                    "Exec [GetAdatandBrokeragePaymentInvList] @ExtrnType ='" + EscapeSql(trnType) +
                    "',@TrnType = 'OnAdat',@Where = '" + whereClause +
                    "', @CurCode = '" + headerCurCode +
                    "',@SCurCode = '" + sCurId +
                    "',@MCurCode = '" + mCurId +
                    "',@CurMasterOperator = '" + EscapeSql(mOp) +
                    "',@MCurRate = '" + masterRate + "'";
                var pending = (await _sqlDataAccess.SelectRawSql<PaymentPendingInvRow>(sql, companyId)).ToList();
                foreach (var p in pending)
                {
                    if (!finalResult.Any(x => SameInvCode(x.InvCode, p.InvCode)))
                        finalResult.Add(MapPending(p));
                }
            }
            else if (string.Equals(adjType, "Brokerage", StringComparison.OrdinalIgnoreCase))
            {
                string whereClause = " AND tsp.CurCode IN (" + sCurId + "," + mCurId + ") AND tsp.BrokerCode = " + party;
                string sql =
                    "Exec [GetAdatandBrokeragePaymentInvList]  @ExtrnType = '" + EscapeSql(trnType) +
                    "',@TrnType = 'OnBrokerage',@Where = '" + whereClause +
                    "', @CurCode = '" + headerCurCode +
                    "',@SCurCode = '" + sCurId +
                    "',@MCurCode = '" + mCurId +
                    "',@CurMasterOperator = '" + EscapeSql(mOp) +
                    "',@MCurRate = '" + masterRate + "'";
                var pending = (await _sqlDataAccess.SelectRawSql<PaymentPendingInvRow>(sql, companyId)).ToList();
                foreach (var p in pending)
                {
                    if (!finalResult.Any(x => SameInvCode(x.InvCode, p.InvCode)))
                        finalResult.Add(MapPending(p));
                }
            }
            else
            {
                string curFilter = " AND TSP.CurCode IN (''" + sCurId + "'',''" + mCurId + "'')";
                string sql =
                    " EXEC [GetPendingPaymentInvList] @TrnType = '" + EscapeSql(trnType) +
                    "' , @Party = '" + EscapeSql(party) + "',@where='" + curFilter + "' ";
                var pending = (await _sqlDataAccess.SelectRawSql<PaymentPendingInvRow>(sql, companyId)).ToList();
                foreach (var p in pending)
                {
                    if (!finalResult.Any(x => SameInvCode(x.InvCode, p.InvCode)))
                        finalResult.Add(MapPending(p));
                }
            }

            return finalResult;
        }

        /// <summary>MVC: POST /payments/edit-payment</summary>
        public async Task<(bool Success, string Message)> EditAsync(PaymentSaveRequest request, string userId, string companyId)
        {
            if (request.adjusdTotalAmt == 0 && request.adjinramount == 0)
                return (false, "Please specified AdjUsdAmount Or InrAmount");

            string connectionString = await _sqlDataAccess.GetClientConnectionString(companyId);
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();
            try
            {
                string discType = request.PayType ?? "Payment";
                var lines = request.Lines ?? new List<PaymentLineDto>();
                int usercode = int.TryParse(userId, out var u) ? u : 0;
                int gCode = int.TryParse(request.GCODE, out var gc) ? gc : 0;
                if (gCode <= 0)
                    return (false, "Invalid GCODE.");

                string mOp = request.MOp ?? "*";
                string sOp = request.SOp ?? "*";

                if (request.Paid > 0)
                {
                    string vNo = await ExecuteCommonDataScalarAsync(conn, trans, "VouNo", "And vtype like '%PAY'");
                    string vouNo = ((int.TryParse(vNo, out var vn) ? vn : 0) + 1).ToString();

                    var (mCurNetAmt, sCurNetAmt) = GetConvertAmount(
                        request.CurCode, request.Paid, request.BaseRate, mOp, request.SRate, sOp);

                    await InsertJvAsync(conn, trans,
                        request.TransDate, request.TransDate,
                        request.trnacc, request.mainacc,
                        Math.Abs(request.Paid), request.CurCode.ToString(),
                        "IPAY", 0, request.Desc ?? "",
                        Math.Round(request.BaseRate, 4), Math.Round(mCurNetAmt, 4),
                        Math.Round(request.SRate, 4), Math.Round(sCurNetAmt, 4),
                        gCode, usercode, userId, companyId, "update");

                    string jvCodeStr = await ExecuteCommonDataScalarAsync(conn, trans, "JVCode",
                        " and Gcode ='" + gCode + "' and dacccode='" + request.trnacc + "'");
                    long jvCode = long.TryParse(jvCodeStr, out var jc) ? jc : 0;

                    using (var cmd = new SqlCommand("SP_DeletePayRec", conn, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@JVcode", jvCode));
                        await cmd.ExecuteNonQueryAsync();
                    }

                    foreach (var payment in lines)
                    {
                        if (payment.AdSCurAmt > 0 && request.SRate == 1)
                        {
                            await InsertPayRecAsync(conn, trans, request.TransDate,
                                Convert.ToInt64(payment.InvCode), jvCode,
                                Math.Abs(payment.AdSCurAmt), request.CurCode.ToString(),
                                Math.Round(request.BaseRate, 4), discType,
                                Math.Round(request.BaseRate, 4), Math.Round(payment.AdMCurAmt, 4),
                                Math.Round(request.SRate, 4), Math.Round(payment.AdSCurAmt, 4),
                                usercode, userId, companyId);
                        }
                        else if (payment.AdMCurAmt > 0 && request.BaseRate == 1)
                        {
                            await InsertPayRecAsync(conn, trans, request.TransDate,
                                Convert.ToInt64(payment.InvCode), jvCode,
                                Math.Abs(payment.AdMCurAmt), request.CurCode.ToString(),
                                Math.Round(request.BaseRate, 4), discType,
                                Math.Round(request.BaseRate, 4), Math.Round(payment.AdMCurAmt, 4),
                                Math.Round(request.SRate, 4), Math.Round(payment.AdSCurAmt, 4),
                                usercode, userId, companyId);
                        }
                    }

                    _ = vouNo;
                }

                if (request.Rec > 0)
                {
                    string vNo = await ExecuteCommonDataScalarAsync(conn, trans, "VouNo", "And vtype like '%REC'");
                    string vouNo = ((int.TryParse(vNo, out var vn) ? vn : 0) + 1).ToString();

                    var (mCurNetAmt, sCurNetAmt) = GetConvertAmount(
                        request.CurCode, request.Rec, request.BaseRate, mOp, request.SRate, sOp);

                    await InsertJvAsync(conn, trans,
                        request.TransDate, request.TransDate,
                        request.mainacc, request.trnacc,
                        Math.Abs(request.Rec), request.CurCode.ToString(),
                        "IREC", 0, request.Desc ?? "",
                        Math.Round(request.BaseRate, 4), Math.Round(mCurNetAmt, 4),
                        Math.Round(request.SRate, 4), Math.Round(sCurNetAmt, 4),
                        gCode, usercode, userId, companyId, "update");

                    string jvCodeStr = await ExecuteCommonDataScalarAsync(conn, trans, "JVCode",
                        " and Gcode ='" + gCode + "' and cacccode='" + request.trnacc + "'");
                    long jvCode = long.TryParse(jvCodeStr, out var jc) ? jc : 0;

                    using (var cmd = new SqlCommand("SP_DeletePayRec", conn, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@JVcode", jvCode));
                        await cmd.ExecuteNonQueryAsync();
                    }

                    foreach (var payment in lines)
                    {
                        if (payment.AdMCurAmt > 0 && request.BaseRate == 1)
                        {
                            await InsertPayRecAsync(conn, trans, request.TransDate,
                                Convert.ToInt64(payment.InvCode), jvCode,
                                Math.Abs(payment.AdMCurAmt), request.CurCode.ToString(),
                                Math.Round(request.BaseRate, 4), discType,
                                Math.Round(request.BaseRate, 4), Math.Round(payment.AdMCurAmt, 4),
                                Math.Round(request.SRate, 4), Math.Round(payment.AdSCurAmt, 4),
                                usercode, userId, companyId);
                        }
                        else if (payment.AdSCurAmt > 0 && request.SRate == 1)
                        {
                            await InsertPayRecAsync(conn, trans, request.TransDate,
                                Convert.ToInt64(payment.InvCode), jvCode,
                                Math.Abs(payment.AdSCurAmt), request.CurCode.ToString(),
                                Math.Round(request.BaseRate, 4), discType,
                                Math.Round(request.BaseRate, 4), Math.Round(payment.AdMCurAmt, 4),
                                Math.Round(request.SRate, 4), Math.Round(payment.AdSCurAmt, 4),
                                usercode, userId, companyId);
                        }
                    }

                    _ = vouNo;
                }

                trans.Commit();
                return (true, "Payment updated successfully.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> EditJvAsync(PaymentSaveRequest request, string userId, string companyId)
        {
            if (request.mainacc == request.trnacc)
                return (false, "Not Select Same Account.");

            string connectionString = await _sqlDataAccess.GetClientConnectionString(companyId);
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();
            try
            {
                int usercode = int.TryParse(userId, out var u) ? u : 0;
                int gCode = int.TryParse(request.GCODE, out var gc) ? gc : 0;
                if (gCode <= 0)
                    return (false, "Invalid GCODE.");

                string mOp = request.MOp ?? "*";
                string sOp = request.SOp ?? "*";
                string disc = request.disc ?? request.Desc ?? "";

                if (request.Paid > 0)
                {
                    string vNo = await ExecuteCommonDataScalarAsync(conn, trans, "VouNo", "And vtype like '%JVPAY%'");
                    string vouNo = "1";
                    if (!string.IsNullOrEmpty(vNo) && int.TryParse(vNo, out var vn))
                        vouNo = (vn + 1).ToString();

                    var (mCurNetAmt, sCurNetAmt) = GetConvertAmount(
                        request.CurCode, request.Paid, request.BaseRate, mOp, request.SRate, sOp);

                    using (var cmd = new SqlCommand("SP_DeleteIncomeExpense", conn, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@GCode", gCode));
                        await cmd.ExecuteNonQueryAsync();
                    }

                    await InsertJvAsync(conn, trans,
                        request.TransDate, request.TransDate,
                        request.trnacc, request.mainacc,
                        Math.Abs(request.Paid), request.CurCode.ToString(),
                        "JVPAY", 0, disc,
                        Math.Round(request.BaseRate, 4), Math.Round(mCurNetAmt, 4),
                        Math.Round(request.SRate, 4), Math.Round(sCurNetAmt, 4),
                        gCode, usercode, userId, companyId, "insert");

                    _ = vouNo;
                }
                else if (request.Rec > 0)
                {
                    string vNo = await ExecuteCommonDataScalarAsync(conn, trans, "VouNo", "And vtype like '%JVREC%'");
                    string vouNo = "1";
                    if (!string.IsNullOrEmpty(vNo) && int.TryParse(vNo, out var vn))
                        vouNo = (vn + 1).ToString();

                    var (mCurNetAmt, sCurNetAmt) = GetConvertAmount(
                        request.CurCode, request.Rec, request.BaseRate, mOp, request.SRate, sOp);

                    using (var cmd = new SqlCommand("SP_DeleteIncomeExpense", conn, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@GCode", gCode));
                        await cmd.ExecuteNonQueryAsync();
                    }

                    await InsertJvAsync(conn, trans,
                        request.TransDate, request.TransDate,
                        request.mainacc, request.trnacc,
                        Math.Abs(request.Rec), request.CurCode.ToString(),
                        "JVREC", 0, disc,
                        Math.Round(request.BaseRate, 4), Math.Round(mCurNetAmt, 4),
                        Math.Round(request.SRate, 4), Math.Round(sCurNetAmt, 4),
                        gCode, usercode, userId, companyId, "insert");

                    _ = vouNo;
                }

                trans.Commit();
                return (true, "JV Updated successfully.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteJvAsync(int id, string userId, string companyId)
        {
            string connectionString = await _sqlDataAccess.GetClientConnectionString(companyId);
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();
            try
            {
                var jvCodes = await GetJvCodesAsync(conn, trans, " AND GCODE = " + id);

                foreach (var row in jvCodes)
                {
                    if (row.Code <= 0) continue;

                    // MVC DeleteJVEntries: SP_DeleteJV only (no DeletePayRec)
                    using var cmd = new SqlCommand("SP_DeleteJV", conn, trans);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@code", row.Code));
                    await cmd.ExecuteNonQueryAsync();
                }

                trans.Commit();
                return (true, "JV Entry Deleted Successfully.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// MVC EditPayment: Selected = Name == Symbol OR (ID == CurCode AND CurCode != 0).
        /// GetPaymentList often returns CurCode = 0; resolve ID from currency master by Symbol.
        /// </summary>
        private async Task<int> ResolveCurCodeAsync(double curCode, string? symbol, string companyId)
        {
            int code = curCode > 0 ? (int)Math.Round(curCode) : 0;
            if (code != 0) return code;
            if (string.IsNullOrWhiteSpace(symbol)) return 0;

            var masterData = (await _sqlDataAccess.SelectDataFromSP<BasicDataDto>(
                "AllMasterDataForAccGrpMaster", new Dictionary<string, object>(), companyId)).ToList();

            var match = masterData.FirstOrDefault(x =>
                string.Equals(x.Type, "Currency", StringComparison.OrdinalIgnoreCase) &&
                (string.Equals((x.Name ?? "").Trim(), symbol.Trim(), StringComparison.OrdinalIgnoreCase) ||
                 string.Equals((x.TextName ?? "").Trim(), symbol.Trim(), StringComparison.OrdinalIgnoreCase)));

            if (match != null && int.TryParse(match.ID, out var id) && id != 0)
                return id;

            return 0;
        }

        private static bool SameInvCode(string? a, string? b)
            => string.Equals((a ?? "").Trim(), (b ?? "").Trim(), StringComparison.OrdinalIgnoreCase);

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

        private async Task<long> GetCurrencyIdAsync(string type, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", type },
                { "@Where", "" }
            };
            var result = await _sqlDataAccess.SelectDataFromSPSingle<string>("GetCurrencyDetail", param, companyId);
            return long.TryParse(result, out var id) ? id : 0L;
        }

        private static async Task<List<JvCodeRow>> GetJvCodesAsync(SqlConnection conn, SqlTransaction trans, string where)
        {
            var jvCodes = new List<JvCodeRow>();
            using var cmd = new SqlCommand("GetJvCode", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@where", where));
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                jvCodes.Add(new JvCodeRow
                {
                    Code = reader["Code"] != DBNull.Value ? Convert.ToInt32(reader["Code"]) : 0
                });
            }
            return jvCodes;
        }

        private static async Task InsertJvAsync(
            SqlConnection conn, SqlTransaction trans,
            DateTime jvDate, DateTime dueDate, long debitAcc, long creditAcc,
            float amount, string curCode, string vType, int vCode, string disc,
            double mCurBaseRate, double mCurAmt, double sCurBaseRate, double sCurAmt,
            double gCode, int usercode, string userId, string companyId, string actionType = "insert")
        {
            using var cmd = new SqlCommand("SP_insertjv", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@dacccode", debitAcc));
            cmd.Parameters.Add(new SqlParameter("@cacccode", creditAcc));
            cmd.Parameters.Add(new SqlParameter("@amount", amount));
            cmd.Parameters.Add(new SqlParameter("@JVdate", jvDate));
            cmd.Parameters.Add(new SqlParameter("@duedate", dueDate));
            cmd.Parameters.Add(new SqlParameter("@curcode", curCode));
            cmd.Parameters.Add(new SqlParameter("@vtype", vType));
            cmd.Parameters.Add(new SqlParameter("@vcode", vCode));
            cmd.Parameters.Add(new SqlParameter("@disc", disc ?? ""));
            cmd.Parameters.Add(new SqlParameter("@Adjustvtype", (object)0));
            cmd.Parameters.Add(new SqlParameter("@Adjustvcode", (object)0));
            cmd.Parameters.Add(new SqlParameter("@BaseRate", mCurBaseRate));
            cmd.Parameters.Add(new SqlParameter("@MasterCurrencyRate", mCurBaseRate));
            cmd.Parameters.Add(new SqlParameter("@MasterCurrencyAmount", mCurAmt));
            cmd.Parameters.Add(new SqlParameter("@StockCurrencyRate", sCurBaseRate));
            cmd.Parameters.Add(new SqlParameter("@StockCurrencyAmount", sCurAmt));
            cmd.Parameters.Add(new SqlParameter("@GCode", gCode));
            cmd.Parameters.Add(new SqlParameter("@UserCode", usercode));
            cmd.Parameters.Add(new SqlParameter("@CreatedBy", userId ?? ""));
            cmd.Parameters.Add(new SqlParameter("@Companyid", companyId));
            cmd.Parameters.Add(new SqlParameter("@AgainstAccCode", (object)0));
            cmd.Parameters.Add(new SqlParameter("@ActionType", actionType ?? "insert"));
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task InsertPayRecAsync(
            SqlConnection conn, SqlTransaction trans,
            DateTime date, long invCode, long jvCode,
            double amount, string curCode, double baseRate, string disc,
            double masterCurrencyRate, double masterCurrencyAmount,
            double stockCurrencyRate, double stockCurrencyAmount,
            int usercode, string userId, string companyId)
        {
            using var cmd = new SqlCommand("SP_InsertPayRec", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@Date", date));
            cmd.Parameters.Add(new SqlParameter("@InvCode", invCode));
            cmd.Parameters.Add(new SqlParameter("@JVCode", jvCode));
            cmd.Parameters.Add(new SqlParameter("@PostCheque", 0L));
            cmd.Parameters.Add(new SqlParameter("@Amount", amount));
            cmd.Parameters.Add(new SqlParameter("@CurCode", curCode));
            cmd.Parameters.Add(new SqlParameter("@BaseRate", baseRate));
            cmd.Parameters.Add(new SqlParameter("@Disc", disc ?? ""));
            cmd.Parameters.Add(new SqlParameter("@MasterCurrencyRate", masterCurrencyRate));
            cmd.Parameters.Add(new SqlParameter("@MasterCurrencyAmount", masterCurrencyAmount));
            cmd.Parameters.Add(new SqlParameter("@StockCurrencyRate", stockCurrencyRate));
            cmd.Parameters.Add(new SqlParameter("@StockCurrencyAmount", stockCurrencyAmount));
            cmd.Parameters.Add(new SqlParameter("@UserCode", usercode));
            cmd.Parameters.Add(new SqlParameter("@CreatedBy", userId ?? ""));
            cmd.Parameters.Add(new SqlParameter("@Companyid", companyId));
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task<string> ExecuteCommonDataScalarAsync(SqlConnection conn, SqlTransaction trans, string type, string where = "")
        {
            using var cmd = new SqlCommand("GetAllCommonData", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@Type", type));
            cmd.Parameters.Add(new SqlParameter("@Where", where ?? ""));
            return (await cmd.ExecuteScalarAsync())?.ToString() ?? string.Empty;
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

        private static string EscapeSql(string value)
            => (value ?? string.Empty).Replace("'", "''");

        private static PaymentPendingInvDto MapPending(PaymentPendingInvRow r) => new()
        {
            Date = r.Date,
            InvNo = r.InvNo,
            InvCode = r.InvCode,
            DueDate = r.DueDate,
            BillNo = r.BillNo,
            TotCts = r.TotCts,
            Symbol = r.Symbol,
            SCurAmt = r.SCurAmt,
            BaseRate = r.BaseRate,
            MCurAmt = r.MCurAmt,
            AdUSDAmount = r.AdUSDAmount,
            PayINRRate = r.PayINRRate,
            AdINRCurAmt = r.AdINRCurAmt,
            RemUSDAmount = r.RemUSDAmount,
            RemINRCurAmt = r.RemINRCurAmt,
            TrnType = r.TrnType
        };

        private static PaymentListItemDto MapListItem(PaymentListRow row) => new()
        {
            GCODE = row.GCODE,
            InvNo = row.InvNo,
            BillNo = row.BillNo,
            code = row.code,
            Date = row.Date,
            DueDate = row.DueDate,
            Vtype = row.Vtype,
            Account = row.Account,
            Party = row.Party,
            CD = row.CD,
            Amount = row.Amount,
            Symbol = row.Symbol,
            MasterCurrencyAmount = row.MasterCurrencyAmount,
            StockCurrencyAmount = row.StockCurrencyAmount,
            disc = row.disc,
            MasterCurrencyRate = row.MasterCurrencyRate,
            StockCurrencyRate = row.StockCurrencyRate,
            CurCode = (int)Math.Round(row.CurCode),
            mainacc = row.mainacc,
            trnacc = row.trnacc
        };
    }
}
