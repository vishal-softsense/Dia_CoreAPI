using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Utility;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;

namespace Dia_CoreAPI.Services.Helpers
{
    public class ImportGradingProcessor
    {
        private readonly IImportGradingRepository _repository;

        public ImportGradingProcessor(IImportGradingRepository repository)
        {
            _repository = repository;
        }

        public Task<ImportGradingResult> UpdateGiaDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => ValidateExcelAndReturnAsync(dto, "GIA Data Upload Succesfull.");

        public Task<ImportGradingResult> UpdateIgiDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => ValidateExcelAndReturnAsync(dto, "IGI Data Upload Succesfull.");

        public Task<ImportGradingResult> UpdateHrdDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => ValidateExcelAndReturnAsync(dto, "HRD Data Upload Succesfull.");

        public Task<ImportGradingResult> UpdateGraderDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => ValidateExcelAndReturnAsync(dto, "Grader Data Upload Succesfull.");

        public Task<ImportGradingResult> UpdatePurchaseDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => ValidateExcelAndReturnAsync(dto, "Purchase Data Upload Succesfull.");

        public async Task<ImportGradingResult> UpdateimagevideoAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
        {
            var tableOrError = ReadExcel(dto.ImportFile);
            if (!tableOrError.Success)
                return tableOrError.Error;

            var dt = tableOrError.Table!;
            if (dt.Columns.Count < 4 || !dt.Columns[1].ColumnName.Equals("Image", StringComparison.OrdinalIgnoreCase))
                return new ImportGradingResult(false, "Image/Video File is not valid.");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                await _repository.UpdateImageVideoPathAsync(
                    dt.Rows[i][0]?.ToString() ?? string.Empty,
                    dt.Rows[i][1]?.ToString() ?? string.Empty,
                    dt.Rows[i][2]?.ToString() ?? string.Empty,
                    dt.Rows[i][3]?.ToString() ?? string.Empty,
                    "Update",
                    companyId);
            }

            return new ImportGradingResult(true, "Image/Video Data Upload Succesfull.");
        }

        public Task<ImportGradingResult> InsUpdparcelitemAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => ValidateExcelAndReturnAsync(dto, "Parcel Item Data Upload Succesfull.");

        public Task<ImportGradingResult> InsUpdsolitaireitemAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => ValidateExcelAndReturnAsync(dto, "Solitaire Item Data Upload Succesfull.");

        public Task<ImportGradingResult> InsUpdaccountitemAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => ValidateExcelAndReturnAsync(dto, "Account Data Upload Succesfull.");

        public Task<ImportGradingResult> UploadPurchaseDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => ValidateExcelAndReturnAsync(dto, "Purchase Data Upload Succesfull.");

        public Task<ImportGradingResult> InsertSaleDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => ValidateExcelAndReturnAsync(dto, "Sale Data Upload Succesfull.");

        public Task<ImportGradingResult> UploadPurchaseDetailsDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => ValidateExcelAndReturnAsync(dto, "Purchase Transaction Upload Succesfull.");

        public Task<ImportGradingResult> UploadJewelleryStockAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => ValidateExcelAndReturnAsync(dto, "Jewellery Stock Upload Succesfull.");

        public Task<ImportGradingResult> UploadJewelleryDiamondStockAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => ValidateExcelAndReturnAsync(dto, "Jewellery Diamond Stock Upload Succesfull.");

        public async Task<ImportGradingResult> UpdateSizeIdsAsync(string companyId, string userTypeId)
        {
            // Same as MVC ImportGradingController.UpdateSizeIds → SqlDataAccess.UpdateSizeIds()
            await _repository.UpdateSizeIdsAsync(companyId);
            return new ImportGradingResult(true, "Size Updated Successfully.");
        }

        public async Task<ImportGradingResult> DeleteDeletedRecordsAsync(string companyId, string userTypeId)
        {
            await _repository.DelRecordsAsync("DeleteFlag = 1", companyId);
            return new ImportGradingResult(true, "Delete Deleted Records Successfully.");
        }

        public async Task<ImportGradingResult> UploadAndUpdateCompanyLoginLogoAsync(IFormFile image, string companyId, string userTypeId, string webRootPath)
        {
            if (image == null || image.Length == 0)
                return new ImportGradingResult(false, null, "No file selected");

            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (extension != ".jpg" && extension != ".jpeg")
                return new ImportGradingResult(false, null, "Only JPG or JPEG images allowed");

            var root = string.IsNullOrWhiteSpace(webRootPath) ? Directory.GetCurrentDirectory() : webRootPath;
            var relativeFolder = Path.Combine("CRreport", "logos");
            var folderPath = Path.Combine(root, relativeFolder);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = image.FileName;
            var fullPath = Path.Combine(folderPath, fileName);
            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var imagePath = "/" + relativeFolder.Replace("\\", "/") + "/" + fileName;
            await _repository.UpdateCompanyLoginLogoAsync(imagePath, "UpdateLogo", companyId);
            return new ImportGradingResult(true, null, "Company Logo updated successfully", imagePath);
        }

        public Task<ImportGradingResult> UploadJewelleryFullAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => ValidateExcelAndReturnAsync(dto, "Jewellery Full Upload Succesfull.");

        public async Task<ImportGradingResult> ImportAskingRateAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
        {
            var tableOrError = ReadExcel(dto.ImportFile);
            if (!tableOrError.Success)
                return tableOrError.Error;

            var dt = tableOrError.Table!;
            if (dt.Columns.Count < 2 ||
                !dt.Columns[0].ColumnName.Equals("LotNo", StringComparison.OrdinalIgnoreCase) ||
                !dt.Columns[1].ColumnName.Equals("AskingRate", StringComparison.OrdinalIgnoreCase))
            {
                return new ImportGradingResult(false, "This File is not valid.");
            }

            int rowsUpdated = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var lotNumber = dt.Rows[i][0]?.ToString()?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(lotNumber))
                    continue;

                var askingRate = dt.Rows[i][1]?.ToString()?.Trim() ?? "0";
                var purchaseRate = dt.Columns.Count > 2 ? (dt.Rows[i][2]?.ToString()?.Trim() ?? "0") : "0";

                await _repository.UpdateAskingPurchaseRateAsync(lotNumber, purchaseRate, askingRate, companyId);
                rowsUpdated++;
            }

            if (rowsUpdated == 0)
                return new ImportGradingResult(false, "No records were updated. Please check the Lot Numbers in your file.");

            return new ImportGradingResult(true, $"Asking Rate Upload Successful. {rowsUpdated} items updated.");
        }

        private Task<ImportGradingResult> ValidateExcelAndReturnAsync(ImportGradingDTO dto, string successMessage)
        {
            var tableOrError = ReadExcel(dto.ImportFile);
            if (!tableOrError.Success)
                return Task.FromResult(tableOrError.Error);

            return Task.FromResult(new ImportGradingResult(true, successMessage));
        }

        private (bool Success, DataTable? Table, ImportGradingResult Error) ReadExcel(IFormFile? file)
        {
            if (file == null)
                return (false, null, new ImportGradingResult(false, "No file uploaded."));

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".xls" && extension != ".xlsx")
                return (false, null, new ImportGradingResult(false, "Invalid file type. Only .xls and .xlsx are allowed."));

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
                    return (false, null, new ImportGradingResult(false, "Excel is empty."));

                return (true, table, new ImportGradingResult(true, null));
            }
            catch (Exception ex)
            {
                return (false, null, new ImportGradingResult(false, ex.Message));
            }
        }
    }
}
