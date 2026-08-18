using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Utility;
using Dia_CoreAPI.Services.Interfaces.Reports.Utility;
using ExcelDataReader;

namespace Dia_CoreAPI.Services.Classes.Reports.Utility
{
    public class ImportPricingService : IImportPricingService
    {
        private const string FormName = "Import Price";
        private readonly IImportPricingRepository _repository;

        public ImportPricingService(IImportPricingRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);

        public async Task<ImportPricingResult> ImportPricingDataAsync(string userTypeId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, FormName, "insert", companyId);
            if (permission >= 1)
                return new ImportPricingResult(true, null, null);

            return new ImportPricingResult(false, "You are not authorized to perform this task.");
        }

        /// <summary>Matches MVC POST /data/import-pricing file upload.</summary>
        public async Task<ImportPricingResult> ImportPricingFileAsync(
            ImportPricingDTO importData,
            string userTypeId,
            string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, FormName, "insert", companyId);
            if (permission < 1)
                return new ImportPricingResult(false, "You are not authorized to perform this task.");

            var file = importData.ImportFile;
            if (file == null || file.Length == 0)
                return new ImportPricingResult(false, "Please upload a file....!");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".xls" && extension != ".xlsx")
                return new ImportPricingResult(false, "Invalid file type....!");

            var tableOrError = ReadExcel(file);
            if (!tableOrError.Success)
                return tableOrError.Error;

            var dt = tableOrError.Table!;
            if (dt.Columns.Count < 2)
                return new ImportPricingResult(false, "Excel must contain at least 2 columns....!");

            const string expectedColumn = "LotNo";
            var columnName = dt.Columns[0].ColumnName.Trim();
            if (!expectedColumn.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                return new ImportPricingResult(false, "The pricing import file is invalid. Please check the file format...!");

            var rows = new List<ImportPricingExcelDto>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var lotNo = dt.Rows[i][0]?.ToString()?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(lotNo))
                    break;

                float rate = 0;
                float.TryParse(dt.Rows[i][1]?.ToString(), out rate);

                rows.Add(new ImportPricingExcelDto
                {
                    LotNo = lotNo,
                    rate = rate
                });
            }

            return new ImportPricingResult(true, rows);
        }

        /// <summary>Matches MVC POST /data/save-price.</summary>
        public async Task<ImportPricingResult> SavePricingAsync(
            SavePricingRequestDto request,
            string userTypeId,
            string userId,
            string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, FormName, "insert", companyId);
            if (permission < 1)
                return new ImportPricingResult(false, "You are not authorized to perform this task.");

            if (request?.Imp == null || request.Imp.Count == 0)
                return new ImportPricingResult(false, "No pricing rows to save.");

            int.TryParse(userId, out var userCode);

            var pricingType = request.Pr?.Pricingtype ?? string.Empty;
            var (success, data) = await _repository.SavePricingAsync(
                pricingType,
                request.Imp,
                userCode,
                companyId);

            return new ImportPricingResult(success, data);
        }

        private static (bool Success, DataTable? Table, ImportPricingResult Error) ReadExcel(Microsoft.AspNetCore.Http.IFormFile file)
        {
            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
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
                    return (false, null, new ImportPricingResult(false, "Excel is empty."));

                return (true, table, new ImportPricingResult(true, null));
            }
            catch (Exception ex)
            {
                return (false, null, new ImportPricingResult(false, ex.Message));
            }
        }
    }
}
