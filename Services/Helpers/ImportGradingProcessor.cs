using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Utility;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;

namespace Dia_CoreAPI.Services.Helpers
{
    public partial class ImportGradingProcessor
    {
        private readonly IImportGradingRepository _repository;
        private readonly ISqlDataAccess _sqlDataAccess;

        public ImportGradingProcessor(IImportGradingRepository repository, ISqlDataAccess sqlDataAccess)
        {
            _repository = repository;
            _sqlDataAccess = sqlDataAccess;
        }

        private async Task<SqlTempAccess> CreateSqlTempAsync(string companyId)
        {
            var cs = await _sqlDataAccess.GetClientConnectionString(companyId);
            return new SqlTempAccess(cs);
        }

        public async Task<ImportGradingResult> UpdateGiaDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
        {
            var tableOrError = ReadExcel(dto.ImportFile);
            if (!tableOrError.Success) return tableOrError.Error;

            var dt = tableOrError.Table!;
            if (dt.Columns.Count < 6 || dt.Columns[5].ColumnName != "Client Ref.")
                return new ImportGradingResult(false, "GIA File is not valid.");

            int companyID = ParseCompanyId(companyId);
            int saved = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var lotNo = Cell(dt.Rows[i], 5);
                var itemOrError = await LoadLotAsync(lotNo, companyId, "Please create a lot number in the Item Master before adding a new record from Excel.");
                if (!itemOrError.Success) return itemOrError.Error;

                foreach (var item in itemOrError.Items)
                {
                    var shape = Cell(dt.Rows[i], 7);
                    var (l, w, d, lw) = Measure(dt.Rows[i], 8, 9, 10, shape);
                    var args = LabSaveArgs.FromItem(item, companyID);
                    args.Weight = CellDbl(dt.Rows[i], 11);
                    args.Shape = shape;
                    args.Color = Cell(dt.Rows[i], 12);
                    args.ColorDisc = Cell(dt.Rows[i], 13);
                    args.Clarity = Cell(dt.Rows[i], 14);
                    args.ClarityStatus = Cell(dt.Rows[i], 15);
                    args.Proportion = Cell(dt.Rows[i], 16);
                    args.Polish = Cell(dt.Rows[i], 17);
                    args.Symmetry = Cell(dt.Rows[i], 18);
                    args.Fl = Cell(dt.Rows[i], 19);
                    args.FlColor = Cell(dt.Rows[i], 20);
                    args.ReportDate = Cell(dt.Rows[i], 4);
                    args.L = l; args.W = w; args.D = d; args.LW = lw;
                    args.TopDepth = CellDbl(dt.Rows[i], 24);
                    args.Table = CellDbl(dt.Rows[i], 25);
                    args.CA = CellDbl(dt.Rows[i], 26);
                    args.CH = CellDbl(dt.Rows[i], 27);
                    args.PA = CellDbl(dt.Rows[i], 28);
                    args.PH = CellDbl(dt.Rows[i], 29);
                    args.GirdlePer = CellPct(dt.Rows[i], 39);
                    args.GirdleCondition = Cell(dt.Rows[i], 22);
                    args.GMin = Cell(dt.Rows[i], 21);
                    args.GMax = Cell(dt.Rows[i], 21);
                    args.Culet = Cell(dt.Rows[i], 23);
                    args.StrLn = CellPct(dt.Rows[i], 30);
                    args.LrHalf = CellPct(dt.Rows[i], 31);
                    args.InscriptionText = Cell(dt.Rows[i], 37);
                    args.KeyToSymbols = Cell(dt.Rows[i], 35);
                    args.LabComments = Cell(dt.Rows[i], 36);
                    args.CertId = "GIA";
                    args.CertNo = Cell(dt.Rows[i], 3);
                    args.Size = "";
                    args.GraderName = "GIA";
                    args.Status = "Selected";
                    await SaveLabAsync(companyId, args);
                    saved++;
                }
            }

            return saved > 0
                ? new ImportGradingResult(true, "GIA Data Upload Succesfull.")
                : new ImportGradingResult(false, "GIA File is not valid.");
        }

        public async Task<ImportGradingResult> UpdateIgiDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
        {
            var tableOrError = ReadExcel(dto.ImportFile);
            if (!tableOrError.Success) return tableOrError.Error;
            var dt = tableOrError.Table!;
            int companyID = ParseCompanyId(companyId);

            if (dto.ChkReplace != true)
                return await UpdateIgiOldPrintAsync(dt, companyId, companyID);
            return await UpdateIgiNewPrintAsync(dt, companyId, companyID);
        }

