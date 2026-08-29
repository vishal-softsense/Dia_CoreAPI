using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;

namespace Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Transaction
{
    public interface IMixSaleReturnService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<MixSaleReturnInvListDto>> GetListAsync(string companyId);
        Task<MixSaleReturnPageDataDto> GetPageDataAsync(string companyId);
        Task<MixSaleReturnDetailDto?> GetDetailAsync(int id, string companyId);
        Task<List<MixSaleReturnLotDetailDto>> GetLotDetailAsync(string lotNo, string companyId);
        Task<MixSaleReturnGridViewDto> AddItemInGridAsync(MixSaleReturnAddLotRequest request, string companyId);
        Task<MixSaleReturnGridViewDto> GetItemFromListAsync(MixSaleReturnGetItemFromListRequest request, string companyId);
        Task<(bool Success, string Message)> AddAsync(MixSaleReturnSaveRequest request, string userId, string companyId);
        Task<(bool Success, string Message)> EditAsync(MixSaleReturnSaveRequest request, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId);
        Task<MixSaleReturnPrintViewDto?> GetPrintViewAsync(string invNo, string companyId);
        Task<List<object>> GetLotsAsync(string? searchBy, string companyId);
        Task<List<MixSaleReturnCSaleLineDto>> GetCSaleForCSaleRtnAsync(string? partyCode, string? memoNo, string companyId);
        Task<MixSaleReturnGridViewDto> FillFromCSaleAsync(List<MixSaleReturnCSaleFillLineRequest> lines, List<MixSaleReturnItemDto> existingItems, string companyId);
    }
}
