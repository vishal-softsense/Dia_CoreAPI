using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Transaction;

namespace Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Transaction
{
    public interface ISolitaireSaleRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<SolitaireSaleInvListDto>> GetListAsync(int pageNo, string companyId);
        Task<SolitaireSalePageDataDto> GetPageDataAsync(string companyId);
        Task<SolitaireSaleDetailDto?> GetDetailAsync(int id, string companyId);
        Task<List<SolitaireSaleLotDetailDto>> GetLotDetailAsync(SolitaireSaleLotRequest request, string companyId);
        Task<SolitaireSaleGridViewDto> AddItemInGridAsync(SolitaireSaleAddLotRequest request, string companyId);
        Task<SolitaireSaleGridViewDto> GetItemFromListAsync(SolitaireSaleGetItemFromListRequest request, string companyId);
        Task<SolitaireSaleGridViewDto> RefreshBackAsync(SolitaireSaleRefreshBackRequest request, string companyId);
        Task<List<SolitaireSaleMemoRowDto>> GetMemoToSaleAsync(string? partyCode, string? memoNo, string companyId);
        Task<(bool Success, string Message)> AddAsync(SolitaireSaleHeaderDto header, List<SolitaireSaleItemDto> items, string userId, string companyId);
        Task<(bool Success, string Message)> EditAsync(SolitaireSaleHeaderDto header, List<SolitaireSaleItemDto> items, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId);
        Task<SolitaireSalePrintViewDto?> GetPrintViewAsync(string invNo, string companyId);
        Task<SolitaireSaleStatusDto?> GetCurrentStatusAsync(string invNo, string companyId);
        Task<List<SolitaireSaleShipViaOptionDto>> GetShipViaAsync(string searchBy, string companyId);
    }
}
