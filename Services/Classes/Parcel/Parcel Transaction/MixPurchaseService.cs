using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Transaction;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;

namespace Dia_CoreAPI.Services.Classes.Parcel.Parcel_Transaction
{
    public class MixPurchaseService : IMixPurchaseService
    {
        private readonly IMixPurchaseRepository _repository;

        public MixPurchaseService(IMixPurchaseRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);

        public Task<List<MixPurchaseInvListDto>> GetListAsync(string companyId)
            => _repository.GetListAsync(1, companyId);

        public Task<MixPurchasePageDataDto> GetPageDataAsync(string companyId)
            => _repository.GetPageDataAsync(companyId);

        public Task<MixPurchaseDetailDto?> GetDetailAsync(int id, string companyId)
            => _repository.GetDetailAsync(id, companyId);

        public Task<List<MixPurchaseLotDetailDto>> GetLotDetailAsync(string lotNo, string companyId)
            => _repository.GetLotDetailAsync(lotNo, companyId);

        public Task<List<MixConsignmentRowDto>> GetConsignmentAsync(string? partyCode, string? memoNo, string companyId)
            => _repository.GetConsignmentAsync(partyCode, memoNo, companyId);

        public Task<MixPurchaseGridViewDto> AddItemInGridAsync(MixPurchaseAddLotRequest request, string companyId)
            => _repository.AddItemInGridAsync(request, companyId);

        public Task<(bool Success, string Message)> AddAsync(MixPurchaseSaveRequest request, string userId, string companyId)
            => _repository.AddAsync(request.SalePur, request.Items ?? new(), userId, companyId);

        public Task<(bool Success, string Message)> EditAsync(MixPurchaseSaveRequest request, string userId, string companyId)
            => _repository.EditAsync(request.SalePur, request.Items ?? new(), userId, companyId);

        public Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId)
            => _repository.DeleteAsync(id, userId, companyId);

        public Task<MixPurchasePrintViewDto?> GetPrintViewAsync(string invNo, string companyId)
            => _repository.GetPrintViewAsync(invNo, companyId);

        public Task<MixPurchaseStatusDto?> GetCurrentStatusAsync(string invNo, string companyId)
            => _repository.GetCurrentStatusAsync(invNo, companyId);

        public Task<List<object>> GetLotsAsync(string? searchBy, string companyId)
            => _repository.GetLotsAsync(searchBy, companyId);

        public Task<string> GetMixLotNoDIAsync(string companyId)
            => _repository.GetMixLotNoDIAsync(companyId);

        /// <summary>MVC MixPurchaseController.GetItemParcelDetails (mix-purchase/import-excel-data).</summary>
        public async Task<(bool Success, object? Data)> ImportExcelDataAsync(
            IFormFile? file,
            float less1,
            float less2,
            float less3,
            string? curCode,
            List<MixPurchaseItemDto>? existingItems,
            string companyId)
        {
            _ = curCode;

            string[] validFileTypes = { ".xls", ".xlsx" };
            if (file == null || file.Length == 0)
                return (false, "Please select an Excel file to import.");

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!validFileTypes.Contains(extension))
                return (false, "File format is not valid.");

