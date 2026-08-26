using System;
using System.Globalization;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Utility;
using Dia_CoreAPI.Services.Interfaces.Reports.Utility;
using Microsoft.AspNetCore.Http;

namespace Dia_CoreAPI.Services.Classes.Reports.Utility
{
    public partial class ImportGradingService : IImportGradingService
    {
        private const string FormName = "Import Grading";
        private readonly IImportGradingRepository _repository;
        private readonly ISqlDataAccess _sqlDataAccess;

        public ImportGradingService(IImportGradingRepository repository, ISqlDataAccess sqlDataAccess)
        {
            _repository = repository;
            _sqlDataAccess = sqlDataAccess;
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

        private async Task<ImportGradingResult> WithInsertPermissionAsync(string userTypeId, string companyId, Func<Task<ImportGradingResult>> action)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, FormName, "insert", companyId);
            if (permission != 1)
                return new ImportGradingResult(false, "You are not authorized to perform this task.");
            return await action();
        }

        private async Task<ImportGradingResult> WithEditPermissionAsync(string userTypeId, string companyId, Func<Task<ImportGradingResult>> action)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, FormName, "edit", companyId);
            if (permission != 1)
                return new ImportGradingResult(false, "You are not authorized to perform this task.");
            return await action();
        }

        private async Task<ImportGradingResult> WithDeletePermissionAsync(string userTypeId, string companyId, Func<Task<ImportGradingResult>> action)
        {
            int permission = await CheckUserPermissionAsync(userTypeId, FormName, "delete", companyId);
            if (permission != 1)
                return new ImportGradingResult(false, "You are not authorized to perform this task.");
            return await action();
        }

        public Task<ImportGradingResult> UpdateGiaDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => UpdateGiaDataInternalAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UpdateIgiDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => UpdateIgiDataInternalAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UpdateHrdDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => UpdateHrdDataInternalAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UpdateGraderDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => UpdateGraderDataInternalAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UpdatePurchaseDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => UpdatePurchaseDataInternalAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UpdateImageVideoDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => UpdateimagevideoInternalAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UpdateParcelItemDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => InsUpdparcelitemInternalAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UpdateSolitaireItemDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => InsUpdsolitaireitemInternalAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UpdateAccountDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => InsUpdaccountitemInternalAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UploadPurchaseDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => UploadPurchaseDataInternalAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UploadPurchaseDetailsAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => UploadPurchaseDetailsInternalAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UploadJewStockDiamondDetailsAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => UploadJewelleryDiamondStockInternalAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> UpdateSizeIdsAsync(string companyId, string userTypeId)
            => WithEditPermissionAsync(userTypeId, companyId, () => UpdateSizeIdsInternalAsync(companyId, userTypeId));

        public Task<ImportGradingResult> DeleteDeletedRecordsAsync(string companyId, string userTypeId)
            => WithDeletePermissionAsync(userTypeId, companyId, () => DeleteDeletedRecordsInternalAsync(companyId, userTypeId));

        public Task<ImportGradingResult> UploadLogoAsync(IFormFile image, string companyId, string userTypeId, string webRootPath)
            => WithEditPermissionAsync(userTypeId, companyId, () => UploadAndUpdateCompanyLoginLogoInternalAsync(image, companyId, userTypeId, webRootPath));

        public Task<ImportGradingResult> UploadJewelleryFullAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => UploadJewelleryFullInternalAsync(dto, companyId, userId, userTypeId));

        public Task<ImportGradingResult> ImportAskingRateAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
            => WithInsertPermissionAsync(userTypeId, companyId, () => ImportAskingRateInternalAsync(dto, companyId, userId, userTypeId));

        /// <summary>Matches MVC BasicFunctionController.GetConvertAmount</summary>
        private static string[] GetConvertAmount(
            float curCode,
            float amount,
            float mCurBaseRate,
            string mCurOperator,
            float sCurBaseRate,
            string sCurOperator,
            float mCurId,
            float sCurId)
        {
            var amountResponse = new string[2];
            try
            {
                float mCurAmt = 0;
                float sCurAmt = 0;

                if (curCode == mCurId)
                {
                    mCurAmt = amount;
                    if (sCurOperator == "*")
                        mCurAmt = amount * sCurBaseRate;
                    else
                        sCurAmt = sCurBaseRate != 0 ? amount / sCurBaseRate : 0f;
                }
                else if (curCode == sCurId)
                {
                    sCurAmt = amount;
                    if (mCurOperator == "*")
                        mCurAmt = amount * mCurBaseRate;
                    else
                        mCurAmt = mCurBaseRate != 0 ? amount / mCurBaseRate : 0f;
                }
                else
                {
                    if (mCurOperator == "*")
                        mCurAmt = amount * mCurBaseRate;
                    else
                        mCurAmt = mCurBaseRate != 0 ? amount / mCurBaseRate : 0f;

                    if (sCurOperator == "*")
                        sCurAmt = amount * sCurBaseRate;
                    else
                        sCurAmt = sCurBaseRate != 0 ? amount / sCurBaseRate : 0f;
                }

                if (float.IsNaN(mCurAmt) || float.IsInfinity(mCurAmt)) mCurAmt = 0;
                if (float.IsNaN(sCurAmt) || float.IsInfinity(sCurAmt)) sCurAmt = 0;

                amountResponse[0] = mCurAmt.ToString(CultureInfo.InvariantCulture);
                amountResponse[1] = sCurAmt.ToString(CultureInfo.InvariantCulture);
            }
            catch
            {
                amountResponse[0] = "0";
                amountResponse[1] = "0";
            }

            return amountResponse;
        }
    }
}