        private async Task<ImportGradingResult> UpdateIgiOldPrintAsync(DataTable dt, string companyId, int companyID)
        {
            if (dt.Columns.Count < 6 || dt.Columns[5].ColumnName != "Client Ref")
                return new ImportGradingResult(false, "IGI File is not valid.");

            int saved = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var itemOrError = await LoadLotAsync(Cell(dt.Rows[i], 5), companyId, "  Please create a lot number in the Item Master before adding a new record from Excel...!");
                if (!itemOrError.Success) return itemOrError.Error;

                foreach (var item in itemOrError.Items)
                {
                    var shape = await MapParam("Shape", Cell(dt.Rows[i], 7), "IGI", companyId);
                    var (l, w, d, lw) = Measure(dt.Rows[i], 8, 9, 10, shape);
                    var args = LabSaveArgs.FromItem(item, companyID);
                    args.Weight = CellDbl(dt.Rows[i], 11);
                    args.Shape = shape;
                    args.Color = Cell(dt.Rows[i], 12);
                    args.ColorDisc = Cell(dt.Rows[i], 13);
                    args.Clarity = await MapParam("Clarity", Cell(dt.Rows[i], 14), "IGI", companyId);
                    args.Size = Cell(dt.Rows[i], 7);
                    args.Proportion = await MapParam("CUT", Cell(dt.Rows[i], 16), "IGI", companyId);
                    args.Polish = await MapParam("Polish", Cell(dt.Rows[i], 17), "IGI", companyId);
                    args.Symmetry = await MapParam("Symmetry", Cell(dt.Rows[i], 18), "IGI", companyId);
                    args.ClarityStatus = Cell(dt.Rows[i], 15);
                    args.Fl = Cell(dt.Rows[i], 19);
                    args.FlColor = Cell(dt.Rows[i], 20);
                    args.ReportDate = Cell(dt.Rows[i], 4);
                    args.CertNo = Cell(dt.Rows[i], 3);
                    args.L = l; args.W = w; args.D = d; args.LW = lw;
                    args.TopDepth = CellDbl(dt.Rows[i], 24);
                    args.Table = CellDbl(dt.Rows[i], 25);
                    args.CA = CellDbl(dt.Rows[i], 26);
                    args.CH = CellDbl(dt.Rows[i], 27);
                    args.PA = CellDbl(dt.Rows[i], 28);
                    args.PH = CellDbl(dt.Rows[i], 29);
                    args.GirdlePer = CellPct(dt.Rows[i], 39);
                    args.GirdleCondition = Cell(dt.Rows[i], 22);
                    args.GMin = Cell(dt.Rows[i], 21);
                    args.GMax = Cell(dt.Rows[i], 21);
                    args.Culet = Cell(dt.Rows[i], 23);
                    args.StrLn = CellPct(dt.Rows[i], 30);
                    args.LrHalf = CellPct(dt.Rows[i], 31);
                    args.InscriptionText = Cell(dt.Rows[i], 37);
                    args.KeyToSymbols = Cell(dt.Rows[i], 35);
                    args.LabComments = Cell(dt.Rows[i], 36);
                    args.CertId = "IGI";
                    args.GraderName = "IGI";
                    args.Status = "Selected";
                    args.UseSizeOnItemMaster = false;
                    await SaveLabAsync(companyId, args);
                    saved++;
                }
            }

