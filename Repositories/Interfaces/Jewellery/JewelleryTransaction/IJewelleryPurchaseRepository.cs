using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Jewellery.JewelleryTransaction;

namespace Dia_CoreAPI.Repositories.Interfaces.Jewellery.JewelleryTransaction
{
    public interface IJewelleryPurchaseRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<JewelleryPurchaseInvListDto>> GetListAsync(int pageNo, string companyId);
        Task<JewelleryPurchasePageDataDto> GetPageDataAsync(string companyId);
        Task<JewelleryPurchaseDetailDto?> GetDetailAsync(int id, string companyId);
        Task<List<JewelleryPurchaseStockOptionDto>> SearchStockAsync(string searchBy, string companyId);
        Task<List<JewelleryPurchaseLotDetailDto>> GetLotDetailAsync(string lotNo, string companyId);
        Task<JewelleryPurchaseGridViewDto> AddItemInGridAsync(JewelleryPurchaseAddLotRequest request, string companyId);
        Task<JewelleryPurchaseGridViewDto> GetItemFromListAsync(JewelleryPurchaseGetItemFromListRequest request, string companyId);
        Task<List<JewelleryPurchaseConsignmentRowDto>> GetConsignmentAsync(string? partyCode, string? memoNo, string companyId);
        Task<(bool Success, string Message)> AddAsync(JewelleryPurchaseHeaderDto header, List<JewelleryPurchaseItemDto> items, string userId, string companyId);
        Task<(bool Success, string Message)> EditAsync(JewelleryPurchaseHeaderDto header, List<JewelleryPurchaseItemDto> items, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId);
        Task<JewelleryPurchasePrintViewDto?> GetPrintViewAsync(string invNo, string companyId);
        Task<JewelleryPurchaseStatusDto?> GetCurrentStatusAsync(string invNo, string companyId);
    }
}