            try
            {
                var readResult = ReadExcel(file);
                if (!readResult.Success || readResult.Table == null)
                    return (false, readResult.ErrorMessage ?? "Failed to read Excel file.");

                var dt = readResult.Table;
                string caratFormat = await _repository.GetSettingValueAsync("CaratFormat", companyId) ?? "0.00";

                if (!dt.Columns.Contains("LotNo") || !dt.Columns.Contains("Shape")
                    || !dt.Columns.Contains("Carats") || !dt.Columns.Contains("Color")
                    || !dt.Columns.Contains("Clarity") || !dt.Columns.Contains("PurchaseRate"))
                {
                    return (false, "** Required column missing. Please check upload format.");
                }

                foreach (DataRow row in dt.Rows)
                {
                    float tempCarat = float.Parse(Cell(row, "Carats"));
                    string carat3D = Convert.ToDouble(tempCarat).ToString("N3");
                    string carat2D = carat3D.Substring(0, carat3D.Length - 1);
                    double carat = caratFormat == "0.000"
                        ? Convert.ToDouble(carat3D)
                        : Convert.ToDouble(carat2D);

                    double back = 0;
                    double purRate = 0;
                    double askingBack = 0;
                    double askingRate = 0;
                    double actualBack = 0;
                    double actualRate = 0;

                    if (!string.IsNullOrWhiteSpace(Cell(row, "PurchaseRate")))
                    {
                        purRate = Convert.ToDouble(Cell(row, "PurchaseRate")) > 0
                            ? Convert.ToDouble(Cell(row, "PurchaseRate"))
                            : 0;
                    }

                    if (!string.IsNullOrWhiteSpace(Cell(row, "Rejection")))
                    {
                        back = Convert.ToDouble(Cell(row, "Rejection")) != 0
                            ? Convert.ToDouble(Cell(row, "Rejection"))
                            : 0;
                    }

                    if (!string.IsNullOrWhiteSpace(Cell(row, "AskingRate")))
                    {
                        askingRate = Convert.ToDouble(Cell(row, "AskingRate")) > 0
                            ? Convert.ToDouble(Cell(row, "AskingRate"))
                            : 0;
                    }

                    if (!string.IsNullOrWhiteSpace(Cell(row, "ActualRate")))
                    {
                        actualRate = Convert.ToDouble(Cell(row, "ActualRate")) > 0
                            ? Convert.ToDouble(Cell(row, "ActualRate"))
                            : 0;
                    }

                    float issueCarats = 0;
                    if (!string.IsNullOrWhiteSpace(Cell(row, "IssueCarats")))
                    {
                        issueCarats = float.Parse(Cell(row, "IssueCarats"));
                        if (issueCarats == 0)
                        {
                            issueCarats = await _repository.GetRapRateAsync(
                                Cell(row, "Shape"), Cell(row, "Color"), Cell(row, "Clarity"), carat, companyId);
                        }
                    }
                    else
                    {
                        issueCarats = await _repository.GetRapRateAsync(
                            Cell(row, "Shape"), Cell(row, "Color"), Cell(row, "Clarity"), carat, companyId);
                    }

                    if (carat > 0)
                    {
                        if (purRate == 0 && back == 0)
                            return (false, "Rate and Discount both can not be 0.");

                        if (purRate != 0 && issueCarats != 0)
                            back = (((purRate * 100) / issueCarats) - 100);
                        else if (issueCarats != 0)
                            purRate = (((100 + back) * issueCarats) / 100);

                        if (askingRate != 0 && issueCarats != 0)
                            askingBack = (((askingRate * 100) / issueCarats) - 100);
                        else if (issueCarats != 0)
                            askingRate = (((100 + askingBack) * issueCarats) / 100);

                        if (actualRate != 0 && issueCarats != 0)
                            actualBack = (((actualRate * 100) / issueCarats) - 100);
                        else if (issueCarats != 0)
                            actualRate = (((100 + actualBack) * issueCarats) / 100);

                        double amount = carat * purRate;
                        double l1 = less1 != 0 ? amount * (100 + less1) / 100 : amount;
                        double l2 = less2 != 0 ? l1 * (100 + less2) / 100 : l1;
                        double l3 = less3 != 0 ? l2 * (100 + less3) / 100 : l2;

                        if (dt.Columns.Contains("PurchaseAmount"))
                            row["PurchaseAmount"] = l3.ToString("N2");
                        if (dt.Columns.Contains("AskingRate"))
                            row["AskingRate"] = askingRate.ToString("N2");
                        if (dt.Columns.Contains("ActualRate"))
                            row["ActualRate"] = actualRate.ToString("N2");
                    }
                    else
                    {
                        return (false, "Carat is invalid.");
                    }
                }

                var purExcel = existingItems != null
                    ? new List<MixPurchaseItemDto>(existingItems)
                    : new List<MixPurchaseItemDto>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var excelRow = dt.Rows[i];
                    var pur = new MixPurchaseItemDto { SrNo = i + 1 };

                    // MVC: Pur.LotNo = dt.Rows[i]["LotNo"].ToString();
                    pur.LotNo = Cell(excelRow, "LotNo");

                    // MVC: GetMixLotNoPurData(" AND LotNo = ''" + Pur.LotNo.Replace("'", "''") + "'' ")
                    MixPurchaseLotDetailDto? lot = null;
                    if (!string.IsNullOrWhiteSpace(pur.LotNo))
                    {
                        var lotData = await _repository.GetLotDetailAsync(pur.LotNo, companyId);
                        lot = lotData.FirstOrDefault();
                    }

                    ApplyImportSpecFields(pur, excelRow, lot);

                    pur.MfgSeriesID = Cell(excelRow, "Kapan/Inv No");
                    pur.LotNo = Cell(excelRow, "LotNo");
                    pur.Weight = ParseFloat(Cell(excelRow, "Carats"));
                    pur.Pcs = ParseInt(Cell(excelRow, "Pcs"));
                    pur.IssueCarats = ParseFloat(Cell(excelRow, "IssueCarats"));
                    pur.PurchaseBack = ParseFloat(Cell(excelRow, "Rej%"));
                    pur.ReturnCarats = ParseFloat(Cell(excelRow, "Rejection"));
                    pur.Weight = ParseFloat(Cell(excelRow, "Carats"));
                    pur.PurchaseRate = ParseFloat(Cell(excelRow, "PurchaseRate"));
                    pur.PurAmount = ParseFloat(Cell(excelRow, "PurchaseAmount"));
                    pur.SuppLotNo = Cell(excelRow, "SuppLotNo");
                    pur.AskingRate = ParseFloat(Cell(excelRow, "AskingRate"));
                    pur.ActualRate = ParseFloat(Cell(excelRow, "ActualRate"));
                    pur.GroupId = Cell(excelRow, "GroupId");
                    pur.StoneType = Cell(excelRow, "StoneType");
                    pur.RMK = Cell(excelRow, "RMK");
                    pur.StockType = Cell(excelRow, "StockType");

                    SyncDisplayFieldsFromPurchaseFields(pur);
                    purExcel.Add(pur);
                }

