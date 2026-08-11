using System;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;

namespace Dia_CoreAPI.Repositories.Interfaces.Reports.Utility
{
    public interface IDownloadMRPRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<RapaportCredentialsDTO> GetRapaportCredentialsAsync(string companyId);
        Task<bool> DeleteTabPriceIndexRAsync(string shape, string companyId);
        Task<bool> InsertTabPriceIndexRAsync(string shape, RapaportPriceRowDTO row, DateTime rapaportDate, string companyId);
        Task<bool> UpdatePriceToCaratAsync(string companyId);
        Task<bool> UpdateMRPAsync(string companyId);
    }
}
