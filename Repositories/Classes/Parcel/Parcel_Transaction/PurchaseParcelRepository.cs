using Dapper;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Dia_CoreAPI.Repositories.Classes.Parcel.Parcel_Transaction
{
    public class PurchaseParcelRepository : IPurchaseParcelRepository
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public PurchaseParcelRepository(
            ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<int> CheckUserPermissionAsync(string userTypeId,string formName, string actionType,string companyId)
        {
            string where = string.Empty;

            if (actionType == "insert")
                where = " InsertData = 1 ";
            else if (actionType == "edit")
                where = " EditData = 1 ";
            else if (actionType == "delete")
                where = " DeleteData = 1 ";
            else if (actionType == "navigate")
                where = " NavigateData = 1 ";
            else if (actionType == "select")
                where = " SelectData = 1 ";

            var parameters = new DynamicParameters();

            parameters.Add("@UserTypeId", "'" + userTypeId + "'");
            parameters.Add("@FormName", "'" + formName + "'");
            parameters.Add("@Where", where);

            var result =
                await _sqlDataAccess.SelectDataFromSP<PermissionCheck>(
                    "GetUserPermission",
                    parameters,
                    companyId);

            return result.Count();
        }

        public async Task<List<InvList>> GetPurchaseParcelListAsync(int pageNo,string companyId)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@PageNo", pageNo);

            var result =
                await _sqlDataAccess.SelectDataFromSP<InvList>("GetPurchaseParcelInvDataUsingPageNo", parameters, companyId);

            return result.ToList();
        }

        public async Task<PurchaseParcelPageData>GetPurchaseParcelPageDataAsync(string companyId)
        {
            PurchaseParcelPageData result =
                new PurchaseParcelPageData();

            List<BasicData> data = await ItemMasterDropDownAsync(companyId);

            result.Shape =
                data.Where(x => x.Type == "Shape").ToList();

            result.Color =
                data.Where(x => x.Type == "Color").ToList();

            result.Size =
                data.Where(x => x.Type == "Size").ToList();

            result.Clarity =
                data.Where(x => x.Type == "Clarity").ToList();

            result.Sieve =
                data.Where(x => x.Type == "Sieve").ToList();

            result.ProductTypeList =
                data.Where(x => x.Type == "ProductType").ToList();

            result.ItemGroup =
                data.Where(x => x.Type == "ItemGroup").ToList();

            var transactionData = await TransactionDropDownAsync(companyId);

            result.Type =
                transactionData.Where(x => x.Type == "Type").ToList();

            result.SubType =
                transactionData.Where(x => x.Type == "SubType").ToList();

            result.Category =
                transactionData.Where(x => x.Type == "Category").ToList();

            result.CurrencyList =
                transactionData.Where(x => x.Type == "Currency").ToList();

            result.BrokerList =
                new List<BasicData>()
                {
            new BasicData
            {
                ID = "",
                Name = "Select Broker"
            }
                };

            return result;
        }

        public async Task<List<BasicData>> ItemMasterDropDownAsync(string companyId)
        {
            var parameters = new DynamicParameters();

            var result =
                await _sqlDataAccess.SelectDataFromSP<BasicData>("GetAllMasterDataForItemMaster", parameters, companyId);

            return result.ToList();
        }

        public async Task<List<BasicData>> TransactionDropDownAsync(string companyId)
        {
            var parameters = new DynamicParameters();

            var result =
                await _sqlDataAccess.SelectDataFromSP<BasicData>("AllMasterDataForPurchase", parameters, companyId);

            return result.ToList();
        }

        public async Task<PurchaseParcelGridView> AddItemInGridAsync(
    AddSingleLotParcel item,
    List<ImportExcelParcel> parcelList,
    string companyId)
        {
            double totalCarats = 0;
            double totalAmount = 0;
            double totalMRPAmount = 0;
            double totalAmountWithoutless = 0;

            string alert = "";

            string caratFormat = "0.000";

            float tempCarat = item.Carat;

            string carat3D =
                Convert.ToDouble(tempCarat).ToString("N3");

            string carat2D =
                carat3D.Substring(0, carat3D.Length - 1);

            float carat =
                caratFormat == "0.000"
                ? float.Parse(carat3D)
                : float.Parse(carat2D);

            float purRate = item.Rate;
            float back = item.Back;
            float issueCarats = item.IssueCarats;

            if (issueCarats <= 0)
                throw new Exception("Carat is invalid.");

            if (purRate == 0 && back == 0)
                throw new Exception("Rate and Discount both can not be 0.");

            if (purRate != 0 && issueCarats != 0)
            {
                back =
                    (((purRate * 100) / issueCarats) - 100);
            }

            float amount = issueCarats * purRate;

            float l1 =
                item.Less1 != 0
                ? amount * (100 + item.Less1) / 100
                : amount;

            float l2 =
                item.Less2 != 0
                ? l1 * (100 + item.Less2) / 100
                : l1;

            float l3 =
                item.Less3 != 0
                ? l2 * (100 + item.Less3) / 100
                : l2;

            ImportExcelParcel pur =
                new ImportExcelParcel
                {
                    ItemName = item.ItemName,
                    LotNo = item.LotNo,
                    ExportNo = item.ExportNo,

                    PShape = item.ShapeId,
                    PColor = item.ColorId,
                    PClarity = item.ClarityId,
                    PSize = item.SizeId,
                    PSieve = item.SieveId,

                    Weight = item.Weight,
                    Pcs = item.Pcs,

                    PurchaseRate = purRate,
                    PurchaseBack = item.PurchaseBack,
                    SRate = item.SRate,

                    Less1 = item.Less1,
                    Less2 = item.Less2,
                    Less3 = item.Less3,

                    PurAmount = item.Amount,

                    IssueCarats = item.IssueCarats,
                    ReturnCarats = item.ReturnCarats,
                    SpCarats = item.SpCarats,

                    MemoTranCode = item.MemoTranCode,
                    MemoTransCode = item.MemoTransCode,

                    StockType = item.StockType,
                    SuppLotNo = item.SuppLotNo
                };

            parcelList ??= new List<ImportExcelParcel>();

            bool isEdit =
                item.editIndex >= 0 &&
                item.editIndex < parcelList.Count;

            if (isEdit)
            {
                parcelList[item.editIndex] = pur;
            }
            else
            {
                parcelList.Add(pur);
            }

            foreach (var row in parcelList)
            {
                double weight = row.Weight;

                totalCarats += row.Weight;
                totalAmount += row.PurAmount;
                totalMRPAmount += row.Weight * row.MRP;
                totalAmountWithoutless += weight * row.PurchaseRate;
            }

            return new PurchaseParcelGridView
            {
                TotalCarats = Math.Round(totalCarats, 3),
                TotalAmount = Math.Round(totalAmount, 2),
                TotalMRPAmount = Math.Round(totalMRPAmount, 3),
                TotalAmountWithoutless = Math.Round(totalAmountWithoutless, 3),
                Alert = alert,
                ExcelData = parcelList,
                IsEdit = isEdit
            };
        }

        public async Task<List<LotNoDataPPur>> GetLotDetailsAsync(string lotNo, string companyId)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@Type", "PPur");
            parameters.Add("@Where", $" AND LotNo = '{lotNo}' ");

            var result =
                await _sqlDataAccess.SelectDataFromSP<LotNoDataPPur>("GetLotNoDataForAll", parameters, companyId);

            return result.ToList();
        }
    }
}