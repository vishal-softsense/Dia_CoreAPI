using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Dashboard;
using Dia_CoreAPI.DTOs.Masters;

namespace Dia_CoreAPI.Repositories.Interfaces.Dashboard
{
    public interface IPurchaseAnalysisReportRepository
    {
        Task<List<BasicData>> GetDropdownDataAsync(string companyId);
        Task<List<PurchaseAnalysisReportDto>> GetPurchaseAnalysisReportAsync(PurchaseAnalysisRequestDto filter, string companyId);
    }
}
