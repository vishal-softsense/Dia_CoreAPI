using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Payable_Receivables;
using BasicDataDto = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Repositories.Interfaces.Reports.Payable_Receivables
{
    public interface IOutstandingReportRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<IEnumerable<BasicDataDto>> GetDropdownDataAsync(string companyId);
        Task<List<OutstandingReportResultDto>> GetOutstandingDataAsync(
            string where,
            string curCode,
            long sCurCode,
            long mCurCode,
            string curMasterOperator,
            double mCurRate,
            string dispCurrency,
            string companyId);
        Task<List<OutstandingReportResultDto>> GetOutstandingSummDataAsync(
            string where,
            string curCode,
            long sCurCode,
            long mCurCode,
            string curMasterOperator,
            double mCurRate,
            string companyId);
        Task<long> GetCurCodeAsync(string where, string companyId);
        Task<string> GetMCurOperatorAsync(string curCode, string companyId);
        Task<double> GetMCurRateAsync(string curCode, string companyId);
        Task<List<SalesPersonOptionDto>> GetSalesPersonsAsync(string companyId);
    }
}
