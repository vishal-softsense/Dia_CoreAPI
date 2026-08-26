using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Transaction;
using Microsoft.Data.SqlClient;
using System.Data;
using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;
using StockBasic = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Transaction
{
    public class SolitaireSaleReturnRepository : ISolitaireSaleReturnRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public SolitaireSaleReturnRepository(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
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
                { "@FormName", "'" + formName + "'" },
                { "@Where", where }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<PermissionCheck>(
                "GetUserPermission", parameters, companyId);

            return result.Count();
        }

        public async Task<List<SolitaireSaleReturnInvListDto>> GetListAsync(int pageNo, string companyId)
        {
            var parameters = new Dictionary<string, object> { { "@PageNo", pageNo } };

            var result = (await _sqlDataAccess.SelectDataFromSP<SolitaireSaleReturnInvListDto>(
                "GetSaleReturnInvDataUsingPageNo", parameters, companyId)).ToList();

            foreach (var row in result)
            {
                row.InvNo = StripHtmlToText(row.InvNo);
                if (string.IsNullOrWhiteSpace(row.Inv) && !string.IsNullOrWhiteSpace(row.InvNo))
                    row.Inv = row.InvNo;
                else
                    row.Inv = StripHtmlToText(row.Inv);
            }

            return result;
        }

        public async Task<SolitaireSaleReturnPageDataDto> GetPageDataAsync(string companyId)
        {
            var transactionData = await _sqlDataAccess.SelectDataFromSP<StockBasic>(
                "AllMasterDataForPurchase", new Dictionary<string, object>(), companyId);

            var list = transactionData.ToList();
            var defaultCurrencyCode = await _sqlDataAccess.ExecuteQuerySingle<string>(
                "SELECT Value FROM TabSettingMaster WHERE Name = 'DefaultCurrency'",
                new { }, companyId);

            return new SolitaireSaleReturnPageDataDto
            {
                Type = MapBasic(list.Where(x => x.Type == "Type")),
                SubType = MapBasic(list.Where(x => x.Type == "SubType")),
                Category = MapBasic(list.Where(x => x.Type == "Category")),
                CurrencyList = MapBasic(list.Where(x => x.Type == "Currency")),
                DefaultCurrencyCode = defaultCurrencyCode
            };
        }

        public async Task<SolitaireSaleReturnDetailDto?> GetDetailAsync(int id, string companyId)
        {
            var header = await _sqlDataAccess.SelectDataFromSPSingle<SolitaireSaleReturnHeaderDto>(
                "GetSalepurRecord",
                new Dictionary<string, object> { { "@Where", $" tsp.TrnType = 'SaleRtn' AND Code = {id}" } },
                companyId);

            if (header == null) return null;

            var items = (await _sqlDataAccess.SelectDataFromSP<SolitaireSaleReturnItemDto>(
                "GetSalepurTran_Sale",
                new Dictionary<string, object> { { "@Where", $" AND InvCode = {id}" } },
                companyId)).ToList();

            foreach (var item in items)
            {
                if (item.MemoTransCode == 0 && item.MemoTranCode != 0)
                    item.MemoTransCode = item.MemoTranCode;
                if (item.ReturnCarats == 0 && item.RtnCarats != 0)
                    item.ReturnCarats = item.RtnCarats;
            }

            return new SolitaireSaleReturnDetailDto { Header = header, Items = items };
        }

        public async Task<List<SolitaireSaleReturnLotDetailDto>> GetLotDetailAsync(
            SolitaireSaleReturnLotRequest request, string companyId)
        {
            var escapedLotNo = (request.LotNo ?? "").Replace("'", "''");
            string sql = $" EXEC [GetLotNoDataForAll] @Type = 'SaleRtn',@Where = ' AND LotNo = ''{escapedLotNo}'' '";
            var result = (await _sqlDataAccess.SelectRawSql<SolitaireSaleReturnLotDetailDto>(sql, companyId)).ToList();
            NormalizeLotWeights(result);
            return result;
        }

        public async Task<SolitaireSaleReturnGridViewDto> AddItemInGridAsync(
            SolitaireSaleReturnAddLotRequest item, string companyId)
        {
            var list = item.Items ?? new List<SolitaireSaleReturnItemDto>();
            string alert = "";

            float carat = RoundCarat(item.Carat);
            if (carat <= 0) throw new Exception("Carat is invalid.");

            float purRate = item.Rate;
            float back = item.Back;
            float mrp = item.MRP;

            if (purRate == 0 && back == 0)
                throw new Exception("Rate and Discount both can not be 0.");

            if (purRate != 0 && mrp != 0)
                back = ((purRate * 100) / mrp) - 100;

            float amount = carat * purRate;
            float l1 = item.Less1 != 0 ? amount * (100 + item.Less1) / 100 : amount;
            float l2 = item.Less2 != 0 ? l1 * (100 + item.Less2) / 100 : l1;
            float l3 = item.Less3 != 0 ? l2 * (100 + item.Less3) / 100 : l2;

            var row = new SolitaireSaleReturnItemDto
            {
                LotNo = item.LotNo,
                ItemName = item.ItemName,
                ExportNo = item.ExportNo,
                Weight = carat,
                Pcs = item.Pcs,
                MRP = mrp,
                Rate = purRate,
                Back = back,
                Less1 = item.Less1,
                Less2 = item.Less2,
                Less3 = item.Less3,
                Amount = l3,
                MemoTransCode = item.MemoTransCode,
                ReturnCarats = item.ReturnCarats
            };

            if (carat < 0.05)
                alert += " Lotno On Memo or Sold: " + item.LotNo;

            bool isEdit = item.editIndex >= 0 && item.editIndex < list.Count;
            if (isEdit)
            {
                list[item.editIndex] = row;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(item.LotNo) &&
                    list.Any(x => string.Equals(x.LotNo, item.LotNo, StringComparison.OrdinalIgnoreCase)))
                    throw new Exception($"Lot number {item.LotNo} already exists.");

                list.Add(row);
            }

            return BuildGridView(list, alert, isEdit);
        }

        public async Task<SolitaireSaleReturnGridViewDto> GetItemFromListAsync(
            SolitaireSaleReturnGetItemFromListRequest request, string companyId)
        {
            var list = new List<SolitaireSaleReturnItemDto>();
            string alert = "";

            string strWhere = " AND LotNo IN ('''')";
            if (!string.IsNullOrWhiteSpace(request.LotNo))
            {
                var tmp = request.LotNo.Replace("\r\n", ",").Replace("\n", ",");
                tmp = tmp.Replace(",", "'',''");
                strWhere = $" AND LotNo IN (''{tmp}'')";
            }

            string sql = $" EXEC [GetLotNoDataForAll] @Type = 'SaleRtn',@Where = '{strWhere}'";
            var lots = (await _sqlDataAccess.SelectRawSql<SolitaireSaleReturnLotDetailDto>(sql, companyId)).ToList();
            NormalizeLotWeights(lots);

            foreach (var lotRow in lots)
            {
                float amount = lotRow.Amount;
                float l1 = request.Less1 != 0 ? amount * (100 + request.Less1) / 100 : amount;
                float l2 = request.Less2 != 0 ? l1 * (100 + request.Less2) / 100 : l1;
                float l3 = request.Less3 != 0 ? l2 * (100 + request.Less3) / 100 : l2;

                if (lotRow.Weight < 0.05)
                {
                    alert += " Lotno On Memo or Sold: " + lotRow.LotNo;
                    continue;
                }

                if (list.Any(x => string.Equals(x.LotNo, lotRow.LotNo, StringComparison.OrdinalIgnoreCase)))
                {
                    alert += " LotNo Already :" + lotRow.LotNo;
                    continue;
                }

                list.Add(new SolitaireSaleReturnItemDto
                {
                    LotNo = lotRow.LotNo,
                    ItemName = lotRow.ItemName,
                    Weight = lotRow.Weight,
                    Pcs = lotRow.Pcs,
                    MRP = lotRow.MRP,
                    Back = lotRow.Back,
                    Rate = lotRow.Rate,
                    Less1 = request.Less1,
                    Less2 = request.Less2,
                    Less3 = request.Less3,
                    Amount = l3
                });
            }

            return BuildGridView(list, alert, false);
        }

        public Task<SolitaireSaleReturnGridViewDto> RefreshBackAsync(
            SolitaireSaleReturnRefreshBackRequest request, string companyId)
        {
            var list = request.Items ?? new List<SolitaireSaleReturnItemDto>();

            if (request.refreshback != 0)
            {
                float avgPRapDown = request.AvgBack;
                float salePRapDown = request.refreshback;
                float pRapDownDiff = avgPRapDown - salePRapDown;

                var newList = new List<SolitaireSaleReturnItemDto>();
                foreach (var itemSData in list)
                {
                    float carat = RoundCarat(itemSData.Weight);
                    float mrp = itemSData.MRP != 0 ? itemSData.MRP : 0;
                    float back = itemSData.Back != 0 ? itemSData.Back : 0;

                    float newBack = back - pRapDownDiff;
                    float newRate = mrp + (mrp * newBack / 100);

                    float amount = carat * newRate;
                    float l1 = itemSData.Less1 != 0 ? amount * (100 + (float)itemSData.Less1) / 100 : amount;
                    float l2 = itemSData.Less2 != 0 ? l1 * (100 + (float)itemSData.Less2) / 100 : l1;
                    float l3 = itemSData.Less3 != 0 ? l2 * (100 + (float)itemSData.Less3) / 100 : l2;

                    newList.Add(CloneItem(itemSData, l3, newBack, newRate));
                }

                return Task.FromResult(BuildGridView(newList, "", false));
            }

            if (request.refreshrate != 0)
            {
                float avgRate = request.AvgRate;
                float salePRate = request.refreshrate;
                float saleRateDiff = avgRate - salePRate;

                var newList = new List<SolitaireSaleReturnItemDto>();
                foreach (var itemSData in list)
                {
                    float carat = RoundCarat(itemSData.Weight);
                    float rate = itemSData.Rate != 0 ? itemSData.Rate : 0;
                    float mrp = itemSData.MRP != 0 ? itemSData.MRP : 0;

                    float newRate = rate - saleRateDiff;
                    float newBack = mrp == 0 ? 0 : ((newRate * 100) / mrp) - 100;

                    float amount = carat * newRate;
                    float l1 = itemSData.Less1 != 0 ? amount * (100 + (float)itemSData.Less1) / 100 : amount;
                    float l2 = itemSData.Less2 != 0 ? l1 * (100 + (float)itemSData.Less2) / 100 : l1;
                    float l3 = itemSData.Less3 != 0 ? l2 * (100 + (float)itemSData.Less3) / 100 : l2;

                    newList.Add(CloneItem(itemSData, l3, newBack, newRate));
                }

                return Task.FromResult(BuildGridView(newList, "", false));
            }

            throw new Exception("Enter AvgBack Or rate");
        }

        public async Task<List<SolitaireSaleReturnSaleRowDto>> GetSaleToSaleReturnAsync(
            string? partyCode, string? invNo, string companyId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Party", partyCode ?? "" },
                { "@invno", invNo ?? "" }
            };

            var result = await _sqlDataAccess.SelectDataFromSP<SolitaireSaleReturnSaleRowDto>(
                "Sp_SelectSaleForSaleRtn",
                parameters,
                companyId);

            return result.ToList();
        }

        public Task<SolitaireSaleReturnGridViewDto> FillSaleToSaleReturnAsync(
            SolitaireSaleReturnFillRequest request, string companyId)
        {
            var list = request.Items ?? new List<SolitaireSaleReturnItemDto>();
            string alert = "";
            var rows = request.Rows ?? new List<SolitaireSaleReturnFillRowDto>();

            foreach (var im in rows)
            {
                if (!im.Chk) continue;

                if (string.IsNullOrWhiteSpace(im.LotNo) || im.Weight < 0 || im.Rate == 0)
                    throw new Exception("invalid data pleasee Eneter valid Data.");

                float mrp = im.MRP;
                float rate = im.Rate;
                float back = im.Back;
                if (rate != 0 && mrp != 0)
                    back = ((rate * 100) / mrp) - 100;

                float amount = im.Weight * rate;
                float l1 = im.Less1 != 0 ? amount * (100 + im.Less1) / 100 : amount;
                float l2 = im.Less2 != 0 ? l1 * (100 + im.Less2) / 100 : l1;
                float l3 = im.Less3 != 0 ? l2 * (100 + im.Less3) / 100 : l2;

                if (im.Weight < 0.05 && im.Weight > 0)
                {
                    alert += " Lotno On Memo or Sold: " + im.LotNo;
                    continue;
                }

                list.Add(new SolitaireSaleReturnItemDto
                {
                    LotNo = im.LotNo,
                    ItemName = im.ItemName,
                    ExportNo = im.ExportNo,
                    Weight = im.Weight,
                    Pcs = im.Pcs,
                    MRP = mrp,
                    Rate = rate,
                    Back = back,
                    Less1 = im.Less1,
                    Less2 = im.Less2,
                    Less3 = im.Less3,
                    Amount = l3,
                    MemoTransCode = im.MemoTransCode,
                    ReturnCarats = im.ReturnCarats
                });
            }

            return Task.FromResult(BuildGridView(list, alert, false));
        }

        public async Task<(bool Success, string Message)> AddAsync(
            SolitaireSaleReturnHeaderDto salePur,
            List<SolitaireSaleReturnItemDto> items,
            string userId,
            string companyId)
        {
            string connectionString = await _sqlDataAccess.GetClientConnectionString(companyId);
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();
            try
            {
                var ctx = await BuildCurrencyContextAsync(conn, trans, salePur);
                int usercode = int.TryParse(userId, out var u) ? u : 0;

                int maxInvNo = int.Parse(await ExecuteCommonDataScalarAsync(conn, trans, "Inv", " And TrnType=''SaleRtn'' ")) + 1;
                if (string.IsNullOrWhiteSpace(salePur.BillNo))
                    salePur.BillNo = maxInvNo.ToString();

                salePur.SalesPerson = salePur.SalesPerson ?? "";

                const bool outstanding = false;
                const bool stockEffected = true;

                await InsertSalePurAsync(conn, trans, maxInvNo, salePur, ctx, outstanding, stockEffected, usercode, userId, companyId);
                int maxSPCode = int.Parse(await ExecuteCommonDataScalarAsync(conn, trans, "SPCode"));

                await ProcessSaleReturnItemsAsync(conn, trans, salePur, items, maxSPCode, ctx, stockEffected, usercode, userId, companyId, isEdit: false);
                await InsertSaleReturnJvEntriesAsync(conn, trans, salePur, maxSPCode, ctx, usercode, userId, companyId);

                trans.Commit();
                return (true, "SaleRtn Record Saved.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> EditAsync(
            SolitaireSaleReturnHeaderDto salePur,
            List<SolitaireSaleReturnItemDto> items,
            string userId,
            string companyId)
        {
            string connectionString = await _sqlDataAccess.GetClientConnectionString(companyId);
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();
            try
            {
                var ctx = await BuildCurrencyContextAsync(conn, trans, salePur);
                int usercode = int.TryParse(userId, out var u) ? u : 0;
                int code = salePur.Code;
                int invNo = int.TryParse(salePur.InvNo, out var inv) ? inv : 0;

                salePur.SalesPerson = salePur.SalesPerson ?? "";

                const bool outstanding = false;
                const bool stockEffected = true;

                await UpdateSalePurAsync(conn, trans, code, invNo, salePur, ctx, outstanding, stockEffected, usercode, userId, companyId);

                using (var cmd = new SqlCommand("SP_DeleteSalePurTran", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@InvCode", code));
                    await cmd.ExecuteNonQueryAsync();
                }

                await ProcessSaleReturnItemsAsync(conn, trans, salePur, items, code, ctx, stockEffected, usercode, userId, companyId, isEdit: true);

                using (var cmd = new SqlCommand("SP_DeleteJV", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@vcode", code));
                    cmd.Parameters.Add(new SqlParameter("@vtype", "SaleRtn"));
                    await cmd.ExecuteNonQueryAsync();
                }

                await InsertSaleReturnJvEntriesAsync(conn, trans, salePur, code, ctx, usercode, userId, companyId);

                trans.Commit();
                return (true, "SaleRtn Record Update.");
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

                using (var cmd = new SqlCommand("GetSalepurCode", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Where", " AND Code = " + id));
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (!await reader.ReadAsync())
                        return (false, "Record not found.");
                }

                using (var cmd = new SqlCommand("SP_DeleteSalePur", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@code", id));
                    await cmd.ExecuteNonQueryAsync();
                }

                using (var cmd = new SqlCommand("SP_DeleteSalePurTran", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@InvCode", id));
                    await cmd.ExecuteNonQueryAsync();
                }

                using (var cmd = new SqlCommand("SP_DeleteJV", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@vcode", id));
                    cmd.Parameters.Add(new SqlParameter("@vtype", "SaleRtn"));
                    await cmd.ExecuteNonQueryAsync();
                }

                trans.Commit();
                return (true, "Record Deleted.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return (false, ex.Message);
            }
        }

        public async Task<SolitaireSaleReturnPrintViewDto?> GetPrintViewAsync(string invNo, string companyId)
        {
            var escapedInvNo = (invNo ?? "").Replace("'", "''");
            string where = $"tsp.TrnType = ''SaleRtn'' AND tsp.InvNo = ''{escapedInvNo}''";
            string sql = $"EXEC GetSalepurRecordWithDetailsForINV @Where = '{where}'";

            var rows = (await _sqlDataAccess.SelectRawSql<PrintInvRow>(sql, companyId)).ToList();
            if (rows.Count == 0)
                return null;

            var header = rows[0];
            var result = new SolitaireSaleReturnPrintViewDto
            {
                InvNo = header.InvNo,
                BillNo = header.BillNo,
                TrnDate = header.TrnDate.HasValue ? header.TrnDate.Value.ToString("MM/dd/yyyy") : null,
                DueDays = header.DueDays,
                PartyName = header.PartyName,
                AddressLine1 = header.AddressLine1,
                AddressLine2 = header.AddressLine2,
                AddressLine3 = header.AddressLine3,
                ShipVia = header.ShipVia,
                Tracking = header.Tracking,
                Items = new List<SolitaireSaleReturnPrintItemDto>()
            };

            foreach (var dr in rows)
            {
                result.Items.Add(new SolitaireSaleReturnPrintItemDto
                {
                    StockNo = dr.LotNo,
                    Description = dr.ItemName,
                    Qty = dr.Pcs,
                    Weight = dr.Carats,
                    PricePerCarat = dr.Rate,
                    Amount = dr.Amount,
                    ShapeId = dr.ShapeId,
                    ColorId = dr.ColorId,
                    ClarityId = dr.ClarityId,
                    PropId = dr.PropId,
                    PolishId = dr.PolishId,
                    SymId = dr.SymId,
                    FLId = dr.FLId,
                    CertId = dr.CertId,
                    CertificateNo = dr.CertificateNo,
                    Rate = dr.Rate,
                    Back = dr.Back,
                    MasterCurrencyAmount = dr.MasterCurrencyAmount
                });
            }

            return result;
        }

        #region Grid helpers

        private static SolitaireSaleReturnItemDto CloneItem(
            SolitaireSaleReturnItemDto itemSData, float amount, float newBack, float newRate)
        {
            return new SolitaireSaleReturnItemDto
            {
                ItemName = itemSData.ItemName,
                LotNo = itemSData.LotNo,
                ExportNo = itemSData.ExportNo,
                Less1 = itemSData.Less1,
                Less2 = itemSData.Less2,
                Less3 = itemSData.Less3,
                Amount = amount,
                Weight = itemSData.Weight,
                MRP = itemSData.MRP,
                Pcs = itemSData.Pcs,
                Back = newBack,
                Rate = newRate,
                SpCarats = itemSData.SpCarats,
                ReturnCarats = itemSData.ReturnCarats,
                MemoTransCode = itemSData.MemoTransCode,
                Status = itemSData.Status,
                SRtnStatus = itemSData.SRtnStatus
            };
        }

        private static SolitaireSaleReturnGridViewDto BuildGridView(
            List<SolitaireSaleReturnItemDto> list, string alert, bool isEdit)
        {
            double totalCarats = 0, totalAmount = 0, totalMRPAmount = 0, totalAmountWithoutless = 0;
            foreach (var item in list)
            {
                double weight = Math.Round(Convert.ToDouble(item.Weight.ToString("N3")), 3);
                totalCarats += weight;
                totalAmount += item.Amount;
                totalMRPAmount += weight * item.MRP;
                totalAmountWithoutless += weight * item.Rate;
            }

            return new SolitaireSaleReturnGridViewDto
            {
                TotalCarats = Math.Round(totalCarats, 3),
                TotalAmount = Math.Round(totalAmount, 2),
                TotalMRPAmount = Math.Round(totalMRPAmount, 3),
                TotalAmountWithoutless = Math.Round(totalAmountWithoutless, 3),
                Alert = alert,
                IsEdit = isEdit,
                ExcelData = list
            };
        }

        private static void NormalizeLotWeights(List<SolitaireSaleReturnLotDetailDto> lots)
        {
            foreach (var lot in lots)
            {
                if (lot.Weight == 0 && float.TryParse(lot.Carats, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var carats) && carats != 0)
                    lot.Weight = carats;
            }
        }

        private static float RoundCarat(float carat)
        {
            string carat3D = Convert.ToDouble(carat).ToString("N3");
            return float.Parse(carat3D);
        }

        private static List<BasicDataDto> MapBasic(IEnumerable<StockBasic> source) =>
            source.Select(x => new BasicDataDto
            {
                ID = x.ID,
                Type = x.Type,
                Priority = x.Priority,
                Name = x.Name,
                TextName = x.TextName
            }).ToList();

        private static string? StripHtmlToText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            var text = value.Trim();
            if (!text.Contains('<')) return text;
            var open = text.IndexOf('>');
            var close = text.LastIndexOf('<');
            if (open >= 0 && close > open)
                text = text.Substring(open + 1, close - open - 1);
            text = System.Text.RegularExpressions.Regex.Replace(text, "<[^>]+>", string.Empty);
            return System.Net.WebUtility.HtmlDecode(text).Trim();
        }

        #endregion

        #region Currency / transaction helpers

        private class CurrencyContext
        {
            public int CurCode { get; set; }
            public float MCurBaseRate { get; set; }
            public float SCurBaseRate { get; set; }
            public string MCurOperator { get; set; } = "*";
            public string SCurOperator { get; set; } = "*";
            public double MCurCode { get; set; }
            public double SCurCode { get; set; }
            public string MCurOperatorM { get; set; } = "*";
            public string SCurOperatorM { get; set; } = "*";
            public float MToSCurRate { get; set; }
            public double MCurFinalAmt { get; set; }
            public double SCurFinalAmt { get; set; }
        }

        private class PrintInvRow
        {
            public string? InvNo { get; set; }
            public string? BillNo { get; set; }
            public DateTime? TrnDate { get; set; }
            public short DueDays { get; set; }
            public string? PartyName { get; set; }
            public string? AddressLine1 { get; set; }
            public string? AddressLine2 { get; set; }
            public string? AddressLine3 { get; set; }
            public string? ShipVia { get; set; }
            public string? Tracking { get; set; }
            public string? LotNo { get; set; }
            public string? ItemName { get; set; }
            public int Pcs { get; set; }
            public double Carats { get; set; }
            public double Rate { get; set; }
            public double MasterCurrencyAmount { get; set; }
            public string? ShapeId { get; set; }
            public string? ColorId { get; set; }
            public string? ClarityId { get; set; }
            public string? PropId { get; set; }
            public string? PolishId { get; set; }
            public string? SymId { get; set; }
            public string? FLId { get; set; }
            public string? CertId { get; set; }
            public string? CertificateNo { get; set; }
            public string? Back { get; set; }
            public double Amount { get; set; }
        }

        private async Task<CurrencyContext> BuildCurrencyContextAsync(
            SqlConnection conn, SqlTransaction trans, SolitaireSaleReturnHeaderDto salePur)
        {
            int curCode = salePur.CurCode;
            float mCurBaseRate = salePur.BaseRate;
            float sCurBaseRate = salePur.SBaseRate;

            string mCurOperator = await ExecuteCurrencyDetailScalarAsync(conn, trans, "MCurOperator", " and ID=" + curCode);
            string sCurOperator = await ExecuteCurrencyDetailScalarAsync(conn, trans, "SCurOperator", " and ID=" + curCode);

            var netRes = GetConvertAmount(curCode, salePur.NetAmount, mCurBaseRate, mCurOperator, sCurBaseRate, sCurOperator,
                double.Parse(await ExecuteCurrencyDetailScalarAsync(conn, trans, "MCurID")),
                double.Parse(await ExecuteCurrencyDetailScalarAsync(conn, trans, "SCurID")));

            float mToSCurRate = netRes.SCurAmt != 0 ? Convert.ToSingle(netRes.MCurAmt / netRes.SCurAmt) : 0f;

            double mCurCode = double.Parse(await ExecuteCurrencyDetailScalarAsync(conn, trans, "MCurID"));
            double sCurCode = double.Parse(await ExecuteCurrencyDetailScalarAsync(conn, trans, "SCurID"));
            string mCurOperatorM = await ExecuteCurrencyDetailScalarAsync(conn, trans, "MCurOperator", " and ID=" + mCurCode);
            string sCurOperatorM = await ExecuteCurrencyDetailScalarAsync(conn, trans, "SCurOperator", " and ID=" + mCurCode);

            var finalRes = GetConvertAmount(mCurCode, salePur.FinalMAmount, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, sCurCode);

            return new CurrencyContext
            {
                CurCode = curCode,
                MCurBaseRate = mCurBaseRate,
                SCurBaseRate = sCurBaseRate,
                MCurOperator = mCurOperator,
                SCurOperator = sCurOperator,
                MCurCode = mCurCode,
                SCurCode = sCurCode,
                MCurOperatorM = mCurOperatorM,
                SCurOperatorM = sCurOperatorM,
                MToSCurRate = mToSCurRate,
                MCurFinalAmt = finalRes.MCurAmt,
                SCurFinalAmt = finalRes.SCurAmt
            };
        }

        private async Task InsertSalePurAsync(
            SqlConnection conn, SqlTransaction trans, int maxInvNo,
            SolitaireSaleReturnHeaderDto salePur, CurrencyContext ctx,
            bool outstanding, bool stockEffected,
            int usercode, string userId, string companyId)
        {
            using var cmd = new SqlCommand("SP_InsertSalePur", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@invno", maxInvNo));
            cmd.Parameters.Add(new SqlParameter("@billno", salePur.BillNo ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@trntype", "SaleRtn"));
            cmd.Parameters.Add(new SqlParameter("@invtype", salePur.InvType ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@category", salePur.Category ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@trndate", salePur.TrnDate));
            cmd.Parameters.Add(new SqlParameter("@duedays", salePur.DueDays));
            cmd.Parameters.Add(new SqlParameter("@duedate", salePur.DueDate));
            cmd.Parameters.Add(new SqlParameter("@partycode", salePur.PartyCode));
            cmd.Parameters.Add(new SqlParameter("@SellerCode", (object)0));
            cmd.Parameters.Add(new SqlParameter("@term", salePur.Term ?? ""));
            cmd.Parameters.Add(new SqlParameter("@countrycode", (object)0));
            cmd.Parameters.Add(new SqlParameter("@BrokerCode", salePur.BrokerCode ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@BrokerPer", salePur.BrokerPer));
            cmd.Parameters.Add(new SqlParameter("@BrokerAmount", salePur.BrokerAmount));
            cmd.Parameters.Add(new SqlParameter("@adatcode", salePur.AdatCode ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@adatper", salePur.AdatPer));
            cmd.Parameters.Add(new SqlParameter("@adatamount", salePur.AdatAmount));
            cmd.Parameters.Add(new SqlParameter("@totcts", salePur.TotCts));
            cmd.Parameters.Add(new SqlParameter("@totamount", Math.Round(salePur.TotAmount, 4)));
            cmd.Parameters.Add(new SqlParameter("@netamount", Math.Round(salePur.NetAmount, 4)));
            cmd.Parameters.Add(new SqlParameter("@PlusMinAmount", Math.Round(salePur.PlusMinusAmount, 4)));
            cmd.Parameters.Add(new SqlParameter("@RoundAmount", Math.Round(salePur.RoundAmount, 4)));
            cmd.Parameters.Add(new SqlParameter("@FinalAmount", Math.Round(salePur.FinalMAmount, 4)));
            cmd.Parameters.Add(new SqlParameter("@BaseRate", Math.Round(salePur.BaseRate, 4)));
            cmd.Parameters.Add(new SqlParameter("@Disc", salePur.InvDisc ?? ""));
            cmd.Parameters.Add(new SqlParameter("@CurCode", ctx.CurCode.ToString()));
            cmd.Parameters.Add(new SqlParameter("@Outstanding", outstanding));
            cmd.Parameters.Add(new SqlParameter("@StockEffected", stockEffected));
            cmd.Parameters.Add(new SqlParameter("@Brokerage", false));
            cmd.Parameters.Add(new SqlParameter("@Commission", false));
            cmd.Parameters.Add(new SqlParameter("@Cheque", false));
            cmd.Parameters.Add(new SqlParameter("@usercode", usercode));
            cmd.Parameters.Add(new SqlParameter("@mastercurrencyrate", Math.Round(ctx.MCurBaseRate, 4)));
            cmd.Parameters.Add(new SqlParameter("@mastercurrencyfinalamount", Math.Round(ctx.MCurFinalAmt, 4)));
            cmd.Parameters.Add(new SqlParameter("@stockcurrencyrate", Math.Round(ctx.SCurBaseRate, 4)));
            cmd.Parameters.Add(new SqlParameter("@stockcurrencyfinalamount", Math.Round(ctx.SCurFinalAmt, 4)));
            cmd.Parameters.Add(new SqlParameter("@ExpPartyCode", salePur.ExpPartyCode));
            cmd.Parameters.Add(new SqlParameter("@RoundDate", salePur.RoundDate == default ? salePur.DueDate : salePur.RoundDate));
            cmd.Parameters.Add(new SqlParameter("@DivideDays", salePur.DivideDays));
            cmd.Parameters.Add(new SqlParameter("@InterestPer", salePur.InterestPer));
            cmd.Parameters.Add(new SqlParameter("@VatPer", salePur.VatPer));
            cmd.Parameters.Add(new SqlParameter("@VatAmount", salePur.VatAmount));
            cmd.Parameters.Add(new SqlParameter("@invdescription", salePur.InvDisc ?? ""));
            cmd.Parameters.Add(new SqlParameter("@bankname", (object)0));
            cmd.Parameters.Add(new SqlParameter("@subtype", salePur.SubType ?? ""));
            cmd.Parameters.Add(new SqlParameter("@ShipVia", salePur.ShipVia ?? ""));
            cmd.Parameters.Add(new SqlParameter("@deliverto", salePur.DeliverTo ?? ""));
            cmd.Parameters.Add(new SqlParameter("@Tracking", salePur.Tracking ?? ""));
            cmd.Parameters.Add(new SqlParameter("@SalesPerson", salePur.SalesPerson ?? ""));
            cmd.Parameters.Add(new SqlParameter("@Attendee", ""));
            cmd.Parameters.Add(new SqlParameter("@labcharges", salePur.LabCharges));
            cmd.Parameters.Add(new SqlParameter("@freightcharges", salePur.FreightCharges));
            cmd.Parameters.Add(new SqlParameter("@duedays1", (object)0));
            cmd.Parameters.Add(new SqlParameter("@duedays2", (object)0));
            cmd.Parameters.Add(new SqlParameter("@CGSTPer", salePur.CGSTPer));
            cmd.Parameters.Add(new SqlParameter("@CGSTAmount", salePur.CGSTAmt));
            cmd.Parameters.Add(new SqlParameter("@SGSTPer", salePur.SGSTPer));
            cmd.Parameters.Add(new SqlParameter("@SGSTAmount", salePur.SGSTAmt));
            cmd.Parameters.Add(new SqlParameter("@IGSTPer", salePur.IGSTPer));
            cmd.Parameters.Add(new SqlParameter("@IGSTAmount", salePur.IGSTAmt));
            cmd.Parameters.Add(new SqlParameter("@CreatedBy", userId ?? ""));
            cmd.Parameters.Add(new SqlParameter("@CompanyID", int.Parse(companyId)));
            cmd.Parameters.Add(new SqlParameter("@netMamount", salePur.NetMAmount));
            cmd.Parameters.Add(new SqlParameter("@FinalPayRecAmount", salePur.FinalMPayRecAmount));
            cmd.Parameters.Add(new SqlParameter("@Address", (object)1));
            cmd.Parameters.Add(new SqlParameter("@ActionType", "insert"));
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task UpdateSalePurAsync(
            SqlConnection conn, SqlTransaction trans, int code, int invNo,
            SolitaireSaleReturnHeaderDto salePur, CurrencyContext ctx,
            bool outstanding, bool stockEffected,
            int usercode, string userId, string companyId)
        {
            using var cmd = new SqlCommand("SP_UpdateSalePur", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@code", code));
            cmd.Parameters.Add(new SqlParameter("@invno", invNo));
            cmd.Parameters.Add(new SqlParameter("@ModifyBy", userId ?? ""));
            cmd.Parameters.Add(new SqlParameter("@BillNo", salePur.BillNo ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@TrnType", "SaleRtn"));
            cmd.Parameters.Add(new SqlParameter("@invtype", salePur.InvType ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@SubType", salePur.SubType ?? ""));
            cmd.Parameters.Add(new SqlParameter("@BankID", (object)0));
            cmd.Parameters.Add(new SqlParameter("@Category", salePur.Category ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@Trndate", salePur.TrnDate));
            cmd.Parameters.Add(new SqlParameter("@RoundDate", salePur.RoundDate == default ? salePur.DueDate : salePur.RoundDate));
            cmd.Parameters.Add(new SqlParameter("@duedays", salePur.DueDays));
            cmd.Parameters.Add(new SqlParameter("@duedate", salePur.DueDate));
            cmd.Parameters.Add(new SqlParameter("@DivideDays", salePur.DivideDays));
            cmd.Parameters.Add(new SqlParameter("@partycode", salePur.PartyCode));
            cmd.Parameters.Add(new SqlParameter("@SellerCode", (object)0));
            cmd.Parameters.Add(new SqlParameter("@countrycode", (object)0));
            cmd.Parameters.Add(new SqlParameter("@brokercode", salePur.BrokerCode ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@brokerper", salePur.BrokerPer));
            cmd.Parameters.Add(new SqlParameter("@BrokerAmount", salePur.BrokerAmount));
            cmd.Parameters.Add(new SqlParameter("@Adatcode", salePur.AdatCode ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@Adatper", salePur.AdatPer));
            cmd.Parameters.Add(new SqlParameter("@AdatAmount", salePur.AdatAmount));
            cmd.Parameters.Add(new SqlParameter("@totcts", salePur.TotCts));
            cmd.Parameters.Add(new SqlParameter("@totamount", Math.Round(salePur.TotAmount, 4)));
            cmd.Parameters.Add(new SqlParameter("@netamount", Math.Round(salePur.NetAmount, 4)));
            cmd.Parameters.Add(new SqlParameter("@NetMAmount", salePur.NetMAmount));
            cmd.Parameters.Add(new SqlParameter("@plusminamount", Math.Round(salePur.PlusMinusAmount, 4)));
            cmd.Parameters.Add(new SqlParameter("@RoundAmount", Math.Round(salePur.RoundAmount, 4)));
            cmd.Parameters.Add(new SqlParameter("@finalamount", Math.Round(salePur.FinalMAmount, 4)));
            cmd.Parameters.Add(new SqlParameter("@FinalPayRecAmount", salePur.FinalMPayRecAmount));
            cmd.Parameters.Add(new SqlParameter("@baserate", Math.Round(salePur.BaseRate, 4)));
            cmd.Parameters.Add(new SqlParameter("@ExpPartyCode", salePur.ExpPartyCode));
            cmd.Parameters.Add(new SqlParameter("@ShipVia", salePur.ShipVia ?? ""));
            cmd.Parameters.Add(new SqlParameter("@Tracking", salePur.Tracking ?? ""));
            cmd.Parameters.Add(new SqlParameter("@SalesPerson", salePur.SalesPerson ?? ""));
            cmd.Parameters.Add(new SqlParameter("@Attendee", ""));
            cmd.Parameters.Add(new SqlParameter("@disc", salePur.InvDisc ?? ""));
            cmd.Parameters.Add(new SqlParameter("@CurCode", ctx.CurCode.ToString()));
            cmd.Parameters.Add(new SqlParameter("@OutStanding", outstanding));
            cmd.Parameters.Add(new SqlParameter("@StockEffected", stockEffected));
            cmd.Parameters.Add(new SqlParameter("@Brokerage", false));
            cmd.Parameters.Add(new SqlParameter("@Commission", false));
            cmd.Parameters.Add(new SqlParameter("@Cheque", false));
            cmd.Parameters.Add(new SqlParameter("@UserCode", usercode));
            cmd.Parameters.Add(new SqlParameter("@term", salePur.Term ?? ""));
            cmd.Parameters.Add(new SqlParameter("@InterestPer", salePur.InterestPer));
            cmd.Parameters.Add(new SqlParameter("@MasterCurrencyRate", Math.Round(ctx.MCurBaseRate, 4)));
            cmd.Parameters.Add(new SqlParameter("@MasterCurrencyFinalAmount", Math.Round(ctx.MCurFinalAmt, 4)));
            cmd.Parameters.Add(new SqlParameter("@StockCurrencyRate", Math.Round(ctx.SCurBaseRate, 4)));
            cmd.Parameters.Add(new SqlParameter("@StockCurrencyFinalAmount", Math.Round(ctx.SCurFinalAmt, 4)));
            cmd.Parameters.Add(new SqlParameter("@VatPer", salePur.VatPer));
            cmd.Parameters.Add(new SqlParameter("@VatAmount", salePur.VatAmount));
            cmd.Parameters.Add(new SqlParameter("@CGSTPer", salePur.CGSTPer));
            cmd.Parameters.Add(new SqlParameter("@CGSTAmount", salePur.CGSTAmt));
            cmd.Parameters.Add(new SqlParameter("@SGSTPer", salePur.SGSTPer));
            cmd.Parameters.Add(new SqlParameter("@SGSTAmount", salePur.SGSTAmt));
            cmd.Parameters.Add(new SqlParameter("@IGSTPer", salePur.IGSTPer));
            cmd.Parameters.Add(new SqlParameter("@IGSTAmount", salePur.IGSTAmt));
            cmd.Parameters.Add(new SqlParameter("@CompanyID", int.Parse(companyId)));
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task ProcessSaleReturnItemsAsync(
            SqlConnection conn, SqlTransaction trans,
            SolitaireSaleReturnHeaderDto salePur,
            List<SolitaireSaleReturnItemDto> items,
            int invCode, CurrencyContext ctx,
            bool stockEffected,
            int usercode, string userId, string companyId,
            bool isEdit)
        {
            foreach (var im in items)
            {
                var amtRes = GetConvertAmount(ctx.CurCode, im.Amount, ctx.MCurBaseRate, ctx.MCurOperator, ctx.SCurBaseRate, ctx.SCurOperator, ctx.MCurCode, ctx.SCurCode);

                string itemCodeStr = await ExecuteCommonDataScalarAsync(conn, trans, "ItemCode", $" AND LotNo = ''{im.LotNo}''");
                int itemCode = int.TryParse(itemCodeStr, out var ic) ? ic : 0;

                float memoTransCode = im.MemoTransCode != 0 ? im.MemoTransCode : im.MemoTranCode;
                float returnCarats = im.ReturnCarats != 0 ? im.ReturnCarats : im.RtnCarats;

                // ADD: ReturnCarats only when from sale invoice (MemoTransCode != 0); EDIT: always pass ReturnCarats (MVC)
                double returnCaratsForInsert = memoTransCode != 0 || isEdit
                    ? Convert.ToDouble(returnCarats.ToString("N2"))
                    : 0d;

                using (var cmd = new SqlCommand("SP_InsertSalePurTran", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SPdate", salePur.TrnDate));
                    cmd.Parameters.Add(new SqlParameter("@TrnType", "SaleRtn"));
                    cmd.Parameters.Add(new SqlParameter("@InvType", salePur.InvType ?? ""));
                    cmd.Parameters.Add(new SqlParameter("@InvCode", invCode));
                    cmd.Parameters.Add(new SqlParameter("@ItemCode", itemCode));
                    cmd.Parameters.Add(new SqlParameter("@ItemName", im.ItemName ?? ""));
                    cmd.Parameters.Add(new SqlParameter("@Carats", float.Parse(im.Weight.ToString("N3"))));
                    cmd.Parameters.Add(new SqlParameter("@Pcs", im.Pcs));
                    cmd.Parameters.Add(new SqlParameter("@Rate", Convert.ToDouble(im.Rate.ToString("N2"))));
                    cmd.Parameters.Add(new SqlParameter("@MRP", im.MRP));
                    cmd.Parameters.Add(new SqlParameter("@PMRPDown", Convert.ToDouble(im.Back.ToString("N2"))));
                    cmd.Parameters.Add(new SqlParameter("@ReturnCarats", returnCaratsForInsert));
                    cmd.Parameters.Add(new SqlParameter("@IssueCarats", 0d));
                    cmd.Parameters.Add(new SqlParameter("@amount", Convert.ToDouble(im.Amount.ToString("N2"))));
                    cmd.Parameters.Add(new SqlParameter("@ExportNo", im.ExportNo ?? ""));
                    cmd.Parameters.Add(new SqlParameter("@Less1", im.Less1));
                    cmd.Parameters.Add(new SqlParameter("@Less2", im.Less2));
                    cmd.Parameters.Add(new SqlParameter("@Less3", im.Less3));
                    cmd.Parameters.Add(new SqlParameter("@CurCode", ctx.CurCode.ToString()));
                    cmd.Parameters.Add(new SqlParameter("@memotrancode", memoTransCode));
                    cmd.Parameters.Add(new SqlParameter("@StockEffected", stockEffected));
                    cmd.Parameters.Add(new SqlParameter("@ExportDone", false));
                    cmd.Parameters.Add(new SqlParameter("@spcarats", 0f));
                    cmd.Parameters.Add(new SqlParameter("@FItemCode", (object)0));
                    cmd.Parameters.Add(new SqlParameter("@TItemCode", (object)0));
                    cmd.Parameters.Add(new SqlParameter("@FItemRate", 0.0));
                    cmd.Parameters.Add(new SqlParameter("@TItemRate", 0.0));
                    cmd.Parameters.Add(new SqlParameter("@FromSuratCode", (object)0));
                    cmd.Parameters.Add(new SqlParameter("@AssorterCode", (object)0));
                    cmd.Parameters.Add(new SqlParameter("@RoughCode", (object)0));
                    cmd.Parameters.Add(new SqlParameter("@MemoRemark", ""));
                    cmd.Parameters.Add(new SqlParameter("@UserCode", usercode));
                    cmd.Parameters.Add(new SqlParameter("@MasterCurrencyRate", Math.Round(ctx.MCurBaseRate, 4)));
                    cmd.Parameters.Add(new SqlParameter("@MasterCurrencyAmount", Convert.ToDouble(amtRes.MCurAmt.ToString("N2"))));
                    cmd.Parameters.Add(new SqlParameter("@StockCurrencyRate", Math.Round(ctx.SCurBaseRate, 4)));
                    cmd.Parameters.Add(new SqlParameter("@StockCurrencyAmount", Convert.ToDouble(amtRes.SCurAmt.ToString("N2"))));
                    cmd.Parameters.Add(new SqlParameter("@CreatedBy", userId ?? ""));
                    cmd.Parameters.Add(new SqlParameter("@CompanyID", int.Parse(companyId)));
                    cmd.Parameters.Add(new SqlParameter("@PShape", ""));
                    cmd.Parameters.Add(new SqlParameter("@PSize", ""));
                    cmd.Parameters.Add(new SqlParameter("@PSieve", ""));
                    cmd.Parameters.Add(new SqlParameter("@PColor", ""));
                    cmd.Parameters.Add(new SqlParameter("@PClarity", ""));
                    cmd.Parameters.Add(new SqlParameter("@COGS", 0.0f));
                    cmd.Parameters.Add(new SqlParameter("@StockType", ""));
                    cmd.Parameters.Add(new SqlParameter("@ActionType", "insert"));
                    await cmd.ExecuteNonQueryAsync();
                }

                // MVC: MemoTransCode != 0 → memotosaletest on ADD / memotosale on EDIT
                //      MemoTransCode == 0 → memotosale on ADD / memotosaletest on EDIT
                string memoAction = memoTransCode != 0
                    ? (isEdit ? "memotosale" : "memotosaletest")
                    : (isEdit ? "memotosaletest" : "memotosale");

                using (var cmd = new SqlCommand("SP_UpdateMemo", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@code", memoTransCode));
                    cmd.Parameters.Add(new SqlParameter("@RtnDate", salePur.TrnDate));
                    cmd.Parameters.Add(new SqlParameter("@rtncts", float.Parse(returnCarats.ToString("N3"))));
                    cmd.Parameters.Add(new SqlParameter("@spcts", float.Parse(im.Weight.ToString("N3"))));
                    cmd.Parameters.Add(new SqlParameter("@Actiontype", memoAction));
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task InsertSaleReturnJvEntriesAsync(
            SqlConnection conn, SqlTransaction trans,
            SolitaireSaleReturnHeaderDto salePur, int maxSPCode, CurrencyContext ctx,
            int usercode, string userId, string companyId)
        {
            long saleCode = 0;
            long.TryParse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''SALE''"), out saleCode);
            long vatCode = 0;
            long.TryParse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''Vat''"), out vatCode);
            long sgstCode = 0;
            long.TryParse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''SGST''"), out sgstCode);
            long cgstCode = 0;
            long.TryParse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''CGST''"), out cgstCode);
            long igstCode = 0;
            long.TryParse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''IGST''"), out igstCode);
            long brokerageCode = 0;
            long.TryParse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''Brokerage''"), out brokerageCode);
            long adatCode = 0;
            long.TryParse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''Adat''"), out adatCode);
            long cashCode = 0;
            long.TryParse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''Cash''"), out cashCode);

            float saleJvAmount = salePur.FinalMAmount;
            var saleConvert = GetConvertAmount(ctx.MCurCode, saleJvAmount, 1, ctx.MCurOperatorM, ctx.MToSCurRate, ctx.SCurOperatorM, ctx.MCurCode, ctx.SCurCode);

            float saleJvAmountF = 0;
            if (ctx.MCurOperator == "*")
                saleJvAmountF = ctx.MCurBaseRate != 0 ? saleJvAmount / ctx.MCurBaseRate : 0f;
            if (ctx.MCurOperator == "/")
                saleJvAmountF = saleJvAmount * ctx.MCurBaseRate;

            // SaleRtn main JV: Debit SALE, Credit Party (reversed vs Sale)
            await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, saleCode, salePur.PartyCode, Math.Abs(saleJvAmountF), ctx.CurCode.ToString(), "SaleRtn", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, ctx.MCurBaseRate, saleConvert.MCurAmt, ctx.SCurBaseRate, saleConvert.SCurAmt, 0, usercode, userId, companyId);

            if (Convert.ToDouble(salePur.BrokerAmount) != 0 && salePur.BrokerCode != null)
            {
                float broAmt = salePur.BrokerAmount;
                var broConvert = GetConvertAmount(ctx.MCurCode, broAmt, 1, ctx.MCurOperatorM, ctx.MToSCurRate, ctx.SCurOperatorM, ctx.MCurCode, ctx.SCurCode);
                // Debit Broker, Credit Brokerage
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, salePur.BrokerCode.Value, brokerageCode, Math.Abs(broAmt), ctx.MCurCode.ToString(), "SaleRtn", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, ctx.MCurBaseRate, broConvert.MCurAmt, ctx.SCurBaseRate, broConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.AdatAmount) != 0 && salePur.AdatCode != null)
            {
                float adatAmt = salePur.AdatAmount;
                var adatConvert = GetConvertAmount(ctx.MCurCode, adatAmt, 1, ctx.MCurOperatorM, ctx.MToSCurRate, ctx.SCurOperatorM, ctx.MCurCode, ctx.SCurCode);
                // Debit Adat party, Credit Adat
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, salePur.AdatCode.Value, adatCode, adatAmt, ctx.MCurCode.ToString(), "SaleRtn", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, ctx.MCurBaseRate, adatConvert.MCurAmt, ctx.SCurBaseRate, adatConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.VatAmount) != 0)
            {
                float vatAmt = salePur.VatAmount;
                var vatConvert = GetConvertAmount(ctx.MCurCode, vatAmt, 1, ctx.MCurOperatorM, ctx.MToSCurRate, ctx.SCurOperatorM, ctx.MCurCode, ctx.SCurCode);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, vatCode, saleCode, vatAmt, ctx.MCurCode.ToString(), "SaleRtn", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, ctx.MCurBaseRate, vatConvert.MCurAmt, ctx.SCurBaseRate, vatConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.SGSTAmt) != 0)
            {
                float sgstAmt = salePur.SGSTAmt;
                var sgstConvert = GetConvertAmount(ctx.MCurCode, sgstAmt, 1, ctx.MCurOperatorM, ctx.MToSCurRate, ctx.SCurOperatorM, ctx.MCurCode, ctx.SCurCode);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, sgstCode, saleCode, sgstAmt, ctx.MCurCode.ToString(), "SaleRtn", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, ctx.MCurBaseRate, sgstConvert.MCurAmt, ctx.SCurBaseRate, sgstConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.CGSTAmt) != 0)
            {
                float cgstAmt = salePur.CGSTAmt;
                var cgstConvert = GetConvertAmount(ctx.MCurCode, cgstAmt, 1, ctx.MCurOperatorM, ctx.MToSCurRate, ctx.SCurOperatorM, ctx.MCurCode, ctx.SCurCode);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, cgstCode, saleCode, cgstAmt, ctx.MCurCode.ToString(), "SaleRtn", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, ctx.MCurBaseRate, cgstConvert.MCurAmt, ctx.SCurBaseRate, cgstConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.IGSTAmt) != 0)
            {
                float igstAmt = salePur.IGSTAmt;
                var igstConvert = GetConvertAmount(ctx.MCurCode, igstAmt, 1, ctx.MCurOperatorM, ctx.MToSCurRate, ctx.SCurOperatorM, ctx.MCurCode, ctx.SCurCode);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, igstCode, saleCode, igstAmt, ctx.MCurCode.ToString(), "SaleRtn", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, ctx.MCurBaseRate, igstConvert.MCurAmt, ctx.SCurBaseRate, igstConvert.SCurAmt, 0, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.RoundAmount) > 0)
            {
                float roundAmt = salePur.RoundAmount;
                var roundConvert = GetConvertAmount(ctx.MCurCode, roundAmt, 1, ctx.MCurOperatorM, ctx.MToSCurRate, ctx.SCurOperatorM, ctx.MCurCode, ctx.SCurCode);
                // Debit Cash, Credit Party
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, cashCode, salePur.PartyCode, salePur.RoundAmount, ctx.MCurCode.ToString(), "SaleRtn", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, ctx.MCurBaseRate, roundConvert.MCurAmt, ctx.SCurBaseRate, roundConvert.SCurAmt, 0, usercode, userId, companyId);
            }
            else if (Convert.ToDouble(salePur.RoundAmount) < 0)
            {
                float roundAmt = salePur.RoundAmount;
                var roundConvert = GetConvertAmount(ctx.MCurCode, roundAmt, 1, ctx.MCurOperatorM, ctx.MToSCurRate, ctx.SCurOperatorM, ctx.MCurCode, ctx.SCurCode);
                // Debit Party, Credit Cash
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, salePur.PartyCode, cashCode, Math.Abs(roundAmt), ctx.MCurCode.ToString(), "SaleRtn", maxSPCode, salePur.InvDisc ?? "", "", 0, salePur.BaseRate, ctx.MCurBaseRate, Math.Abs(roundConvert.MCurAmt), ctx.SCurBaseRate, Math.Abs(roundConvert.SCurAmt), 0, usercode, userId, companyId);
            }
        }

        private async Task InsertJvAsync(
            SqlConnection conn, SqlTransaction trans,
            DateTime jvDate, DateTime dueDate,
            long dAccCode, long cAccCode, double amount,
            string curCode, string vType, int vCode,
            string disc, string adjustVType, int adjustVCode,
            double baseRate, double masterCurrencyRate, double masterCurrencyAmount,
            double stockCurrencyRate, double stockCurrencyAmount,
            double gCode, int userCode, string createdBy, string companyId,
            long againstAccCode = 0)
        {
            using var cmd = new SqlCommand("SP_insertjv", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@dacccode", dAccCode));
            cmd.Parameters.Add(new SqlParameter("@cacccode", cAccCode));
            cmd.Parameters.Add(new SqlParameter("@amount", amount));
            cmd.Parameters.Add(new SqlParameter("@JVdate", jvDate));
            cmd.Parameters.Add(new SqlParameter("@duedate", dueDate));
            cmd.Parameters.Add(new SqlParameter("@curcode", curCode ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@vtype", vType ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@vcode", vCode));
            cmd.Parameters.Add(new SqlParameter("@disc", disc ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@Adjustvtype", adjustVType ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@Adjustvcode", adjustVCode));
            cmd.Parameters.Add(new SqlParameter("@BaseRate", baseRate));
            cmd.Parameters.Add(new SqlParameter("@MasterCurrencyRate", masterCurrencyRate));
            cmd.Parameters.Add(new SqlParameter("@MasterCurrencyAmount", masterCurrencyAmount));
            cmd.Parameters.Add(new SqlParameter("@StockCurrencyRate", stockCurrencyRate));
            cmd.Parameters.Add(new SqlParameter("@StockCurrencyAmount", stockCurrencyAmount));
            cmd.Parameters.Add(new SqlParameter("@GCode", gCode));
            cmd.Parameters.Add(new SqlParameter("@UserCode", userCode));
            cmd.Parameters.Add(new SqlParameter("@CreatedBy", createdBy ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@Companyid", companyId));
            cmd.Parameters.Add(new SqlParameter("@AgainstAccCode", againstAccCode));
            cmd.Parameters.Add(new SqlParameter("@ActionType", "insert"));
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task<string> ExecuteCommonDataScalarAsync(SqlConnection conn, SqlTransaction trans, string type, string where = "")
        {
            using var cmd = new SqlCommand("GetAllCommonData", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@Type", type));
            cmd.Parameters.Add(new SqlParameter("@Where", where?.Replace("''", "'") ?? ""));
            return (await cmd.ExecuteScalarAsync())?.ToString() ?? string.Empty;
        }

        private async Task<string> ExecuteCurrencyDetailScalarAsync(SqlConnection conn, SqlTransaction trans, string type, string where = "")
        {
            using var cmd = new SqlCommand("GetCurrencyDetail", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@Type", type));
            cmd.Parameters.Add(new SqlParameter("@Where", where?.Replace("''", "'") ?? ""));
            return (await cmd.ExecuteScalarAsync())?.ToString() ?? string.Empty;
        }

        private static (double MCurAmt, double SCurAmt) GetConvertAmount(
            double curCode, double amount, double mCurBaseRate, string mCurOperator,
            double sCurBaseRate, string sCurOperator, double mCurCode, double sCurCode)
        {
            double mCurAmt = 0, sCurAmt = 0;

            if (curCode == mCurCode)
            {
                mCurAmt = amount;
                sCurAmt = sCurOperator == "*" ? amount * sCurBaseRate : (sCurBaseRate != 0 ? amount / sCurBaseRate : 0);
            }
            else if (curCode == sCurCode)
            {
                sCurAmt = amount;
                mCurAmt = mCurOperator == "*" ? amount * mCurBaseRate : (mCurBaseRate != 0 ? amount / mCurBaseRate : 0);
            }
            else
            {
                mCurAmt = mCurOperator == "*" ? amount * mCurBaseRate : (mCurBaseRate != 0 ? amount / mCurBaseRate : 0);
                sCurAmt = sCurOperator == "*" ? amount * sCurBaseRate : (sCurBaseRate != 0 ? amount / sCurBaseRate : 0);
            }

            if (double.IsNaN(mCurAmt) || double.IsInfinity(mCurAmt)) mCurAmt = 0;
            if (double.IsNaN(sCurAmt) || double.IsInfinity(sCurAmt)) sCurAmt = 0;
            return (mCurAmt, sCurAmt);
        }

        #endregion
    }
}
