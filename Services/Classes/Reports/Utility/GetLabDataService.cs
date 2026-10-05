using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Utility;
using Dia_CoreAPI.Services.Interfaces.Reports.Utility;
using ExcelDataReader;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dia_CoreAPI.Services.Classes.Reports.Utility
{
    public class GetLabDataService : IGetLabDataService
    {
        private const string FormName = "Get Lab Data";
        private const string DefaultGiaUrl = "https://api.reportresults.gia.edu/";
        private const string DefaultIgiUrl = "https://api.igi.org/ReportDetail.php?Printno=";

        private static readonly string[] ExcelHeaders =
        {
            "Kapan/Inv No", "Lot No", "Shape", "Size", "Color", "Clarity", "Carats", "Pcs", "MRP", "Discount", "PurPrice", "PurAmount",
            "Cut", "Polish", "Symmetry", "FL", "L:W %", "L", "W", "D", "TotDep", "Table", "Culet", "G.Min", "G.Max", "Min", "Max", "GirdlePer",
            "Brown", "Milky", "H&A", "Natts", "EyeClean", "OiT", "OiG", "OiC", "OiP", "TabInc", "Inclusion Pattern", "Cert", "CertificateNo",
            "Inscription Text", "CH", "CA", "PA", "PH", "StrLn", "Lr_Half", "Luster", "KeyToSymbols", "LabComments", "ReportDate", "MfgComments",
            "MemberRemarks", "IGICut", "HRDCut", "STONEID", "GirdleCondition", "AskingBack", "ActualBack", "AskingRate", "ActualRate",
            "Green", "FLColor", "CuletCondition", "Shade", "CrownInclusion", "BlackInTable", "BlackInSide", "FancyColor",
            "FancyColorIntensity", "FancyColorOvertone", "Proportion", "RMK", "ItemGroup", "Treatment", "RFID","ImagePath","VideoPath","PagePath","CountryOfOrigin","CountryOfMfg"
        };

        private readonly IGetLabDataRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public GetLabDataService(
            IGetLabDataRepository repository,
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment env,
            IConfiguration configuration)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
            _env = env;
            _configuration = configuration;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);

        /// <summary>Matches MVC POST getlab/get-item-from-list</summary>
        public async Task<GetLabDataResult> GetItemsFromListAsync(GetLabLotRequestDto request, string userTypeId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, FormName, "insert", companyId);
            if (permission != 1)
                return new GetLabDataResult(false, "You are not authorized to perform this task.");

            var report = new List<GetLabLotDto>();
            if (!string.IsNullOrWhiteSpace(request?.LotNo))
            {
                string strTmpNewLine = request.LotNo.Replace(" ", "");
                strTmpNewLine = strTmpNewLine.Replace("\r\n", ",");
                strTmpNewLine = strTmpNewLine.Replace("\n", ",");
                strTmpNewLine = strTmpNewLine.Replace(",", "'',''");

                string strWhere = " AND LotNo IN (''" + strTmpNewLine + "'')";
                report = await _repository.GetLotCrtAsync(strWhere, companyId);
            }

            return new GetLabDataResult(true, report);
        }

        /// <summary>Matches MVC POST data/import-cert</summary>
        public async Task<GetLabDataResult> ImportCertAsync(GetLabDataDTO importData, string userTypeId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, FormName, "insert", companyId);
            if (permission != 1)
                return new GetLabDataResult(false, "You are not authorized to perform this task.");

            var file = importData?.ImportFile;
            if (file == null || file.Length == 0)
                return new GetLabDataResult(false, "Please upload a file....!");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            string[] validFileTypes = { ".xls", ".xlsx" };
            if (!validFileTypes.Contains(extension))
                return new GetLabDataResult(false, "Invalid file type....!");

            var tableOrError = ReadExcel(file);
            if (!tableOrError.Success)
                return tableOrError.Error;

            var dt = tableOrError.Table!;
            var importEx = new List<GetLabLotDto>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i][0].ToString() == "")
                    break;

                var caratText = dt.Columns.Count > 2 ? dt.Rows[i][2]?.ToString() : "";
                float.TryParse(caratText, NumberStyles.Any, CultureInfo.InvariantCulture, out var caratValue);

                importEx.Add(new GetLabLotDto
                {
                    LotNo = dt.Rows[i][0]?.ToString() ?? string.Empty,
                    CertificateNo = dt.Columns.Count > 1 ? dt.Rows[i][1]?.ToString() ?? string.Empty : string.Empty,
                    Carats = caratValue
                });
            }

            return new GetLabDataResult(true, importEx);
        }

        /// <summary>Matches MVC GET/POST common/get-gia-data</summary>
        public async Task<GetLabDataResult> GetGiaDataAsync(GetCertRequestDto request, string companyId)
        {
            var items = request?.Item ?? new List<GetCertItemDto>();
            var certs = new List<string>();
            var lotByCert = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var im in items)
            {
                if (string.IsNullOrWhiteSpace(im.GetCertNo))
                    continue;

                string strTmpNewLine = im.GetCertNo.Replace(" ", "");
                strTmpNewLine = strTmpNewLine.Replace("\r\n", ",");
                strTmpNewLine = strTmpNewLine.Replace("\n", ",");

                foreach (var cert in strTmpNewLine.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    var certNo = cert.Trim();
                    if (string.IsNullOrWhiteSpace(certNo))
                        continue;
                    certs.Add(certNo);
                    if (!lotByCert.ContainsKey(certNo) && !string.IsNullOrWhiteSpace(im.Lotno))
                        lotByCert[certNo] = im.Lotno;
                }
            }

            if (certs.Count == 0)
                return new GetLabDataResult(true, new List<GiaDataDto>());

            string query = LoadGiaGraphqlQuery();
            string giaKey = await _repository.GetSettingValueAsync("GIADATAKEY", companyId) ?? string.Empty;
            string url = _configuration["GetLabData:GiaApiUrl"] ?? DefaultGiaUrl;

            var client = _httpClientFactory.CreateClient();
            var purExcel = new List<GiaDataDto>();
            int srNo = 1;

            foreach (var certificateNo in certs)
            {
                var payload = new Dictionary<string, object>
                {
                    { "query", query },
                    { "variables", new Dictionary<string, string> { { "ReportNumber", certificateNo } } }
                };

                string json = JsonConvert.SerializeObject(payload);
                string responseD;
                try
                {
                    using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
                    if (!string.IsNullOrWhiteSpace(giaKey))
                        httpRequest.Headers.TryAddWithoutValidation("Authorization", giaKey);
                    httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

                    using var response = await client.SendAsync(httpRequest);
                    responseD = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    responseD = ex.ToString();
                }

                JObject? responseData;
                try
                {
                    responseData = JObject.Parse(responseD);
                }
                catch
                {
                    continue;
                }

                var getReport = responseData.SelectToken("data.getReport");
                if (getReport == null || getReport.Type == JTokenType.Null)
                    continue;

                var shapeCode = TokenStr(responseData, "data.getReport.results.data.shape.shape_code");
                string lengthOrMin = shapeCode == "RBC"
                    ? TokenStr(responseData, "data.getReport.results.data.measurements.min_diam")
                    : TokenStr(responseData, "data.getReport.results.data.measurements.length");

                var colorGrade = TokenStr(responseData, "data.getReport.results.color_grade");
                lotByCert.TryGetValue(certificateNo, out var lotNo);
                var graphqlLot = TokenStr(responseData, "data.getReport.results.lotno");

                purExcel.Add(new GiaDataDto
                {
                    SrNo = srNo++,
                    report_number = TokenStr(responseData, "data.getReport.report_number"),
                    report_date = TokenStr(responseData, "data.getReport.report_date"),
                    report_type = TokenStr(responseData, "data.getReport.report_type"),
                    shape_and_cutting_style = TokenStr(responseData, "data.getReport.results.shape_and_cutting_style"),
                    measurements = TokenStr(responseData, "data.getReport.results.measurements"),
                    carat_weight = TokenStr(responseData, "data.getReport.results.carat_weight"),
                    Color = colorGrade,
                    color_grade = colorGrade,
                    clarity_grade = TokenStr(responseData, "data.getReport.results.clarity_grade"),
                    cut_grade = TokenStr(responseData, "data.getReport.results.cut_grade"),
                    polish = TokenStr(responseData, "data.getReport.results.polish"),
                    symmetry = TokenStr(responseData, "data.getReport.results.symmetry"),
                    L = lengthOrMin,
                    W = lengthOrMin,
                    D = TokenStr(responseData, "data.getReport.results.data.measurements.depth"),
                    fluorescence = TokenStr(responseData, "data.getReport.results.fluorescence"),
                    clarity_characteristics = TokenStr(responseData, "data.getReport.results.clarity_characteristics"),
                    inscriptions = TokenStr(responseData, "data.getReport.results.inscriptions"),
                    depth_pct = TokenStr(responseData, "data.getReport.results.proportions.depth_pct"),
                    table_pct = TokenStr(responseData, "data.getReport.results.proportions.table_pct"),
                    crown_angle = TokenStr(responseData, "data.getReport.results.proportions.crown_angle"),
                    crown_height = TokenStr(responseData, "data.getReport.results.proportions.crown_height"),
                    pavilion_angle = TokenStr(responseData, "data.getReport.results.proportions.pavilion_angle"),
                    pavilion_depth = TokenStr(responseData, "data.getReport.results.proportions.pavilion_depth"),
                    star_length = TokenStr(responseData, "data.getReport.results.proportions.star_length"),
                    lower_half = TokenStr(responseData, "data.getReport.results.proportions.lower_half"),
                    girdle = TokenStr(responseData, "data.getReport.results.proportions.girdle"),
                    culet = TokenStr(responseData, "data.getReport.results.culet.culet_code"),
                    color_origin = TokenStr(responseData, "data.getReport.results.color_origin"),
                    color_distribution = TokenStr(responseData, "data.getReport.results.color_distribution"),
                    country_of_origin = TokenStr(responseData, "data.getReport.results.country_of_origin"),
                    diamond_type = TokenStr(responseData, "data.getReport.results.diamond_type"),
                    report_comments = TokenStr(responseData, "data.getReport.results.report_comments"),
                    lotno = string.IsNullOrWhiteSpace(graphqlLot) ? (lotNo ?? string.Empty) : graphqlLot,
                    Cert = "GIA"
                });
            }

            return new GetLabDataResult(true, purExcel);
        }

        /// <summary>Matches MVC GET/POST common/get-igi-data</summary>
        public async Task<GetLabDataResult> GetIgiDataAsync(GetCertRequestDto request, string companyId)
        {
            var items = request?.Item ?? new List<GetCertItemDto>();
            string igiKey = await _repository.GetSettingValueAsync("IGIUserKey", companyId) ?? string.Empty;
            string igiKeyValue = await _repository.GetSettingValueAsync("IGIUserKeyValue", companyId) ?? string.Empty;
            string igiBase = _configuration["GetLabData:IgiApiUrl"] ?? DefaultIgiUrl;

            var client = _httpClientFactory.CreateClient();
            var purExcel = new List<IgiDataDto>();
            int srNo = 1;

            foreach (var im in items)
            {
                if (string.IsNullOrWhiteSpace(im.GetCertNo))
                    continue;

                string cert = im.GetCertNo.Trim();
                string jsonUrl = igiBase + Uri.EscapeDataString(cert);
                string responseD;
                try
                {
                    using var httpRequest = new HttpRequestMessage(HttpMethod.Get, jsonUrl);
                    if (!string.IsNullOrWhiteSpace(igiKey) && !string.IsNullOrWhiteSpace(igiKeyValue))
                        httpRequest.Headers.TryAddWithoutValidation(igiKey, igiKeyValue);

                    using var response = await client.SendAsync(httpRequest);
                    responseD = await response.Content.ReadAsStringAsync();
                }
                catch
                {
                    continue;
                }

                DataTable? table;
                try
                {
                    table = JsonConvert.DeserializeObject<DataTable>(responseD);
                }
                catch
                {
                    continue;
                }

                if (table == null)
                    continue;

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    var row = table.Rows[i];
                    string shape = Col(row, "SHAPE AND CUT");
                    bool isRound = string.Equals(shape.Trim(), "ROUND BRILLIANT", StringComparison.OrdinalIgnoreCase);

                    string crown = Col(row, "Crown Height");
                    string[] cach = crown.Split('-');
                    string pavilion = Col(row, "Pavilion Depth");
                    string[] paph = pavilion.Split('-');

                    string[] measurement = Col(row, "Measurements").Split(' ');

                    purExcel.Add(new IgiDataDto
                    {
                        SrNo = srNo++,
                        ReportNumber = Col(row, "Report Number"),
                        ReportDate = Col(row, "REPORT DATE"),
                        Description = Col(row, "DESCRIPTION"),
                        Shape = shape,
                        Weight = Col(row, "CARAT WEIGHT"),
                        Color = Col(row, "COLOR GRADE"),
                        Clarity = Col(row, "CLARITY GRADE"),
                        CutProp = isRound ? Col(row, "CUT GRADE") : "",
                        Sym = isRound ? Col(row, "SYMMETRY") : "",
                        Pol = Col(row, "POLISH"),
                        TableDiameter = Col(row, "Table Size"),
                        CrownHeight = cach.Length > 0 ? cach[0].Trim() : "",
                        CrownAngel = cach.Length > 1 ? cach[1].Trim() : "",
                        PavilionDepth = paph.Length > 0 ? paph[0].Trim() : "",
                        PavilionAngel = paph.Length > 1 ? paph[1].Trim() : "",
                        GirdleName = Col(row, "Girdle Thickness"),
                        CuletSize = Col(row, "Culet"),
                        TotalDepth = Col(row, "Total Depth"),
                        Fluorescence = Col(row, "FLUORESCENCE"),
                        Inscription = Col(row, "Inscription(s)"),
                        Measurement1 = measurement.Length > 0 ? measurement[0] : "",
                        Measurement2 = measurement.Length > 2 ? measurement[2] : "",
                        Measurement3 = measurement.Length > 4 ? measurement[4] : "",
                        Comments = Col(row, "COMMENTS"),
                        Cert = "IGI",
                        LotNo = im.Lotno ?? string.Empty
                    });
                }
            }

            return new GetLabDataResult(true, purExcel);
        }

        /// <summary>Matches MVC POST excel/getlabdataprint — returns mapped Excel rows for the React download.</summary>
        public async Task<GetLabDataResult> PrintExcelAsync(GetLabDataExcelRequestDto request, string companyId)
        {
            var spte = request?.spte ?? new List<GiaDataDto>();
            var ptse = request?.ptse ?? new List<IgiDataDto>();

            if (spte.Count == 0 && ptse.Count == 0)
            {
                return new GetLabDataResult
                {
                    Success = false,
                    Error = true,
                    Message = "No data to export."
                };
            }

            var rows = new List<List<object?>>();

            foreach (var im in spte)
            {
                if (!string.Equals(im.Cert, "GIA", StringComparison.OrdinalIgnoreCase)
                    && string.IsNullOrWhiteSpace(im.report_number)
                    && string.IsNullOrWhiteSpace(im.shape_and_cutting_style))
                    continue;

                var shape = await _repository.GetLabDataIdAsync("Shape", im.shape_and_cutting_style ?? "", "GIA", companyId);
                var cut = await _repository.GetLabDataIdAsync("Cut", im.cut_grade ?? "", "GIA", companyId);
                var polish = await _repository.GetLabDataIdAsync("Polish", im.polish ?? "", "GIA", companyId);
                var sym = await _repository.GetLabDataIdAsync("Symmetry", im.symmetry ?? "", "GIA", companyId);

                string carats = FirstToken(im.carat_weight);
                string color = string.IsNullOrWhiteSpace(im.color_grade) ? im.Color : im.color_grade;

                var row = EmptyExcelRow();
                row[1] = im.lotno;
                row[2] = shape;
                row[4] = color;
                row[5] = im.clarity_grade;
                row[6] = carats;
                row[12] = cut;
                row[13] = polish;
                row[14] = sym;
                row[15] = im.fluorescence;
                row[17] = im.L;
                row[18] = im.W;
                row[19] = im.D;
                row[20] = im.depth_pct;
                row[21] = im.table_pct;
                row[22] = im.culet;
                row[39] = "GIA";
                row[40] = im.report_number;
                row[41] = im.inscriptions;
                row[42] = im.crown_height;
                row[43] = im.crown_angle;
                row[44] = im.pavilion_angle;
                row[45] = im.pavilion_depth;
                row[46] = im.star_length;
                row[47] = im.lower_half;
                row[51] = im.report_date;
                rows.Add(row);
            }

            foreach (var tm in ptse)
            {
                if (!string.Equals(tm.Cert, "IGI", StringComparison.OrdinalIgnoreCase)
                    && string.IsNullOrWhiteSpace(tm.ReportNumber)
                    && string.IsNullOrWhiteSpace(tm.Shape))
                    continue;

                var shape = await _repository.GetLabDataIdAsync("Shape", tm.Shape ?? "", "IGI", companyId);
                var cla = await _repository.GetLabDataIdAsync("Clarity", tm.Clarity ?? "", "IGI", companyId);
                var cut = await _repository.GetLabDataIdAsync("Cut", tm.CutProp ?? "", "IGI", companyId);
                var polish = await _repository.GetLabDataIdAsync("Polish", tm.Pol ?? "", "IGI", companyId);
                var sym = await _repository.GetLabDataIdAsync("Symmetry", tm.Sym ?? "", "IGI", companyId);

                var row = EmptyExcelRow();
                row[1] = tm.LotNo;
                row[2] = shape;
                row[4] = tm.Color;
                row[5] = cla;
                row[6] = FirstToken(tm.Weight);
                row[12] = cut;
                row[13] = polish;
                row[14] = sym;
                row[15] = tm.Fluorescence;
                row[17] = tm.Measurement1;
                row[18] = tm.Measurement2;
                row[19] = tm.Measurement3;
                row[20] = CleanNumber(tm.TotalDepth);
                row[21] = CleanNumber(tm.TableDiameter);
                row[22] = tm.CuletSize;
                row[39] = "IGI";
                row[40] = tm.ReportNumber;
                row[41] = tm.Inscription;
                row[42] = CleanNumber(tm.CrownHeight);
                row[43] = CleanNumber(tm.CrownAngel);
                row[44] = CleanNumber(tm.PavilionAngel);
                row[45] = CleanNumber(tm.PavilionDepth);
                row[50] = tm.Comments;
                row[51] = tm.ReportDate;
                rows.Add(row);
            }

            return new GetLabDataResult
            {
                Success = true,
                Error = false,
                Message = "Your Data Saved Successfully..",
                Headers = ExcelHeaders.ToList(),
                Rows = rows
            };
        }

        private string LoadGiaGraphqlQuery()
        {
            var configured = _configuration["GetLabData:GiaGraphqlPath"];
            var candidates = new[]
            {
                configured,
                Path.Combine(_env.ContentRootPath, "Files", "report_results.graphql"),
                Path.Combine(_env.ContentRootPath, "report_results.graphql"),
                @"C:/Hosting Space/MVC Softsense/report_results.graphql",
                @"C:/RAP/report_results.graphql"
            };

            foreach (var path in candidates)
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    try
                    {
                        return File.ReadAllText(path);
                    }
                    catch
                    {
                        // try next
                    }
                }
            }

            return DefaultGiaGraphql;
        }

        private static (bool Success, DataTable? Table, GetLabDataResult Error) ReadExcel(Microsoft.AspNetCore.Http.IFormFile file)
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
                    return (false, null, new GetLabDataResult(false, "Excel is empty."));

                return (true, table, new GetLabDataResult(true, null));
            }
            catch (Exception ex)
            {
                return (false, null, new GetLabDataResult(false, ex.Message));
            }
        }

        private static string TokenStr(JToken root, string path)
        {
            var token = root.SelectToken(path);
            return token == null || token.Type == JTokenType.Null ? string.Empty : token.ToString();
        }

        private static string Col(DataRow row, string column)
        {
            if (row.Table.Columns.Contains(column) && row[column] != null && row[column] != DBNull.Value)
                return row[column].ToString() ?? string.Empty;
            return string.Empty;
        }

        private static string FirstToken(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            return value.Split(' ')[0];
        }

        private static string CleanNumber(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            return new string(input.Where(c => char.IsDigit(c) || c == '.').ToArray());
        }

        private static List<object?> EmptyExcelRow()
        {
            var row = new List<object?>(ExcelHeaders.Length);
            for (int i = 0; i < ExcelHeaders.Length; i++)
                row.Add("");
            return row;
        }

        private const string DefaultGiaGraphql = @"
query GetReport($ReportNumber: String!) {
  getReport(report_number: $ReportNumber) {
    report_number
    report_date
    report_type
    results {
      shape_and_cutting_style
      measurements
      carat_weight
      color_grade
      clarity_grade
      cut_grade
      polish
      symmetry
      fluorescence
      clarity_characteristics
      inscriptions
      color_origin
      color_distribution
      country_of_origin
      diamond_type
      report_comments
      lotno
      proportions {
        depth_pct
        table_pct
        crown_angle
        crown_height
        pavilion_angle
        pavilion_depth
        star_length
        lower_half
        girdle
      }
      culet {
        culet_code
      }
      data {
        shape {
          shape_code
        }
        measurements {
          min_diam
          max_diam
          length
          width
          depth
        }
      }
    }
  }
}";
    }
}
