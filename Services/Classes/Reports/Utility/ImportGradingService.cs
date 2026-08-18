using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Utility;
using Dia_CoreAPI.Services.Helpers;
using Dia_CoreAPI.Services.Interfaces.Reports.Utility;
using Microsoft.AspNetCore.Http;

namespace Dia_CoreAPI.Services.Classes.Reports.Utility
{
    public class ImportGradingService : IImportGradingService
    {
        private const string FormName = "Import Grading";
        private readonly IImportGradingRepository _repository;
        private readonly ImportGradingProcessor _processor;

        public ImportGradingService(IImportGradingRepository repository, ImportGradingProcessor processor)
        {
            _repository = repository;
            _processor = processor;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);

        public async Task<ImportGradingResult> ImportGradingDataAsync(string userTypeId, string companyId)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, FormName, "insert", companyId);
            if (permission == 1)
                return new ImportGradingResult(true, null, null, null, "Add");
            return new ImportGradingResult(false, "You are not authorized to perform this task.");
        }

        private async Task<ImportGradingResult> WithInsertPermissionAsync(string userTypeId, string companyId, System.Func<Task<ImportGradingResult>> action)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, FormName, "insert", companyId);
            if (permission != 1)
                return new ImportGradingResult(false, "You are not authorized to perform this task.");
            return await action();
        }

        private async Task<ImportGradingResult> WithEditPermissionAsync(string userTypeId, string companyId, System.Func<Task<ImportGradingResult>> action)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, FormName, "edit", companyId);
            if (permission != 1)
                return new ImportGradingResult(false, "You are not authorized to perform this task.");
            return await action();
        }

        private async Task<ImportGradingResult> WithDeletePermissionAsync(string userTypeId, string companyId, System.Func<Task<ImportGradingResult>> action)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, FormName, "delete", companyId);
            if (permission != 1)
                return new ImportGradingResult(false, "You are not authorized to perform this task.");
            return await action();
        }

        public Task<ImportGradingResult> UpdateGiaDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => _processor.UpdateGiaDataAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UpdateIgiDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => _processor.UpdateIgiDataAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UpdateHrdDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => _processor.UpdateHrdDataAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UpdateGraderDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => _processor.UpdateGraderDataAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UpdatePurchaseDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => _processor.UpdatePurchaseDataAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UpdateImageVideoDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => _processor.UpdateimagevideoAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UpdateParcelItemDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => _processor.InsUpdparcelitemAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UpdateSolitaireItemDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => _processor.InsUpdsolitaireitemAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UpdateAccountDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => _processor.InsUpdaccountitemAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UploadPurchaseDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => _processor.UploadPurchaseDataAsync(dto, companyId, userId, userTypeId));

        // Disabled: Import Sale commented in MVC UI
        //public Task<ImportGradingResult> ImportSaleDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
        //    => WithInsertPermissionAsync(userTypeId, companyId, () => _processor.InsertSaleDataAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UploadPurchaseDetailsAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => _processor.UploadPurchaseDetailsDataAsync(dto, companyId, userId, userTypeId));

        // Disabled: legacy UploadJewData() commented in MVC UI
        //public Task<ImportGradingResult> UploadJewStockDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
        //    => WithInsertPermissionAsync(userTypeId, companyId, () => _processor.UploadJewelleryStockAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UploadJewStockDiamondDetailsAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => _processor.UploadJewelleryDiamondStockAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UpdateSizeIdsAsync(string companyId, string userTypeId)
            => WithEditPermissionAsync(userTypeId, companyId, () => _processor.UpdateSizeIdsAsync(companyId, userTypeId));

        public Task<ImportGradingResult> DeleteDeletedRecordsAsync(string companyId, string userTypeId)
            => WithDeletePermissionAsync(userTypeId, companyId, () => _processor.DeleteDeletedRecordsAsync(companyId, userTypeId));

        public Task<ImportGradingResult> UploadLogoAsync(IFormFile image, string companyId, string userTypeId, string webRootPath)
            => WithEditPermissionAsync(userTypeId, companyId, () => _processor.UploadAndUpdateCompanyLoginLogoAsync(image, companyId, userTypeId, webRootPath));

        public Task<ImportGradingResult> UploadJewelleryFullAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => _processor.UploadJewelleryFullAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> ImportAskingRateAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => _processor.ImportAskingRateAsync(dto, companyId, userId, userTypeId));
    }
}
