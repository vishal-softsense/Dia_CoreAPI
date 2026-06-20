using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Repositories.Classes.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Transaction;

namespace Dia_CoreAPI.Services.Classes.Parcel.Parcel_Transaction
{
    public class PurchaseParcelService : IPurchaseParcelService
    {
        private readonly IPurchaseParcelRepository _repository;

        public PurchaseParcelService(
            IPurchaseParcelRepository repository)
        {
            _repository = repository;
        }
        public async Task<int> CheckUserPermissionAsync(string userTypeId,string formName,string actionType,string companyId)
        {
            return await _repository.CheckUserPermissionAsync(userTypeId,formName,actionType,companyId);
        }
        public async Task<List<InvList>> GetPurchaseParcelListAsync(int pageNo,string companyId)
        {
            return await _repository.GetPurchaseParcelListAsync(pageNo,companyId);
        }
        public async Task<PurchaseParcelPageData>GetPurchaseParcelPageDataAsync(string companyId)
        {
            return await _repository.GetPurchaseParcelPageDataAsync(companyId);
        }

        public async Task<List<BasicData>> ItemMasterDropDownAsync(string companyId)
        {
            return await _repository.ItemMasterDropDownAsync(companyId);
        }

        public async Task<List<BasicData>> TransactionDropDownAsync(string companyId)
        {
            return await _repository.TransactionDropDownAsync(companyId);
        }

        public async Task<PurchaseParcelGridView> AddItemInGridAsync(AddSingleLotParcel item, List<ImportExcelParcel> parcelList, string companyId)
        {
            return await _repository.AddItemInGridAsync(
                item,
                parcelList,
                companyId);
        }

        public async Task<List<LotNoDataPPur>> GetLotDetailsAsync(string lotNo, string companyId)
        {
            return await _repository.GetLotDetailsAsync(lotNo,companyId);
        }
    }
}