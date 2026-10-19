using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports;
using Microsoft.Data.SqlClient;

namespace Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Reports
{
    public class SolitarieGradingComparisonReportRepository : ISolitarieGradingComparisonReportRepository
    {
        private readonly ISqlDataAccess _db;

        public SolitarieGradingComparisonReportRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<IEnumerable<BasicDataDto>> GetDropdownsAsync(string companyId)
        {
            return await _db.SelectDataFromSP<BasicDataDto>(
                "AllMasterDataForPurchaseReport",
                new Dictionary<string, object>(),
                companyId);
        }

        public async Task<IEnumerable<GradingComparisonReportRowDto>> GetReportAsync(
            GradingComparisonReportFilterDto filter,
            string companyId)
        {
            var where = BuildGradingComparisonWhere(filter);

            // Match MVC ReportProcessor.GetGradingComparisionReport (EXEC + Dapper-style flexible mapping).
            // Dynamic mapping avoids MapReaderToList picking the first duplicate "rate"/"Back" column (often 0).
            var sql = " EXEC [GetGradingComparision] @Where = '" + where + "' ";
            var rows = await _db.SelectDataAdoDynamic(sql, companyId);
            return rows.Select(MapGradingComparisonRow).ToList();
        }

        private static GradingComparisonReportRowDto MapGradingComparisonRow(dynamic row)
        {
            var dict = row as IDictionary<string, object> ?? new Dictionary<string, object>();

            return new GradingComparisonReportRowDto
            {
                Ref = GetString(dict, "Ref"),
                sr = GetInt(dict, "sr", "Sr"),
                LotNo = GetString(dict, "LotNo", "lotNo"),
                Kapan = GetString(dict, "Kapan"),
                PacketNo = GetString(dict, "PacketNo"),
                ReportDate = GetString(dict, "ReportDate", "GDate"),
                GDate = GetString(dict, "GDate", "ReportDate"),
                Grader = GetString(dict, "Grader"),
                carats = GetBestNumericString(dict, "carats", "Carats"),
                rate = GetBestNumericString(dict, "rate", "Rate"),
                amount = GetBestNumericString(dict, "amount", "Amount"),
                MSP = GetBestNumericString(dict, "MSP", "msp", "MRP"),
                Back = GetBestNumericString(dict, "Back", "back"),
                Shape = GetString(dict, "Shape"),
                Color = GetString(dict, "Color"),
                Clarity = GetString(dict, "Clarity"),
                Proportion = GetString(dict, "Proportion"),
                Polish = GetString(dict, "Polish"),
                Symm = GetString(dict, "Symm", "Symmetry"),
                FLCode = GetString(dict, "FLCode"),
                HACode = GetString(dict, "HACode"),
                Diameter = GetString(dict, "Diameter"),
                Depth = GetString(dict, "Depth"),
                Table = GetString(dict, "Table"),
                CH = GetString(dict, "CH"),
                CA = GetString(dict, "CA"),
                GerdleMin = GetString(dict, "GerdleMin", "GrMin"),
                Girdle = GetString(dict, "Girdle", "GrMax"),
                PA = GetString(dict, "PA"),
                PH = GetString(dict, "PH"),
                Culet = GetString(dict, "Culet"),
                MilkyCode = GetString(dict, "MilkyCode"),
                Brown = GetString(dict, "Brown"),
                LusterCode = GetString(dict, "LusterCode"),
                Natts = GetString(dict, "Natts"),
                TableInclusion = GetString(dict, "TableInclusion"),
                CrownInclusion = GetString(dict, "CrownInclusion"),
                IP = GetString(dict, "IP"),
                CertificateNo = GetString(dict, "CertificateNo", "ReportNo"),
                KeyToSymbols = GetString(dict, "KeyToSymbols"),
                ReportComments = GetString(dict, "ReportComments"),
                LaserInscriptionText = GetString(dict, "LaserInscriptionText"),
                ControlNo = GetString(dict, "ControlNo"),
                SuratRemarks = GetString(dict, "SuratRemarks"),
                IGICut = GetString(dict, "IGICut", "FinalCut"),
                Buth = GetString(dict, "Buth"),
                ClarityStatus = GetString(dict, "ClarityStatus"),
                HRDcut = GetString(dict, "HRDcut"),
                Fees = GetBestNumericString(dict, "Fees", "fees"),
                ColorCarats = GetString(dict, "ColorCarats"),
                ColorColor = GetString(dict, "ColorColor"),
                ClarityColor = GetString(dict, "ClarityColor"),
                PropColor = GetString(dict, "PropColor"),
                PolishColor = GetString(dict, "PolishColor"),
                SymColor = GetString(dict, "SymColor"),
                FLCodeColor = GetString(dict, "FLCodeColor"),
                DepthColor = GetString(dict, "DepthColor"),
                TableColor = GetString(dict, "TableColor"),
                CAColor = GetString(dict, "CAColor"),
                CHColor = GetString(dict, "CHColor"),
                PAColor = GetString(dict, "PAColor"),
                PHColor = GetString(dict, "PHColor")
            };
        }

