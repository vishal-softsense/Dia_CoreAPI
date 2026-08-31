using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.StockAccount;
using Dia_CoreAPI.Services.Interfaces.StockAccount;
using Microsoft.AspNetCore.Hosting;

namespace Dia_CoreAPI.Services.Classes.StockAccount
{
    public class JewStockMasterService : IJewStockMasterService
    {
        private readonly IJewStockMasterRepository _repository;
        private readonly IWebHostEnvironment _env;

        public JewStockMasterService(IJewStockMasterRepository repository, IWebHostEnvironment env)
        {
            _repository = repository;
            _env = env;
        }

        public async Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            return await _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        public async Task<IEnumerable<JewStockMasterDto>> GetMountingStockAsync(string companyId, string whereClause = "")
        {
            return await _repository.GetMountingStockAsync(companyId, whereClause);
        }

        public async Task<IEnumerable<JewMetalMasterDto>> GetJewMetalMasterAsync(string companyId, string whereClause = "")
        {
            return await _repository.GetJewMetalMasterAsync(companyId, whereClause);
        }

        public async Task<IEnumerable<JewColorMasterDto>> GetJewColorMasterAsync(string companyId, string whereClause = "")
        {
            return await _repository.GetJewColorMasterAsync(companyId, whereClause);
        }

        public async Task<IEnumerable<GetJewPurityDto>> GetJewPurityAsync(string companyId,string metalType)
        {
            return await _repository.GetJewPurityAsync(companyId, metalType);
        }
        public async Task<double> GetPureRatioAsync(string companyId,string metalType,string metalPurity)
        {
            return await _repository.GetPureRatioAsync(companyId, metalType, metalPurity);
        }

        public async Task<AddMountingStockDto> GetAddMountingStockDataAsync(string companyId)
        {
            return await _repository.GetAddMountingStockDataAsync(companyId);
        }

        public async Task<List<JewCategoryDto>> GetJewCategoryMasterAsync(string companyId,string whereClause = "")
        {
            return await _repository.GetJewCategoryMasterAsync(companyId, whereClause);
        }

        public async Task<List<JewSubCategoryDto>> GetJewSubCategoryMasterAsync(string companyId, string whereClause = "")
        {
            return await _repository.GetJewSubCategoryMasterAsync(companyId, whereClause);
        }

        public async Task<List<SettingMasterDto>> GetJewSettingValueAsync(string companyId)
        {
            return await _repository.GetJewSettingValueAsync(companyId);
        }

        // Mounting-specific service methods
        public async Task<JewStockMasterDto?> GetMountingStockByIdAsync(int id, string companyId)
        {
            return await _repository.GetMountingStockByIdAsync(id, companyId);
        }

        public async Task<(bool success, string message)> SaveMountingStockAsync(JewStockMasterDto model, List<DiamondDetails> diamond, List<SettingDetails> setting, string actionType, string userTypeId, string companyId)
        {
            if (string.IsNullOrWhiteSpace(model.DesignID) && string.IsNullOrWhiteSpace(model.Stock_ID))
            {
                return (false, "Stock Name is required.");
            }

            if (model.JewCode == 0 && actionType == "insert")
            {
                if (string.IsNullOrWhiteSpace(model.DesignID))
                {
                    // Check if stock ID already exists
                    bool exists = await _repository.CheckStockIdExistsAsync(model.Stock_ID.Trim(), companyId);
                    if (exists)
                    {
                        return (false, "This Stock ID already exists. Please use a different Stock ID.");
                    }
                }

                // Check if UseMounting is already used
                if (!string.IsNullOrWhiteSpace(model.UseMounting))
                {
                    bool isUsed = await _repository.CheckMountingUsedAsync(model.UseMounting, companyId);
                    if (isUsed)
                    {
                        return (false, "This Mounting Lot No is already used in another stock.");
                    }
                }

                // Permission check AFTER validation (matching MVC FINAL logic)
                int permission = await _repository.CheckUserPermissionAsync(userTypeId, "Jew Stock Master", "insert", companyId);
                if (permission != 1)
                {
                    return (false, "You are not authorized to perform this task.");
                }
            }
            else if (actionType == "update")
            {
                // Permission check for update
                int permission = await _repository.CheckUserPermissionAsync(userTypeId, "Jew Stock Master", "edit", companyId);
                if (permission != 1)
                {
                    return (false, "You are not authorized to perform this task.");
                }
            }

            model.ImageBinary = ResolveImageBinary(model.ImagePath, model.ImageBinary);

            bool saveSuccess = await _repository.SaveMountingStockAsync(model, diamond, setting, actionType, companyId);
            if (saveSuccess)
            {
                string successMsg = actionType == "update" ? "Jewellery Stock record update successfully." : "Jewellery Stock record saved successfully.";
                return (true, successMsg);
            }
            return (false, $"Failed to {actionType} Jewellery Stock record.");
        }

        public async Task<(bool success, string message)> DeleteMountingStockMasterAsync(int id, string companyId)
        {
            bool deleteSuccess = await _repository.DeleteMountingStockMasterAsync(id, companyId);
            if (deleteSuccess)
            {
                return (true, "Record Deleted.");
            }
            return (false, "Record not found.");
        }

