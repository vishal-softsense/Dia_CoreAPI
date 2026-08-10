using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Payable_Receivables;
using BasicDataDto = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Interfaces.Reports.Payable_Receivables
{
    public interface IBrokerageOutstandingReportRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<IEnumerable<BasicDataDto>> GetDropdownDataAsync(string companyId);
        Task<List<BrokerageOutstandingReportResultDto>> GetBrokerageOutstandingDataAsync(
            string where,
            long curCode,
            long sCurCode,
            long mCurCode,
            string curMasterOperator,
            double mCurRate,
            string companyId);
        Task<long> GetCurCodeAsync(string where, string companyId);
        Task<string> GetMCurOperatorAsync(string curCode, string companyId);
        Task<double> GetMCurRateAsync(string curCode, string companyId);
    }
}
