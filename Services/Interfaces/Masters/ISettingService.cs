using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Models;
using Dia_CoreAPI.Models.Masters;

namespace Dia_CoreAPI.Services.Interfaces.Masters
{
    public interface ISettingService
    {
        Task<IEnumerable<CompanySettingMaster>> GetSettingListAsync(string companyId, string userName);
        Task<IEnumerable<CompanySettingMaster>> FilterSettingListAsync(string groupId, string companyId, string userName);
        Task<(bool Success, string Message)> SubmitSettingDataAsync(CompanySettingMasterDto dto, int? userTypeId, string companyId);
        Task<CompanySettingMaster?> GetSettingByIdAndNameAsync(int id, string name, string companyId);
        Task<(bool Success, string Message)> DeleteSettingAsync(int id, int? userTypeId, string companyId);
        Task<(CompanySettingMaster? Setting, IEnumerable<GetInvoiceFormatData> Formats)> GetInvoiceFormatSettingAsync(int id, string name, string companyId);
        Task<string?> GetInvoicePreviewPathAsync(string name, string value, string companyId);
        Task<IEnumerable<ColumnAbbrMaster>> GetColumnAbbrListAsync(string companyId, string userName);
        Task<(bool Success, string Message)> UpdateColumnAbbrListAsync(IEnumerable<ColumnAbbrMaster> rows, int? userTypeId, string companyId);
    }
}
