using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Payable_Receivables;
using BasicDataDto = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Interfaces.Reports.Payable_Receivables
{
    public interface ISalePurchaseSummaryReportRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<IEnumerable<BasicDataDto>> GetDropdownDataAsync(string companyId);
        Task<List<SalePurchaseSummaryReportResultDto>> GetSalePurchaseSummaryDataAsync(string where, string companyId);
    }
}
