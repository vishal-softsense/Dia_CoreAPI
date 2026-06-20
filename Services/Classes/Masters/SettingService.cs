using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.Interfaces;
using Dia_CoreAPI.Models;
using Dia_CoreAPI.Repositories.Interfaces.Masters;
using Dia_CoreAPI.Services.Interfaces.Masters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dia_CoreAPI.Services.Classes.Masters
{
    public class SettingService : ISettingService
    {
        private readonly ISettingRepository _settingRepository;
        private readonly IUserSessionService _sessionService;

        public SettingService(ISettingRepository settingRepository, IUserSessionService sessionService)
        {
            _settingRepository = settingRepository;
            _sessionService = sessionService;
        }

        private async Task LogAccessAsync(string status)
        {
            string userName = _sessionService.UserName ?? "Unknown";
            string device = _sessionService.SearchLogDevice ?? "Unknown";
            string deviceType = _sessionService.SearchLogDeviceType ?? "Unknown";
            string os = _sessionService.SearchLogOS ?? "Unknown";
            string ip = _sessionService.SearchLogIP ?? "Unknown";
            string companyId = _sessionService.CompanyId?.ToString() ?? "0";

            await _settingRepository.LogPageAccessAsync(userName, status, device, deviceType, os, ip, companyId);
        }

        public async Task<IEnumerable<CompanySettingMaster>> GetSettingListAsync()
        {
            string companyId = _sessionService.CompanyId?.ToString() ?? "0";
            await LogAccessAsync("SettingMasterList");

            var data = await _settingRepository.GetSettingListAsync("and GroupId=''COMPANY'' and Active=''1''Order By Priority", companyId);
            return data;
        }
        public async Task<IEnumerable<CompanySettingMaster>> FilterSettingListAsync(string groupId)
        {
            string companyId = _sessionService.CompanyId?.ToString() ?? "0";

            // Clean GroupId
            groupId = groupId?.Replace(":", "").Replace("\"", "").Replace("'", "").Trim() ?? string.Empty;

            await LogAccessAsync("SettingMasterList");

            var data = await _settingRepository.GetSettingListAsync("and GroupId=''" + groupId + "'' and Active=''1'' Order By Priority",companyId);

            return data;
        }

        //public async Task<IEnumerable<CompanySettingMaster>> FilterSettingListAsync(string groupId)
        //{
        //    string companyId = _sessionService.CompanyId?.ToString() ?? "0";
        //    groupId = groupId?.Trim('"', '\'') ?? string.Empty;
        //    await LogAccessAsync("SettingMasterList");

        //    var data = await _settingRepository.GetSettingListAsync("and GroupId=''" + groupId + "'' and Active=''1''Order By Priority", companyId);
        //    return data;
        //}

        public async Task<(bool Success, string Message)> SubmitSettingDataAsync(CompanySettingMasterDto dto)
        {
            int? userTypeId = _sessionService.UserTypeId;
            string companyId = _sessionService.CompanyId?.ToString() ?? "0";

            if (dto.Id == 0) // Insert
            {
                bool hasPermission = await _settingRepository.CheckUserPermissionAsync(userTypeId, "Company Setting Master", "insert", companyId);
                if (!hasPermission)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var existingSettings = await _settingRepository.GetSettingListAsync($" AND Name = ''{dto.Name}'' ", companyId);
                if (existingSettings.Any())
                {
                    return (false, $"Record already available with shape id - {dto.Name}");
                }

                var model = new CompanySettingMaster
                {
                   
                    Name = dto.Name,
                    Value = dto.Value,
                    Description = dto.Description,
                    COMMENT = dto.COMMENT,
                    CompanyID = _sessionService.CompanyId ?? 0
                };

                bool saved = await _settingRepository.SaveSettingDataAsync(model, "insert", companyId);
                if (!saved)
                {
                    return (false, "Record could not be saved.");
                }

                return (true, "Record saved successfully.");
            }
            else // Update
            {
                bool hasPermission = await _settingRepository.CheckUserPermissionAsync(userTypeId, "Company Setting Master", "edit", companyId);
                if (!hasPermission)
                {
                    return (false, "You are not authorized to perform this task.");
                }

                var existingSettings = await _settingRepository.GetSettingListAsync($" AND Name = ''{dto.Name}'' AND Id != {dto.Id}", companyId);
                if (existingSettings.Any())
                {
                    return (false, "Sorry.! You can not update record because Name already exists.");
                }

                var model = new CompanySettingMaster
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Value = dto.Value,
                    Description = dto.Description,
                    COMMENT = dto.COMMENT,
                    CompanyID = _sessionService.CompanyId ?? 0
                };

                bool saved = await _settingRepository.SaveSettingDataAsync(model, "update", companyId);
                if (!saved)
                {
                    return (false, "Record could not be updated.");
                }

                return (true, "Record updated successfully.");
            }
        }

        public async Task<CompanySettingMaster?> GetSettingByIdAndNameAsync(int id, string name)
        {
            string companyId = _sessionService.CompanyId?.ToString() ?? "0";
            var list = await _settingRepository.GetSettingListAsync($" AND ID = {id} AND Name = ''{name}'' ", companyId);
            return list.FirstOrDefault();
        }

        public async Task<(bool Success, string Message)> DeleteSettingAsync(int id)
        {
            int? userTypeId = _sessionService.UserTypeId;
            string companyId = _sessionService.CompanyId?.ToString() ?? "0";

            bool hasPermission = await _settingRepository.CheckUserPermissionAsync(userTypeId, "Company Setting Master", "delete", companyId);
            if (!hasPermission)
            {
                return (false, "You are not authorized to perform this task.");
            }

            var list = await _settingRepository.GetSettingListAsync($" AND ID = {id}", companyId);
            var model = list.FirstOrDefault();
            if (model == null)
            {
                return (false, "Record not found.");
            }

            bool deleted = await _settingRepository.SaveSettingDataAsync(model, "delete", companyId);
            if (deleted)
            {
                return (true, "Records Deleted.");
            }
            else
            {
                return (false, "Record is in use, You can not delete record.");
            }
        }

        public async Task<(CompanySettingMaster? Setting, IEnumerable<GetInvoiceFormatData> Formats)> GetInvoiceFormatSettingAsync(int Id, string name)
        {
            string companyId = _sessionService.CompanyId?.ToString() ?? "0";
            var list = await _settingRepository.GetSettingListAsync($" AND ID = {Id}", companyId);
            var setting = list.FirstOrDefault();

            string type = "";
            switch (name)
            {
                case "DEFAULT MEMO PRINT FORMAT SOLITAIRE":
                    type = "Memo";
                    break;
                case "DEFAULT MEMO PRINT FORMAT PARCEL":
                    type = "ParcelMemo";
                    break;
                case "DEFAULT SALE INVOICE  PRINT FORMAT SOLITAIRE":
                    type = "Sale";
                    break;
                case "DEFAULT SALE INVOICE  PRINT FORMAT PARCEL":
                    type = "ParcelSale";
                    break;
                case "DEFAULT STOCK LIST FORMAT":
                    type = "StockList";
                    break;
                case "Inv Print Date":
                    type = "InvPrintDate";
                    break;
                case "DEFAULT SALE RETURN INVOICE  PRINT FORMAT PARCEL":
                    type = "ParcelSaleReturn";
                    break;
                case "DecimalinInvoiceAmount":
                    type = "SetDeci";
                    break;
                case "JewelleryMetalAmountCalculateOnGoldWeight":
                    type = "SetCalculate";
                    break;
                case "DEFAULT MIX MEMO PRINT FORMAT":
                    type = "MixMemo";
                    break;
            }

            var formats = await _settingRepository.GetInvoiceFormatListAsync($"AND Type = ''{type}''", companyId);
            return (setting, formats);
        }

        public async Task<string?> GetInvoicePreviewPathAsync(string name, string value)
        {
            string companyId = _sessionService.CompanyId?.ToString() ?? "0";
            string type = "";
            switch (name)
            {
                case "DEFAULT MEMO PRINT FORMAT SOLITAIRE":
                    type = "Memo";
                    break;
                case "DEFAULT MEMO PRINT FORMAT PARCEL":
                    type = "ParcelMemo";
                    break;
                case "DEFAULT SALE INVOICE  PRINT FORMAT SOLITAIRE":
                    type = "Sale";
                    break;
                case "DEFAULT SALE INVOICE  PRINT FORMAT PARCEL":
                    type = "ParcelSale";
                    break;
                case "DEFAULT MIX MEMO PRINT FORMAT":
                    type = "MixMemo";
                    break;
            }

            return await _settingRepository.GetInvoicePathAsync($"AND Type = ''{type}'' AND FormateName = ''{value}''", companyId);
        }

        public async Task<IEnumerable<ColumnAbbrMaster>> GetColumnAbbrListAsync()
        {
            string companyId = _sessionService.CompanyId?.ToString() ?? "0";
            await LogAccessAsync("SettingMasterList");
            return await _settingRepository.GetColumnAbbrListAsync("SELECT", "", "", companyId);
        }

        public async Task<(bool Success, string Message)> UpdateColumnAbbrListAsync(IEnumerable<ColumnAbbrMaster> rows)
        {
            int? userTypeId = _sessionService.UserTypeId;
            string companyId = _sessionService.CompanyId?.ToString() ?? "0";

            bool hasPermission = await _settingRepository.CheckUserPermissionAsync(userTypeId, "Company Setting Master", "edit", companyId);
            if (!hasPermission)
            {
                return (false, "You are not authorized to perform this task.");
            }

            try
            {
                foreach (var row in rows)
                {
                    await _settingRepository.GetColumnAbbrListAsync("EDIT", row.ColumnName, row.Abbreviation, companyId);
                }
                return (true, "Record updated successfully.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