                double totalCarats = 0;
                double totalAmount = 0;
                double totalMRPAmount = 0;
                double totalAmountWithoutless = 0;

                foreach (var item in purExcel)
                {
                    float tempWeight = item.Weight;
                    string weight3D = Convert.ToDouble(tempWeight).ToString("N3");
                    string weight2D = weight3D.Substring(0, weight3D.Length - 1);
                    double weight = caratFormat == "0.000"
                        ? Convert.ToDouble(weight3D)
                        : Convert.ToDouble(weight2D);

                    totalCarats += weight;
                    totalAmount += item.PurAmount;
                    totalMRPAmount += weight * item.MRP;
                    totalAmountWithoutless += weight * item.PurchaseRate;
                }

                return (true, new MixPurchaseGridViewDto
                {
                    TotalCarats = Math.Round(totalCarats, 3),
                    TotalAmount = totalAmount,
                    TotalMRPAmount = Math.Round(totalMRPAmount, 3),
                    TotalAmountWithoutless = Math.Round(totalAmountWithoutless, 3),
                    ExcelData = purExcel
                });
            }
            catch (Exception ex)
            {
                return (false, ex.ToString());
            }
        }

        /// <summary>
        /// MVC MixPurchaseController import loop: Excel Shape/Size./Color/Clarity/Sieve override,
        /// else fallback to GetMixLotNoPurData lot master fields.
        /// </summary>
        private static void ApplyImportSpecFields(
            MixPurchaseItemDto pur,
            DataRow excelRow,
            MixPurchaseLotDetailDto? lot)
        {
            string shape = Cell(excelRow, "Shape");
            string size = Cell(excelRow, "Size.");
            string color = Cell(excelRow, "Color");
            string clarity = Cell(excelRow, "Clarity");
            string sieve = Cell(excelRow, "Sieve");

            if (!string.IsNullOrWhiteSpace(shape)
                || !string.IsNullOrWhiteSpace(size)
                || !string.IsNullOrWhiteSpace(color)
                || !string.IsNullOrWhiteSpace(clarity)
                || !string.IsNullOrWhiteSpace(sieve))
            {
                pur.PShape = shape;
                pur.PSize = size;
                pur.PColor = color;
                pur.PClarity = clarity;
                pur.PSieve = sieve;
                pur.ItemName = shape + " " + color + " " + clarity;
            }
            else if (lot != null)
            {
                pur.PShape = lot.ShapeId;
                pur.PSize = lot.SizeId;
                pur.PColor = lot.ColorId;
                pur.PClarity = lot.ClarityId;
                pur.PSieve = lot.SieveId;
                pur.ItemName = lot.ShapeId + " " + lot.ColorId + " " + lot.ClarityId;
            }

            // MVC line 2824: Pur.PSieve = dt.Rows[i]["Sieve"].ToString();
            pur.PSieve = Cell(excelRow, "Sieve");
        }

        private static void SyncDisplayFieldsFromPurchaseFields(MixPurchaseItemDto pur)
        {
            pur.Shape = pur.PShape;
            pur.ShapeId = pur.PShape;
            pur.Size = pur.PSize;
            pur.Color = pur.PColor;
            pur.ColorId = pur.PColor;
            pur.Clarity = pur.PClarity;
            pur.ClarityId = pur.PClarity;
            pur.Sieve = pur.PSieve;
            pur.SieveId = pur.PSieve;
            pur.Rate = pur.PurchaseRate;
            pur.Amount = pur.PurAmount;
            pur.Carats = pur.Weight;
        }

        private static string Cell(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value)
                return string.Empty;
            return row[column]?.ToString()?.Trim() ?? string.Empty;
        }

        private static float ParseFloat(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;
            return float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                ? result
                : float.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out result)
                    ? result
                    : 0;
        }

        private static int ParseInt(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;
            return int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                ? result
                : int.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out result)
                    ? result
                    : 0;
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
