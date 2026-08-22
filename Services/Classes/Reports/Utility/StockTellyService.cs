using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;
using BasicDataDto = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Utility;
using Dia_CoreAPI.Services.Interfaces.Reports.Utility;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;

namespace Dia_CoreAPI.Services.Classes.Reports.Utility
{
    public class StockTellyService : IStockTellyService
    {
        private const string FormName = "StockTelly";
        private static readonly string[] ValidExtensions = { ".xls", ".xlsx" };

        private readonly IStockTellyRepository _repository;

        public StockTellyService(IStockTellyRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);

        public async Task<StockTellyPageDto> LoadPageDataAsync(string companyId)
        {
            var data = await _repository.GetStockSearchDropdownAsync(companyId);

            return new StockTellyPageDto
            {
                Shape = MapDropdown(data, "Shape"),
                Color = MapDropdown(data, "Color"),
                Size = MapDropdown(data, "Size"),
                Clarity = MapDropdown(data, "Clarity")
            };
        }

        public async Task<List<StockTellyListDto>> GetStockTellyReportAsync(StockTellyFilterDto filter, string companyId)
        {
            string date = string.IsNullOrWhiteSpace(filter?.Date) || !DateTime.TryParse(filter.Date, out _)
                ? DateTime.Now.ToString("yyyy-MM-dd")
                : filter.Date!;

            string where = BuildWhere(filter);
            string askingQry = BuildAskingQuery(filter);

            var rows = await _repository.GetParcelStockSearchDataAsync(date, where, askingQry, companyId);
            return rows.Select(MapReportRow).ToList();
        }

        public async Task<(bool Success, object? Data)> ImportStockTellyAsync(
            IFormFile? file,
            string userTypeId,
            string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, FormName, "insert", companyId);
            if (permission != 1)
                return (false, "You are not authorized to perform this task.");

            if (file == null || file.Length == 0)
                return (false, "Please select an Excel file to import.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!ValidExtensions.Contains(extension))
                return (false, "Invalid file type. Please upload .xls or .xlsx file.");

            var readResult = ReadExcel(file);
            if (!readResult.Success || readResult.Table == null)
                return (false, readResult.ErrorMessage ?? "Failed to read Excel file.");

            var importRows = new List<StockTellyListDto>();
            var dt = readResult.Table;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(dt.Rows[i][0]?.ToString()))
                    break;

                string lotNo = dt.Rows[i][0].ToString() ?? string.Empty;
                string itemCode = await _repository.GetLotNoAsync(" AND LotNo = '" + lotNo.Replace("'", "''") + "'", companyId) ?? "0";

                importRows.Add(new StockTellyListDto
                {
                    ItemCode = Convert.ToInt32(itemCode),
                    LotNo = lotNo,
                    Pcs = Convert.ToInt32(dt.Rows[i][1]),
                    carats = Convert.ToDouble(float.Parse(dt.Rows[i][2].ToString() ?? "0", CultureInfo.InvariantCulture)),
                    OnHand = Convert.ToDouble(float.Parse(dt.Rows[i][3].ToString() ?? "0", CultureInfo.InvariantCulture)),
                    Rate = Convert.ToDouble(float.Parse(dt.Rows[i][4].ToString() ?? "0", CultureInfo.InvariantCulture)),
                    Amount = Convert.ToDouble(float.Parse(dt.Rows[i][5].ToString() ?? "0", CultureInfo.InvariantCulture))
                });
            }

