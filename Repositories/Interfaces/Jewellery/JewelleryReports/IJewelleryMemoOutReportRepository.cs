using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryReports;
using Dia_CoreAPI.DTOs.StockAccount;

namespace Dia_CoreAPI.Repositories.Interfaces.Jewellery.JewelleryReports
{
    public interface IJewelleryMemoOutReportRepository
    {
        Task<IEnumerable<BasicDataDto>> GetDropdownsAsync(string companyId);

        Task<IEnumerable<JewelleryMemoOutReportRowDto>> GetReportAsync(
            JewelleryMemoOutReportFilterDto filter,
            string companyId);
    }
}
