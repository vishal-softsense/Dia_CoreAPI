using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs;
using Dia_CoreAPI.Models;

namespace Dia_CoreAPI.Services.Interfaces.Masters
{
    public interface ISettingService
    {
        Task<IEnumerable<CompanySettingMaster>> GetSettingListAsync();
        Task<IEnumerable<CompanySettingMaster>> FilterSettingListAsync(string groupId);
        Task<(bool Success, string Message)> SubmitSettingDataAsync(CompanySettingMasterDto dto);
        Task<CompanySettingMaster?> GetSettingByIdAndNameAsync(int id, string name);
        Task<(bool Success, string Message)> DeleteSettingAsync(int id);
        Task<(CompanySettingMaster? Setting, IEnumerable<GetInvoiceFormatData> Formats)> GetInvoiceFormatSettingAsync(int id, string name);
        Task<string?> GetInvoicePreviewPathAsync(string name, string value);
        Task<IEnumerable<ColumnAbbrMaster>> GetColumnAbbrListAsync();
        Task<(bool Success, string Message)> UpdateColumnAbbrListAsync(IEnumerable<ColumnAbbrMaster> rows);
    }
}