            return (true, importRows);
        }

        public async Task<(bool Success, string Message)> SaveStockTellyAsync(
            StockTellySaveRequestDto request,
            int userId,
            string userName,
            int companyId,
            string companyIdStr)
        {
            return await _repository.SaveStockTellyAsync(request, userId, userName, companyId, companyIdStr);
        }

        private static List<StockTellyDropdownOptionDto> MapDropdown(IEnumerable<BasicDataDto> data, string type)
        {
            return data
                .Where(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase))
                .Select(x => new StockTellyDropdownOptionDto
                {
                    ID = x.ID,
                    Name = string.IsNullOrWhiteSpace(x.Name) ? x.ID : x.Name
                })
                .ToList();
        }

        private static StockTellyListDto MapReportRow(Dia_CoreAPI.DTOs.Parcel.Parcel_Reports.ParcelStockrpt row)
        {
            return new StockTellyListDto
            {
                ItemCode = row.ItemCode,
                LotNo = row.LotNo ?? string.Empty,
                Pcs = row.Pcs,
                carats = ParseDouble(row.carats),
                OnHand = ParseDouble(row.OnHand),
                Rate = ParseDouble(row.Rate),
                Amount = ParseDouble(row.Amount)
            };
        }

        private static double ParseDouble(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                ? result
                : 0;
        }

        /// <summary>WHERE clause for ParcelGetStockReportData @Where parameter (single quotes — not MVC dynamic-SQL doubling).</summary>
        private static string BuildWhere(StockTellyFilterDto filter)
        {
            string where = " and singlepcs = 0 and  Lotno NOT IN ('TempMixingLot')";
            //string where = " and singlepcs = 0 AND LotNo IN ('xyz123') ";

            where += BuildInClause("Shp", filter?.Shape);
            where += BuildInClause("Col", filter?.Color);
            where += BuildInClause("Size", filter?.Size);
            where += BuildInClause("Cla", filter?.Clarity);

            return where;
        }

        private static string BuildInClause(string column, IList<string>? values)
        {
            if (values == null || values.Count == 0)
                return string.Empty;

            var items = values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => "'" + v.Replace("'", "''") + "'")
                .ToList();

            if (items.Count == 0)
                return string.Empty;

            return $" And {column} In ({string.Join(",", items)})";
        }

        /// <summary>Matches MVC StockTellyController.StockTellyReport AskingQry logic.</summary>
        private static string BuildAskingQuery(StockTellyFilterDto filter)
        {
            float backPer = filter?.BackPer ?? 0;
            float backPlusMinus = filter?.BackPlusMinus ?? 0;
            float ratePer = filter?.RatePer ?? 0;
            float ratePlusMinus = filter?.RatePlusMinus ?? 0;

            if (backPer != 0)
            {
                return $" Rapa+(Rapa*({backPer})/100) AS AskingRate,(Rapa+(Rapa*({backPer})/100)) * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) AS AskingAmount,{backPer} As AskingBack ";
            }

            if (backPlusMinus != 0)
            {
                return $" Rapa+(Rapa*(ViewStockReport.back+{backPlusMinus})/100) AS AskingRate,(Rapa+(Rapa*(ViewStockReport.back+{backPlusMinus})/100)) * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) AS AskingAmount,ViewStockReport.Back+{backPlusMinus} As AskingBack ";
            }

            if (ratePer != 0)
            {
                return $" (ROUND(((Rate+(Rate*({ratePer})/100))/10)+.49,0))*10 AS AskingRate,(ROUND(((Rate+(Rate*({ratePer})/100))/10)+.49,0))*10 * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) AS AskingAmount,CASE WHEN Rapa = 0 OR Rapa IS NULL THEN 0 ELSE (ROUND(((Rate+(Rate*({ratePer})/100))/10)+.49,0))*10 * 100 / Rapa - 100 END AS AskingBack ";
            }

            if (ratePlusMinus != 0)
            {
                return $" Rate+{ratePlusMinus} AS AskingRate, ((rate+{ratePlusMinus}) * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1)) AS AskingAmount,CASE WHEN Rapa = 0 OR Rapa IS NULL THEN 0 ELSE ((((Rate+{ratePlusMinus} )*100) / Rapa) - 100) END As AskingBack ";
            }

            return " Rate AS AskingRate,Back As AskingBack,Round((CONVERT(decimal(18, 2), Round((CONVERT(decimal(18, 3), TotCarats + MemoIn)), 3, 1) * ISNULL(Rate, 0))), 2, 1) AS AskingAmount ";
        }

        private static (bool Success, DataTable? Table, string? ErrorMessage) ReadExcel(IFormFile file)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using var stream = file.OpenReadStream();
                using var excelReader = ExcelReaderFactory.CreateReader(stream);
                var conf = new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true
                    }
                };

                var dataSet = excelReader.AsDataSet(conf);
                var table = dataSet.Tables.Count > 0 ? dataSet.Tables[0] : null;
                if (table == null)
                    return (false, null, "Excel is empty.");

                return (true, table, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
    }
}