            return saved > 0
                ? new ImportGradingResult(true, "IGI Data Upload Succesfull.")
                : new ImportGradingResult(false, "IGI File is not valid.");
        }

        private async Task<ImportGradingResult> UpdateIgiNewPrintAsync(DataTable dt, string companyId, int companyID)
        {
            if (dt.Columns.Count < 36 || dt.Columns[35].ColumnName != "Customer Ref No.")
                return new ImportGradingResult(false, "IGI File is not valid.");

            int saved = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var itemOrError = await LoadLotAsync(Cell(dt.Rows[i], 35), companyId, "Please create a lot number in the Item Master before adding a new record from Excel.");
                if (!itemOrError.Success) return itemOrError.Error;

                foreach (var item in itemOrError.Items)
                {
                    var shape = await MapParam("Shape", Cell(dt.Rows[i], 5), "IGI", companyId);
                    var (l, w, d, lw) = Measure(dt.Rows[i], 9, 10, 11, shape);
                    var args = LabSaveArgs.FromItem(item, companyID);
                    args.Weight = CellDbl(dt.Rows[i], 6);
                    args.Shape = shape;
                    args.Color = Cell(dt.Rows[i], 25);
                    args.ColorDisc = Cell(dt.Rows[i], 13);
                    args.Clarity = await MapParam("Clarity", Cell(dt.Rows[i], 24), "IGI", companyId);
                    args.Size = await _repository.GetSizeAsync(Cell(dt.Rows[i], 6), "GetSize", companyId);
                    args.Proportion = await MapParam("CUT", Cell(dt.Rows[i], 23), "IGI", companyId);
                    args.Polish = await MapParam("Polish", Cell(dt.Rows[i], 21), "IGI", companyId);
                    args.Symmetry = await MapParam("Symmetry", Cell(dt.Rows[i], 21), "IGI", companyId);
                    args.ClarityStatus = Cell(dt.Rows[i], 15);
                    args.Fl = Cell(dt.Rows[i], 31);
                    args.FlColor = "";
                    args.ReportDate = Cell(dt.Rows[i], 1);
                    args.CertNo = Cell(dt.Rows[i], 2);
                    args.L = l; args.W = w; args.D = d; args.LW = lw;
                    args.TopDepth = CellDbl(dt.Rows[i], 15);
                    args.Table = CellDbl(dt.Rows[i], 12);
                    args.CA = CellDbl(dt.Rows[i], 16);
                    args.CH = CellDbl(dt.Rows[i], 13);
                    args.PA = CellDbl(dt.Rows[i], 17);
                    args.PH = CellDbl(dt.Rows[i], 14);
                    args.GirdlePer = CellPct(dt.Rows[i], 19);
                    args.GirdleCondition = Cell(dt.Rows[i], 22);
                    args.GMin = Cell(dt.Rows[i], 20);
                    args.GMax = Cell(dt.Rows[i], 20);
                    args.Culet = Cell(dt.Rows[i], 18);
                    args.InscriptionText = Cell(dt.Rows[i], 38);
                    args.KeyToSymbols = Cell(dt.Rows[i], 40);
                    args.LabComments = Cell(dt.Rows[i], 34);
                    args.CertId = "IGI";
                    args.GraderName = "IGI";
                    args.Status = "Selected";
                    await SaveLabAsync(companyId, args);
                    saved++;
                }
            }

            return saved > 0
                ? new ImportGradingResult(true, "IGI Data Upload Succesfull.")
                : new ImportGradingResult(false, "IGI File is not valid.");
        }

        public async Task<ImportGradingResult> UpdateHrdDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
        {
            var tableOrError = ReadExcel(dto.ImportFile);
            if (!tableOrError.Success) return tableOrError.Error;
            var dt = tableOrError.Table!;
            int companyID = ParseCompanyId(companyId);

            if (dto.ChkReplace != true)
                return await UpdateHrdOldPrintAsync(dt, companyId, companyID);
            return await UpdateHrdNewPrintAsync(dt, companyId, companyID);
        }

        private async Task<ImportGradingResult> UpdateHrdOldPrintAsync(DataTable dt, string companyId, int companyID)
        {
            if (dt.Columns.Count < 4 || dt.Columns[3].ColumnName != "Client Ref.")
                return new ImportGradingResult(false, "Hrd file is not valid.");

            int saved = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var itemOrError = await LoadLotAsync(Cell(dt.Rows[i], 3), companyId, "Please create a lot number in the Item Master before adding a new record from Excel.");
                if (!itemOrError.Success) return itemOrError.Error;

                foreach (var item in itemOrError.Items)
                {
                    var shape = await MapParam("Shape", Cell(dt.Rows[i], 4), "HRD", companyId);
                    var (l, w, d, lw) = Measure(dt.Rows[i], 12, 13, 14, shape);
                    var args = LabSaveArgs.FromItem(item, companyID);
                    args.Weight = CellDbl(dt.Rows[i], 5);
                    args.Shape = shape;
                    args.Color = Cell(dt.Rows[i], 10);
                    args.Clarity = await MapParam("Clarity", Cell(dt.Rows[i], 6), "HRD", companyId);
                    args.Proportion = await MapParam("CUT", Cell(dt.Rows[i], 20), "HRD", companyId);
                    args.Polish = await MapParam("Polish", Cell(dt.Rows[i], 21), "HRD", companyId);
                    args.Symmetry = await MapParam("Symmetry", Cell(dt.Rows[i], 22), "HRD", companyId);
                    args.Size = item.SizeId;
                    args.Fl = Cell(dt.Rows[i], 9);
                    args.CertNo = Cell(dt.Rows[i], 0);
                    args.ReportDate = DateTime.Now.ToString();
                    args.L = l; args.W = w; args.D = d; args.LW = lw;
                    args.Table = CellDbl(dt.Rows[i], 17);
                    args.CH = CellDbl(dt.Rows[i], 18);
                    args.PA = CellDbl(dt.Rows[i], 19);
                    args.GirdleCondition = Cell(dt.Rows[i], 22);
                    args.GMin = Cell(dt.Rows[i], 21);
                    args.GMax = Cell(dt.Rows[i], 21);
                    args.Culet = Cell(dt.Rows[i], 16);
                    args.InscriptionText = "HRD" + Cell(dt.Rows[i], 0);
                    args.LabComments = Cell(dt.Rows[i], 25);
                    args.CertId = "HRD";
                    args.GraderName = "HRD";
                    args.Status = "Selected";
                    await SaveLabAsync(companyId, args);
                    saved++;
                }
            }

            return saved > 0
                ? new ImportGradingResult(true, "Hrd Data Upload Succesfull.")
                : new ImportGradingResult(false, "Hrd file is not valid.");
        }

        private async Task<ImportGradingResult> UpdateHrdNewPrintAsync(DataTable dt, string companyId, int companyID)
        {
            if (dt.Columns.Count < 5 || dt.Columns[4].ColumnName != "Client ref")
                return new ImportGradingResult(false, "Hrd file is not valid.");

            int saved = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var itemOrError = await LoadLotAsync(Cell(dt.Rows[i], 4), companyId, "Please create a lot number in the Item Master before adding a new record from Excel....!");
                if (!itemOrError.Success) return itemOrError.Error;

                foreach (var item in itemOrError.Items)
                {
                    var shape = await MapParam("Shape", Cell(dt.Rows[i], 5), "HRD", companyId);
                    var (l, w, d, lw) = Measure(dt.Rows[i], 14, 15, 16, shape);
                    var args = LabSaveArgs.FromItem(item, companyID);
                    args.Weight = CellDbl(dt.Rows[i], 6);
                    args.Shape = shape;
                    args.Color = Cell(dt.Rows[i], 12);
                    args.Clarity = await MapParam("Clarity", Cell(dt.Rows[i], 8), "HRD", companyId);
                    args.Proportion = await MapParam("CUT", Cell(dt.Rows[i], 33), "HRD", companyId);
                    args.Polish = await MapParam("Polish", Cell(dt.Rows[i], 34), "HRD", companyId);
                    args.Symmetry = await MapParam("Symmetry", Cell(dt.Rows[i], 35), "HRD", companyId);
                    args.Size = item.SizeId;
                    args.Fl = Cell(dt.Rows[i], 11);
                    args.CertNo = Cell(dt.Rows[i], 0);
                    args.ReportDate = DateTime.Now.ToString();
                    args.L = l; args.W = w; args.D = d; args.LW = lw;
                    args.Table = CellDbl(dt.Rows[i], 23);
                    args.CH = CellDbl(dt.Rows[i], 24);
                    args.PA = CellDbl(dt.Rows[i], 25);
                    args.GirdleCondition = Cell(dt.Rows[i], 36);
                    args.GMin = Cell(dt.Rows[i], 35);
                    args.GMax = Cell(dt.Rows[i], 35);
                    args.Culet = Cell(dt.Rows[i], 22);
                    args.InscriptionText = "HRD" + Cell(dt.Rows[i], 0);
                    args.LabComments = Cell(dt.Rows[i], 38);
                    args.CertId = "HRD";
                    args.GraderName = "HRD";
                    args.Status = "Selected";
                    await SaveLabAsync(companyId, args);
                    saved++;
                }
            }

            return saved > 0
                ? new ImportGradingResult(true, "Hrd Data Upload Succesfull.")
                : new ImportGradingResult(false, "Hrd file is not valid.");
        }

        public async Task<ImportGradingResult> UpdateGraderDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
        {
            var tableOrError = ReadExcel(dto.ImportFile);
            if (!tableOrError.Success) return tableOrError.Error;
            var dt = tableOrError.Table!;
            int companyID = ParseCompanyId(companyId);
            int saved = 0;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (string.IsNullOrEmpty(Cell(dt.Rows[i], 0)))
                    break;

                if (Cell(dt.Rows[i], 40) == "Y")
                    await _repository.UpdateMultiGradingStatusAsync(Cell(dt.Rows[i], 0), "Update", companyId);

                var items = await _repository.SelectMultiGradingAsync(Cell(dt.Rows[i], 0), "Select", companyId);
                if (items == null || items.Count == 0)
                    return new ImportGradingResult(false, "Please create a lot number in the Item Master before adding a new record from Excel.");

                foreach (var item in items)
                {
                    var args = LabSaveArgs.FromItem(item, companyID);
                    args.Weight = CellDbl(dt.Rows[i], 5);
                    args.Shape = Cell(dt.Rows[i], 2);
                    args.Color = Cell(dt.Rows[i], 3);
                    args.Clarity = Cell(dt.Rows[i], 4);
                    args.Size = item.SizeId;
                    args.Proportion = Cell(dt.Rows[i], 10);
                    args.Polish = Cell(dt.Rows[i], 11);
                    args.Symmetry = Cell(dt.Rows[i], 12);
                    args.LW = CellDbl(dt.Rows[i], 15);
                    args.L = CellDbl(dt.Rows[i], 16);
                    args.W = CellDbl(dt.Rows[i], 17);
                    args.D = CellDbl(dt.Rows[i], 18);
                    args.TopDepth = CellDbl(dt.Rows[i], 19);
                    args.Table = CellDbl(dt.Rows[i], 20);
                    args.GirdlePer = CellPct(dt.Rows[i], 21);
                    args.GirdleCondition = Cell(dt.Rows[i], 21);
                    args.CH = CellDbl(dt.Rows[i], 22);
                    args.CA = CellDbl(dt.Rows[i], 23);
                    args.GMin = Cell(dt.Rows[i], 24);
                    args.GMax = Cell(dt.Rows[i], 25);
                    args.PH = CellDbl(dt.Rows[i], 26);
                    args.PA = CellDbl(dt.Rows[i], 27);
                    args.LusterId = Cell(dt.Rows[i], 28);
                    args.OIC = Cell(dt.Rows[i], 29);
                    args.OIP = Cell(dt.Rows[i], 30);
                    args.OIG = Cell(dt.Rows[i], 31);
                    args.OIT = Cell(dt.Rows[i], 32);
                    args.NattsId = Cell(dt.Rows[i], 33);
                    args.CertId = Cell(dt.Rows[i], 34);
                    args.CertNo = Cell(dt.Rows[i], 35);
                    args.LabComments = Cell(dt.Rows[i], 37);
                    args.GraderName = Cell(dt.Rows[i], 38);
                    args.MilkyId = Cell(dt.Rows[i], 41);
                    args.BrownId = Cell(dt.Rows[i], 42);
                    args.HA = Cell(dt.Rows[i], 46);
                    args.ButhId = Cell(dt.Rows[i], 48);
                    args.Culet = Cell(dt.Rows[i], 49);
                    args.Fl = Cell(dt.Rows[i], 14);
                    args.ReportDate = DateTime.Now.ToString();
                    args.Status = Cell(dt.Rows[i], 40) == "Y" ? "Selected" : "";
                    await SaveLabAsync(companyId, args);
                    saved++;
                }
            }

            return saved > 0
                ? new ImportGradingResult(true, "Grader Data Upload Succesfull.")
                : new ImportGradingResult(false, "Grader file is not valid.");
        }

        public async Task<ImportGradingResult> UpdatePurchaseDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
        {
            var tableOrError = ReadExcel(dto.ImportFile);
            if (!tableOrError.Success) return tableOrError.Error;
            var dt = tableOrError.Table!;
            if (dt.Columns.Count < 2 || dt.Columns[1].ColumnName != "Lot No")
                return new ImportGradingResult(false, "Purchase file is not valid.");

            int companyID = ParseCompanyId(companyId);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (string.IsNullOrEmpty(Cell(dt.Rows[i], 56)))
                    break;

                await _repository.UpdateMultiGradingStatusAsync(Cell(dt.Rows[i], 56), "Update", companyId);
                var itemOrError = await LoadLotAsync(Cell(dt.Rows[i], 56), companyId, "Please create a lot number in the Item Master before adding a new record from Excel.");
                if (!itemOrError.Success) return itemOrError.Error;

                foreach (var item in itemOrError.Items)
                {
                    var args = LabSaveArgs.FromItem(item, companyID);
                    args.Weight = CellDbl(dt.Rows[i], 6);
                    args.GraderName = "Purchase";
                    args.CertId = Cell(dt.Rows[i], 39);
                    args.Shape = Cell(dt.Rows[i], 2);
                    args.Color = Cell(dt.Rows[i], 4);
                    args.Clarity = Cell(dt.Rows[i], 5);
                    args.Size = item.SizeId;
                    args.Proportion = Cell(dt.Rows[i], 12);
                    args.Polish = Cell(dt.Rows[i], 13);
                    args.Symmetry = Cell(dt.Rows[i], 14);
                    args.Fl = Cell(dt.Rows[i], 15);
                    args.LW = CellDbl(dt.Rows[i], 16);
                    args.L = CellDbl(dt.Rows[i], 17);
                    args.W = CellDbl(dt.Rows[i], 18);
                    args.D = CellDbl(dt.Rows[i], 19);
                    args.TopDepth = CellDbl(dt.Rows[i], 20);
                    args.Table = CellDbl(dt.Rows[i], 21);
                    args.Culet = Cell(dt.Rows[i], 22);
                    args.GMin = Cell(dt.Rows[i], 23);
                    args.GMax = Cell(dt.Rows[i], 24);
                    args.GirdleMinMs = CellDbl(dt.Rows[i], 25);
                    args.GirdleMaxMs = CellDbl(dt.Rows[i], 26);
                    args.GirdlePer = CellPct(dt.Rows[i], 27);
                    args.BrownId = Cell(dt.Rows[i], 28);
                    args.MilkyId = Cell(dt.Rows[i], 29);
                    args.HA = Cell(dt.Rows[i], 30);
                    args.NattsId = Cell(dt.Rows[i], 31);
                    args.OIT = Cell(dt.Rows[i], 33);
                    args.OIG = Cell(dt.Rows[i], 34);
                    args.OIC = Cell(dt.Rows[i], 35);
                    args.OIP = Cell(dt.Rows[i], 36);
                    args.CertNo = Cell(dt.Rows[i], 40);
                    args.InscriptionText = Cell(dt.Rows[i], 41);
                    args.CA = CellDbl(dt.Rows[i], 42);
                    args.CH = CellDbl(dt.Rows[i], 43);
                    args.PA = CellDbl(dt.Rows[i], 44);
                    args.PH = CellDbl(dt.Rows[i], 45);
                    args.StrLn = CellPct(dt.Rows[i], 46);
                    args.LrHalf = CellPct(dt.Rows[i], 47);
                    args.LusterId = Cell(dt.Rows[i], 48);
                    args.KeyToSymbols = Cell(dt.Rows[i], 49);
                    args.GirdleCondition = Cell(dt.Rows[i], 57);
                    args.CuletCondition = Cell(dt.Rows[i], 64);
                    args.FancyColor = Cell(dt.Rows[i], 69);
                    args.FancyColorIntensity = Cell(dt.Rows[i], 70);
                    args.FancyColorOvertone = Cell(dt.Rows[i], 71);
                    args.Diameter = Cell(dt.Rows[i], 17) + "-" + Cell(dt.Rows[i], 18) + "-" + Cell(dt.Rows[i], 19);
                    args.ReportDate = DateTime.Now.ToString();
                    args.Status = dto.ChkReplace ? "Selected" : "";
                    args.UseFancyOnItemMaster = true;
                    await SaveLabAsync(companyId, args);
                }
            }

            return new ImportGradingResult(true, "Purchase Data Upload Succesfull.");
        }

        public async Task<ImportGradingResult> UpdateimagevideoAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
        {
            var tableOrError = ReadExcel(dto.ImportFile);
            if (!tableOrError.Success) return tableOrError.Error;

            var dt = tableOrError.Table!;
            if (dt.Columns.Count < 4 || dt.Columns[1].ColumnName != "Image")
                return new ImportGradingResult(false, "Image/Video File is not valid.");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                await _repository.UpdateImageVideoPathAsync(
                    Cell(dt.Rows[i], 0),
                    Cell(dt.Rows[i], 1),
                    Cell(dt.Rows[i], 2),
                    Cell(dt.Rows[i], 3),
                    "Update",
                    companyId);
            }

            return new ImportGradingResult(true, "Image/Video Data Upload Succesfull.");
        }

        public async Task<ImportGradingResult> UpdateSizeIdsAsync(string companyId, string userTypeId)
        {
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
            var updated = await _repository.UpdateCompanyLoginLogoAsync(imagePath, "UpdateLogo", companyId);
            if (!updated)
                return new ImportGradingResult(false, null, "DB update failed");

            return new ImportGradingResult(true, null, "Company Logo updated successfully", imagePath);
        }

        public async Task<ImportGradingResult> ImportAskingRateAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
        {
            var tableOrError = ReadExcel(dto.ImportFile);
            if (!tableOrError.Success) return tableOrError.Error;

            var dt = tableOrError.Table!;
            if (dt == null || dt.Columns.Count < 2)
                return new ImportGradingResult(false, "Invalid file format. Please upload the correct Excel file.");

            if (!dt.Columns[0].ColumnName.Equals("LotNo", StringComparison.OrdinalIgnoreCase) ||
                !dt.Columns[1].ColumnName.Equals("AskingRate", StringComparison.OrdinalIgnoreCase))
            {
                return new ImportGradingResult(false, "This File is not valid.");
            }

            int rowsUpdated = 0;
            var lotNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var lotNumber = Cell(dt.Rows[i], 0).Trim();
                if (string.IsNullOrEmpty(lotNumber))
                {
                    var askingValue = Cell(dt.Rows[i], 1).Trim();
                    if (!string.IsNullOrEmpty(askingValue) && askingValue != "0")
                        return new ImportGradingResult(false, $"Lot Number is missing in row {i + 1}.");
                    continue;
                }

                if (!lotNumbers.Add(lotNumber))
                    return new ImportGradingResult(false, $"Duplicate Lot Number found: {lotNumber} in row {i + 1}.");

                var lotExists = await _repository.GetLotExistAsync(" and LotNo='" + lotNumber.Replace("'", "''") + "'",companyId);
                if (lotExists == "0")
                    return new ImportGradingResult(false, $"Lot Number '{lotNumber}' in row {i + 1} is not valid.");

                var askingRate = Cell(dt.Rows[i], 1).Trim();
                if (string.IsNullOrEmpty(askingRate)) askingRate = "0";
                var purchaseRate = dt.Columns.Count > 2 ? Cell(dt.Rows[i], 2).Trim() : "0";
                if (string.IsNullOrEmpty(purchaseRate)) purchaseRate = "0";

                if (await _repository.UpdateAskingPurchaseRateAsync(lotNumber, purchaseRate, askingRate, companyId))
                    rowsUpdated++;
            }

            if (rowsUpdated == 0)
                return new ImportGradingResult(false, "No records were updated. Please check the Lot Numbers in your file.");

            return new ImportGradingResult(true, $"Asking Rate Upload Successful. {rowsUpdated} items updated.");
        }

        private async Task<(bool Success, List<GradingListDto> Items, ImportGradingResult Error)> LoadLotAsync(string lotNo, string companyId, string missingMessage)
        {
            await _repository.UpdateMultiGradingStatusAsync(lotNo, "Update", companyId);
            var items = await _repository.SelectMultiGradingAsync(lotNo, "Select", companyId);
            if (items == null || items.Count == 0)
                return (false, new List<GradingListDto>(), new ImportGradingResult(false, missingMessage));
            return (true, items, new ImportGradingResult(true, null));
        }

        private async Task<string> MapParam(string parameter, string value, string usedFor, string companyId)
        {
            var mapped = await _repository.GetIgiParameterAsync(parameter, value, usedFor, companyId);
            return mapped ?? string.Empty;
        }

        private async Task SaveLabAsync(string companyId, LabSaveArgs a)
        {
            string ReportDate = DateTime.Now.ToString();

            if (a.GraderName == "IGI")
            {
                ReportDate = a.ReportDate;
            }
            else
            {
                ReportDate = "";
            }

            using var sql = await CreateSqlTempAsync(companyId);
            sql.StartTransaction();
            sql.SaveGradingDataForPurchase(
                a.ItemCode, a.LotNo, a.ItemSrNo, a.Weight, "", a.Shape, a.Color, a.Size, a.Clarity,
                a.MRP, a.Back, a.Rate, Math.Round(a.Rate * a.Weight, 4), "", 0, a.CertId, a.CertNo, a.HA, a.Proportion, a.Polish, a.Symmetry, a.Fl, a.FlColor, "", "", "",
                Math.Round(a.L, 2), Math.Round(a.W, 2), Math.Round(a.D, 2), "", "", a.LW, a.TopDepth, a.Table, a.CH, a.CA, a.PA, a.PH, a.GirdlePer, a.GirdleCondition, a.GMin,
                a.GMax, a.GirdleMinMs, a.GirdleMaxMs, a.Culet, a.CuletCondition, a.StrLn, a.LrHalf, false, a.InscriptionText, "", a.KeyToSymbols,
                a.LabComments, a.BrownId, "", a.MilkyId, a.LusterId, a.ButhId, a.NattsId, "", "", "", "", a.OIT, a.OIC, a.OIP, a.OIG, "",
                "", "", "", "", "", "", 0, 0, 0, a.ActualBack, Math.Round(a.ActualRate, 4), Math.Round(a.ActualRate * a.Weight, 4),
                a.MfgSeriesID, a.MfgPacketID, "", "", DateTime.Now, a.CompanyID, ReportDate, a.GraderName, a.Status);

            var itemSize = a.UseSizeOnItemMaster ? a.Size : "";
            var fancy = a.UseFancyOnItemMaster ? a.FancyColor : "";
            var fancyInt = a.UseFancyOnItemMaster ? a.FancyColorIntensity : "";
            var fancyOver = a.UseFancyOnItemMaster ? a.FancyColorOvertone : "";
            var diameter = a.UseFancyOnItemMaster ? a.Diameter : "";

            sql.SaveItemMasterDataForPurchase(
                a.ItemCode, a.LotNo, a.ItemSrNo, "", "", "", "", 0, a.Weight, "", a.Shape, a.Color, itemSize, a.Clarity,
                a.MRP, a.Back, a.Rate, Math.Round(a.Rate * a.Weight, 4), "", 0, a.CertId, a.CertNo, a.HA, a.Proportion, a.Polish, a.Symmetry, a.Fl, a.FlColor, fancy, fancyInt, fancyOver,
                Math.Round(a.L, 2), Math.Round(a.W, 2), Math.Round(a.D, 2), diameter, diameter, a.LW, a.TopDepth, a.Table, a.CH, a.CA, a.PA, a.PH, a.GirdlePer, a.GirdleCondition, a.GMin,
                a.GMax, a.GirdleMinMs, a.GirdleMaxMs, a.Culet, a.CuletCondition, a.StrLn, a.LrHalf, false, a.InscriptionText, "", a.KeyToSymbols,
                a.LabComments, a.BrownId, "", a.MilkyId, a.LusterId, a.ButhId, a.NattsId, "", "", "", "", a.OIT, a.OIC, a.OIP, a.OIG, "",
                "", a.ColorDisc, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, a.MfgSeriesID, a.MfgPacketID, "", "", DateTime.Now, 0, 0, 0, a.CompanyID, "UpdateGrading", 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, "", 1);
            sql.CommitTransaction();
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
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
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

        private static int ParseCompanyId(string companyId) => int.TryParse(companyId, out var id) ? id : 0;

        private static string Cell(DataRow row, int col)
        {
            if (row.Table.Columns.Count <= col) return string.Empty;
            var value = row[col];
            return value == null || value == DBNull.Value ? string.Empty : value.ToString() ?? string.Empty;
        }

        private static double CellDbl(DataRow row, int col)
        {
            var text = Cell(row, col).Replace("%", "").Trim();
            return double.TryParse(text, out var number) ? number : 0;
        }

        private static float CellFlt(DataRow row, int col) => (float)CellDbl(row, col);

        private static int CellInt(DataRow row, int col)
        {
            var text = Cell(row, col).Trim();
            return int.TryParse(text, out var number) ? number : 0;
        }

        private static string Col(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName)) return string.Empty;
            var value = row[columnName];
            return value == null || value == DBNull.Value ? string.Empty : value.ToString()?.Trim() ?? string.Empty;
        }

        private static double ColDbl(DataRow row, string columnName) => ToDouble(ColObj(row, columnName));
        private static float ColFlt(DataRow row, string columnName) => ToFloat(ColObj(row, columnName));
        private static int ColInt(DataRow row, string columnName) => ToInt(ColObj(row, columnName));

        private static object ColObj(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName)) return DBNull.Value;
            return row[columnName];
        }

        private static double ToDouble(object val)
        {
            return val != null && val != DBNull.Value && !string.IsNullOrWhiteSpace(val.ToString())
                ? Convert.ToDouble(val)
                : 0;
        }

        private static int ToInt(object val)
        {
            return val != null && val != DBNull.Value && !string.IsNullOrWhiteSpace(val.ToString())
                ? Convert.ToInt32(val)
                : 0;
        }

        private static float ToFloat(object val)
        {
            return val != null && val != DBNull.Value && !string.IsNullOrWhiteSpace(val.ToString())
                ? Convert.ToSingle(val)
                : 0;
        }

        private static double CellPct(DataRow row, int col) => CellDbl(row, col);

        private static bool CellBool(DataRow row, int col)
        {
            var s = Cell(row, col).Trim().ToLowerInvariant();
            if (s == "" || s == "-" || s == "na") return false;
            if (s is "true" or "yes" or "1") return true;
            if (s is "false" or "no" or "0") return false;
            return bool.TryParse(s, out var result) && result;
        }

        private static (double L, double W, double D, double LW) Measure(DataRow row, int lCol, int wCol, int dCol, string shape)
        {
            double l = CellDbl(row, lCol);
            double w = CellDbl(row, wCol);
            double d = CellDbl(row, dCol);
            double lw = 0;
            if (shape != "RBC" && w != 0)
                lw = l / w;
            else
                lw = 0;
            return (l, w, d, lw);
        }

        private sealed class LabSaveArgs
        {
            public int ItemCode;
            public string LotNo = "";
            public int ItemSrNo;
            public double Weight;
            public string Shape = "";
            public string Color = "";
            public string Size = "";
            public string Clarity = "";
            public double MRP;
            public double Back;
            public double Rate;
            public double ActualRate;
            public double ActualBack;
            public string CertId = "";
            public string CertNo = "";
            public string HA = "";
            public string Proportion = "";
            public string Polish = "";
            public string Symmetry = "";
            public string Fl = "";
            public string FlColor = "";
            public string ColorDisc = "";
            public string ClarityStatus = "";
            public double L, W, D, LW, TopDepth, Table, CH, CA, PA, PH, GirdlePer, StrLn, LrHalf;
            public double GirdleMinMs, GirdleMaxMs;
            public string GirdleCondition = "";
            public string GMin = "";
            public string GMax = "";
            public string Culet = "";
            public string CuletCondition = "";
            public string InscriptionText = "";
            public string KeyToSymbols = "";
            public string LabComments = "";
            public string BrownId = "";
            public string MilkyId = "";
            public string LusterId = "";
            public string ButhId = "";
            public string NattsId = "";
            public string OIT = "";
            public string OIC = "";
            public string OIP = "";
            public string OIG = "";
            public string MfgSeriesID = "";
            public string MfgPacketID = "";
            public int CompanyID;
            public string ReportDate = "";
            public string GraderName = "";
            public string Status = "";
            public string FancyColor = "";
            public string FancyColorIntensity = "";
            public string FancyColorOvertone = "";
            public string Diameter = "";
            public bool UseSizeOnItemMaster = true;
            public bool UseFancyOnItemMaster;

            public static LabSaveArgs FromItem(GradingListDto item, int companyID) => new()
            {
                ItemCode = item.ItemCode,
                LotNo = item.LotNo,
                ItemSrNo = item.itemsrno,
                Rate = item.AskingRate,
                ActualRate = item.ActualRate,
                Back = item.Back,
                ActualBack = item.ActualBack,
                MRP = item.MRP,
                HA = item.HAID,
                GirdleMinMs = item.GirdleMInMS,
                GirdleMaxMs = item.GirdleMaxMS,
                BrownId = item.BrownId,
                MilkyId = item.MilkyId,
                LusterId = item.LusterId,
                NattsId = item.NattsId,
                OIC = item.OIC,
                OIG = item.OIG,
                OIP = item.OIP,
                OIT = item.OIT,
                MfgSeriesID = item.MfgSeriesID,
                MfgPacketID = item.MfgPacketID,
                CompanyID = companyID
            };
        }
    }
}
