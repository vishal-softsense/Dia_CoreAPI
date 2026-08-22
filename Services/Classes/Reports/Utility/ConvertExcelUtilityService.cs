using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Utility;
using Dia_CoreAPI.Services.Interfaces.Reports.Utility;
using ExcelDataReader;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;

namespace Dia_CoreAPI.Services.Classes.Reports.Utility
{
    public class ConvertExcelUtilityService : IConvertExcelUtilityService
    {
        private readonly IConvertExcelUtilityRepository _repository;
        private readonly IWebHostEnvironment _env;

        public ConvertExcelUtilityService(
            IConvertExcelUtilityRepository repository,
            IWebHostEnvironment env)
        {
            _repository = repository;
            _env = env;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId = "")
            => _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);

        public async Task<ConvertExcelResultDto> ProcessUploadBothTablesAsync(IFormFile file, string companyId = "")
        {
            if (file == null || file.Length == 0)
            {
                return new ConvertExcelResultDto
                {
                    Success = false,
                    Error = "Please upload a valid file."
                };
            }

            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                DataTable originalTable;
                using (var stream = file.OpenReadStream())
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var conf = new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
                    };
                    originalTable = reader.AsDataSet(conf).Tables[0];
                }

                DataTable uploadedTable = originalTable.Copy();

                string referencePath = Path.Combine(_env.WebRootPath, "excelfolder", "DiaSense.xlsx");
                if (!File.Exists(referencePath))
                {
                    referencePath = Path.Combine(_env.ContentRootPath, "excelfolder", "DiaSense.xlsx");
                }
                if (!File.Exists(referencePath))
                {
                    referencePath = Path.Combine(_env.WebRootPath, "DocFormat", "DiaSense.xlsx");
                }
                if (!File.Exists(referencePath))
                {
                    referencePath = Path.Combine(_env.ContentRootPath, "DocFormat", "DiaSense.xlsx");
                }

                if (!File.Exists(referencePath))
                {
                    return new ConvertExcelResultDto
                    {
                        Success = false,
                        Error = "Reference format file (DiaSense.xlsx) not found."
                    };
                }

                DataTable referenceTable;
                using (var stream = File.OpenRead(referencePath))
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var conf = new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
                    };
                    referenceTable = reader.AsDataSet(conf).Tables[0];
                }

                var map = await _repository.GetColumnAbbreviationMapAsync(companyId);

                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (DataColumn col in uploadedTable.Columns)
                {
                    if (map.TryGetValue(col.ColumnName, out string? full) && !string.IsNullOrEmpty(full))
                    {
                        string finalName = full;
                        int i = 1;
                        while (seen.Contains(finalName))
                            finalName = full + "_" + (i++);
                        col.ColumnName = finalName;
                        seen.Add(finalName);
                    }
                }

                var mappedTable = new DataTable();
                foreach (DataColumn col in referenceTable.Columns)
                    mappedTable.Columns.Add(col.ColumnName);

                foreach (DataRow row in uploadedTable.Rows)
                {
                    var newRow = mappedTable.NewRow();
                    foreach (DataColumn refCol in referenceTable.Columns)
                    {
                        string name = refCol.ColumnName;
                        newRow[name] = uploadedTable.Columns.Contains(name) && row[name] != DBNull.Value ? row[name] : "";
                    }
                    mappedTable.Rows.Add(newRow);
                }

                byte[] fileBytes;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage())
                {
                    var sheet = package.Workbook.Worksheets.Add("MappedData");

                    for (int i = 0; i < mappedTable.Columns.Count; i++)
                        sheet.Cells[1, i + 1].Value = mappedTable.Columns[i].ColumnName;

                    for (int r = 0; r < mappedTable.Rows.Count; r++)
                        for (int c = 0; c < mappedTable.Columns.Count; c++)
                            sheet.Cells[r + 2, c + 1].Value = mappedTable.Rows[r][c];

                    fileBytes = package.GetAsByteArray();
                }

                var jsonOriginal = originalTable.AsEnumerable()
                    .Select(row => originalTable.Columns.Cast<DataColumn>()
                        .ToDictionary(col => col.ColumnName, col => row[col] == DBNull.Value ? "" : row[col]))
                    .ToList();

                var jsonMapped = mappedTable.AsEnumerable()
                    .Select(row => mappedTable.Columns.Cast<DataColumn>()
                        .ToDictionary(col => col.ColumnName, col => row[col] == DBNull.Value ? "" : row[col]))
                    .ToList();

                return new ConvertExcelResultDto
                {
                    Success = true,
                    OriginalData = jsonOriginal,
                    MappedData = jsonMapped,
                    File = Convert.ToBase64String(fileBytes),
                    FileName = "MappedExcel.xlsx"
                };
            }
            catch (Exception ex)
            {
                return new ConvertExcelResultDto
                {
                    Success = false,
                    Error = "An error occurred while processing the file.",
                    Message = ex.Message
                };
            }
        }
    }
}
