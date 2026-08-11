using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;
using Dia_CoreAPI.Repositories.Interfaces.Jewellery.JewelleryReports;
using Dia_CoreAPI.Services.Interfaces.Jewellery.JewelleryReports;

namespace Dia_CoreAPI.Services.Classes.Jewellery.JewelleryReports
{
    public class JewelleryStockHistoryReportService : IJewelleryStockHistoryReportService
    {
        private readonly IJewelleryStockHistoryReportRepository _repository;

        public JewelleryStockHistoryReportService(IJewelleryStockHistoryReportRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(
            string userTypeId,
            string formName,
            string actionType,
            string companyId)
        {
            return _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        public Task<IEnumerable<BasicDataDto>> GetDropdownsAsync(string companyId)
        {
            return _repository.GetPurchaseReportDropDownAsync(companyId);
        }

        public Task<IEnumerable<BasicDataDto>> GetJewStockIdListAsync(string searchby, string companyId)
        {
            return _repository.GetJewStockIdListAsync(searchby, companyId);
        }

        public Task<IEnumerable<JewelleryStockHistoryReportDatatableDto>> GetReportAsync(
            JewelleryStockHistoryReportFilterDto filter,
            string companyId)
        {
            // Ported logic from MVC JewReportController.JewStockRegisterReport:

            // 1. Determine price type (RadioValue)
            string arg1 = "SalePurAvgPrice";
            if (!string.IsNullOrEmpty(filter.RadioValue))
            {
                var val = filter.RadioValue.ToLower();
                if (val == "marketprice")
                    arg1 = "marketprice";
                else if (val == "sale-pur-avg-price")
                    arg1 = "SalePurAvgPrice";
                else if (val == "pur-avg-price")
                    arg1 = "PurAvgPrice";
                else if (val == "cogs")
                    arg1 = "COGS";
            }

            // 2. Determine stock transaction types lists
            List<string> stkIn = new List<string> { "OpStk" };
            List<string> stkOut = new List<string>();

            if (filter.selectedTypes != null && filter.selectedTypes.Any())
            {
                foreach (var type in filter.selectedTypes)
                {
                    switch (type)
                    {
                        case "ALL":
                            stkIn.AddRange(new[] { "OpStk", "PJEW", "JMemoIn", "JMemoRtn", "CPUR", "CSaleRtn", "SJEWRtn", "CMemoIn", "MixMemoRtnP", "JewTrn" });
                            stkOut.AddRange(new[] { "SJEW", "JMemoOut", "JMemoInRtn", "CSALE", "CPurrtn", "CMemoOut", "MixMemoInRtn", "JewTrn" });
                            break;
                        case "PUR": stkIn.Add("PJEW"); break;
                        case "SALE": stkOut.Add("SJEW"); break;
                        case "SALERTN": stkIn.Add("SJEWRtn"); break;
                        case "MEMOIN": stkIn.Add("JMemoIn"); break;
                        case "MEMOINRTN": stkOut.Add("JMemoInRtn"); break;
                        case "MEMOOUT": stkOut.Add("JMemoOut"); break;
                        case "MEMOOUTRTN": stkIn.Add("JMemoRtn"); break;
                        case "GPUR": stkIn.Add("CPUR"); break;
                        case "GPURRTN": stkOut.Add("CPurrtn"); break;
                        case "GSALE": stkOut.Add("CSALE"); break;
                        case "GSALERTN": stkIn.Add("CSaleRtn"); break;
                        case "GMEMOIN": stkIn.Add("CMemoIn"); break;
                        case "GMEMOOUT": stkOut.Add("CMemoOut"); break;
                        case "GMEMOINRTN": stkOut.Add("MixMemoInRtn"); break;
                        case "GMEMOOUTRTN": stkIn.Add("MixMemoRtnP"); break;
                        case "STKTRN":
                            stkIn.Add("JewTrn");
                            stkOut.Add("JewTrn");
                            break;
                    }
                }
            }
            else if (filter.selectedTypesTrn != null && filter.selectedTypesTrn.Any())
            {
                foreach (var type in filter.selectedTypesTrn)
                {
                    switch (type)
                    {
                        case "ALL":
                            stkIn.AddRange(new[] { "OpStk", "PJEW", "JMemoIn", "JMemoRtn", "CPUR", "CSaleRtn", "SJEWRtn", "CMemoIn", "MixMemoRtnP", "JewTrn" });
                            stkOut.AddRange(new[] { "SJEW", "JMemoOut", "JMemoInRtn", "CSALE", "CPurrtn", "CMemoOut", "MixMemoInRtn", "JewTrn" });
                            break;
                        case "PUR":
                            stkIn.Add("PJEW");
                            stkIn.Add("CPUR");
                            stkOut.Add("CPurrtn");
                            break;
                        case "MEMOIN":
                            stkIn.Add("JMemoIn");
                            stkOut.Add("JMemoInRtn");
                            stkIn.Add("CMemoIn");
                            stkOut.Add("MixMemoInRtn");
                            break;
                        case "MEMOOUT":
                            stkOut.Add("JMemoOut");
                            stkIn.Add("JMemoRtn");
                            stkOut.Add("CMemoOut");
                            stkIn.Add("MixMemoRtnP");
                            break;
                        case "SALE":
                            stkOut.Add("SJEW");
                            stkIn.Add("SJEWRtn");
                            stkOut.Add("CSALE");
                            stkIn.Add("CSaleRtn");
                            break;
                        case "STKTRN":
                            stkIn.Add("JewTrn");
                            stkOut.Add("JewTrn");
                            break;
                    }
                }
            }
            else
            {
                // Fallback default
                stkIn.AddRange(new[] { "OpStk", "PJEW", "JMemoIn", "JMemoRtn", "CPUR", "CSaleRtn", "SJEWRtn", "CMemoIn", "MixMemoRtnP", "JewTrn" });
                stkOut.AddRange(new[] { "SJEW", "JMemoOut", "JMemoInRtn", "CSALE", "CPurrtn", "CMemoOut", "MixMemoInRtn", "JewTrn" });
            }

            if (!stkIn.Any()) stkIn.Add("__NA__");
            if (!stkOut.Any()) stkOut.Add("__NA__");

            stkIn = stkIn.Distinct().ToList();
            stkOut = stkOut.Distinct().ToList();

            string stkInStr = string.Join(", ", stkIn.Select(x => $"''{x}''"));
            string stkOutStr = string.Join(", ", stkOut.Select(x => $"''{x}''"));

            // 3. Format Date
            string fromDate = DateTime.TryParse(filter.FromDate, out var fromD) ? fromD.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd");
            string toDate = DateTime.TryParse(filter.ToDate, out var toD) ? toD.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd");

            // 4. Build Filter Where clause
            string strWhereNew = BuildHistoryWhere(filter);

            return _repository.GetJewStockRegisterReportAsync(arg1, stkInStr, stkOutStr, strWhereNew, fromDate, toDate, companyId);
        }

        private static string BuildHistoryWhere(JewelleryStockHistoryReportFilterDto filter)
        {
            string strWhere = " ";

            if (!string.IsNullOrWhiteSpace(filter.LotNo))
            {
                string strTmpNewLine = filter.LotNo;
                strTmpNewLine = strTmpNewLine.Replace("\r\n", ",");
                strTmpNewLine = strTmpNewLine.Replace("\n", ",");
                strTmpNewLine = strTmpNewLine.Replace(",", "'',''");
                strWhere += " AND (v.StockID IN (''" + strTmpNewLine + "''))";
            }

            if (filter.Shape != null && filter.Shape.Any())
            {
                string shapes = string.Join("','", filter.Shape);
                strWhere += " AND im.ShapeID IN (''" + shapes + "'')";
            }

            if (filter.Color != null && filter.Color.Any())
            {
                string colors = string.Join("','", filter.Color);
                strWhere += " AND im.ColorID IN (''" + colors + "'')";
            }

            if (filter.Clarity != null && filter.Clarity.Any())
            {
                string clarities = string.Join("','", filter.Clarity);
                strWhere += " AND im.ClarityID IN (''" + clarities + "'')";
            }

            if (filter.Size != null && filter.Size.Any())
            {
                string sizes = string.Join("','", filter.Size);
                strWhere += " AND im.SizeID IN (''" + sizes + "'')";
            }

            if (!filter.StkEff)
            {
                strWhere += " AND StockEffected = 1 ";
            }

            return strWhere;
        }
    }
}
