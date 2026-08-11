using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Microsoft.AspNetCore.Http;

namespace Dia_CoreAPI.Services.Interfaces.Reports.Utility
{
    public interface IImportGradingService
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<ImportGradingResult> ImportGradingDataAsync(string userTypeId, string companyId);
        Task<ImportGradingResult> UpdateGiaDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId);
        Task<ImportGradingResult> UpdateIgiDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId);
        Task<ImportGradingResult> UpdateHrdDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId);
        Task<ImportGradingResult> UpdateGraderDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId);
        Task<ImportGradingResult> UpdatePurchaseDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId);
        Task<ImportGradingResult> UpdateImageVideoDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId);
        Task<ImportGradingResult> UpdateParcelItemDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId);
        Task<ImportGradingResult> UpdateSolitaireItemDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId);
        Task<ImportGradingResult> UpdateAccountDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId);
        Task<ImportGradingResult> UploadPurchaseDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId);
        Task<ImportGradingResult> ImportSaleDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId);
        Task<ImportGradingResult> UploadPurchaseDetailsAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId);
        Task<ImportGradingResult> UploadJewStockDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId);
        Task<ImportGradingResult> UploadJewStockDiamondDetailsAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId);
        Task<ImportGradingResult> UpdateSizeIdsAsync(string companyId, string userTypeId);
        Task<ImportGradingResult> DeleteDeletedRecordsAsync(string companyId, string userTypeId);
        Task<ImportGradingResult> UploadLogoAsync(IFormFile image, string companyId, string userTypeId, string webRootPath);
        Task<ImportGradingResult> UploadJewelleryFullAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId);
        Task<ImportGradingResult> ImportAskingRateAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId);
    }
}
