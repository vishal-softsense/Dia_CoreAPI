using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;

namespace Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction
{
    public interface IMixSaleRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<MixSaleInvListDto>> GetListAsync(int pageNo, string companyId);
        Task<MixSalePageDataDto> GetPageDataAsync(string companyId);
        Task<MixSaleDetailDto?> GetDetailAsync(int id, string companyId);
        Task<List<MixSaleLotDetailDto>> GetLotDetailAsync(string lotNo, string companyId);
        Task<List<MixConsignmentRowDto>> GetConsignmentAsync(string? partyCode, string? memoNo, string companyId);
        Task<MixSaleGridViewDto> AddItemInGridAsync(MixSaleAddLotRequest request, string companyId);
        Task<(bool Success, string Message)> AddAsync(MixSaleHeaderDto header, List<MixSaleItemDto> items, string userId, string companyId);
        Task<(bool Success, string Message)> EditAsync(MixSaleHeaderDto header, List<MixSaleItemDto> items, string userId, string companyId);
        Task<(bool Success, string Message)> DeleteAsync(int id, string userId, string companyId);
        Task<MixSalePrintViewDto?> GetPrintViewAsync(string invNo, string companyId);
        Task<MixSaleStatusDto?> GetCurrentStatusAsync(string invNo, string companyId);
        Task<List<object>> GetLotsAsync(string? searchBy, string companyId);
    }
}
