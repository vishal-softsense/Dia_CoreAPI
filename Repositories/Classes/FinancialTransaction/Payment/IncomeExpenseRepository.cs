using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
    public class IncomeExpenseRepository : IIncomeExpenseRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;
        private const string FormName = "IncomeExpense";

        public IncomeExpenseRepository(ISqlDataAccess sqlDataAccess)
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

        /// <summary>MVC: GetIncomeExpenseList / IncomeExpensesFilters</summary>
        public async Task<List<IncomeExpenseListItemDto>> GetListAsync(string? transDate, string companyId)
        {
            string sql = " EXEC [GetIncome_ExpenceListUsingPageNo] @PageNo = '1' ";
            var rows = (await _sqlDataAccess.SelectRawSql<IncomeExpenseListRow>(sql, companyId)).ToList();

            if (!string.IsNullOrWhiteSpace(transDate))
            {
                if (!DateTime.TryParse(transDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsedDate)
                    && !DateTime.TryParse(transDate, out parsedDate))
                {
                    throw new InvalidOperationException("Invalid date format");
                }

                rows = rows.Where(x => x.Date.Date == parsedDate.Date).ToList();
            }

            return rows.Select(MapListItem).ToList();
        }

        /// <summary>MVC: AddIncomeExpense currency dropdowns</summary>
        public async Task<IncomeExpensePageDataDto> GetPageDataAsync(string companyId)
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

            return new IncomeExpensePageDataDto
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

        /// <summary>MVC: GetIncomeExpenseLists(" AND GCODE = " + id, 1)</summary>
        public async Task<IncomeExpenseDetailDto?> GetDetailAsync(int id, string companyId)
        {
            string sql = " EXEC [GetIncome_ExpenceListUsingPageNo] @Where = ' AND GCODE = " + id + "', @PageNo = '1'";
            var row = (await _sqlDataAccess.SelectRawSql<IncomeExpenseDetailRow>(sql, companyId)).FirstOrDefault();
            if (row == null) return null;

            return new IncomeExpenseDetailDto
            {
                GCODE = row.GCODE,
                code = row.code,
                Date = row.Date,
                Vtype = row.Vtype,
                DAccount = row.DAccount,
                CAccount = row.CAccount,
                mainacc = row.mainacc,
                trnacc = row.trnacc,
                Amount = row.Amount,
                Symbol = row.Symbol,
                MasterCurrencyAmount = row.MasterCurrencyAmount,
                StockCurrencyAmount = row.StockCurrencyAmount,
                CurCode = row.CurCode,
                disc = row.disc,
                MasterCurrencyRate = row.MasterCurrencyRate,
                StockCurrencyRate = row.StockCurrencyRate
            };
        }

        /// <summary>MVC: POST /Payment/save-incomeexpense</summary>
        public async Task<(bool Success, string Message)> AddAsync(IncomeExpenseSaveRequest request, string userId, string companyId)
        {
            string connectionString = await _sqlDataAccess.GetClientConnectionString(companyId);
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();
            try
            {
                int usercode = int.TryParse(userId, out var u) ? u : 0;

                if (request.mainacc == request.trnacc)
                {
                    try { trans.Rollback(); } catch { /* ignore */ }
                    return (false, "Not Select Same Account.");
                }

                long mCurId = await GetCurrencyIdAsync("MCurID", companyId);
                long sCurId = await GetCurrencyIdAsync("SCurID", companyId);
                string mCurOperator = string.IsNullOrWhiteSpace(request.MOp) ? "*" : request.MOp;
                string sCurOperator = string.IsNullOrWhiteSpace(request.SOp) ? "*" : request.SOp;

                if (request.Paid > 0)
                {
                    string gcodeStr = await ExecuteCommonDataScalarAsync(conn, trans, "Gcode", "");
                    int maxGcode = (int.TryParse(gcodeStr, out var g) ? g : 0) + 1;

                    // MVC: GetVouNo("And vtype like ''%JV%''") even for PAY insert
                    string vNo = await ExecuteCommonDataScalarAsync(conn, trans, "VouNo", "And vtype like '%JV%'");
                    string vouNo = ((int.TryParse(vNo, out var vn) ? vn : 0) + 1).ToString();

                    var (mCurNetAmt, sCurNetAmt) = GetConvertAmount(
                        request.CurCode, request.Paid, request.BaseRate, mCurOperator, request.SRate, sCurOperator, mCurId, sCurId);

                    // PAY: debit=trnacc, credit=mainacc
                    await InsertJvAsync(
                        conn, trans,
                        request.TransDate, request.TransDate,
                        request.trnacc, request.mainacc,
                        Math.Abs(request.Paid), request.CurCode.ToString(), "PAY", 0,
                        request.disc ?? "",
                        Math.Round(request.BaseRate, 4),
                        Math.Round(mCurNetAmt, 4),
                        Math.Round(request.SRate, 4),
                        Math.Round(sCurNetAmt, 4),
                        maxGcode, usercode, userId, companyId);
                }
                else if (request.Rec > 0)
                {
                    string gcodeStr = await ExecuteCommonDataScalarAsync(conn, trans, "Gcode", "");
                    int maxGcode = (int.TryParse(gcodeStr, out var g) ? g : 0) + 1;

                    string vNo = await ExecuteCommonDataScalarAsync(conn, trans, "VouNo", "And vtype like '%JV%'");
                    string vouNo = ((int.TryParse(vNo, out var vn) ? vn : 0) + 1).ToString();

                    var (mCurNetAmt, sCurNetAmt) = GetConvertAmount(
                        request.CurCode, request.Rec, request.BaseRate, mCurOperator, request.SRate, sCurOperator, mCurId, sCurId);

                    // REC: debit=mainacc, credit=trnacc
                    await InsertJvAsync(
                        conn, trans,
                        request.TransDate, request.TransDate,
                        request.mainacc, request.trnacc,
                        Math.Abs(request.Rec), request.CurCode.ToString(), "REC", 0,
                        request.disc ?? "",
                        Math.Round(request.BaseRate, 4),
                        Math.Round(mCurNetAmt, 4),
                        Math.Round(request.SRate, 4),
                        Math.Round(sCurNetAmt, 4),
                        maxGcode, usercode, userId, companyId);
                }

                trans.Commit();
                return (true, "Income Expense Save successfully.");
            }
            catch (Exception ex)
            {
                // MVC catch: success = true; temp = ex.ToString();
                try { trans.Rollback(); } catch { /* ignore */ }
                return (true, ex.ToString());
            }
        }

        /// <summary>MVC: POST /Payment/EditIncomeExpenseSave</summary>
        public async Task<(bool Success, string Message)> EditAsync(IncomeExpenseSaveRequest request, string userId, string companyId)
        {
            string connectionString = await _sqlDataAccess.GetClientConnectionString(companyId);
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();
            try
            {
                int usercode = int.TryParse(userId, out var u) ? u : 0;

                if (request.mainacc == request.trnacc)
                {
                    try { trans.Rollback(); } catch { /* ignore */ }
                    return (false, "Not Select Same Account.");
                }

                long mCurId = await GetCurrencyIdAsync("MCurID", companyId);
                long sCurId = await GetCurrencyIdAsync("SCurID", companyId);
                string mCurOperator = string.IsNullOrWhiteSpace(request.MOp) ? "*" : request.MOp;
                string sCurOperator = string.IsNullOrWhiteSpace(request.SOp) ? "*" : request.SOp;

                if (request.Paid > 0)
                {
                    int gCode = int.TryParse(request.GCODE, out var gc) ? gc : 0;
                    int maxGcode = gCode;

                    string vNo = await ExecuteCommonDataScalarAsync(conn, trans, "VouNo", "And vtype like '%PAY%'");
                    string vouNo = ((int.TryParse(vNo, out var vn) ? vn : 0) + 1).ToString();

                    var (mCurNetAmt, sCurNetAmt) = GetConvertAmount(
                        request.CurCode, request.Paid, request.BaseRate, mCurOperator, request.SRate, sCurOperator, mCurId, sCurId);

                    await DeleteIncomeExpenseAsync(conn, trans, gCode);

                    await InsertJvAsync(
                        conn, trans,
                        request.TransDate, request.TransDate,
                        request.trnacc, request.mainacc,
                        Math.Abs(request.Paid), request.CurCode.ToString(), "PAY", 0,
                        request.disc ?? "",
                        Math.Round(request.BaseRate, 4),
                        Math.Round(mCurNetAmt, 4),
                        Math.Round(request.SRate, 4),
                        Math.Round(sCurNetAmt, 4),
                        maxGcode, usercode, userId, companyId);
                }
                else if (request.Rec > 0)
                {
                    int gCode = int.TryParse(request.GCODE, out var gc) ? gc : 0;
                    int maxGcode = gCode;

                    string vNo = await ExecuteCommonDataScalarAsync(conn, trans, "VouNo", "And vtype like '%REC%'");
                    string vouNo = ((int.TryParse(vNo, out var vn) ? vn : 0) + 1).ToString();

                    var (mCurNetAmt, sCurNetAmt) = GetConvertAmount(
                        request.CurCode, request.Rec, request.BaseRate, mCurOperator, request.SRate, sCurOperator, mCurId, sCurId);

                    await DeleteIncomeExpenseAsync(conn, trans, gCode);

                    await InsertJvAsync(
                        conn, trans,
                        request.TransDate, request.TransDate,
                        request.mainacc, request.trnacc,
                        Math.Abs(request.Rec), request.CurCode.ToString(), "REC", 0,
                        request.disc ?? "",
                        Math.Round(request.BaseRate, 4),
                        Math.Round(mCurNetAmt, 4),
                        Math.Round(request.SRate, 4),
                        Math.Round(sCurNetAmt, 4),
                        maxGcode, usercode, userId, companyId);
                }

                trans.Commit();
                return (true, "Income Expense Updated successfully.");
            }
            catch (Exception ex)
            {
                // MVC catch: success = true; temp = ex.ToString();
                try { trans.Rollback(); } catch { /* ignore */ }
                return (true, ex.ToString());
            }
        }

        /// <summary>MVC: POST /Payment/delete-incomeexpense-list</summary>
        public async Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId)
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
                return (true, "Income & Expense Record Deleted.");
            }
            catch (Exception ex)
            {
                try { trans.Rollback(); } catch { /* ignore */ }
                return (false, ex.Message);
            }
        }

        private static IncomeExpenseListItemDto MapListItem(IncomeExpenseListRow r) => new()
        {
            GCODE = r.GCODE,
            code = r.code,
            Date = r.Date.ToString("yyyy-MM-ddTHH:mm:ss"),
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
        };

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

        private static async Task<List<IncomeExpenseJvCodeRow>> GetJvCodesAsync(SqlConnection conn, SqlTransaction trans, string where)
        {
            var jvCodes = new List<IncomeExpenseJvCodeRow>();
            using var cmd = new SqlCommand("GetJvCode", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@where", where));
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                jvCodes.Add(new IncomeExpenseJvCodeRow
                {
                    Code = reader["Code"] != DBNull.Value ? Convert.ToInt32(reader["Code"]) : 0
                });
            }
            return jvCodes;
        }

        private static async Task DeleteIncomeExpenseAsync(SqlConnection conn, SqlTransaction trans, int gCode)
        {
            using var cmd = new SqlCommand("SP_DeleteIncomeExpense", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@GCode", gCode));
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task InsertJvAsync(
            SqlConnection conn, SqlTransaction trans,
            DateTime jvDate, DateTime dueDate, long debitAcc, long creditAcc,
            float amount, string curCode, string vType, int vCode, string disc,
            double mCurBaseRate, double mCurAmt, double sCurBaseRate, double sCurAmt,
            double gCode, int usercode, string userId, string companyId)
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
            cmd.Parameters.Add(new SqlParameter("@ActionType", "insert"));
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

        /// <summary>MVC BasicFunctionController.GetConvertAmount</summary>
        private static (double MCurAmt, double SCurAmt) GetConvertAmount(
            double curCode, double amount, double mCurBaseRate, string mCurOperator,
            double sCurBaseRate, string sCurOperator, long mCurId, long sCurId)
        {
            double mCurAmt = 0;
            double sCurAmt = 0;

            if (curCode == mCurId)
            {
                mCurAmt = amount;
                if (sCurOperator == "*")
                    sCurAmt = amount * sCurBaseRate;
                else
                    sCurAmt = sCurBaseRate != 0 ? amount / sCurBaseRate : 0;
            }
            else if (curCode == sCurId)
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

            if (double.IsNaN(mCurAmt) || double.IsInfinity(mCurAmt)) mCurAmt = 0;
            if (double.IsNaN(sCurAmt) || double.IsInfinity(sCurAmt)) sCurAmt = 0;
            return (mCurAmt, sCurAmt);
        }
    }
}
