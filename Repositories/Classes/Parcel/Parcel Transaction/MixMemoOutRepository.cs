using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Data;
using BasicDataDto = Dia_CoreAPI.DTOs.BasicDataDto;
using StockBasic = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Classes.Parcel.Parcel_Transaction
{
    public class MixMemoOutRepository : IMixMemoOutRepository
    {
        private const string TrnType = "CMemoOut";
        private readonly ISqlDataAccess _sqlDataAccess;

        public MixMemoOutRepository(ISqlDataAccess sqlDataAccess)
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

        public async Task<List<MixMemoOutInvListDto>> GetListAsync(int pageNo, string companyId)
        {
            var parameters = new Dictionary<string, object> { { "@PageNo", pageNo } };

            var result = (await _sqlDataAccess.SelectDataFromSP<MixMemoOutInvListDto>(
                "GetMixMemoOutParcelInvDataUsingPageNo", parameters, companyId)).ToList();

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

        public async Task<MixMemoOutPageDataDto> GetPageDataAsync(string companyId)
        {
            var transactionData = await _sqlDataAccess.SelectDataFromSP<StockBasic>(
                "AllMasterDataForPurchase", new Dictionary<string, object>(), companyId);

            var list = transactionData.ToList();
            var defaultCurrencyCode = await _sqlDataAccess.ExecuteQuerySingle<string>(
                "SELECT Value FROM TabSettingMaster WHERE Name = 'DefaultCurrency'",
                new { }, companyId);

            return new MixMemoOutPageDataDto
            {
                Type = MapBasic(list.Where(x => x.Type == "Type")),
                SubType = MapBasic(list.Where(x => x.Type == "SubType")),
                Category = MapBasic(list.Where(x => x.Type == "Category")),
                CurrencyList = MapBasic(list.Where(x => x.Type == "Currency")),
                DefaultCurrencyCode = defaultCurrencyCode
            };
        }

        public async Task<MixMemoOutDetailDto?> GetDetailAsync(int id, string companyId)
        {
            var header = await _sqlDataAccess.SelectDataFromSPSingle<MixMemoOutHeaderDto>(
                "GetSalepurRecord",
                new Dictionary<string, object> { { "@Where", $" tsp.TrnType = '{TrnType}' AND Code = {id}" } },
                companyId);

            if (header == null) return null;

            NormalizeHeader(header);

            // MVC EditMemoD uses GetSalepurTran_SaleWithStatus → GetSalepurTran_MemoWithStatus with InvCode only
            string itemsSql =
                $" EXEC [GetSalepurTran_MemoWithStatus] @Where = ' AND InvCode = {id}'";

            var items = (await _sqlDataAccess.SelectRawSql<MixMemoOutItemDto>(itemsSql, companyId)).ToList();

            foreach (var item in items)
            {
                bool isParcelLot = string.Equals(item.StockType, "Diamond", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(item.StockType, "Shipping", StringComparison.OrdinalIgnoreCase)
                    || string.IsNullOrWhiteSpace(item.StockType);

                if (!isParcelLot && !string.IsNullOrWhiteSpace(item.Stock_ID))
                    item.LotNo = item.Stock_ID;
            }

            return new MixMemoOutDetailDto { Header = header, Items = items };
        }

        /// <summary>
        /// GetSalepurRecord returns invoice MasterCurrencyRate/StockCurrencyRate.
        /// MVC Edit binds those to #BaseRate/#SBaseRate — keep both in sync for React Edit.
        /// </summary>
        private static void NormalizeHeader(MixMemoOutHeaderDto header)
        {
            if (header == null) return;

            if (header.MasterCurrencyRate != 0)
                header.BaseRate = header.MasterCurrencyRate;
            else if (header.BaseRate != 0)
                header.MasterCurrencyRate = header.BaseRate;

            if (header.StockCurrencyRate != 0)
                header.SBaseRate = header.StockCurrencyRate;
            else if (header.SBaseRate != 0)
                header.StockCurrencyRate = header.SBaseRate;
        }

        public async Task<List<MixMemoOutLotDetailDto>> GetLotDetailAsync(
            MixMemoOutLotRequest request, string companyId)
        {
            var escapedLotNo = (request.LotNo ?? "").Replace("'", "''");
            List<MixMemoOutLotDetailDto> result;

            if (request.SBaseRate == 1)
            {
                // MVC GetMixMemoLotNoDataForPMemo → GetLotNoDataForAll @Type = 'MixMemo'
                string sql = $" EXEC [GetLotNoDataForAll] @Type = 'MixMemo',@Where = ' AND LotNo = ''{escapedLotNo}'' '";
                result = (await _sqlDataAccess.SelectRawSql<MixMemoOutLotDetailDto>(sql, companyId)).ToList();
            }
            else
            {
                string sCurOperator = await GetSCurOperatorAsync(request.CurCode, companyId);
                string sCurOperatorToCon = GetSCurOperatorToCon(sCurOperator);
                string where = $" AND LotNo = '{escapedLotNo}' ";
                string sql = BuildMixMemoOtherCurSql(where, request.SBaseRate, sCurOperatorToCon);
                result = (await _sqlDataAccess.SelectRawSql<MixMemoOutLotDetailDto>(sql, companyId)).ToList();
            }

            NormalizeLotWeights(result);
            return result;
        }

        public async Task<MixMemoOutGridViewDto> AddItemInGridAsync(
            MixMemoOutAddLotRequest item, string companyId)
        {
            var list = item.Items ?? new List<MixMemoOutItemDto>();
            string alert = "";
            string stockType = string.IsNullOrWhiteSpace(item.StockType) ? "Diamond" : item.StockType;
            bool isJewellery = string.Equals(stockType, "Jewellery", StringComparison.OrdinalIgnoreCase);
            bool isShipping = string.Equals(stockType, "Shipping", StringComparison.OrdinalIgnoreCase);

            float carat = RoundCarat(item.Carat);

            if (!isShipping && carat <= 0)
                throw new Exception("Carat is invalid.");

            float purRate = item.Rate;
            float back = item.Back;
            float mrp = item.MRP;

            if (!isShipping && purRate == 0 && back == 0)
                throw new Exception("Rate and Discount both can not be 0.");

            if (purRate != 0 && mrp != 0)
                back = ((purRate * 100) / mrp) - 100;

            float amount;
            if (isJewellery)
                amount = item.Pcs * purRate;
            else if (isShipping)
                amount = item.Amount;
            else
                amount = carat * purRate;

            float l1 = item.Less1 != 0 ? amount * (100 + item.Less1) / 100 : amount;
            float l2 = item.Less2 != 0 ? l1 * (100 + item.Less2) / 100 : l1;
            float l3 = item.Less3 != 0 ? l2 * (100 + item.Less3) / 100 : l2;

            var row = new MixMemoOutItemDto
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
                StockType = stockType,
                InvDescription = item.InvDescription
            };

            float alertThreshold = isJewellery ? 0.009f : 0.05f;
            if (carat < alertThreshold && !isShipping)
                alert += " Lotno On Memo or Sold: " + item.LotNo;

            // MVC GeneralMemoController add-single-lot: duplicate LotNo check is commented out —
            // always append a new row unless editing by editIndex.
            bool isEdit = item.editIndex >= 0 && item.editIndex < list.Count;
            if (isEdit)
            {
                list[item.editIndex] = row;
            }
            else
            {
                list.Add(row);
            }

            return BuildGridView(list, alert, isEdit);
        }

        public async Task<MixMemoOutGridViewDto> GetItemFromListAsync(
            MixMemoOutGetItemFromListRequest request, string companyId)
        {
            var list = new List<MixMemoOutItemDto>();
            string alert = "";

            string strWhere = " AND LotNo IN ('''')";
            if (!string.IsNullOrWhiteSpace(request.LotNo))
            {
                var tmp = request.LotNo.Replace("\r\n", ",").Replace("\n", ",");
                tmp = tmp.Replace(",", "'',''");
                strWhere = $" AND LotNo IN (''{tmp}'')";
            }

            List<MixMemoOutLotDetailDto> lots;
            if (request.SBaseRate == 1)
            {
                string sql = $" EXEC [GetLotNoDataForAll] @Type = 'MixMemo',@Where = '{strWhere}'";
                lots = (await _sqlDataAccess.SelectRawSql<MixMemoOutLotDetailDto>(sql, companyId)).ToList();
            }
            else
            {
                string sCurOperator = await GetSCurOperatorAsync(request.CurCode, companyId);
                string sCurOperatorToCon = GetSCurOperatorToCon(sCurOperator);
                string sql = BuildMixMemoOtherCurSql(strWhere, request.SBaseRate, sCurOperatorToCon);
                lots = (await _sqlDataAccess.SelectRawSql<MixMemoOutLotDetailDto>(sql, companyId)).ToList();
            }

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

                list.Add(new MixMemoOutItemDto
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
                    Amount = l3,
                    StockType = string.IsNullOrWhiteSpace(lotRow.StockType) ? "Diamond" : lotRow.StockType,
                    InvDescription = lotRow.InvDescription,
                    ExportNo = lotRow.ExportNo,
                    Status = lotRow.Status,
                    SpCarats = lotRow.SpCarats,
                    ReturnCarats = lotRow.ReturnCarats
                });
            }

            return BuildGridView(list, alert, false);
        }

        public Task<MixMemoOutGridViewDto> RefreshBackAsync(
            MixMemoOutRefreshBackRequest request, string companyId)
        {
            var list = request.Items ?? new List<MixMemoOutItemDto>();

            if (request.refreshback != 0)
            {
                float avgPRapDown = request.AvgBack;
                float salePRapDown = request.refreshback;
                float pRapDownDiff = avgPRapDown - salePRapDown;

                var newList = new List<MixMemoOutItemDto>();
                foreach (var itemSData in list)
                {
                    float carat = RoundCarat(itemSData.Weight);
                    float back = itemSData.Back != 0 ? itemSData.Back : 0;
                    float mrp = itemSData.MRP != 0 ? itemSData.MRP : 0;

                    float newBack = back - pRapDownDiff;
                    float newRate = mrp + (mrp * newBack / 100);

                    bool isJewellery = string.Equals(itemSData.StockType, "Jewellery", StringComparison.OrdinalIgnoreCase);
                    float amount = isJewellery ? itemSData.Pcs * newRate : carat * newRate;
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

                var newList = new List<MixMemoOutItemDto>();
                foreach (var itemSData in list)
                {
                    float carat = RoundCarat(itemSData.Weight);
                    float rate = itemSData.Rate != 0 ? itemSData.Rate : 0;
                    float mrp = itemSData.MRP != 0 ? itemSData.MRP : 0;

                    float newRate = rate - saleRateDiff;
                    float newBack = mrp == 0 ? 0 : ((newRate * 100) / mrp) - 100;

                    bool isJewellery = string.Equals(itemSData.StockType, "Jewellery", StringComparison.OrdinalIgnoreCase);
                    float amount = isJewellery ? itemSData.Pcs * newRate : carat * newRate;
                    float l1 = itemSData.Less1 != 0 ? amount * (100 + (float)itemSData.Less1) / 100 : amount;
                    float l2 = itemSData.Less2 != 0 ? l1 * (100 + (float)itemSData.Less2) / 100 : l1;
                    float l3 = itemSData.Less3 != 0 ? l2 * (100 + (float)itemSData.Less3) / 100 : l2;

                    newList.Add(CloneItem(itemSData, l3, newBack, newRate));
                }

                return Task.FromResult(BuildGridView(newList, "", false));
            }

            throw new Exception("Enter AvgBack Or rate");
        }

        public async Task<(bool Success, string Message)> AddAsync(
            MixMemoOutHeaderDto salePur,
            List<MixMemoOutItemDto> items,
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

                string salesPerson = await ResolveSalesPersonAsync(conn, trans, salePur);
                salePur.SalesPerson = salesPerson;

                await InsertSalePurAsync(conn, trans, maxInvNo, salePur, ctx, usercode, userId, companyId);
                int maxSPCode = int.Parse(await ExecuteCommonDataScalarAsync(conn, trans, "SPCode"));

                await ProcessMemoOutItemsAsync(conn, trans, salePur, items, maxSPCode, ctx, usercode, userId, companyId, isEdit: false);

                trans.Commit();
                return (true, "Mix Memo Out Record Saved.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> EditAsync(
            MixMemoOutHeaderDto salePur,
            List<MixMemoOutItemDto> items,
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

                string salesPerson = await ResolveSalesPersonAsync(conn, trans, salePur);
                salePur.SalesPerson = salesPerson;

                await UpdateSalePurAsync(conn, trans, code, invNo, salePur, ctx, usercode, userId, companyId);

                using (var cmd = new SqlCommand("SP_DeleteSalePurTran", conn, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@InvCode", code));
                    await cmd.ExecuteNonQueryAsync();
                }

                await ProcessMemoOutItemsAsync(conn, trans, salePur, items, code, ctx, usercode, userId, companyId, isEdit: true);

                trans.Commit();
                return (true, "Mix Memo Out Record Update.");
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

                trans.Commit();
                return (true, "Record Deleted.");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return (false, ex.Message);
            }
        }

        public async Task<MixMemoOutPrintViewDto?> GetPrintViewAsync(string invNo, string companyId)
        {
            string where;
            if (int.TryParse(invNo, out var codeId))
                where = $"tsp.TrnType = ''{TrnType}'' AND tsp.Code = {codeId}";
            else
            {
                var escapedInvNo = (invNo ?? "").Replace("'", "''");
                where = $"tsp.TrnType = ''{TrnType}'' AND tsp.InvNo = ''{escapedInvNo}''";
            }

            string sql = $"EXEC GetMixSalepurRecordWithDetailsForINV @Where = '{where}'";

            var rows = (await _sqlDataAccess.SelectRawSql<PrintInvRow>(sql, companyId)).ToList();
            if (rows.Count == 0 && int.TryParse(invNo, out _))
            {
                var escapedInvNo = (invNo ?? "").Replace("'", "''");
                where = $"tsp.TrnType = ''{TrnType}'' AND tsp.InvNo = ''{escapedInvNo}''";
                sql = $"EXEC GetMixSalepurRecordWithDetailsForINV @Where = '{where}'";
                rows = (await _sqlDataAccess.SelectRawSql<PrintInvRow>(sql, companyId)).ToList();
            }

            if (rows.Count == 0)
                return null;

            var header = rows[0];
            var result = new MixMemoOutPrintViewDto
            {
                InvNo = header.InvNo,
                BillNo = header.BillNo,
                TrnDate = header.TrnDate.HasValue ? header.TrnDate.Value.ToString("MM/dd/yyyy") : null,
                RoundDate = header.RoundDate.HasValue ? header.RoundDate.Value.ToString("MM/dd/yyyy") : null,
                DueDays = header.DueDays,
                PartyName = header.PartyName,
                AddressLine1 = header.AddressLine1,
                AddressLine2 = header.AddressLine2,
                AddressLine3 = header.AddressLine3,
                ShipVia = header.ShipVia,
                Tracking = header.Tracking,
                SalesPerson = header.SalesPersons ?? header.SalesPerson,
                SalesPersons = header.SalesPersons ?? header.SalesPerson,
                PlusMinusAmount = header.PlusMinusAmount,
                BaseRate = header.BaseRate,
                FinalAmount = header.FinalAmount,
                Items = new List<MixMemoOutPrintItemDto>()
            };

            foreach (var dr in rows)
            {
                result.Items.Add(new MixMemoOutPrintItemDto
                {
                    StockNo = dr.LotNo ?? dr.lotno,
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
                    MasterCurrencyAmount = dr.MasterCurrencyAmount,
                    ReturnCarats = dr.ReturnCarats,
                    IssueCarats = dr.IssueCarats
                });
            }

            return result;
        }

        /// <summary>MVC PrintParcelinvcurrent → EXEC GetMemoReportData (MemoRandumPending.rpt)</summary>
        public async Task<MixMemoOutPrintViewDto?> GetCurrentStatusAsync(string invNo, string companyId)
        {
            async Task<List<dynamic>> LoadRows(string where)
            {
                var sql = $"EXEC [GetMemoReportData] @Where = '{where}',@Type = ''";
                return (await _sqlDataAccess.SelectDataAdoDynamic(sql, companyId)).ToList();
            }

            List<dynamic> rows;
            if (int.TryParse(invNo, out var codeId))
            {
                rows = await LoadRows($"and tm.Trntype=''CMemoOut'' and tm.Code=''{codeId}''");
                if (rows.Count == 0)
                {
                    var escapedInvNo = (invNo ?? "").Replace("'", "''");
                    rows = await LoadRows($"and tm.Trntype=''CMemoOut'' and tm.InvNo=''{escapedInvNo}''");
                }
            }
            else
            {
                var escapedInvNo = (invNo ?? "").Replace("'", "''");
                rows = await LoadRows($"and tm.Trntype=''CMemoOut'' and tm.InvNo=''{escapedInvNo}''");
            }

            // Fallback: Mix memo report SP (same columns) if GetMemoReportData has no rows
            if (rows.Count == 0)
            {
                async Task<List<dynamic>> LoadMixRows(string where)
                {
                    var sql = $"EXEC [GetMixMemoReportData] @Where = '{where}',@Type = ''";
                    return (await _sqlDataAccess.SelectDataAdoDynamic(sql, companyId)).ToList();
                }

                if (int.TryParse(invNo, out var mixCode))
                {
                    rows = await LoadMixRows($"and tm.Trntype=''CMemoOut'' and tm.Code=''{mixCode}''");
                    if (rows.Count == 0)
                    {
                        var escapedInvNo = (invNo ?? "").Replace("'", "''");
                        rows = await LoadMixRows($"and tm.Trntype=''CMemoOut'' and tm.InvNo=''{escapedInvNo}''");
                    }
                }
                else
                {
                    var escapedInvNo = (invNo ?? "").Replace("'", "''");
                    rows = await LoadMixRows($"and tm.Trntype=''CMemoOut'' and tm.InvNo=''{escapedInvNo}''");
                }
            }

            if (rows.Count == 0)
                return null;

            var header = rows[0];
            var result = new MixMemoOutPrintViewDto
            {
                InvNo = StatusStr(header, "MemoNo", "InvNo", "memoNo", "invNo"),
                BillNo = StatusStr(header, "BillNo", "billNo"),
                TrnDate = FormatMemoStatusDate(StatusStr(header, "TrnDate", "trnDate", "ConDATE", "ConDate")),
                RoundDate = FormatMemoStatusDate(StatusStr(header, "RoundDate", "roundDate"))
                    ?? DateTime.Now.ToString("MM/dd/yyyy"),
                DueDays = (short)StatusInt(header, "DueDays", "dueDays"),
                PartyName = StatusStr(header, "Party", "PartyName", "party", "partyName"),
                AddressLine1 = StatusStr(header, "AddressLine1", "addressLine1"),
                AddressLine2 = StatusStr(header, "AddressLine2", "addressLine2"),
                AddressLine3 = StatusStr(header, "AddressLine3", "addressLine3"),
                ShipVia = StatusStr(header, "ShipVia", "shipVia"),
                Tracking = StatusStr(header, "Tracking", "tracking"),
                SalesPerson = StatusStr(header, "SalesPerson", "Seller", "salesPerson", "seller"),
                SalesPersons = StatusStr(header, "SalesPerson", "Seller", "salesPerson", "seller"),
                Items = new List<MixMemoOutPrintItemDto>()
            };

            foreach (var dr in rows)
            {
                var carats = StatusDbl(dr, "Carats", "carats");
                var balance = StatusDbl(dr, "Balance", "balance");
                var balanceAmt = StatusDbl(dr, "BalanceAmt", "balanceAmt");
                var price = StatusDbl(dr, "Price", "price", "Rate", "rate");
                var amount = StatusDbl(dr, "Amount", "amount");
                var ret = StatusDbl(dr, "Return", "return");

                result.Items.Add(new MixMemoOutPrintItemDto
                {
                    StockNo = StatusStr(dr, "Lotno", "LotNo", "lotno", "lotNo"),
                    Description = StatusStr(dr, "ItemName", "itemName", "Description", "description"),
                    Qty = StatusInt(dr, "Pcs", "pcs"),
                    Weight = carats,
                    PricePerCarat = price,
                    Rate = price,
                    Amount = amount,
                    MasterCurrencyAmount = balanceAmt != 0 ? balanceAmt : amount,
                    ReturnCarats = ret,
                    Balance = balance,
                    BalanceAmt = balanceAmt,
                    ShapeId = StatusStr(dr, "ShapeId", "shapeId"),
                    ColorId = StatusStr(dr, "ColorId", "colorId"),
                    ClarityId = StatusStr(dr, "ClarityId", "clarityId"),
                    PropId = StatusStr(dr, "PropId", "propId"),
                    PolishId = StatusStr(dr, "PolishId", "polishId"),
                    SymId = StatusStr(dr, "SymId", "symId"),
                    FLId = StatusStr(dr, "FLId", "flId"),
                    CertId = StatusStr(dr, "CertId", "certId"),
                    CertificateNo = StatusStr(dr, "CertificateNo", "certificateNo")
                });
            }

            return result;
        }

        private static object? StatusRaw(dynamic row, params string[] keys)
        {
            if (row is IDictionary<string, object> dict)
            {
                foreach (var key in keys)
                {
                    var match = dict.Keys.FirstOrDefault(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
                    if (match == null) continue;
                    var value = dict[match];
                    if (value != null && value != DBNull.Value) return value;
                }
            }
            return null;
        }

        private static string? StatusStr(dynamic row, params string[] keys)
        {
            var v = StatusRaw(row, keys);
            var s = v?.ToString();
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }

        private static int StatusInt(dynamic row, params string[] keys)
        {
            var s = StatusStr(row, keys);
            if (string.IsNullOrWhiteSpace(s)) return 0;
            int n;
            return int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out n) ? n : 0;
        }

        private static double StatusDbl(dynamic row, params string[] keys)
        {
            var s = StatusStr(row, keys);
            if (string.IsNullOrWhiteSpace(s)) return 0;
            double n;
            return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out n) ? n : 0;
        }

        private static string? FormatMemoStatusDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var text = value.Trim();
            // Strip time: "24-01-2026 12:00:00 AM" → "24-01-2026"
            var spaceIdx = text.IndexOf(' ');
            var tIdx = text.IndexOf('T');
            var cut = -1;
            if (spaceIdx > 0) cut = spaceIdx;
            if (tIdx > 0 && (cut < 0 || tIdx < cut)) cut = tIdx;
            if (cut > 0) text = text.Substring(0, cut);

            DateTime dt;
            if (DateTime.TryParseExact(
                    text,
                    new[] { "dd-MM-yyyy", "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd", "dd-MMM-yyyy" },
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out dt))
                return dt.ToString("MM/dd/yyyy");
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.ToString("MM/dd/yyyy");
            if (DateTime.TryParse(value, out dt))
                return dt.ToString("MM/dd/yyyy");
            return text;
        }

        public async Task<List<MixMemoOutShipViaOptionDto>> GetShipViaAsync(string searchBy, string companyId)
        {
            if (string.IsNullOrWhiteSpace(searchBy) || searchBy.Trim().Length < 2)
                return new List<MixMemoOutShipViaOptionDto>();

            var escaped = searchBy.Trim().Replace("'", "''");
            string sql = $" EXEC [GetShipViaForDropdown] @strWhere = 'Name LIKE ''%{escaped}%'' '";
            var rows = (await _sqlDataAccess.SelectRawSql<ShipViaRow>(sql, companyId)).ToList();

            return rows.Select(x => new MixMemoOutShipViaOptionDto
            {
                id = x.Name,
                text = x.Name
            }).ToList();
        }

        #region Grid helpers

        private static MixMemoOutItemDto CloneItem(MixMemoOutItemDto src, float amount, float back, float rate) =>
            new MixMemoOutItemDto
            {
                ItemName = src.ItemName,
                LotNo = src.LotNo,
                ExportNo = src.ExportNo,
                Less1 = src.Less1,
                Less2 = src.Less2,
                Less3 = src.Less3,
                Amount = amount,
                Weight = src.Weight,
                MRP = src.MRP,
                Pcs = src.Pcs,
                Back = back,
                Rate = rate,
                SpCarats = src.SpCarats,
                ReturnCarats = src.ReturnCarats,
                Status = src.Status,
                StockType = src.StockType,
                Stock_ID = src.Stock_ID
            };

        private static MixMemoOutGridViewDto BuildGridView(
            List<MixMemoOutItemDto> list, string alert, bool isEdit)
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

            return new MixMemoOutGridViewDto
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

        private static void NormalizeLotWeights(List<MixMemoOutLotDetailDto> lots)
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

        private async Task<string> GetSCurOperatorAsync(int curCode, string companyId)
        {
            var param = new Dictionary<string, object>
            {
                { "@Type", "SCurOperator" },
                { "@Where", $" and ID={curCode}" }
            };
            var result = await _sqlDataAccess.SelectDataFromSPSingle<string>("GetCurrencyDetail", param, companyId);
            return string.IsNullOrWhiteSpace(result) ? "*" : result;
        }

        private static string GetSCurOperatorToCon(string sCurOperator) =>
            sCurOperator == "*" ? "/" : "*";

        // MVC GetLotNoDataForPSaleOtherCur
        private static string BuildMixMemoOtherCurSql(string strWhere, float sBaseRate, string sCurOperatorToCon) =>
            " SELECT  ItemName,LotNo,BalanceCarats3Dec ,Rate" + sCurOperatorToCon + sBaseRate +
            " Rate,OnHandCarats3Dec Weight,0 MRP, 0 Back,format(Rate*BalanceCarats3Dec " + sCurOperatorToCon +
            sBaseRate + " ,'0.00') as Amount,ShapeId,ColorId,ClarityId,SizeId,CertId FROM ViewItemClosingStockDateWise(GETDATE()) WHERE 1=1 " + strWhere;

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
            public DateTime? RoundDate { get; set; }
            public short DueDays { get; set; }
            public string? PartyName { get; set; }
            public string? AddressLine1 { get; set; }
            public string? AddressLine2 { get; set; }
            public string? AddressLine3 { get; set; }
            public string? ShipVia { get; set; }
            public string? Tracking { get; set; }
            public string? SalesPerson { get; set; }
            public string? SalesPersons { get; set; }
            public double PlusMinusAmount { get; set; }
            public double BaseRate { get; set; }
            public double FinalAmount { get; set; }
            public string? LotNo { get; set; }
            public string? lotno { get; set; }
            public string? ItemName { get; set; }
            public int Pcs { get; set; }
            public double Carats { get; set; }
            public double Rate { get; set; }
            public double MasterCurrencyAmount { get; set; }
            public double ReturnCarats { get; set; }
            public double IssueCarats { get; set; }
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

        private class ShipViaRow
        {
            public string? Name { get; set; }
        }

        private static object? GetDynRaw(dynamic row, params string[] keys)
        {
            if (row is IDictionary<string, object> dict)
            {
                foreach (var key in keys)
                {
                    var match = dict.Keys.FirstOrDefault(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
                    if (match == null) continue;
                    var value = dict[match];
                    if (value != null && value != DBNull.Value) return value;
                }
            }
            return null;
        }

        private static string? GetDynString(dynamic row, params string[] keys)
        {
            return GetDynRaw(row, keys)?.ToString();
        }

        

        

        private async Task<CurrencyContext> BuildCurrencyContextAsync(
            SqlConnection conn, SqlTransaction trans, MixMemoOutHeaderDto salePur)
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

        private async Task<string> ResolveSalesPersonAsync(
            SqlConnection conn, SqlTransaction trans, MixMemoOutHeaderDto salePur)
        {
            if (!string.IsNullOrWhiteSpace(salePur.SalesPerson))
                return salePur.SalesPerson;

            return await ExecuteCommonDataScalarAsync(conn, trans, "KAM", $" AND AccGrpCode = ''{salePur.PartyCode}''");
        }

        private async Task InsertSalePurAsync(
            SqlConnection conn, SqlTransaction trans, int maxInvNo,
            MixMemoOutHeaderDto salePur, CurrencyContext ctx,
            int usercode, string userId, string companyId)
        {
            using var cmd = new SqlCommand("SP_InsertSalePur", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@invno", maxInvNo));
            cmd.Parameters.Add(new SqlParameter("@billno", salePur.BillNo ?? (object)DBNull.Value));
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
            cmd.Parameters.Add(new SqlParameter("@Outstanding", false));
            cmd.Parameters.Add(new SqlParameter("@StockEffected", true));
            cmd.Parameters.Add(new SqlParameter("@Brokerage", false));
            cmd.Parameters.Add(new SqlParameter("@Commission", false));
            cmd.Parameters.Add(new SqlParameter("@Cheque", false));
            cmd.Parameters.Add(new SqlParameter("@usercode", usercode));
            cmd.Parameters.Add(new SqlParameter("@mastercurrencyrate", Math.Round(ctx.MCurBaseRate, 4)));
            cmd.Parameters.Add(new SqlParameter("@mastercurrencyfinalamount", Math.Round(ctx.MCurFinalAmt, 4)));
            cmd.Parameters.Add(new SqlParameter("@stockcurrencyrate", Math.Round(ctx.SCurBaseRate, 4)));
            cmd.Parameters.Add(new SqlParameter("@stockcurrencyfinalamount", Math.Round(ctx.SCurFinalAmt, 4)));
            cmd.Parameters.Add(new SqlParameter("@ExpPartyCode", salePur.ExpPartyCode));
            cmd.Parameters.Add(new SqlParameter("@RoundDate", salePur.RoundDate));
            cmd.Parameters.Add(new SqlParameter("@DivideDays", salePur.DivideDays));
            cmd.Parameters.Add(new SqlParameter("@InterestPer", salePur.InterestPer));
            cmd.Parameters.Add(new SqlParameter("@VatPer", salePur.VatPer));
            cmd.Parameters.Add(new SqlParameter("@VatAmount", salePur.VatAmount));
            cmd.Parameters.Add(new SqlParameter("@invdescription", salePur.InvDisc ?? salePur.Description ?? ""));
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
            MixMemoOutHeaderDto salePur, CurrencyContext ctx,
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
            cmd.Parameters.Add(new SqlParameter("@BankID", (object)0));
            cmd.Parameters.Add(new SqlParameter("@Category", salePur.Category ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@Trndate", salePur.TrnDate));
            cmd.Parameters.Add(new SqlParameter("@RoundDate", salePur.RoundDate));
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
            cmd.Parameters.Add(new SqlParameter("@OutStanding", false));
            cmd.Parameters.Add(new SqlParameter("@StockEffected", true));
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

        private async Task ProcessMemoOutItemsAsync(
            SqlConnection conn, SqlTransaction trans,
            MixMemoOutHeaderDto salePur,
            List<MixMemoOutItemDto> items,
            int invCode, CurrencyContext ctx,
            int usercode, string userId, string companyId,
            bool isEdit)
        {
            foreach (var im in items)
            {
                var amtRes = GetConvertAmount(ctx.CurCode, im.Amount, ctx.MCurBaseRate, ctx.MCurOperator, ctx.SCurBaseRate, ctx.SCurOperator, ctx.MCurCode, ctx.SCurCode);

                string stockType = string.IsNullOrWhiteSpace(im.StockType) ? "Diamond" : im.StockType;
                int itemCode = 0;
                string lotKey = (im.LotNo ?? "").Replace("'", "''");

                if (string.Equals(stockType, "Jewellery", StringComparison.OrdinalIgnoreCase))
                {
                    string itemCodeStr = await ExecuteCommonDataScalarAsync(conn, trans, "StockCode", $" AND Stock_ID = ''{lotKey}''");
                    itemCode = int.TryParse(itemCodeStr, out var jc) ? jc : 0;
                }
                else
                {
                    // Diamond / Shipping
                    string itemCodeStr = await ExecuteCommonDataScalarAsync(conn, trans, "ItemCode", $" AND LotNo = ''{lotKey}''");
                    itemCode = int.TryParse(itemCodeStr, out var ic) ? ic : 0;
                }

                string itemName = !string.IsNullOrWhiteSpace(im.ItemName) ? im.ItemName! : (im.LotNo ?? "");

                double returnCarats = 0;
                float spCarats = 0;

                if (isEdit)
                {
                    if (im.SpCarats != 0 && im.ReturnCarats != 0)
                    {
                        returnCarats = im.ReturnCarats;
                        spCarats = im.SpCarats;
                    }
                    else if (im.SpCarats != 0)
                    {
                        spCarats = im.SpCarats;
                    }
                    else if (im.ReturnCarats != 0)
                    {
                        returnCarats = im.ReturnCarats;
                    }
                }

                using var cmd = new SqlCommand("SP_InsertSalePurTran", conn, trans);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@SPdate", salePur.TrnDate));
                cmd.Parameters.Add(new SqlParameter("@TrnType", TrnType));
                cmd.Parameters.Add(new SqlParameter("@InvType", salePur.InvType ?? ""));
                cmd.Parameters.Add(new SqlParameter("@InvCode", invCode));
                cmd.Parameters.Add(new SqlParameter("@ItemCode", itemCode));
                cmd.Parameters.Add(new SqlParameter("@ItemName", itemName));
                cmd.Parameters.Add(new SqlParameter("@Carats", float.Parse(im.Weight.ToString("N3"))));
                cmd.Parameters.Add(new SqlParameter("@Pcs", im.Pcs));
                cmd.Parameters.Add(new SqlParameter("@Rate", Convert.ToDouble(im.Rate.ToString("N2"))));
                cmd.Parameters.Add(new SqlParameter("@MRP", im.MRP));
                cmd.Parameters.Add(new SqlParameter("@PMRPDown", Convert.ToDouble(im.Back.ToString("N2"))));
                cmd.Parameters.Add(new SqlParameter("@ReturnCarats", returnCarats));
                cmd.Parameters.Add(new SqlParameter("@IssueCarats", 0d));
                cmd.Parameters.Add(new SqlParameter("@amount", Convert.ToDouble(im.Amount.ToString("N2"))));
                cmd.Parameters.Add(new SqlParameter("@ExportNo", im.ExportNo ?? ""));
                cmd.Parameters.Add(new SqlParameter("@Less1", im.Less1));
                cmd.Parameters.Add(new SqlParameter("@Less2", im.Less2));
                cmd.Parameters.Add(new SqlParameter("@Less3", im.Less3));
                cmd.Parameters.Add(new SqlParameter("@CurCode", ctx.CurCode.ToString()));
                cmd.Parameters.Add(new SqlParameter("@memotrancode", (object)0));
                cmd.Parameters.Add(new SqlParameter("@StockEffected", true));
                cmd.Parameters.Add(new SqlParameter("@ExportDone", false));
                cmd.Parameters.Add(new SqlParameter("@spcarats", spCarats));
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
                cmd.Parameters.Add(new SqlParameter("@StockType", stockType));
                cmd.Parameters.Add(new SqlParameter("@InvDescription", im.InvDescription ?? ""));
                cmd.Parameters.Add(new SqlParameter("@ActionType", "insert"));
                await cmd.ExecuteNonQueryAsync();
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

        /// <summary>MVC common/get-Mixno-list → GetMixLotNoForDropdown → GetMixlotDataForDropdown</summary>
        public async Task<List<MixMemoOutLotOptionDto>> GetLotsAsync(string? searchBy, string companyId)
        {
            if (string.IsNullOrWhiteSpace(searchBy) || searchBy.Trim().Length < 1)
                return new List<MixMemoOutLotOptionDto>();

            string safe = searchBy.Trim().Replace("'", "''");
            // Exact MVC EXEC quoting: LIKE ''%term%'' inside the @strWhere string literal
            string strWhere = $" AND (LotNo LIKE ''%{safe}%'' OR Itemname LIKE ''%{safe}%'') ";
            string sql = $"EXEC [GetMixlotDataForDropdown] @strWhere = '{strWhere}'";

            var rows = (await _sqlDataAccess.SelectRawSql<MixLotDropdownRow>(sql, companyId)).ToList();

            // MVC: id = Name (LotNo), text = TextName (e.g. "SS_10 RBC D FL 28.000")
            return rows
                .Select(r =>
                {
                    var lotNo = (r.Name ?? r.LotNo ?? "").Trim();
                    var display = (r.TextName ?? r.ItemName ?? lotNo).Trim();
                    if (string.IsNullOrEmpty(lotNo)) return null;
                    return new MixMemoOutLotOptionDto
                    {
                        id = lotNo,
                        name = lotNo,
                        text = display,
                        TextName = display
                    };
                })
                .Where(x => x != null)
                .Select(x => x!)
                .ToList();
        }

        private class MixLotDropdownRow
        {
            public string? ID { get; set; }
            public string? Name { get; set; }
            public string? TextName { get; set; }
            public string? Type { get; set; }
            public string? LotNo { get; set; }
            public string? ItemName { get; set; }
        }

        #endregion
    }
}
