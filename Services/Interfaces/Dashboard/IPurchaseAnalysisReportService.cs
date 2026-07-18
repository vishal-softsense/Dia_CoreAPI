using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Dashboard;
using Dia_CoreAPI.DTOs.Masters;

namespace Dia_CoreAPI.Services.Interfaces.Dashboard
{
    public interface IPurchaseAnalysisReportService
    {
        Task<List<BasicData>> GetDropdownDataAsync(string companyId);
        Task<List<PurchaseAnalysisReportDto>> GetPurchaseAnalysisReportAsync(PurchaseAnalysisRequestDto filter, string companyId);
    }
}
