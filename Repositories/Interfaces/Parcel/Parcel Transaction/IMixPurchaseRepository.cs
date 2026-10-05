using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;

namespace Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction
{
    public interface IMixPurchaseRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<MixPurchaseInvListDto>> GetListAsync(int pageNo, string companyId);
        Task<MixPurchasePageDataDto> GetPageDataAsync(string companyId);
        Task<MixPurchaseDetailDto?> GetDetailAsync(int id, string companyId);
        Task<List<MixPurchaseLotDetailDto>> GetLotDetailAsync(string lotNo, string companyId);
        Task<List<MixConsignmentRowDto>> GetConsignmentAsync(string? partyCode, string? memoNo, string companyId);
        Task<MixPurchaseGridViewDto> AddItemInGridAsync(MixPurchaseAddLotRequest request, string companyId);
        Task<(bool Success, string Message)> AddAsync(MixPurchaseHeaderDto header, List<MixPurchaseItemDto> items, string userId, string companyId);
        Task<(bool Success, string Message)> EditAsync(MixPurchaseHeaderDto header, List<MixPurchaseItemDto> items, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId);
        Task<MixPurchasePrintViewDto?> GetPrintViewAsync(string invNo, string companyId);
        Task<MixPurchaseStatusDto?> GetCurrentStatusAsync(string invNo, string companyId);
        Task<List<object>> GetLotsAsync(string? searchBy, string companyId);
        Task<string> GetMixLotNoDIAsync(string companyId);
        Task<string?> GetSettingValueAsync(string name, string companyId);
        Task<float> GetRapRateAsync(string shape, string color, string clarity, double carat, string companyId);
    }
}