        private static string? GetString(IDictionary<string, object> dict, params string[] keys)
        {
            foreach (var key in keys)
            {
                var match = dict.Keys.FirstOrDefault(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
                if (match == null) continue;
                var value = dict[match];
                if (value == null || value == DBNull.Value) continue;
                var text = Convert.ToString(value, CultureInfo.InvariantCulture);
                if (!string.IsNullOrWhiteSpace(text))
                    return text;
            }
            return null;
        }

        /// <summary>
        /// Reads rate/Back/amount-style columns, including duplicate names renamed to Rate1/Back1 by SelectDataAdoDynamic.
        /// Prefers the first non-zero numeric value so a leading zero duplicate does not hide the real value.
        /// </summary>
        private static string? GetBestNumericString(IDictionary<string, object> dict, params string[] baseKeys)
        {
            string? fallback = null;

            foreach (var kvp in dict)
            {
                if (!baseKeys.Any(bk => IsColumnMatch(kvp.Key, bk)))
                    continue;
                if (kvp.Value == null || kvp.Value == DBNull.Value)
                    continue;

                var text = Convert.ToString(kvp.Value, CultureInfo.InvariantCulture);
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                fallback ??= text;

                if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var num) && num != 0m)
                    return text;
            }

            return fallback;
        }

        private static bool IsColumnMatch(string columnName, string baseKey)
        {
            if (string.Equals(columnName, baseKey, StringComparison.OrdinalIgnoreCase))
                return true;

            // Duplicate columns from SelectDataAdoDynamic: Rate1, Back2, etc.
            if (columnName.Length <= baseKey.Length)
                return false;

            if (!columnName.StartsWith(baseKey, StringComparison.OrdinalIgnoreCase))
                return false;

            var suffix = columnName.Substring(baseKey.Length);
            return suffix.Length > 0 && suffix.All(char.IsDigit);
        }

