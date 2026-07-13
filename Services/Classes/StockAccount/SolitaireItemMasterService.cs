using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.StockAccount;
using Dia_CoreAPI.Services.Interfaces.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.Masters;

namespace Dia_CoreAPI.Services.Classes.StockAccount
{
    public class SolitaireItemMasterService : ISolitaireItemMasterService
    {
        private readonly ISolitaireItemMasterRepository _repository;
        private readonly ISettingRepository _settingRepository;

        public SolitaireItemMasterService(ISolitaireItemMasterRepository repository, ISettingRepository settingRepository)
        {
            _repository = repository;
            _settingRepository = settingRepository;
        }

        #region Solitaire

        public async Task<IEnumerable<ItemMaster>> GetItemMasterListAsync(string companyId, string whereClause)
        {
            return await _repository.GetItemMasterListAsync(whereClause, companyId);
        }

        public async Task<IEnumerable<BasicDataDto>> ItemMasterDropDownAsync(string companyId)
        {
            return await _repository.ItemMasterDropDownAsync(companyId);
        }

        public async Task<IEnumerable<BasicDataDto>> FillCityStateCountryItemMasterDropDownAsync(string companyId)
        {
            return await _repository.FillCityStateCountryItemMasterDropDownAsync(companyId);
        }

        public async Task<SolitaireItemMasterDto?> GetItemMasterFullListAsync(string id, string companyId)
        {
            return await _repository.GetItemMasterFullListAsync(id, companyId);
        }

        public async Task<string> GetLotExistAsync(string lotNo, string companyId)
        {
            return await _repository.GetLotExistAsync($" AND LotNo = '{lotNo}'", companyId);
        }

        public async Task<bool> AddItemAsync(SolitaireItemMasterDto item, string companyId)
        {
            string lotAlready = await _repository.GetLotExistAsync($" AND LotNo = '{item.LotNo}'", companyId);
            if (lotAlready != "0")
            {
                throw new Exception("LotNo Already Created.");
            }

            int singlePcs = 0;
            if (item.StoneTypes == "Single")
            {
                singlePcs += 1;
            }
                
            string itemName = "";
            if (!string.IsNullOrWhiteSpace(item.ItemName))
            {
                itemName = item.ItemName;
            }
            else
            {
                itemName = item.LotNo;
            }
            item.ItemName = itemName;

            return await _repository.SaveItemMasterDataAsync(item, "insert", companyId, singlePcs);
        }

        public async Task<bool> EditItemAsync(SolitaireItemMasterDto item, string companyId)
        {
            string itemName = "";
            if (!string.IsNullOrWhiteSpace(item.ItemName))
            {
                itemName = item.ItemName;
            }
            else
            {
                itemName = item.LotNo;
            }
            item.ItemName = itemName;

            int singlePcs = 0;
            if (item.StoneTypes == "Single")
            {
                singlePcs += 1;
            }

            string lotAlready = await _repository.GetLotExistAsync($" AND LotNo = '{item.LotNo}' AND ItemCode != {item.ItemCode}", companyId);
            if (lotAlready == "0")
            {
                return await _repository.SaveItemMasterDataAsync(item, "update", companyId, singlePcs);
            }
            else
            {
                throw new Exception("Sorry.! You Can not Update Record Because LotNo Already Created.");
            }
        }

        public async Task<bool> DeleteItemAsync(int id, string companyId)
        {
            string where = $"  (ItemCode = {id} OR FItemCode = {id} OR TItemCode = {id})";
            string where2 = $"  (ItemCode = {id})";

            string i = await _repository.GetItemCountAsync(where, where2, companyId);

            if (Convert.ToInt32(i) <= 0)
            {
                var data = await _repository.GetItemMasterListAsync($" AND ItemCode = {id}", companyId);
                
                bool hasData = false;
                foreach (var item in data)
                {
                    hasData = true;
                    break;
                }
                
                if (hasData)
                {
                    return await _repository.DeleteItemMasterDataAsync(id, companyId, "delete");
                }
                else
                {
                    throw new Exception("Record not found.");
                }
            }
            else
            {
                throw new Exception("Record is in use, You can not delete record.");
            }
        }

