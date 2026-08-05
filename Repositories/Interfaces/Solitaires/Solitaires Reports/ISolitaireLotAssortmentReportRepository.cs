using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;

namespace Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports
{
    public interface ISolitaireLotAssortmentReportRepository
    {
        Task<IEnumerable<SolitaireLotAssortmentLotGroupDto>> GetReportAsync(
            SolitaireLotAssortmentReportFilterDto filter,
            string companyId);
    }
}