        private static int GetInt(IDictionary<string, object> dict, params string[] keys)
        {
            var text = GetString(dict, keys);
            if (int.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                return value;
            return 0;
        }

        public async Task<MakeLabWeightDiffEntryResultDto> MakeLabWeightDiffEntryAsync(
            MakeLabWeightDiffEntryRequestDto request,
            string companyId)
        {
            if (request?.Rows == null || !request.Rows.Any())
            {
                return new MakeLabWeightDiffEntryResultDto
                {
                    Success = false,
                    Message = "No rows selected for Lab Weight Diff entry."
                };
            }

            var connectionString = await _db.GetClientConnectionString(companyId);
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            await using var trans = (SqlTransaction)await conn.BeginTransactionAsync();

            try
            {
                long tempItemCode;
                await using (var cmdTemp = new SqlCommand(
                    "SELECT ItemCode FROM TabItemMaster WHERE LotNo = @LotNo",
                    conn, trans))
                {
                    cmdTemp.Parameters.Add("@LotNo", SqlDbType.NVarChar, 100).Value = "LabWeightDiff";
                    var result = await cmdTemp.ExecuteScalarAsync();
                    tempItemCode = result == null || result == DBNull.Value
                        ? 0
                        : Convert.ToInt64(result, CultureInfo.InvariantCulture);
                }

                if (tempItemCode == 0)
                {
                    await trans.RollbackAsync();
                    return new MakeLabWeightDiffEntryResultDto
                    {
                        Success = false,
                        Message = "LabWeightDiff Doesn't Exist!!!"
                    };
                }

                var entryDate = request.UseLabWeightDate && request.LabWeightDate.HasValue
                    ? request.LabWeightDate.Value.Date
                    : DateTime.Now.Date;

                int insertedCount = 0;

                foreach (var row in request.Rows)
                {
                    if (string.IsNullOrWhiteSpace(row.LotNo))
                        continue;

                    long curItemCode;
                    await using (var cmdItem = new SqlCommand(
                        "SELECT ItemCode FROM TabItemMaster WHERE LotNo = @LotNo",
                        conn, trans))
                    {
                        cmdItem.Parameters.Add("@LotNo", SqlDbType.NVarChar, 100).Value = row.LotNo.Trim();
                        var itemResult = await cmdItem.ExecuteScalarAsync();
                        curItemCode = itemResult == null || itemResult == DBNull.Value
                            ? 0
                            : Convert.ToInt64(itemResult, CultureInfo.InvariantCulture);
                    }

                    if (curItemCode == 0)
                        continue;

                    var oldWeight = Math.Round(row.OldWeight, 3, MidpointRounding.AwayFromZero);
                    var newWeight = Math.Round(row.NewWeight, 3, MidpointRounding.AwayFromZero);

                    if (oldWeight == newWeight)
                        continue;

                    long fItemCode;
                    long tItemCode;
                    decimal weightDiff;

                    if (oldWeight > newWeight)
                    {
                        weightDiff = Math.Round(oldWeight - newWeight, 3, MidpointRounding.AwayFromZero);
                        fItemCode = curItemCode;
                        tItemCode = tempItemCode;
                    }
                    else
                    {
                        weightDiff = Math.Round(newWeight - oldWeight, 3, MidpointRounding.AwayFromZero);
                        fItemCode = tempItemCode;
                        tItemCode = curItemCode;
                    }

                    await using (var cmdInsert = new SqlCommand(
                        @"INSERT INTO TabSalePurTran (SPDate, FitemCode, TitemCode, Carats, TrnType)
                          VALUES (@SPDate, @FitemCode, @TitemCode, @Carats, @TrnType)",
                        conn, trans))
                    {
                        cmdInsert.Parameters.Add("@SPDate", SqlDbType.DateTime).Value = entryDate;
                        cmdInsert.Parameters.Add("@FitemCode", SqlDbType.BigInt).Value = fItemCode;
                        cmdInsert.Parameters.Add("@TitemCode", SqlDbType.BigInt).Value = tItemCode;
                        cmdInsert.Parameters.Add("@Carats", SqlDbType.Decimal).Value = weightDiff;
                        cmdInsert.Parameters["@Carats"].Precision = 18;
                        cmdInsert.Parameters["@Carats"].Scale = 3;
                        cmdInsert.Parameters.Add("@TrnType", SqlDbType.NVarChar, 20).Value = "StkTrn";
                        await cmdInsert.ExecuteNonQueryAsync();
                        insertedCount++;
                    }
                }

                await trans.CommitAsync();
                return new MakeLabWeightDiffEntryResultDto
                {
                    Success = true,
                    Message = "LabWeightDiff Done!!!",
                    InsertedCount = insertedCount
                };
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                return new MakeLabWeightDiffEntryResultDto
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        private static string BuildGradingComparisonWhere(GradingComparisonReportFilterDto filter)
        {
            string strWhere = "";
            try
            {
                if (!string.IsNullOrWhiteSpace(filter.LotNo))
                {
                    string strTmpNewLine = filter.LotNo.Replace(" ", ",");
                    strTmpNewLine = strTmpNewLine.Replace("\r\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace("\n", ",");
                    strTmpNewLine = strTmpNewLine.Replace(",", "'',''");

                    strWhere += " AND a.LotNo  IN (''" + strTmpNewLine + "'')";
                }

                AppendInClause(ref strWhere, filter.Shape, "a.Shape");
                AppendInClause(ref strWhere, filter.Color, "a.Color");
                AppendInClause(ref strWhere, filter.Clarity, "a.Clarity");
                AppendInClause(ref strWhere, filter.Size, "a.Size");
                AppendInClause(ref strWhere, filter.Proportion, "a.Proportion");
                AppendInClause(ref strWhere, filter.Polish, "a.Polish");
                AppendInClause(ref strWhere, filter.Symmetry, "a.Symm");
                AppendInClause(ref strWhere, filter.Fluorescence, "a.FLCode");

                if (filter.LAB != null && filter.LAB.Any())
                {
                    string lab = "";
                    filter.LAB.ForEach(el => lab += "''" + el + "[]'',");
                    if (lab.Trim().Length > 1)
                        lab = lab.Remove(lab.Length - 1);

                    if (lab.Length > 0)
                        strWhere += " And a.grader In (" + lab + ")";
                }

                if (!string.IsNullOrWhiteSpace(filter.FromDate) && !string.IsNullOrWhiteSpace(filter.ToDate))
                    strWhere += " AND a.Gdate BETWEEN ''" + filter.FromDate + "'' And ''" + filter.ToDate + "''";
            }
            catch
            {
            }

            return strWhere;
        }

        private static void AppendInClause(ref string strWhere, List<string>? values, string column)
        {
            if (values == null || !values.Any()) return;

            string joined = "";
            values.ForEach(el => joined += "''" + el + "'',");
            if (joined.Trim().Length > 1)
                joined = joined.Remove(joined.Length - 1);

            if (joined.Length > 0)
                strWhere += " And " + column + " In (" + joined + ")";
        }
    }
}
