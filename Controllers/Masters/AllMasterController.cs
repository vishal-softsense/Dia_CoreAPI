using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Services.Interfaces.Masters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.Masters
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AllMasterController : ControllerBase
    {
        private readonly IAllMasterService _masterService;
        private readonly I4CMasterService _4cmasterService;
        private readonly ILocationService _locationService;
        private readonly IInvoiceService _invoiceService;
        private readonly IDiamondOriginService _originService;
        private readonly IJewelleryService _jewelleryService;

        public AllMasterController(
            IAllMasterService masterService, 
            I4CMasterService fourCMasterService, 
            ILocationService locationService,
            IInvoiceService invoiceService,
            IDiamondOriginService originService,
            IJewelleryService jewelleryService)
        {
            _masterService = masterService;
            _4cmasterService = fourCMasterService;
            _locationService = locationService;
            _invoiceService = invoiceService;
            _originService = originService;
            _jewelleryService = jewelleryService;
        }

        private string GetUserId() =>  HttpContext.User.FindFirst("UserID")?.Value ?? "0";
        private string GetUserTypeId() => HttpContext.User.FindFirst("UserTypeId")?.Value ?? "0";
        private string GetCompanyId() => HttpContext.User.FindFirst("CompanyID")?.Value ?? "0";

        #region All Master
        [HttpGet("all-master-list")]
        public async Task<IActionResult> GetAllMasterList()
        {
            try
            {
                var result =
                    await _masterService.GetAllMasterListAsync(GetUserTypeId(), GetCompanyId());
                
                return Ok(new
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Success = false,
                    Data = ex.Message
                });
            }
        }
        #endregion

        //4CMaster
        #region Shape Master

        [HttpGet("list-shape")]
        public async Task<IActionResult> ShapeMaster()
        {
            try
            {
                var result =
                    await _4cmasterService.GetShapeMasterListAsync("",GetCompanyId());

                return Ok(new
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Success = false,
                    Data = ex.Message
                });
            }
        }

        [HttpGet("get-shape/{id}")]
        public async Task<IActionResult> GetShape(int id)
        {
            try
            {
                var data = await _4cmasterService.GetShapeMasterListAsync($" AND ID = {id}",GetCompanyId());
                return Ok(new
                {
                    Success = true,
                    Data = data.FirstOrDefault()
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Success = false,
                    Data = ex.Message
                });
            }
        }

        [HttpPost("save-shape")]
        public async Task<IActionResult> SaveShape([FromBody] ShapeMaster model)
        {
            try
            {
                if (model.ID == 0)
                {
                    int permission =
                        await _4cmasterService.CheckUserPermissionAsync(GetUserTypeId(),"Shape Master", "insert", GetCompanyId());

                    if (permission == 0)
                    {
                        return Ok(new
                        {
                            Success = false,
                            Data = "You are not authorized to perform this task."
                        });
                    }

                    var duplicateData =
                        await _4cmasterService.GetShapeMasterListAsync($" AND ShapeID = '{model.ShapeID}' ", GetCompanyId());

                    if (duplicateData.Count > 0)
                    {
                        return Ok(new
                        {
                            Success = false,
                            Data = "Record already available with shape id - " + model.ShapeID
                        });
                    }

                    bool result =
                        await _4cmasterService.SaveShapeMasterAsync(model,GetUserId(),GetCompanyId(),"insert");

                    return Ok(new
                    {
                        Success = result,
                        Data = "Record saved successfully."
                    });
                }
                else
                {
                    int permission =
                        await _4cmasterService.CheckUserPermissionAsync(GetUserTypeId(), "Shape Master", "edit", GetCompanyId());

                    if (permission == 0)
                    {
                        return Ok(new
                        {
                            Success = false,
                            Data = "You are not authorized to perform this task."
                        });
                    }

                    var duplicateData =
                        await _4cmasterService.GetShapeMasterListAsync($" AND ShapeID = '{model.ShapeID}' AND ID <> {model.ID}", GetCompanyId());

                    if (duplicateData.Count > 0)
                    {
                        return Ok(new
                        {
                            Success = false,
                            Data = "Sorry.! You can not update record because shape id already exists."
                        });
                    }

                    bool result =
                        await _4cmasterService.SaveShapeMasterAsync(model,GetUserId(),GetCompanyId(),"update");

                    return Ok(new
                    {
                        Success = result,
                        Data = "Record updated successfully."
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Success = false,
                    Data = ex.Message
                });
            }
        }

        [HttpPost("delete-shape/{id}")]
        public async Task<IActionResult> DeleteShape(int id)
        {
            try
            {
                bool result =
                    await _4cmasterService.DeleteShapeAsync(id, GetUserTypeId(), GetUserId(), GetCompanyId());

                return Ok(new
                {
                    Success = result,
                    Data = result
                        ? "Records Deleted."
                        : "Record is in use, You can not delete record."
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Success = false,
                    Data = ex.Message
                });
            }
        }

        #endregion

        //Location
        #region Area Master
        [HttpGet("list-area")]
        public async Task<IActionResult> AreaMaster()
        {
            try
            {
                var result = await _locationService.GetAreaMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpGet("get-area/{id}")]
        public async Task<IActionResult> GetArea(int id)
        {
            try
            {
                var data = await _locationService.GetAreaMasterListAsync($" AND ID = {id}", GetCompanyId());
                return Ok(new { Success = true, Data = data.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("save-area")]
        public async Task<IActionResult> SaveArea([FromBody] AreaMaster model)
        {
            try
            {
                var actionType = model.ID == 0 ? "insert" : "update";
                var (success, message) = await _locationService.SaveAreaMasterAsync(
                    model,
                    GetUserId(),
                    GetUserTypeId(),
                    GetCompanyId(),
                    actionType);

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("delete-area/{id}")]
        public async Task<IActionResult> DeleteArea(int id)
        {
            try
            {
                var (success, message) = await _locationService.DeleteAreaAsync(
                    id,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }
        #endregion

        #region City Master
        [HttpGet("list-city")]
        public async Task<IActionResult> CityMaster()
        {
            try
            {
                var result = await _locationService.GetCityMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpGet("get-city/{id}")]
        public async Task<IActionResult> GetCity(int id)
        {
            try
            {
                var data = await _locationService.GetCityMasterListAsync($" AND ID = {id}", GetCompanyId());
                return Ok(new { Success = true, Data = data.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("save-city")]
        public async Task<IActionResult> SaveCity([FromBody] CityMaster model)
        {
            try
            {
                var actionType = model.ID == 0 ? "insert" : "update";
                var (success, message) = await _locationService.SaveCityMasterAsync(
                    model,
                    GetUserId(),
                    GetUserTypeId(),
                    GetCompanyId(),
                    actionType);

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("delete-city/{id}")]
        public async Task<IActionResult> DeleteCity(int id)
        {
            try
            {
                var (success, message) = await _locationService.DeleteCityAsync(
                    id,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }
        #endregion

        #region State Master
        [HttpGet("list-state")]
        public async Task<IActionResult> StateMaster()
        {
            try
            {
                var result = await _locationService.GetStateMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpGet("get-state/{id}")]
        public async Task<IActionResult> GetState(int id)
        {
            try
            {
                var data = await _locationService.GetStateMasterListAsync($" AND ID = {id}", GetCompanyId());
                return Ok(new { Success = true, Data = data.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("save-state")]
        public async Task<IActionResult> SaveState([FromBody] StateMaster model)
        {
            try
            {
                var actionType = model.ID == 0 ? "insert" : "update";
                var (success, message) = await _locationService.SaveStateMasterAsync(
                    model,
                    GetUserId(),
                    GetUserTypeId(),
                    GetCompanyId(),
                    actionType);

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("delete-state/{id}")]
        public async Task<IActionResult> DeleteState(int id)
        {
            try
            {
                var (success, message) = await _locationService.DeleteStateAsync(
                    id,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }
        #endregion

        #region Country Master
        [HttpGet("list-country")]
        public async Task<IActionResult> CountryMaster()
        {
            try
            {
                var result = await _locationService.GetCountryMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpGet("get-country/{id}")]
        public async Task<IActionResult> GetCountry(int id)
        {
            try
            {
                var data = await _locationService.GetCountryMasterListAsync($" AND ID = {id}", GetCompanyId());
                return Ok(new { Success = true, Data = data.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("save-country")]
        public async Task<IActionResult> SaveCountry([FromBody] CountryMaster model)
        {
            try
            {
                var actionType = model.ID == 0 ? "insert" : "update";
                var (success, message) = await _locationService.SaveCountryMasterAsync(
                    model,
                    GetUserId(),
                    GetUserTypeId(),
                    GetCompanyId(),
                    actionType);

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("delete-country/{id}")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            try
            {
                var (success, message) = await _locationService.DeleteCountryAsync(
                    id,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }
        #endregion

        //Invoice
        #region Term Master
        [HttpGet("list-term")]
        public async Task<IActionResult> GetTermList()
        {
            try
            {
                var result = await _invoiceService.GetTermMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpGet("get-term/{id}")]
        public async Task<IActionResult> GetTerm(int id)
        {
            try
            {
                var result = await _invoiceService.GetTermByIdAsync(id, GetCompanyId());
                if (result != null)
                    return Ok(new { Success = true, Data = result });
                else
                    return Ok(new { Success = false, Data = "Selected record not found." });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("submit-term")]
        public async Task<IActionResult> SubmitTerm([FromBody] TermMaster model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("<br>", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return Ok(new { Success = false, Data = errors });
            }

            try
            {
                var (success, message) = await _invoiceService.SubmitTermDataAsync(
                    model,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("delete-term/{id}")]
        public async Task<IActionResult> DeleteTerm(int id)
        {
            try
            {
                var (success, message) = await _invoiceService.DeleteTermAsync(
                    id,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }
        #endregion

        #region Type Master
        [HttpGet("list-type")]
        public async Task<IActionResult> GetTypeList()
        {
            try
            {
                var result = await _invoiceService.GetTypeMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpGet("get-type/{id}")]
        public async Task<IActionResult> GetType(int id)
        {
            try
            {
                var result = await _invoiceService.GetTypeByIdAsync(id, GetCompanyId());
                if (result != null)
                    return Ok(new { Success = true, Data = result });
                else
                    return Ok(new { Success = false, Data = "Selected record not found." });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("submit-type")]
        public async Task<IActionResult> SubmitType([FromBody] TypeMaster model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("<br>", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return Ok(new { Success = false, Data = errors });
            }

            try
            {
                var (success, message) = await _invoiceService.SubmitTypeDataAsync(
                    model,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("delete-type/{id}")]
        public async Task<IActionResult> DeleteType(int id)
        {
            try
            {
                var (success, message) = await _invoiceService.DeleteTypeAsync(
                    id,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }
        #endregion

        #region SubType Master
        [HttpGet("list-subtype")]
        public async Task<IActionResult> GetSubTypeList()
        {
            try
            {
                var result = await _invoiceService.GetSubTypeMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpGet("get-subtype/{id}")]
        public async Task<IActionResult> GetSubType(int id)
        {
            try
            {
                var result = await _invoiceService.GetSubTypeByIdAsync(id, GetCompanyId());
                if (result != null)
                    return Ok(new { Success = true, Data = result });
                else
                    return Ok(new { Success = false, Data = "Selected record not found." });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("submit-subtype")]
        public async Task<IActionResult> SubmitSubType([FromBody] SubTypeMaster model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("<br>", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return Ok(new { Success = false, Data = errors });
            }

            try
            {
                var (success, message) = await _invoiceService.SubmitSubTypeDataAsync(
                    model,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("delete-subtype/{id}")]
        public async Task<IActionResult> DeleteSubType(int id)
        {
            try
            {
                var (success, message) = await _invoiceService.DeleteSubTypeAsync(
                    id,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }
        #endregion

        #region Currency Master
        [HttpGet("list-currency")]
        public async Task<IActionResult> GetCurrencyList()
        {
            try
            {
                var result = await _invoiceService.GetCurrencyMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpGet("get-currency/{id}")]
        public async Task<IActionResult> GetCurrency(int id)
        {
            try
            {
                var result = await _invoiceService.GetCurrencyByIdAsync(id, GetCompanyId());
                if (result != null)
                    return Ok(new { Success = true, Data = result });
                else
                    return Ok(new { Success = false, Data = "Selected record not found." });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }
        #endregion

        //Origin
        #region Mines Master
        [HttpGet("list-mines")]
        public async Task<IActionResult> GetMinesList()
        {
            try
            {
                var result = await _originService.GetMinesMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpGet("get-mines/{id}")]
        public async Task<IActionResult> GetMines(int id)
        {
            try
            {
                var result = await _originService.GetMinesByIdAsync(id, GetCompanyId());
                if (result != null)
                    return Ok(new { Success = true, Data = result });
                else
                    return Ok(new { Success = false, Data = "Selected record not found." });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("submit-mines")]
        public async Task<IActionResult> SubmitMines([FromBody] MinesMaster model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("<br>", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return Ok(new { Success = false, Data = errors });
            }

            try
            {
                var (success, message) = await _originService.SubmitMinesDataAsync(
                    model,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("delete-mines/{id}")]
        public async Task<IActionResult> DeleteMines(int id)
        {
            try
            {
                var (success, message) = await _originService.DeleteMinesAsync(
                    id,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }
        #endregion

        #region Rough Master
        [HttpGet("list-rough")]
        public async Task<IActionResult> GetRoughList()
        {
            try
            {
                var result = await _originService.GetRoughMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpGet("get-rough/{id}")]
        public async Task<IActionResult> GetRough(int id)
        {
            try
            {
                var result = await _originService.GetRoughByIdAsync(id, GetCompanyId());
                if (result != null)
                    return Ok(new { Success = true, Data = result });
                else
                    return Ok(new { Success = false, Data = "Selected record not found." });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("submit-rough")]
        public async Task<IActionResult> SubmitRough([FromBody] RoughMaster model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("<br>", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return Ok(new { Success = false, Data = errors });
            }

            try
            {
                var (success, message) = await _originService.SubmitRoughDataAsync(
                    model,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("delete-rough/{id}")]
        public async Task<IActionResult> DeleteRough(int id)
        {
            try
            {
                var (success, message) = await _originService.DeleteRoughAsync(
                    id,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }
        #endregion

        //Jewellery
        #region Jewellery Category Master
        [HttpGet("list-jew-category")]
        public async Task<IActionResult> GetJewCategoryList()
        {
            try
            {
                var result = await _jewelleryService.GetJewCategoryMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpGet("get-jew-category/{id}")]
        public async Task<IActionResult> GetJewCategory(int id)
        {
            try
            {
                var result = await _jewelleryService.GetJewCategoryByIdAsync(id, GetCompanyId());
                if (result != null)
                    return Ok(new { Success = true, Data = result });
                else
                    return Ok(new { Success = false, Data = "Selected record not found." });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("submit-jew-category")]
        public async Task<IActionResult> SubmitJewCategory([FromBody] JewCategoryMaster model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("<br>", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return Ok(new { Success = false, Data = errors });
            }

            try
            {
                var (success, message) = await _jewelleryService.SubmitJewCategoryDataAsync(
                    model,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("delete-jew-category/{id}")]
        public async Task<IActionResult> DeleteJewCategory(int id)
        {
            try
            {
                var (success, message) = await _jewelleryService.DeleteJewCategoryAsync(
                    id,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }
        #endregion

        #region Jewellery SubCategory Master
        [HttpGet("list-jew-subcategory")]
        public async Task<IActionResult> GetJewSubCategoryList()
        {
            try
            {
                var result = await _jewelleryService.GetJewSubCategoryMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpGet("get-jew-subcategory/{id}")]
        public async Task<IActionResult> GetJewSubCategory(int id)
        {
            try
            {
                var result = await _jewelleryService.GetJewSubCategoryByIdAsync(id, GetCompanyId());
                if (result != null)
                    return Ok(new { Success = true, Data = result });
                else
                    return Ok(new { Success = false, Data = "Selected record not found." });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("submit-jew-subcategory")]
        public async Task<IActionResult> SubmitJewSubCategory([FromBody] JewSubCategoryMaster model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("<br>", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return Ok(new { Success = false, Data = errors });
            }

            try
            {
                var (success, message) = await _jewelleryService.SubmitJewSubCategoryDataAsync(
                    model,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("delete-jew-subcategory/{id}")]
        public async Task<IActionResult> DeleteJewSubCategory(int id)
        {
            try
            {
                var (success, message) = await _jewelleryService.DeleteJewSubCategoryAsync(
                    id,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }
        #endregion

        #region Jewellery Metal Master
        [HttpGet("list-jew-metal")]
        public async Task<IActionResult> GetJewMetalList([FromQuery] string? metalType = null)
        {
            try
            {
                if (metalType != null)
                {
                    var result = await _jewelleryService.GetPurityDataAsync($"and Metal=''{metalType}''", GetCompanyId());
                    return Ok(new { Success = true, Data = result });
                }
                else
                {
                    var result = await _jewelleryService.GetJewMetalMasterListAsync("", GetCompanyId());
                    return Ok(new { Success = true, Data = result });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpGet("get-jew-metal/{id}")]
        public async Task<IActionResult> GetJewMetal(int id)
        {
            try
            {
                var result = await _jewelleryService.GetJewMetalByIdAsync(id, GetCompanyId());
                if (result != null)
                    return Ok(new { Success = true, Data = result });
                else
                    return Ok(new { Success = false, Data = "Selected record not found." });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("submit-jew-metal")]
        public async Task<IActionResult> SubmitJewMetal([FromBody] JewMetalMaster model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("<br>", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return Ok(new { Success = false, Data = errors });
            }

            try
            {
                var (success, message) = await _jewelleryService.SubmitJewMetalDataAsync(
                    model,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("delete-jew-metal/{id}")]
        public async Task<IActionResult> DeleteJewMetal(int id)
        {
            try
            {
                var (success, message) = await _jewelleryService.DeleteJewMetalAsync(
                    id,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }
        #endregion

        #region Jewellery Metal Purity Master
        [HttpGet("list-jew-metal-purity")]
        public async Task<IActionResult> GetJewMetalPurityList()
        {
            try
            {
                var result = await _jewelleryService.GetJewMetalPurityMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpGet("get-jew-metal-purity/{id}")]
        public async Task<IActionResult> GetJewMetalPurity(int id)
        {
            try
            {
                var result = await _jewelleryService.GetJewMetalPurityByIdAsync(id, GetCompanyId());
                if (result != null)
                    return Ok(new { Success = true, Data = result });
                else
                    return Ok(new { Success = false, Data = "Selected record not found." });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("submit-jew-metal-purity")]
        public async Task<IActionResult> SubmitJewMetalPurity([FromBody] JewMetalPurityMaster model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("<br>", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return Ok(new { Success = false, Data = errors });
            }

            try
            {
                var (success, message) = await _jewelleryService.SubmitJewMetalPurityDataAsync(
                    model,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("delete-jew-metal-priority/{id}")]
        public async Task<IActionResult> DeleteJewMetalPurity(int id)
        {
            try
            {
                var (success, message) = await _jewelleryService.DeleteJewMetalPurityAsync(
                    id,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }
        #endregion

        #region Jewellery Color Master
        [HttpGet("list-jew-color")]
        public async Task<IActionResult> GetJewColorList()
        {
            try
            {
                var result = await _jewelleryService.GetJewColorMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpGet("get-jew-color/{id}")]
        public async Task<IActionResult> GetJewColor(int id)
        {
            try
            {
                var result = await _jewelleryService.GetJewColorByIdAsync(id, GetCompanyId());
                if (result != null)
                    return Ok(new { Success = true, Data = result });
                else
                    return Ok(new { Success = false, Data = "Selected record not found." });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("submit-jew-color")]
        public async Task<IActionResult> SubmitJewColor([FromBody] JewColorMaster model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("<br>", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return Ok(new { Success = false, Data = errors });
            }

            try
            {
                var (success, message) = await _jewelleryService.SubmitJewColorDataAsync(
                    model,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("delete-jew-color/{id}")]
        public async Task<IActionResult> DeleteJewColor(int id)
        {
            try
            {
                var (success, message) = await _jewelleryService.DeleteJewColorAsync(
                    id,
                    GetUserTypeId(),
                    GetUserId(),
                    GetCompanyId());

                return Ok(new { Success = success, Data = message });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }
        #endregion
    }
}