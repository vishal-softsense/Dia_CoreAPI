using System.Collections.Generic;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Utility;
using Dia_CoreAPI.Repositories.Interfaces.StockAccount;
using Dia_CoreAPI.Services.Interfaces.Reports.Utility;

namespace Dia_CoreAPI.Services.Classes.Reports.Utility
{
    public class StockTransferService : IStockTransferService
    {
        private readonly IStockTransferRepository _repository;
        private readonly IJewStockMasterRepository _jewStockMasterRepository;

        public StockTransferService(
            IStockTransferRepository repository,
            IJewStockMasterRepository jewStockMasterRepository)
        {
            _repository = repository;
            _jewStockMasterRepository = jewStockMasterRepository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);

        public Task<List<StockTransferListDto>> GetListAsync(string companyId)
            => _repository.GetStockTransferListAsync(companyId);

        public Task<List<StockTranDataDto>> GetLotDetailsAsync(string lotNo, string companyId)
            => _repository.GetLotDetailsAsync(lotNo, companyId);

        /// <summary>
        /// Uses the same lot lookup as JewelleryStockMaster get-alllotno-listall (empty stoneType),
        /// which already searches LotNo/ItemName via GetItemDataUsingPageNo.
        /// </summary>
        public Task<IEnumerable<BasicDataDto>> GetAllLotNoListAsync(string searchby, string companyId)
            => _jewStockMasterRepository.GetAllLotNoListAllAsync(searchby ?? "", "", companyId);

        public Task<(bool Success, string Message)> SaveAsync(
            StockTransferSaveDto model,
            int userId,
            string userName,
            int companyId,
            string companyIdStr)
            => _repository.SaveStockTransferAsync(model, userId, userName, companyId, companyIdStr);

        public Task<(bool Success, string Message)> DeleteAsync(int id, string companyIdStr)
            => _repository.DeleteStockTransferAsync(id, companyIdStr);
    }
}
