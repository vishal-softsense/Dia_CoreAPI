using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction;

namespace Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Transaction
{
    public interface ISolitaireSaleReturnRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<SolitaireSaleReturnInvListDto>> GetListAsync(int pageNo, string companyId);
        Task<SolitaireSaleReturnPageDataDto> GetPageDataAsync(string companyId);
        Task<SolitaireSaleReturnDetailDto?> GetDetailAsync(int id, string companyId);
        Task<List<SolitaireSaleReturnLotDetailDto>> GetLotDetailAsync(SolitaireSaleReturnLotRequest request, string companyId);
        Task<SolitaireSaleReturnGridViewDto> AddItemInGridAsync(SolitaireSaleReturnAddLotRequest request, string companyId);
        Task<SolitaireSaleReturnGridViewDto> GetItemFromListAsync(SolitaireSaleReturnGetItemFromListRequest request, string companyId);
        Task<SolitaireSaleReturnGridViewDto> RefreshBackAsync(SolitaireSaleReturnRefreshBackRequest request, string companyId);
        Task<List<SolitaireSaleReturnSaleRowDto>> GetSaleToSaleReturnAsync(string? partyCode, string? invNo, string companyId);
        Task<SolitaireSaleReturnGridViewDto> FillSaleToSaleReturnAsync(SolitaireSaleReturnFillRequest request, string companyId);
        Task<(bool Success, string Message)> AddAsync(SolitaireSaleReturnHeaderDto header, List<SolitaireSaleReturnItemDto> items, string userId, string companyId);
        Task<(bool Success, string Message)> EditAsync(SolitaireSaleReturnHeaderDto header, List<SolitaireSaleReturnItemDto> items, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId);
        Task<SolitaireSaleReturnPrintViewDto?> GetPrintViewAsync(string invNo, string companyId);
    }
}
