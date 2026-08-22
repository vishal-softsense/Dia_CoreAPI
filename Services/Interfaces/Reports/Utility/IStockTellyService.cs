using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Microsoft.AspNetCore.Http;

namespace Dia_CoreAPI.Services.Interfaces.Reports.Utility
{
    public interface IStockTellyService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<StockTellyPageDto> LoadPageDataAsync(string companyId);
        Task<List<StockTellyListDto>> GetStockTellyReportAsync(StockTellyFilterDto filter, string companyId);
        Task<(bool Success, object? Data)> ImportStockTellyAsync(IFormFile? file, string userTypeId, string companyId);
        Task<(bool Success, string Message)> SaveStockTellyAsync(
            StockTellySaveRequestDto request,
            int userId,
            string userName,
            int companyId,
            string companyIdStr);
    }
}
