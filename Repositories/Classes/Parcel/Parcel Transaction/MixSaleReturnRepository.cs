using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction;
using Microsoft.Data.SqlClient;
using System.Data;
using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;
using StockBasic = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Classes.Parcel.Parcel_Transaction
{
    public class MixSaleReturnRepository : IMixSaleReturnRepository
    {
        private const string TrnType = "CSaleRtn";
        private readonly ISqlDataAccess _sqlDataAccess;

        public MixSaleReturnRepository(ISqlDataAccess sqlDataAccess)
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

        public async Task<List<MixSaleReturnInvListDto>> GetListAsync(int pageNo, string companyId)
        {
            var parameters = new Dictionary<string, object> { { "@PageNo", pageNo } };

            var result = (await _sqlDataAccess.SelectDataFromSP<MixSaleReturnInvListDto>(
                "GetMixSaleReturnParcelInvDataUsingPageNo", parameters, companyId)).ToList();

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

        public async Task<MixSaleReturnPageDataDto> GetPageDataAsync(string companyId)
        {
            var transactionData = await _sqlDataAccess.SelectDataFromSP<StockBasic>(
                "AllMasterDataForPurchase", new Dictionary<string, object>(), companyId);

            var list = transactionData.ToList();

            var itemMasterData = await _sqlDataAccess.SelectDataFromSP<StockBasic>(
                "GetAllMasterDataForItemMaster", new Dictionary<string, object>(), companyId);
            var itemList = itemMasterData.ToList();

            var defaultCurrencyCode = await _sqlDataAccess.ExecuteQuerySingle<string>(
                "SELECT Value FROM TabSettingMaster WHERE Name = 'DefaultCurrency'",
                new { }, companyId);

            return new MixSaleReturnPageDataDto
            {
                Type = MapBasic(list.Where(x => x.Type == "Type")),
                SubType = MapBasic(list.Where(x => x.Type == "SubType")),
                Category = MapBasic(list.Where(x => x.Type == "Category")),
                CurrencyList = MapBasic(list.Where(x => x.Type == "Currency")),
                Address = MapBasic(list.Where(x => x.Type == "Bank")),
                Shape = MapBasic(itemList.Where(x => x.Type == "Shape")),
                Color = MapBasic(itemList.Where(x => x.Type == "Color")),
                Clarity = MapBasic(itemList.Where(x => x.Type == "Clarity")),
                Size = MapBasic(itemList.Where(x => x.Type == "Size")),
                Sieve = MapBasic(itemList.Where(x => x.Type == "Sieve")),
                DefaultCurrencyCode = defaultCurrencyCode
            };
        }

        public async Task<MixSaleReturnDetailDto?> GetDetailAsync(int id, string companyId)
        {
            var header = await _sqlDataAccess.SelectDataFromSPSingle<MixSaleReturnHeaderDto>(
                "GetSalepurRecord",
                new Dictionary<string, object> { { "@Where", $" tsp.TrnType = '{TrnType}' AND Code = {id}" } },
                companyId);

            if (header == null) return null;

            var items = (await _sqlDataAccess.SelectDataFromSP<MixSaleReturnItemDto>(
                "GetSalepurTran",
                new Dictionary<string, object> { { "@Where", $" AND InvCode = {id}" } },
                companyId)).ToList();

            foreach (var item in items)
                NormalizeDetailItem(item);

            return new MixSaleReturnDetailDto { Header = header, Items = items };
        }

        public async Task<List<MixSaleReturnLotDetailDto>> GetLotDetailAsync(string lotNo, string companyId)
        {
            var escapedLotNo = lotNo.Replace("'", "''");
            string sql = $" EXEC [GetLotNoDataForAll] @Type = 'Mix',@Where = ' AND LotNo = ''{escapedLotNo}'' '";
            var result = (await _sqlDataAccess.SelectRawSql<MixSaleReturnLotDetailDto>(sql, companyId)).ToList();

            foreach (var lot in result)
                NormalizeLotDetail(lot);

            return result;
        }

        public async Task<MixSaleReturnGridViewDto> AddItemInGridAsync(
            MixSaleReturnAddLotRequest item, string companyId)
        {
            var list = item.Items ?? new List<MixSaleReturnItemDto>();
            string alert = "";

            float weight = item.Weight != 0 ? item.Weight : item.Carat;
            float selectionCarats = RoundCarat(weight);
            float rate = item.Rate;
            float back = item.Back;
            float mrp = item.MRP;
            bool isJewellery = string.Equals(item.StockType, "Jewellery", StringComparison.OrdinalIgnoreCase);

            if (!((weight != 0 && rate != 0) || (weight == 0 && rate == 0)))
                throw new Exception("Carat or Rate can not be 0.");

            float amount;
            if (rate != 0)
            {
                if (rate != 0 && mrp != 0)
                    back = ((rate * 100) / mrp) - 100;

                amount = isJewellery ? item.Pcs * rate : selectionCarats * rate;
            }
            else
            {
                amount = item.Amount;
            }

            float l1 = item.Less1 != 0 ? amount * (100 + item.Less1) / 100 : amount;
            float l2 = item.Less2 != 0 ? l1 * (100 + item.Less2) / 100 : l1;
            float l3 = item.Less3 != 0 ? l2 * (100 + item.Less3) / 100 : l2;

            var row = new MixSaleReturnItemDto
            {
                LotNo = item.LotNo,
                ItemName = item.ItemName,
                Shape = item.ShapeId,
                ShapeId = item.ShapeId,
                PShape = item.ShapeId,
                Size = item.SizeId,
                PSize = item.SizeId,
                Color = item.ColorId,
                ColorId = item.ColorId,
                PColor = item.ColorId,
                Clarity = item.ClarityId,
                ClarityId = item.ClarityId,
                PClarity = item.ClarityId,
                Sieve = item.SieveId,
                SieveId = item.SieveId,
                PSieve = item.SieveId,
                Weight = selectionCarats,
                IssueCarats = item.IssueCarats != 0 ? item.IssueCarats : selectionCarats,
                ReturnCarats = item.ReturnCarats,
                Pcs = item.Pcs,
                MRP = mrp,
                COGS = item.COGS,
                Rate = rate,
                SRate = item.SRate,
                Back = back,
                Less1 = item.Less1,
                Less2 = item.Less2,
                Less3 = item.Less3,
                Amount = l3,
                ExportNo = item.ExportNo,
                MemoTransCode = item.MemoTransCode,
                StockType = string.IsNullOrWhiteSpace(item.StockType) ? "Diamond" : item.StockType
            };

            bool isEdit = item.editIndex >= 0 && item.editIndex < list.Count;
            if (isEdit)
                list[item.editIndex] = row;
            else
                list.Add(row);

            return BuildGridView(list, alert, isEdit);
        }

        public async Task<MixSaleReturnGridViewDto> GetItemFromListAsync(
            MixSaleReturnGetItemFromListRequest request, string companyId)
        {
            var list = request.Items ?? new List<MixSaleReturnItemDto>();
            string alert = "";

            string strWhere = " AND LotNo IN ('''')";
            if (!string.IsNullOrWhiteSpace(request.LotNo))
            {
                var tmp = request.LotNo.Replace("\r\n", ",").Replace("\n", ",");
                tmp = tmp.Replace(",", "'',''");
                strWhere = $" AND LotNo IN (''{tmp}'')";
            }

            string sql = $" EXEC [GetLotNoDataForAll] @Type = 'PSale',@Where = '{strWhere}'";
            var lots = (await _sqlDataAccess.SelectRawSql<MixSaleRtnLotLookupRow>(sql, companyId)).ToList();

            foreach (var lotRow in lots)
            {
                NormalizeLotDetail(lotRow);

                float amount = lotRow.Amount;
                float l1 = request.Less1 != 0 ? amount * (100 + request.Less1) / 100 : amount;
                float l2 = request.Less2 != 0 ? l1 * (100 + request.Less2) / 100 : l1;
                float l3 = request.Less3 != 0 ? l2 * (100 + request.Less3) / 100 : l2;

                if (list.Any(x => string.Equals(x.LotNo, lotRow.LotNo, StringComparison.OrdinalIgnoreCase)))
                {
                    alert += " LotNo Already :" + lotRow.LotNo;
                    continue;
                }

                list.Add(new MixSaleReturnItemDto
                {
                    LotNo = lotRow.LotNo,
                    ItemName = lotRow.ItemName,
                    Weight = lotRow.Weight,
                    IssueCarats = lotRow.IssueCarats != 0 ? lotRow.IssueCarats : lotRow.Weight,
                    ReturnCarats = 0,
                    Pcs = lotRow.Pcs,
                    MRP = lotRow.MRP,
                    Back = lotRow.Back,
                    Rate = lotRow.Rate,
                    Less1 = request.Less1,
                    Less2 = request.Less2,
                    Less3 = request.Less3,
                    Amount = l3,
                    ExportNo = lotRow.ExportNo,
                    MemoTransCode = lotRow.MemoTransCode,
                    Shape = lotRow.Shape,
                    ShapeId = lotRow.ShapeId,
                    Color = lotRow.Color,
                    ColorId = lotRow.ColorId,
                    Clarity = lotRow.Clarity,
                    ClarityId = lotRow.ClarityId,
                    Size = lotRow.Size,
                    Sieve = lotRow.Sieve,
                    SieveId = lotRow.SieveId,
                    PShape = lotRow.ShapeId ?? lotRow.Shape,
                    PColor = lotRow.ColorId ?? lotRow.Color,
                    PSize = lotRow.PSize ?? lotRow.Size,
                    PSieve = lotRow.SieveId ?? lotRow.Sieve ?? lotRow.PSieve,
                    PClarity = lotRow.PClarity ?? lotRow.ClarityId ?? lotRow.Clarity,
                    StockType = lotRow.StockType
                });
            }

            return BuildGridView(list, alert, false);
        }

        public async Task<(bool Success, string Message)> AddAsync(
            MixSaleReturnHeaderDto salePur,
            List<MixSaleReturnItemDto> items,
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

                int maxInvNo = int.Parse(await ExecuteCommonDataScalarAsync(conn, trans, "Inv", $" And TrnType=''{TrnType}'' ")) + 1;

                bool outstanding = salePur.chkoldsale;
                bool stockEffected = !salePur.chkoldsale;

                await InsertSalePurAsync(conn, trans, maxInvNo, salePur, ctx, outstanding, stockEffected, usercode, userId, companyId);
                int maxSPCode = int.Parse(await ExecuteCommonDataScalarAsync(conn, trans, "SPCode"));

                await ProcessReturnItemsAsync(conn, trans, salePur, items, maxSPCode, ctx, stockEffected, usercode, userId, companyId, isAdd: true);

                if (!salePur.chkoldsale)
                    await InsertMixSaleRtnJvEntriesAsync(conn, trans, salePur, maxSPCode, ctx, usercode, userId, companyId);

                trans.Commit();
                return (true, "Mix Sale Return Record Saved.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> EditAsync(
            MixSaleReturnHeaderDto salePur,
            List<MixSaleReturnItemDto> items,
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

                bool outstanding = salePur.chkoldsale;
                bool stockEffected = !salePur.chkoldsale;

                await UpdateSalePurAsync(conn, trans, code, invNo, salePur, ctx, outstanding, stockEffected, usercode, userId, companyId);

                using (var cmd = new SqlCommand("SP_DeleteSalePurTran", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@InvCode", code));
                    await cmd.ExecuteNonQueryAsync();
                }

                await ProcessReturnItemsAsync(conn, trans, salePur, items, code, ctx, stockEffected, usercode, userId, companyId, isAdd: false);

                using (var cmd = new SqlCommand("SP_DeleteJV", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@vcode", code));
                    cmd.Parameters.Add(new SqlParameter("@vtype", TrnType));
                    await cmd.ExecuteNonQueryAsync();
                }

                if (!salePur.chkoldsale)
                    await InsertMixSaleRtnJvEntriesAsync(conn, trans, salePur, code, ctx, usercode, userId, companyId);

                trans.Commit();
                return (true, "Mix Sale Return Record Updated.");
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
                    cmd.Parameters.Add(new SqlParameter("@vtype", TrnType));
                    await cmd.ExecuteNonQueryAsync();
                }

                trans.Commit();
                return (true, "Mix Sale Return Record Deleted.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return (false, ex.Message);
            }
        }

        public async Task<MixSaleReturnPrintViewDto?> GetPrintViewAsync(string invNo, string companyId)
        {
            string where;
            if (int.TryParse(invNo, out var codeId))
                where = $"tsp.TrnType = ''{TrnType}'' AND tsp.Code = {codeId}";
            else
                where = $"tsp.TrnType = ''{TrnType}'' AND tsp.InvNo = ''{invNo.Replace("'", "''")}''";

            string sql = $"EXEC GetMixSalepurRecordWithDetailsForINV @Where = '{where}'";

            var rows = (await _sqlDataAccess.SelectRawSql<PrintInvRow>(sql, companyId)).ToList();
            if (rows.Count == 0 && int.TryParse(invNo, out _))
            {
                where = $"tsp.TrnType = ''{TrnType}'' AND tsp.InvNo = ''{invNo.Replace("'", "''")}''";
                sql = $"EXEC GetMixSalepurRecordWithDetailsForINV @Where = '{where}'";
                rows = (await _sqlDataAccess.SelectRawSql<PrintInvRow>(sql, companyId)).ToList();
            }

            if (rows.Count == 0)
                return null;

            var header = rows[0];
            var result = new MixSaleReturnPrintViewDto
            {
                InvNo = header.InvNo,
                BillNo = header.BillNo,
                MemoNo = header.MemoNo,
                TrnDate = header.TrnDate.HasValue ? header.TrnDate.Value.ToString("d-MMM-yyyy") : null,
                RoundDate = header.RoundDate.HasValue ? header.RoundDate.Value.ToString("d-MMM-yyyy") : null,
                DueDate = header.DueDate.HasValue ? header.DueDate.Value.ToString("dd-MMM-yyyy") : null,
                DueDays = header.DueDays != 0 ? header.DueDays.ToString() : (header.DueDaysText ?? ""),
                PartyName = header.PartyName,
                BrokerName = header.BrokerName,
                AddressLine1 = header.AddressLine1,
                AddressLine2 = header.AddressLine2,
                AddressLine3 = header.AddressLine3,
                ShipVia = header.ShipVia,
                SalesPersons = header.SalesPersons ?? header.SalesPerson,
                Tracking = header.Tracking,
                PlusMinusAmount = header.PlusMinusAmount,
                BaseRate = header.BaseRate,
                FinalAmount = header.FinalAmount,
                bnm = header.bnm,
                BankAccNo = header.BankAccNo,
                ABARoutingNumber = header.ABARoutingNumber,
                SwiftCode = header.SwiftCode,
                IbanNo = header.IbanNo,
                ZelleID = header.ZelleID,
                CompnyTaxRegisterNumber = header.CompnyTaxRegisterNumber,
                Items = new List<MixSaleReturnPrintItemDto>()
            };

            foreach (var dr in rows)
            {
                var lot = dr.LotNo ?? dr.lotno;
                var itemName = dr.ItemName;
                result.Items.Add(new MixSaleReturnPrintItemDto
                {
                    InvNo = header.InvNo,
                    Lotno = lot,
                    StockNo = lot,
                    ItemName = itemName,
                    Description = itemName,
                    Pcs = dr.Pcs,
                    Qty = dr.Pcs,
                    Carats = dr.Carats,
                    Weight = dr.Carats,
                    Rate = dr.Rate,
                    PricePerCarat = dr.Rate,
                    MasterCurrencyAmount = dr.MasterCurrencyAmount,
                    Amount = dr.MasterCurrencyAmount
                });
            }

            return result;
        }

        public async Task<List<object>> GetLotsAsync(string? searchBy, string companyId)
        {
            // MVC: MixController GetMixLotNoList — require 1+ chars, return { id = Name, text = TextName }
            if (string.IsNullOrWhiteSpace(searchBy) || searchBy.Trim().Length < 1)
                return new List<object>();

            string safe = searchBy.Trim().Replace("'", "''");
            string strWhere = $" AND (LotNo LIKE ''%{safe}%'' OR Itemname LIKE ''%{safe}%'') ";
            string sql = $"EXEC [GetMixlotDataForDropdown] @strWhere = '{strWhere}'";
            var rows = (await _sqlDataAccess.SelectRawSql<MixLotDropdownRow>(sql, companyId)).ToList();

            return rows.Select(r =>
            {
                var lotNo = r.Name ?? r.LotNo ?? r.ID ?? r.Id ?? "";
                var text = r.TextName ?? r.ItemName ?? r.Name ?? r.LotNo ?? lotNo;
                return (object)new { id = lotNo, text };
            }).ToList();
        }

        public async Task<List<MixSaleReturnCSaleLineDto>> GetCSaleForCSaleRtnAsync(string? partyCode, string? memoNo, string companyId)
        {
            // MVC: Sp_SelectCSaleForCSaleRtn
            string party = (partyCode ?? "").Replace("'", "''");
            string invNo = (memoNo ?? "").Replace("'", "''");
            string sql = $" EXEC [Sp_SelectCSaleForCSaleRtn] @Party = '{party}',@invno = '{invNo}' ";
            return (await _sqlDataAccess.SelectRawSql<MixSaleReturnCSaleLineDto>(sql, companyId)).ToList();
        }

        public Task<MixSaleReturnGridViewDto> FillFromCSaleAsync(
            List<MixSaleReturnCSaleFillLineRequest> lines,
            List<MixSaleReturnItemDto> existingItems,
            string companyId)
        {
            // MVC: CSaleToCSaleRtnFill — merge checked CSale lines into return item grid
            var list = existingItems?.ToList() ?? new List<MixSaleReturnItemDto>();
            string alert = "";

            foreach (var im in lines ?? new())
            {
                if (!im.Chk) continue;

                if (string.IsNullOrWhiteSpace(im.LotNo) || im.Weight < 0 || im.Rate == 0)
                    return Task.FromResult(new MixSaleReturnGridViewDto
                    {
                        Alert = "invalid data pleasee Eneter valid Data.",
                        ExcelData = list
                    });

                float rate = im.Rate;
                float amount;
                if (string.Equals(im.StockType, "Jewellery", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(im.StockType, "Diamond", StringComparison.OrdinalIgnoreCase) ||
                    string.IsNullOrWhiteSpace(im.StockType))
                {
                    amount = im.Weight * rate;
                }
                else
                {
                    rate = 0;
                    amount = im.Rate;
                }

                float l1 = im.Less1 != 0 ? amount * (100 + im.Less1) / 100f : amount;
                float l2 = im.Less2 != 0 ? l1 * (100 + im.Less2) / 100f : l1;
                float l3 = im.Less3 != 0 ? l2 * (100 + im.Less3) / 100f : l2;

                if (im.Weight < 0.05f && im.Weight > 0)
                {
                    alert += " Lotno On Memo or Sold: " + im.LotNo;
                    continue;
                }

                float issue = im.IssueCarats != 0 ? im.IssueCarats : im.Weight;
                float rejection = im.ReturnCarats;
                float rejP = issue != 0 ? (rejection * 100f / issue) : 0f;

                list.Add(new MixSaleReturnItemDto
                {
                    SrNo = list.Count + 1,
                    LotNo = im.LotNo,
                    ItemName = im.ItemName ?? im.LotNo,
                    Weight = im.Weight,
                    IssueCarats = issue,
                    ReturnCarats = rejection,
                    Back = rejP,
                    Pcs = im.Pcs,
                    Rate = rate,
                    SRate = 0,
                    Less1 = im.Less1,
                    Less2 = im.Less2,
                    Less3 = im.Less3,
                    Amount = l3,
                    ExportNo = im.ExportNo ?? "",
                    MemoTransCode = im.MemoTransCode,
                    Shape = im.PShape,
                    ShapeId = im.PShape,
                    PShape = im.PShape,
                    Color = im.PColor,
                    ColorId = im.PColor,
                    PColor = im.PColor,
                    Size = im.PSize,
                    PSize = im.PSize,
                    Clarity = im.PClarity,
                    ClarityId = im.PClarity,
                    PClarity = im.PClarity,
                    Sieve = im.PSieve,
                    SieveId = im.PSieve,
                    PSieve = im.PSieve,
                    StockType = string.IsNullOrWhiteSpace(im.StockType) ? "Diamond" : im.StockType
                });
            }

            var grid = BuildGridView(list, alert, false);
            return Task.FromResult(grid);
        }

        #region Grid helpers

        private static MixSaleReturnGridViewDto BuildGridView(
            List<MixSaleReturnItemDto> list, string alert, bool isEdit)
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

            return new MixSaleReturnGridViewDto
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

        private static void NormalizeLotDetail(MixSaleReturnLotDetailDto lot)
        {
            if (lot == null) return;

            if (lot.Weight == 0 && float.TryParse(lot.Carats, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var carats) && carats != 0)
                lot.Weight = carats;

            if (string.IsNullOrWhiteSpace(lot.Shape) && !string.IsNullOrWhiteSpace(lot.ShapeId))
                lot.Shape = lot.ShapeId;

            if (string.IsNullOrWhiteSpace(lot.Color) && !string.IsNullOrWhiteSpace(lot.ColorId))
                lot.Color = lot.ColorId;

            if (string.IsNullOrWhiteSpace(lot.Clarity) && !string.IsNullOrWhiteSpace(lot.ClarityId))
                lot.Clarity = lot.ClarityId;
        }

        private static void NormalizeDetailItem(MixSaleReturnItemDto item)
        {
            if (item == null) return;

            // GetSalepurTran may return MemoTranCode instead of MemoTransCode (same as SolitaireSaleReturn)
            if (item.MemoTransCode == 0 && item.MemoTranCode != 0)
                item.MemoTransCode = item.MemoTranCode;

            if (item.IssueCarats == 0 && item.Weight != 0)
                item.IssueCarats = item.Weight;

            if (string.IsNullOrWhiteSpace(item.Shape) && !string.IsNullOrWhiteSpace(item.ShapeId))
                item.Shape = item.ShapeId;

            if (string.IsNullOrWhiteSpace(item.Color) && !string.IsNullOrWhiteSpace(item.ColorId))
                item.Color = item.ColorId;

            if (string.IsNullOrWhiteSpace(item.Clarity) && !string.IsNullOrWhiteSpace(item.ClarityId))
                item.Clarity = item.ClarityId;

            if (string.IsNullOrWhiteSpace(item.PShape))
                item.PShape = item.ShapeId ?? item.Shape;

            if (string.IsNullOrWhiteSpace(item.PColor))
                item.PColor = item.ColorId ?? item.Color;

            if (string.IsNullOrWhiteSpace(item.PClarity))
                item.PClarity = item.ClarityId ?? item.Clarity;

            if (string.IsNullOrWhiteSpace(item.PSize))
                item.PSize = item.Size;

            if (string.IsNullOrWhiteSpace(item.PSieve))
                item.PSieve = item.SieveId ?? item.Sieve;
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

        private async Task<CurrencyContext> BuildCurrencyContextAsync(
            SqlConnection conn, SqlTransaction trans, MixSaleReturnHeaderDto salePur)
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
            MixSaleReturnHeaderDto salePur, CurrencyContext ctx,
            bool outstanding, bool stockEffected,
            int usercode, string userId, string companyId)
        {
            string billNo = string.IsNullOrWhiteSpace(salePur.BillNo) ? maxInvNo.ToString() : salePur.BillNo;

            using var cmd = new SqlCommand("SP_InsertSalePur", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@invno", maxInvNo));
            cmd.Parameters.Add(new SqlParameter("@billno", billNo));
            cmd.Parameters.Add(new SqlParameter("@trntype", TrnType));
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
            cmd.Parameters.Add(new SqlParameter("@RoundDate", salePur.DueDate));
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
            cmd.Parameters.Add(new SqlParameter("@Address", salePur.Address));
            cmd.Parameters.Add(new SqlParameter("@ActionType", "insert"));
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task UpdateSalePurAsync(
            SqlConnection conn, SqlTransaction trans, int code, int invNo,
            MixSaleReturnHeaderDto salePur, CurrencyContext ctx,
            bool outstanding, bool stockEffected,
            int usercode, string userId, string companyId)
        {
            using var cmd = new SqlCommand("SP_UpdateSalePur", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@code", code));
            cmd.Parameters.Add(new SqlParameter("@invno", invNo));
            cmd.Parameters.Add(new SqlParameter("@ModifyBy", userId ?? ""));
            cmd.Parameters.Add(new SqlParameter("@BillNo", salePur.BillNo ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@TrnType", TrnType));
            cmd.Parameters.Add(new SqlParameter("@invtype", salePur.InvType ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@SubType", salePur.SubType ?? ""));
            cmd.Parameters.Add(new SqlParameter("@BankID", salePur.Address));
            cmd.Parameters.Add(new SqlParameter("@Category", salePur.Category ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@Trndate", salePur.TrnDate));
            cmd.Parameters.Add(new SqlParameter("@RoundDate", salePur.DueDate));
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

        private async Task ProcessReturnItemsAsync(
            SqlConnection conn, SqlTransaction trans,
            MixSaleReturnHeaderDto salePur,
            List<MixSaleReturnItemDto> items,
            int invCode, CurrencyContext ctx,
            bool stockEffected, int usercode, string userId, string companyId,
            bool isAdd)
        {
            foreach (var im in items)
            {
                var amtRes = GetConvertAmount(ctx.CurCode, im.Amount, ctx.MCurBaseRate, ctx.MCurOperator, ctx.SCurBaseRate, ctx.SCurOperator, ctx.MCurCode, ctx.SCurCode);

                int maxItemCode = 0;
                if (string.Equals(im.StockType, "Jewellery", StringComparison.OrdinalIgnoreCase))
                {
                    string itemCodeStr = await ExecuteCommonDataScalarAsync(conn, trans, "StockCode", $" AND Stock_ID = ''{im.LotNo}''");
                    int.TryParse(itemCodeStr, out maxItemCode);
                }
                else
                {
                    string itemCodeStr = await ExecuteCommonDataScalarAsync(conn, trans, "ItemCode", $" AND LotNo = ''{im.LotNo}''");
                    int.TryParse(itemCodeStr, out maxItemCode);
                }

                string itemName = !string.IsNullOrWhiteSpace(im.ItemName) ? im.ItemName : im.LotNo ?? "";

                float issueCarats = im.IssueCarats != 0 ? im.IssueCarats : im.Weight;
                string pShape = im.PShape ?? im.Shape ?? im.ShapeId ?? "";
                string pSize = im.PSize ?? im.Size ?? "";
                string pSieve = im.PSieve ?? im.Sieve ?? im.SieveId ?? "";
                string pColor = im.PColor ?? im.Color ?? im.ColorId ?? "";
                string pClarity = im.PClarity ?? im.Clarity ?? im.ClarityId ?? "";

                // Ensure MemoTranCode alias is applied before save (GetSalepurTran / fill payloads)
                float memoTransCode = im.MemoTransCode != 0 ? im.MemoTransCode : im.MemoTranCode;

                using (var cmd = new SqlCommand("SP_InsertSalePurTran", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SPdate", salePur.TrnDate));
                    cmd.Parameters.Add(new SqlParameter("@TrnType", TrnType));
                    cmd.Parameters.Add(new SqlParameter("@InvType", salePur.InvType ?? ""));
                    cmd.Parameters.Add(new SqlParameter("@InvCode", invCode));
                    cmd.Parameters.Add(new SqlParameter("@ItemCode", maxItemCode));
                    cmd.Parameters.Add(new SqlParameter("@ItemName", itemName));
                    cmd.Parameters.Add(new SqlParameter("@Carats", float.Parse(im.Weight.ToString("N3"))));
                    cmd.Parameters.Add(new SqlParameter("@Pcs", im.Pcs));
                    cmd.Parameters.Add(new SqlParameter("@Rate", Convert.ToDouble(im.Rate.ToString("N2"))));
                    cmd.Parameters.Add(new SqlParameter("@MRP", im.MRP));
                    cmd.Parameters.Add(new SqlParameter("@PMRPDown", Convert.ToDouble(im.Back.ToString("N2"))));
                    cmd.Parameters.Add(new SqlParameter("@ReturnCarats", Convert.ToDouble(im.ReturnCarats.ToString("N2"))));
                    cmd.Parameters.Add(new SqlParameter("@IssueCarats", Convert.ToDouble(issueCarats.ToString("N2"))));
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
                    cmd.Parameters.Add(new SqlParameter("@PShape", pShape));
                    cmd.Parameters.Add(new SqlParameter("@PSize", pSize));
                    cmd.Parameters.Add(new SqlParameter("@PSieve", pSieve));
                    cmd.Parameters.Add(new SqlParameter("@PColor", pColor));
                    cmd.Parameters.Add(new SqlParameter("@PClarity", pClarity));
                    cmd.Parameters.Add(new SqlParameter("@COGS", im.COGS));
                    cmd.Parameters.Add(new SqlParameter("@StockType", im.StockType ?? "Diamond"));
                    cmd.Parameters.Add(new SqlParameter("@ActionType", "insert"));
                    await cmd.ExecuteNonQueryAsync();
                }

                if (memoTransCode != 0)
                {
                    // Match MVC MixSaleReturnPartialController exactly:
                    // UpdateMemoForSale(code, date, ReturnCarats, Weight, action)
                    //   → method args (spcts, rtncts) → SP @spcts=ReturnCarats, @rtncts=Weight
                    // Add:  MemoTransCode != 0 → "csalertntest"
                    // Edit: MemoTransCode != 0 → "csalertn"
                    string memoAction = isAdd ? "csalertntest" : "csalertn";
                    using var cmd = new SqlCommand("SP_UpdateMemo", conn, trans);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@code", memoTransCode));
                    cmd.Parameters.Add(new SqlParameter("@RtnDate", salePur.TrnDate));
                    cmd.Parameters.Add(new SqlParameter("@rtncts", float.Parse(im.Weight.ToString("N3"))));
                    cmd.Parameters.Add(new SqlParameter("@spcts", float.Parse(im.ReturnCarats.ToString("N3"))));
                    cmd.Parameters.Add(new SqlParameter("@Actiontype", memoAction));
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task InsertMixSaleRtnJvEntriesAsync(
            SqlConnection conn, SqlTransaction trans,
            MixSaleReturnHeaderDto salePur, int maxSPCode,
            CurrencyContext ctx, int usercode, string userId, string companyId)
        {
            long saleCode = long.Parse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''SALE''"));
            long vatCode = long.Parse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''Vat''"));
            long sgstCode = long.Parse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''SGST''"));
            long cgstCode = long.Parse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''CGST''"));
            long igstCode = long.Parse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''IGST''"));
            long brokerageCode = long.Parse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''Brokerage''"));
            long adatAccCode = long.Parse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''Adat''"));
            long cashCode = long.Parse(await ExecuteCommonDataScalarAsync(conn, trans, "Party", " And AccGrpName=''Cash''"));

            float saleJvAmount = salePur.FinalMAmount;
            var jvConvert = GetConvertAmount(ctx.MCurCode, saleJvAmount, 1, ctx.MCurOperatorM, ctx.MToSCurRate, ctx.SCurOperatorM, ctx.MCurCode, ctx.SCurCode);

            float saleJvAmountF = ctx.MCurOperator == "*"
                ? (ctx.MCurBaseRate != 0 ? saleJvAmount / ctx.MCurBaseRate : 0f)
                : saleJvAmount * ctx.MCurBaseRate;

            // Main JV: Debit Sale, Credit Party (MVC MixSaleReturn)
            await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, saleCode, salePur.PartyCode,
                Math.Abs(saleJvAmountF), ctx.CurCode.ToString(), TrnType, maxSPCode, salePur.InvDisc ?? "", 0,
                salePur.BaseRate, ctx.MCurBaseRate, jvConvert.MCurAmt, ctx.SCurBaseRate, jvConvert.SCurAmt, usercode, userId, companyId);

            if (Convert.ToDouble(salePur.BrokerAmount) != 0 && salePur.BrokerCode != null)
            {
                var broConvert = GetConvertAmount(ctx.MCurCode, salePur.BrokerAmount, 1, ctx.MCurOperatorM, ctx.MToSCurRate, ctx.SCurOperatorM, ctx.MCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, salePur.BrokerCode.Value, brokerageCode,
                    Math.Abs(salePur.BrokerAmount), ctx.MCurCode.ToString(), TrnType, maxSPCode, salePur.InvDisc ?? "", 0,
                    salePur.BaseRate, ctx.MCurBaseRate, broConvert.MCurAmt, ctx.SCurBaseRate, broConvert.SCurAmt, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.AdatAmount) != 0 && salePur.AdatCode != null)
            {
                var adatConvert = GetConvertAmount(ctx.MCurCode, salePur.AdatAmount, 1, ctx.MCurOperatorM, ctx.MToSCurRate, ctx.SCurOperatorM, ctx.MCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, salePur.AdatCode.Value, adatAccCode,
                    salePur.AdatAmount, ctx.MCurCode.ToString(), TrnType, maxSPCode, salePur.InvDisc ?? "", 0,
                    salePur.BaseRate, ctx.MCurBaseRate, adatConvert.MCurAmt, ctx.SCurBaseRate, adatConvert.SCurAmt, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.VatAmount) != 0)
            {
                var vatConvert = GetConvertAmount(ctx.MCurCode, salePur.VatAmount, 1, ctx.MCurOperatorM, ctx.MToSCurRate, ctx.SCurOperatorM, ctx.MCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, vatCode, saleCode,
                    salePur.VatAmount, ctx.MCurCode.ToString(), TrnType, maxSPCode, salePur.InvDisc ?? "", 0,
                    salePur.BaseRate, ctx.MCurBaseRate, vatConvert.MCurAmt, ctx.SCurBaseRate, vatConvert.SCurAmt, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.SGSTAmt) != 0)
            {
                var sgstConvert = GetConvertAmount(ctx.MCurCode, salePur.SGSTAmt, 1, ctx.MCurOperatorM, ctx.MToSCurRate, ctx.SCurOperatorM, ctx.MCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, sgstCode, saleCode,
                    salePur.SGSTAmt, ctx.MCurCode.ToString(), TrnType, maxSPCode, salePur.InvDisc ?? "", 0,
                    salePur.BaseRate, ctx.MCurBaseRate, sgstConvert.MCurAmt, ctx.SCurBaseRate, sgstConvert.SCurAmt, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.CGSTAmt) != 0)
            {
                var cgstConvert = GetConvertAmount(ctx.MCurCode, salePur.CGSTAmt, 1, ctx.MCurOperatorM, ctx.MToSCurRate, ctx.SCurOperatorM, ctx.MCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, cgstCode, saleCode,
                    salePur.CGSTAmt, ctx.MCurCode.ToString(), TrnType, maxSPCode, salePur.InvDisc ?? "", 0,
                    salePur.BaseRate, ctx.MCurBaseRate, cgstConvert.MCurAmt, ctx.SCurBaseRate, cgstConvert.SCurAmt, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.IGSTAmt) != 0)
            {
                var igstConvert = GetConvertAmount(ctx.MCurCode, salePur.IGSTAmt, 1, ctx.MCurOperatorM, ctx.MToSCurRate, ctx.SCurOperatorM, ctx.MCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, igstCode, saleCode,
                    salePur.IGSTAmt, ctx.MCurCode.ToString(), TrnType, maxSPCode, salePur.InvDisc ?? "", 0,
                    salePur.BaseRate, ctx.MCurBaseRate, igstConvert.MCurAmt, ctx.SCurBaseRate, igstConvert.SCurAmt, usercode, userId, companyId);
            }

            if (Convert.ToDouble(salePur.RoundAmount) > 0)
            {
                var roundConvert = GetConvertAmount(ctx.MCurCode, salePur.RoundAmount, 1, ctx.MCurOperatorM, ctx.MToSCurRate, ctx.SCurOperatorM, ctx.MCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, cashCode, salePur.PartyCode,
                    salePur.RoundAmount, ctx.MCurCode.ToString(), TrnType, maxSPCode, salePur.InvDisc ?? "", 0,
                    salePur.BaseRate, ctx.MCurBaseRate, roundConvert.MCurAmt, ctx.SCurBaseRate, roundConvert.SCurAmt, usercode, userId, companyId);
            }
            else if (Convert.ToDouble(salePur.RoundAmount) < 0)
            {
                var roundConvert = GetConvertAmount(ctx.MCurCode, salePur.RoundAmount, 1, ctx.MCurOperatorM, ctx.MToSCurRate, ctx.SCurOperatorM, ctx.MCurCode, 0);
                await InsertJvAsync(conn, trans, salePur.TrnDate, salePur.DueDate, salePur.PartyCode, cashCode,
                    Math.Abs(salePur.RoundAmount), ctx.MCurCode.ToString(), TrnType, maxSPCode, salePur.InvDisc ?? "", 0,
                    salePur.BaseRate, ctx.MCurBaseRate, Math.Abs(roundConvert.MCurAmt), ctx.SCurBaseRate, Math.Abs(roundConvert.SCurAmt), usercode, userId, companyId);
            }
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

        private async Task InsertJvAsync(
            SqlConnection conn, SqlTransaction trans, DateTime jvDate, DateTime dueDate,
            long dAccCode, long cAccCode, double amount, string curCode, string vType, int vCode,
            string disc, int adjustVCode, double baseRate, double masterCurrencyRate, double masterCurrencyAmount,
            double stockCurrencyRate, double stockCurrencyAmount, int userCode, string createdBy, string companyId)
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
            cmd.Parameters.Add(new SqlParameter("@Adjustvtype", (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@Adjustvcode", adjustVCode));
            cmd.Parameters.Add(new SqlParameter("@BaseRate", baseRate));
            cmd.Parameters.Add(new SqlParameter("@MasterCurrencyRate", masterCurrencyRate));
            cmd.Parameters.Add(new SqlParameter("@MasterCurrencyAmount", masterCurrencyAmount));
            cmd.Parameters.Add(new SqlParameter("@StockCurrencyRate", stockCurrencyRate));
            cmd.Parameters.Add(new SqlParameter("@StockCurrencyAmount", stockCurrencyAmount));
            cmd.Parameters.Add(new SqlParameter("@GCode", 0));
            cmd.Parameters.Add(new SqlParameter("@UserCode", userCode));
            cmd.Parameters.Add(new SqlParameter("@CreatedBy", createdBy ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@Companyid", companyId));
            cmd.Parameters.Add(new SqlParameter("@AgainstAccCode", 0));
            cmd.Parameters.Add(new SqlParameter("@ActionType", "insert"));
            await cmd.ExecuteNonQueryAsync();
        }

        #endregion

        #region Raw row DTOs

        private class MixSaleRtnLotLookupRow : MixSaleReturnLotDetailDto
        {
            public float MemoTransCode { get; set; }
            public string? ExportNo { get; set; }
            public string? PSize { get; set; }
            public string? PSieve { get; set; }
            public string? PClarity { get; set; }
        }

        private class PrintInvRow
        {
            public string? InvNo { get; set; }
            public string? BillNo { get; set; }
            public string? MemoNo { get; set; }
            public DateTime? TrnDate { get; set; }
            public DateTime? RoundDate { get; set; }
            public DateTime? DueDate { get; set; }
            public short DueDays { get; set; }
            public string? DueDaysText { get; set; }
            public string? PartyName { get; set; }
            public string? BrokerName { get; set; }
            public string? AddressLine1 { get; set; }
            public string? AddressLine2 { get; set; }
            public string? AddressLine3 { get; set; }
            public string? ShipVia { get; set; }
            public string? SalesPersons { get; set; }
            public string? SalesPerson { get; set; }
            public string? Tracking { get; set; }
            public double PlusMinusAmount { get; set; }
            public double BaseRate { get; set; }
            public double FinalAmount { get; set; }
            public string? bnm { get; set; }
            public string? BankAccNo { get; set; }
            public string? ABARoutingNumber { get; set; }
            public string? SwiftCode { get; set; }
            public string? IbanNo { get; set; }
            public string? ZelleID { get; set; }
            public string? CompnyTaxRegisterNumber { get; set; }
            public string? LotNo { get; set; }
            public string? lotno { get; set; }
            public string? ItemName { get; set; }
            public int Pcs { get; set; }
            public double Carats { get; set; }
            public double Rate { get; set; }
            public double MasterCurrencyAmount { get; set; }
        }

        private class MixLotDropdownRow
        {
            public string? ID { get; set; }
            public string? Id { get; set; }
            public string? Name { get; set; }
            public string? TextName { get; set; }
            public string? LotNo { get; set; }
            public string? ItemName { get; set; }
        }

        #endregion
    }
}
