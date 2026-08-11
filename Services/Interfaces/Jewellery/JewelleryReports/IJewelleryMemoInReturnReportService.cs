using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;
using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Services.Interfaces.Jewellery.JewelleryReports
{
    public interface IJewelleryMemoInReturnReportService
    {
        Task<IEnumerable<BasicDataDto>> GetDropdownsAsync(string companyId);

        Task<IEnumerable<JewelleryMemoInReturnReportRowDto>> GetReportAsync(
            JewelleryMemoInReturnReportFilterDto filter,
            string companyId);
    }
}
