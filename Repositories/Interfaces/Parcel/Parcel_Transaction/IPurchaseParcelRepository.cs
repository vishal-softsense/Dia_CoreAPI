using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction
{
    public interface IPurchaseParcelRepository
    {
        Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId);
        Task<List<InvList>> GetPurchaseParcelListAsync(int pageNo,string companyId);
        Task<PurchaseParcelPageData>GetPurchaseParcelPageDataAsync(string companyId);
        Task<List<BasicData>> ItemMasterDropDownAsync(string companyId);
        Task<List<BasicData>> TransactionDropDownAsync(string companyId);

        Task<PurchaseParcelGridView> AddItemInGridAsync(
    AddSingleLotParcel item,
    List<ImportExcelParcel> parcelList,
    string companyId);

        Task<List<LotNoDataPPur>> GetLotDetailsAsync(string lotNo,string companyId);

        Task<(bool Success, string Message)> AddPurchaseParcelAsync(
    SalePurs salePur,
    List<ImportExcelpurAdd> items,
    string userId,
    string companyId);
    }
}