        public async Task<(bool success, string message, JewGridData? data)> AddMountingItemInGridAsync(AddJewLotNo item, List<JewDiamondDetails> existingGrid)
        {
            try
            {
                double totalCarats = 0;
                double totalAmount = 0;
                int totPcs = 0;

                float purRate = item.Rate != 0 ? item.Rate : 0;
                float back = item.Back != 0 ? item.Back : 0;
                double issueCarats = item.IssueCarats;

                if (issueCarats <= 0)
                {
                    return (false, "Carat is invalid.", null);
                }

                if (purRate == 0 && back == 0)
                {
                    return (false, "Rate and Discount both can not be 0.", null);
                }

                if (purRate != 0 && issueCarats != 0)
                {
                    back = (float)(((purRate * 100) / issueCarats) - 100);
                }

                var jewDiamond = new JewDiamondDetails
                {
                    Category = item.Category,
                    LotNo = item.LotNo,
                    Shape = item.ShapeId,
                    Size = item.SizeId,
                    Color = item.ColorId,
                    Clarity = item.ClarityId,
                    Sieve = item.SieveId,
                    Pcs = item.Pcs,
                    IssueCarats = item.IssueCarats,
                    Rate = purRate,
                    Amount = item.Amount,
                    Cert = item.Cert
                };

                var jewExcel = existingGrid ?? new List<JewDiamondDetails>();

                bool isEdit = item.editIndex >= 0 && item.editIndex < jewExcel.Count;
                if (isEdit)
                {
                    jewExcel[item.editIndex] = jewDiamond;
                }
                else
                {
                    jewExcel.Add(jewDiamond);
                }

                foreach (var gridItem in jewExcel)
                {
                    totPcs += gridItem.Pcs;
                    totalCarats += gridItem.IssueCarats;
                    totalAmount += gridItem.Amount;
                }

                var gridData = new JewGridData
                {
                    TotalCarats = Math.Round(totalCarats, 3),
                    TotalAmount = Math.Round(totalAmount, 2),
                    TotalPcs = totPcs,
                    ExcelData = jewExcel,
                    IsEdit = isEdit
                };

                return (true, "Item added successfully.", gridData);
            }
            catch (Exception ex)
            {
                return (false, "Error: " + ex.Message, null);
            }
        }

        public async Task<(bool success, string message, JewGridData? data)> DeleteMountingItemFromGridAsync(int index, List<JewDiamondDetails> existingGrid)
        {
            if (existingGrid == null)
            {
                return (false, "Data not found or session expired. Please reload the page.", null);
            }

            if (index < 0 || index >= existingGrid.Count)
            {
                return (false, "Invalid index provided.", null);
            }

            existingGrid.RemoveAt(index);

            double totalCarats = 0;
            double totalAmount = 0;
            int totPcs = 0;

            foreach (var item in existingGrid)
            {
                totPcs += item.Pcs;
                totalCarats += item.IssueCarats;
                totalAmount += item.Amount;
            }

            var gridData = new JewGridData
            {
                TotalCarats = Math.Round(totalCarats, 3),
                TotalAmount = Math.Round(totalAmount, 2),
                TotalPcs = totPcs,
                ExcelData = existingGrid
            };

            return (true, "Item removed successfully.", gridData);
        }

        public async Task<string> GetNextMountingStockIdAsync(string designId, string companyId)
        {
            return await _repository.GetNextMountingStockIdAsync(designId, companyId);
        }

        public async Task<IEnumerable<BasicDataDto>> GetMountingStockDropdownAsync(string category, string actionType, string companyId)
        {
            return await _repository.GetMountingStockDropdownAsync(category, actionType, companyId);
        }

        public async Task<IEnumerable<BasicDataDto>> GetAllLotNoListAllAsync(string searchby, string stoneType, string companyId)
        {
            return await _repository.GetAllLotNoListAllAsync(searchby, stoneType, companyId);
        }

        public async Task<(JewStockMasterDto? data, List<DiamondDetails> diamondList)> GetAllMountingDetailsAsync(int id, string companyId)
        {
            return await _repository.GetAllMountingDetailsAsync(id, companyId);
        }

        public async Task<List<LotNoDataJew>> GetJewLotNoDataAsync(string lotNo, string companyId)
        {
            return await _repository.GetJewLotNoDataAsync(lotNo, companyId);
        }

        /// <summary>
        /// Same as MVC SaveMountingStock: read file bytes from ImagePath on server disk.
        /// </summary>
        private byte[] ResolveImageBinary(string? imagePath, byte[]? existingBinary)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return existingBinary ?? Array.Empty<byte>();
            }

            try
            {
                string trimmed = imagePath.Trim().Replace('\\', '/');
                if (trimmed.StartsWith("~/", StringComparison.Ordinal))
                {
                    trimmed = trimmed[2..];
                }

                var candidates = new List<string>();

                if (Path.IsPathRooted(trimmed))
                {
                    candidates.Add(trimmed);
                }
                else
                {
                    if (!string.IsNullOrEmpty(_env.WebRootPath))
                    {
                        candidates.Add(Path.Combine(_env.WebRootPath, trimmed.TrimStart('/')));
                    }

                    if (!string.IsNullOrEmpty(_env.ContentRootPath))
                    {
                        candidates.Add(Path.Combine(_env.ContentRootPath, trimmed.TrimStart('/')));
                    }
                }

                foreach (var fullPath in candidates.Distinct())
                {
                    if (File.Exists(fullPath))
                    {
                        return File.ReadAllBytes(fullPath);
                    }
                }

                // MVC Server.MapPath("/JewelleryPhotos/...") resolves under application root.
                if (trimmed.StartsWith("/JewelleryPhotos/", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrEmpty(_env.ContentRootPath))
                {
                    var mvcStylePath = Path.Combine(
                        _env.ContentRootPath,
                        "JewelleryPhotos",
                        trimmed["/JewelleryPhotos/".Length..].Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(mvcStylePath))
                    {
                        return File.ReadAllBytes(mvcStylePath);
                    }
                }
            }
            catch
            {
                // Fall through to existing/empty binary.
            }

            return existingBinary ?? Array.Empty<byte>();
        }
    }
}
