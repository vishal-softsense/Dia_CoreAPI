using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.Models;
using Dia_CoreAPI.Models.Masters;

namespace Dia_CoreAPI.Repositories.Interfaces.Masters
{
    public interface ISettingRepository
    {
        Task<IEnumerable<CompanySettingMaster>> GetSettingListAsync(string filter, string companyId);
        Task<bool> SaveSettingDataAsync(CompanySettingMaster model, string actionType, string companyId);
        Task<bool> CheckUserPermissionAsync(int? userTypeId, string formName, string action, string companyId);
        Task LogPageAccessAsync(string userName, string status, string device, string deviceType, string os, string ip, string companyId);
        Task<IEnumerable<GetInvoiceFormatData>> GetInvoiceFormatListAsync(string filter, string companyId);
        Task<string?> GetInvoicePathAsync(string filter, string companyId);
        Task<IEnumerable<ColumnAbbrMaster>> GetColumnAbbrListAsync(string type, string columnName, string abbreviation, string companyId);
    }
}
