using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Reports;
using Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Services
{
    public class ParcelStockRegisterReportService : IParcelStockRegisterReportService
    {
        private readonly IParcelStockRegisterReportRepository _repository;

        public ParcelStockRegisterReportService(IParcelStockRegisterReportRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> HasUserPermissionAsync(
            string userTypeId,
            string formName,
            string actionType,
            string companyId)
        {
            int permissionCount = await _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
            return permissionCount == 1;
        }

        public async Task<ParcelStockDropdownDto> GetReportDropdownsAsync(string companyId)
        {
            var rawData = await _repository.GetPurchaseReportDropDownAsync(companyId);

            return new ParcelStockDropdownDto
            {
                Shape = rawData.Where(x => x.Type == "Shape").ToList(),
                Color = rawData.Where(x => x.Type == "Color").ToList(),
                Size = rawData.Where(x => x.Type == "Size").ToList(),
                Clarity = rawData.Where(x => x.Type == "Clarity").ToList()
            };
        }

        public async Task<IEnumerable<BasicDataDto>> GetParcelLotNoListAsync(string searchby, string companyId)
        {
            return await _repository.GetParcelLotNoListAsync(searchby, companyId);
        }

        public async Task<IEnumerable<ParcelStockRegisterReportResponseDto>> GetParcelStockRegisterReportAsync(
            StockRegisterReportFilterDto filter,
            string companyId)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            string Type = GetPriceType(filter);
            var (StkIn, StkOut) = ProcessStockInAndOutTypes(filter);
            string WhereFilter = BuildWhereClause(filter);

            string FromDate = DateTime.Parse(filter.FromDate ?? DateTime.Today.ToString("yyyy-MM-dd"))
                .ToString("yyyy-MM-dd");
            string ToDate = DateTime.Parse(filter.ToDate ?? DateTime.Today.ToString("yyyy-MM-dd"))
                .ToString("yyyy-MM-dd");

            var rows = await _repository.GetParcelStockRegisterReportAsync(
                Type,
                StkIn,
                StkOut,
                WhereFilter,
                FromDate,
                ToDate,
                companyId);

            return rows.Select(MapToResponse).ToList();
        }

        private static ParcelStockRegisterReportResponseDto MapToResponse(ParcelStockRegisterReportDatatable row)
        {
            return new ParcelStockRegisterReportResponseDto
            {
                TrnDate = row.TrnDate,
                Trntype = row.TrnType,
                Perticulors = row.Particulars,
                LotNo = row.StockID,
                InCts = row.StockIn,
                InRate = row.StockInPrice,
                InAmount = row.StockInAmount,
                OutCts = row.StockOut,
                OutRate = row.StockOutPrice,
                OutAmount = row.StockOutAmount,
                COGS = row.COGS,
                BalCts = row.BalanceStock,
                BalRate = row.BalancePrice,
                BalAmount = row.BalanceAmount,
                invcode = row.invcode > 0 ? row.invcode.ToString() : null
            };
        }

        private static string GetPriceType(StockRegisterReportFilterDto filter)
        {
            // Match MVC ParcelStockRegisterReport: RadioValue first, then flags
            if (filter == null) return "SalePurAvgPrice";

            if (!string.IsNullOrEmpty(filter.RadioValue))
            {
                return filter.RadioValue.ToLower() switch
                {
                    "marketprice" => "marketprice",
                    "sale-pur-avg-price" => "SalePurAvgPrice",
                    "pur-avg-price" => "PurAvgPrice",
                    "cogs" => "COGS",
                    _ => "SalePurAvgPrice"
                };
            }

            if (filter.MarketAvg) return "marketprice";
            if (filter.PurAvg) return "PurAvgPrice";
            if (filter.COGS) return "COGS";
            if (filter.SalpurAvg) return "SalePurAvgPrice";

            return "SalePurAvgPrice";
        }

        private static string NormalizeType(string type)
        {
            if (string.IsNullOrWhiteSpace(type)) return type;
            return type.Trim().ToUpperInvariant() switch
            {
                "GENPUR" => "GPUR",
                "GENPURRTN" => "GPURRTN",
                "GENSALE" => "GSALE",
                "GENSALERTN" => "GSALERTN",
                "GENMEMOIN" => "GMEMOIN",
                "GENMEMOINRTN" => "GMEMOINRTN",
                "GENMEMOOUT" => "GMEMOOUT",
                "GENMEMOOUTRTN" => "GMEMOOUTRTN",
                _ => type.Trim().ToUpperInvariant()
            };
        }

        private (string StkInStr, string StkOutStr) ProcessStockInAndOutTypes(StockRegisterReportFilterDto filter)
        {
            List<string> stkIn = new List<string> { "OpStk" };
            List<string> stkOut = new List<string>();

            if (filter.SelectedTypes != null && filter.SelectedTypes.Any())
            {
                foreach (var rawType in filter.SelectedTypes)
                {
                    var type = NormalizeType(rawType);
                    switch (type)
                    {
                        case "ALL":
                            stkIn.AddRange(new[] { "OpStk", "PPUR", "CPUR", "PSaleRtn", "CSaleRtn", "PMemoIn", "CMemoIn", "MemoRtnP", "StkTrn", "JewTrn", "MixMemoRtnP" });
                            stkOut.AddRange(new[] { "PSALE", "CSALE", "PPurRtn", "CPurrtn", "PMemoOut", "CMemoOut", "MemoInRtnP", "StkTrn", "JewTrn", "MixMemoInRtn" });
                            break;

                        case "PUR": stkIn.Add("PPUR"); break;
                        case "PURRTN": stkOut.Add("PPurRtn"); break;
                        case "SALE": stkOut.Add("PSALE"); break;
                        case "SALERTN": stkIn.Add("PSaleRtn"); break;
                        case "MEMOIN": stkIn.Add("PMemoIn"); break;
                        case "MEMOINRTN": stkOut.Add("MemoInRtnP"); break;
                        case "MEMOOUT": stkOut.Add("PMemoOut"); break;
                        case "MEMOOUTRTN": stkIn.Add("MemoRtnP"); break;
                        case "GPUR": stkIn.Add("CPUR"); break;
                        case "GPURRTN": stkOut.Add("CPurrtn"); break;
                        case "GSALE": stkOut.Add("CSALE"); break;
                        case "GSALERTN": stkIn.Add("CSaleRtn"); break;
                        case "GMEMOIN": stkIn.Add("CMemoIn"); break;
                        case "GMEMOOUT": stkOut.Add("CMemoOut"); break;
                        case "GMEMOINRTN": stkOut.Add("MixMemoInRtn"); break;
                        case "GMEMOOUTRTN": stkIn.Add("MixMemoRtnP"); break;

                        case "STKTRN":
                            stkIn.Add("StkTrn");
                            stkOut.Add("StkTrn");
                            stkOut.Add("JewTrn");
                            break;
                    }
                }
            }
            else if (filter.SelectedTypesTrn != null && filter.SelectedTypesTrn.Any())
            {
                foreach (var rawType in filter.SelectedTypesTrn)
                {
                    var type = NormalizeType(rawType);
                    switch (type)
                    {
                        case "ALL":
                            stkIn.AddRange(new[] { "OpStk", "PPUR", "CPUR", "PSaleRtn", "CSaleRtn", "PMemoIn", "CMemoIn", "MemoRtnP", "StkTrn", "JewTrn", "MixMemoRtnP" });
                            stkOut.AddRange(new[] { "PSALE", "CSALE", "PPurRtn", "CPurrtn", "PMemoOut", "CMemoOut", "MemoInRtnP", "StkTrn", "JewTrn", "MixMemoInRtn" });
                            break;

                        case "PUR":
                            stkIn.Add("PPUR");
                            stkOut.Add("PPurRtn");
                            stkIn.Add("CPUR");
                            stkOut.Add("CPurrtn");
                            break;

                        case "MEMOIN":
                            stkIn.Add("PMemoIn");
                            stkOut.Add("MemoInRtnP");
                            stkIn.Add("CMemoIn");
                            stkOut.Add("MixMemoInRtn");
                            break;

                        case "MEMOOUT":
                            stkOut.Add("PMemoOut");
                            stkIn.Add("MemoRtnP");
                            stkOut.Add("CMemoOut");
                            stkIn.Add("MixMemoRtnP");
                            break;

                        case "SALE":
                            stkOut.Add("PSALE");
                            stkIn.Add("PSaleRtn");
                            stkOut.Add("CSALE");
                            stkIn.Add("CSaleRtn");
                            break;

                        case "STKTRN":
                            stkIn.Add("StkTrn");
                            stkOut.Add("StkTrn");
                            stkOut.Add("JewTrn");
                            break;
                    }
                }
            }

            if (!stkIn.Any()) stkIn.Add("__NA__");
            if (!stkOut.Any()) stkOut.Add("__NA__");

            // Doubled quotes required for MVC-style inline EXEC string embedding
            string stkInStr = string.Join(", ", stkIn.Distinct().Select(x => $"''{x}''"));
            string stkOutStr = string.Join(", ", stkOut.Distinct().Select(x => $"''{x}''"));

            return (stkInStr, stkOutStr);
        }

        private string BuildWhereClause(StockRegisterReportFilterDto filter)
        {
            // Exact port of MVC ReportController.ParcelStockRegisterReportFilterWhereNew
            var strWhere = new StringBuilder(" ");

            if (!string.IsNullOrWhiteSpace(filter.LotNo))
            {
                string strTmpNewLine = filter.LotNo;
                strTmpNewLine = strTmpNewLine.Replace("\r\n", ",");
                strTmpNewLine = strTmpNewLine.Replace("\n", ",");
                strTmpNewLine = strTmpNewLine.Replace("\r", ",");
                // drop empty entries from trailing newlines: "A,B," → "A,B"
                var lots = strTmpNewLine
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim())
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .ToList();

                if (lots.Any())
                {
                    strTmpNewLine = string.Join(",", lots);
                    strTmpNewLine = strTmpNewLine.Replace(",", "'',''");
                    strWhere.Append(" AND (v.StockID IN (''" + strTmpNewLine + "''))");
                }
            }

            if (filter.Shape != null && filter.Shape.Count > 0)
            {
                string Shape = string.Join("','", filter.Shape);
                strWhere.Append(" AND im.ShapeID IN (''" + Shape + "'')");
            }

            if (filter.Color != null && filter.Color.Count > 0)
            {
                string Color = string.Join("','", filter.Color);
                strWhere.Append(" AND  im.ColorID IN (''" + Color + "'')");
            }

            if (filter.Clarity != null && filter.Clarity.Count > 0)
            {
                string Clarity = string.Join("','", filter.Clarity);
                strWhere.Append(" AND im.ClarityID IN (''" + Clarity + "'')");
            }

            if (filter.Size != null && filter.Size.Count > 0)
            {
                string Size = string.Join("','", filter.Size);
                strWhere.Append(" AND im.SizeID IN (''" + Size + "'')");
            }

            if (filter.StkEff == false)
            {
                strWhere.Append(" AND StockEffected = 1 ");
            }

            return strWhere.ToString();
        }
    }
}