        public async Task<string?> GetParameterIdAsync(string parameter, string parameterValue, string usedFor, string companyId)
        {
            return await _repository.GetParameterIdAsync(parameter, parameterValue, usedFor, companyId);
        }

        public async Task<IEnumerable<ItemMasterDto>> PreviewFullItemAsync(ImportCertExcel input, string companyId)
        {
            if (input == null || (string.IsNullOrWhiteSpace(input.LotNo) && string.IsNullOrWhiteSpace(input.CertificateNo) && input.Carats <= 0))
            {
                throw new ArgumentException("Invalid input. Enter LotNo or CertificateNo or Carats/Weight.");
            }

            var mappedItems = new List<ItemMasterDto>();

            var baseDto = new ItemMasterDto
            {
                LotNo = (input.LotNo ?? "").Trim(),
                CertificateNo = (input.CertificateNo ?? "").Trim(),
                Weight = (input.Carats > 0) ? input.Carats.ToString("0.##") : ""
            };

            if (!string.IsNullOrWhiteSpace(baseDto.CertificateNo))
            {
                string cert = baseDto.CertificateNo;
                string responseD = "";
                string JsonString = "https://api.igi.org/ReportDetail.php?Printno=" + cert;

                HttpWebRequest wrIcons = (HttpWebRequest)WebRequest.Create(JsonString);

                var settings = await _settingRepository.GetSettingListAsync(" AND (Name = 'IGIUserKey' OR Name = 'IGIUserKeyValue') ", companyId);
                if (settings != null)
                {
                    string? IGIDATAKEY = settings.Where(a => a.Name == "IGIUserKey").Select(x => x.Value).SingleOrDefault();
                    string? IGIDATAKEYVALUE = settings.Where(a => a.Name == "IGIUserKeyValue").Select(x => x.Value).SingleOrDefault();
                    if (!string.IsNullOrWhiteSpace(IGIDATAKEY) && !string.IsNullOrWhiteSpace(IGIDATAKEYVALUE))
                    {
                        wrIcons.Headers.Add(IGIDATAKEY, IGIDATAKEYVALUE);
                    }
                }
                wrIcons.Method = "GET";

                using (HttpWebResponse wrResponse = (HttpWebResponse)await wrIcons.GetResponseAsync())
                using (StreamReader srIcons = new StreamReader(wrResponse.GetResponseStream()))
                {
                    responseD = await srIcons.ReadToEndAsync();
                }

                DataTable? DTabIGIReportCheck = JsonConvert.DeserializeObject<DataTable>(responseD);

                if (DTabIGIReportCheck != null && DTabIGIReportCheck.Rows.Count > 0)
                {
                    for (int i = 0; i < DTabIGIReportCheck.Rows.Count; i++)
                    {
                        DataRow r = DTabIGIReportCheck.Rows[i];

                        string Get(string col) => r.Table.Columns.Contains(col) ? (r[col]?.ToString() ?? "") : "";

                        var dto = new ItemMasterDto
                        {
                            LotNo = baseDto.LotNo,
                            CertificateNo = cert,
                            ReportNumber = Get("Report Number"),
                            ItemName = Get("Description"),
                            ShapeId = Get("SHAPE AND CUT"),
                            Weight = CleanNumericString(Get("CARAT WEIGHT")) ?? baseDto.Weight,
                            ColorId = Get("COLOR GRADE"),
                            ClarityId = Get("CLARITY GRADE"),
                            PropId = (Get("SHAPE AND CUT").Trim() == "ROUND BRILLIANT") ? Get("CUT GRADE") : "",
                            Pol = Get("POLISH"),
                            Sym = (Get("SHAPE AND CUT").Trim() == "ROUND BRILLIANT") ? Get("SYMMETRY") : "",
                            TableDiameter = Get("Table Size"),
                            TotalDepth = Get("Total Depth"),
                            Fluorescence = Get("FLUORESCENCE"),
                            LaserInscriptionText = Get("Inscription(s)"),
                            GirdleName = Get("Girdle Thickness"),
                            CuletSize = Get("Culet")
                        };

                        try
                        {
                            var crown = Get("Crown Height");
                            if (!string.IsNullOrWhiteSpace(crown))
                            {
                                if (crown.Contains("-"))
                                {
                                    var parts = crown.Split('-');
                                    dto.CrownHeight = CleanNumericString(parts[0]);
                                    dto.CrownAngle = CleanNumericString(parts.Length > 1 ? parts[1] : "");
                                }
                                else
                                {
                                    dto.CrownHeight = CleanNumericString(crown);
                                    dto.CrownAngle = CleanNumericString(Get("Crown Angle"));
                                }
                            }
                            else
                            {
                                dto.CrownHeight = CleanNumericString(Get("Crown Height"));
                                dto.CrownAngle = CleanNumericString(Get("Crown Angle"));
                            }

                            var pav = Get("Pavilion Depth");
                            if (!string.IsNullOrWhiteSpace(pav))
                            {
                                if (pav.Contains("-"))
                                {
                                    var parts = pav.Split('-');
                                    dto.PavilionDepth = CleanNumericString(parts[0]);
                                    dto.PavilionAngel = CleanNumericString(parts.Length > 1 ? parts[1] : "");
                                }
                                else
                                {
                                    dto.PavilionDepth = CleanNumericString(pav);
                                    dto.PavilionAngel = CleanNumericString(Get("Pavilion Angle"));
                                }
                            }
                            else
                            {
                                dto.PavilionDepth = CleanNumericString(Get("Pavilion Depth"));
                                dto.PavilionAngel = CleanNumericString(Get("Pavilion Angle"));
                            }

                            var measurement = Get("Measurements");
                            if (!string.IsNullOrWhiteSpace(measurement))
                            {
                                var mm = Regex.Split(measurement.Replace("×", "x").Replace("X", "x"), @"[^\d\.]+")
                                              .Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                                if (mm.Length >= 1) dto.Measurement1 = CleanNumericString(mm[0]);
                                if (mm.Length >= 2) dto.Measurement2 = CleanNumericString(mm[1]);
                                if (mm.Length >= 3) dto.Measurement3 = CleanNumericString(mm[2]);
                            }
                            else
                            {
                                dto.Measurement1 = CleanNumericString(Get("Measurement1"));
                                dto.Measurement2 = CleanNumericString(Get("Measurement2"));
                                dto.Measurement3 = CleanNumericString(Get("Measurement3"));
                            }
                        }
                        catch
                        {
                            // Ignore extraction issues to keep processing other fields
                        }

                        try
                        {
                            // shape
                            var shapeId = await _repository.GetParameterIdAsync("Shape", dto.ShapeId ?? "", "IGI", companyId);
                            dto.ShapeId = shapeId ?? "";

                            // cutprop
                            var cutId = await _repository.GetParameterIdAsync("CUT", dto.PropId ?? "", "IGI", companyId);
                            dto.PropId = cutId ?? "";

                            // polish
                            var polishId = await _repository.GetParameterIdAsync("Polish", dto.Pol ?? "", "IGI", companyId);
                            dto.Pol = polishId ?? "";

                            // symmetry
                            var symId = await _repository.GetParameterIdAsync("Symmetry", dto.Sym ?? "", "IGI", companyId);
                            dto.Sym = symId ?? "";

                            // clarity
                            var clarityId = await _repository.GetParameterIdAsync("Clarity", dto.ClarityId ?? "", "IGI", companyId);
                            dto.ClarityId = clarityId ?? "";

                            // fl
                            var flId = await _repository.GetParameterIdAsync("FL", dto.Fluorescence ?? "", "IGI", companyId);
                            dto.Fluorescence = flId ?? "";
                        }
                        catch
                        {
                            // If mapping fails, leave IDs as strings (preview only)
                        }

                        mappedItems.Add(dto);
                    }
                }
            }

            return mappedItems;
        }

        private string? CleanNumericString(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            var cleaned = Regex.Replace(input, @"[^0-9\.\-]", "");
            return string.IsNullOrWhiteSpace(cleaned) ? null : cleaned;
        }

        #endregion
    }
}
