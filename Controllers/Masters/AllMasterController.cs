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
        private readonly IGradingService _gradingService;

        public AllMasterController(
            IAllMasterService masterService, 
            I4CMasterService fourCMasterService, 
            ILocationService locationService,
            IInvoiceService invoiceService,
            IDiamondOriginService originService,
            IJewelleryService jewelleryService,
            IGradingService gradingService)
        {
            _masterService = masterService;
            _4cmasterService = fourCMasterService;
            _locationService = locationService;
            _invoiceService = invoiceService;
            _originService = originService;
            _jewelleryService = jewelleryService;
            _gradingService = gradingService;
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

        //4C Master
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
                var (success, data) = await _4cmasterService.SaveShapeMasterAsync(model, GetUserTypeId(), GetUserId(), GetCompanyId());
                return Ok(new { Success = success, Data = data });
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

        #region Color Master

        [HttpGet("list-color")]
        public async Task<IActionResult> ColorMaster()
        {
            try
            {
                var result = await _4cmasterService.GetColorMasterListAsync("", GetCompanyId());
                return Ok(new{Success = true,Data = result});
            }
            catch (Exception ex){return Ok(new{Success = false,Data = ex.Message});}
        }

        [HttpGet("get-color/{id}")]
        public async Task<IActionResult> GetColor(int id)
        {
            try
            {
                var data = await _4cmasterService.GetColorMasterListAsync($" AND ID = {id}", GetCompanyId());
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

        [HttpPost("save-color")]
        public async Task<IActionResult> SaveColor([FromBody] ColorMaster model)
        {
            try
            {
                var (success, data) = await _4cmasterService.SaveColorMasterAsync(model, GetUserTypeId(), GetUserId(), GetCompanyId());
                return Ok(new { Success = success, Data = data });
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

        [HttpPost("delete-color/{id}")]
        public async Task<IActionResult> DeleteColor(int id)
        {
            try
            {
                bool result =
                    await _4cmasterService.DeleteColorAsync(id, GetUserTypeId(), GetUserId(), GetCompanyId());

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

        #region Size Master
        [HttpGet("list-Size")]
        public async Task<IActionResult> sizeMaster()
        {
            try
            {
                var result = await _4cmasterService.GetSizeMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex) { return Ok(new { Success = false, Data = ex.Message }); }
        }

        [HttpGet("get-Size/{id}")]
        public async Task<IActionResult> GetSize(int id)
        {
            try
            {
                var data = await _4cmasterService.GetSizeMasterListAsync($" AND ID = {id}", GetCompanyId());
                return Ok(new{Success = true,Data = data.FirstOrDefault()});
            }
            catch (Exception ex)
            {
                return Ok(new{Success = false,Data = ex.Message});
            }
        }

        [HttpPost("save-Size")]
        public async Task<IActionResult> SaveSize([FromBody] SizeMaster model)
        {
            try
            {
                var (success, data) = await _4cmasterService.SaveSizeMasterAsync(model, GetUserTypeId(), GetUserId(), GetCompanyId());
                return Ok(new { Success = success, Data = data });
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

        [HttpPost("delete-Size/{id}")]
        public async Task<IActionResult> DeleteSize(int id)
        {
            try
            {
                bool result =
                    await _4cmasterService.DeleteSizeAsync(id, GetUserTypeId(), GetUserId(), GetCompanyId());

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

        #region Clarity Master
        [HttpGet("list-clarity")]
        public async Task<IActionResult> ClarityMaster()
        {
            try
            {
                var result = await _4cmasterService.GetClarityMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex) { return Ok(new { Success = false, Data = ex.Message }); }
        }

        [HttpGet("get-clarity/{id}")]
        public async Task<IActionResult> GetClarity(int id)
        {
            try
            {
                var data = await _4cmasterService.GetClarityMasterListAsync($" AND ID = {id}", GetCompanyId());
                return Ok(new { Success = true, Data = data.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("save-clarity")]
        public async Task<IActionResult> SaveClarity([FromBody] ClarityMaster model)
        {
            try
            {
                var (success, data) = await _4cmasterService.SaveClarityMasterAsync(model, GetUserTypeId(), GetUserId(), GetCompanyId());
                return Ok(new { Success = success, Data = data });
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

        [HttpPost("delete-clarity/{id}")]
        public async Task<IActionResult> DeleteClarity(int id)
        {
            try
            {
                bool result =
                    await _4cmasterService.DeleteClarityAsync(id, GetUserTypeId(), GetUserId(), GetCompanyId());

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

        #region Sieve Master
        [HttpGet("list-sieve")]
        public async Task<IActionResult> SieveMaster()
        {
            try
            {
                var result = await _4cmasterService.GetSieveMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex) { return Ok(new { Success = false, Data = ex.Message }); }
        }

        [HttpGet("get-sieve/{id}")]
        public async Task<IActionResult> GetSieve(int id)
        {
            try
            {
                var data = await _4cmasterService.GetSieveMasterListAsync($" AND ID = {id}", GetCompanyId());
                return Ok(new { Success = true, Data = data.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("save-sieve")]
        public async Task<IActionResult> SaveSieve([FromBody] SieveMaster model)
        {
            try
            {
                var (success, data) = await _4cmasterService.SaveSieveMasterAsync(model, GetUserTypeId(), GetUserId(), GetCompanyId());
                return Ok(new { Success = success, Data = data });
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

        [HttpPost("delete-sieve/{id}")]
        public async Task<IActionResult> DeleteSieve(int id)
        {
            try
            {
                bool result =
                    await _4cmasterService.DeleteSieveAsync(id, GetUserTypeId(), GetUserId(), GetCompanyId());

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

        #region LAB Master
        [HttpGet("list-lab")]
        public async Task<IActionResult> LABMaster()
        {
            try
            {
                var result = await _4cmasterService.GetLABMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex) { return Ok(new { Success = false, Data = ex.Message }); }
        }

        [HttpGet("get-lab/{id}")]
        public async Task<IActionResult> GetLAB(int id)
        {
            try
            {
                var data = await _4cmasterService.GetLABMasterListAsync($" AND ID = {id}", GetCompanyId());
                return Ok(new { Success = true, Data = data.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("save-lab")]
        public async Task<IActionResult> SaveLAB([FromBody] LABMaster model)
        {
            try
            {
                var (success, data) = await _4cmasterService.SaveLABMasterAsync(model, GetUserTypeId(), GetUserId(), GetCompanyId());
                return Ok(new { Success = success, Data = data });
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

        [HttpPost("delete-lab/{id}")]
        public async Task<IActionResult> DeleteLAB(int id)
        {
            try
            {
                bool result =
                    await _4cmasterService.DeleteLABAsync(id, GetUserTypeId(), GetUserId(), GetCompanyId());

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

        #region Prop Master
        [HttpGet("list-prop")]
        public async Task<IActionResult> PropMaster()
        {
            try
            {
                var result = await _4cmasterService.GetPropMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex) { return Ok(new { Success = false, Data = ex.Message }); }
        }

        [HttpGet("get-prop/{id}")]
        public async Task<IActionResult> GetProp(int id)
        {
            try
            {
                var data = await _4cmasterService.GetPropMasterListAsync($" AND ID = {id}", GetCompanyId());
                return Ok(new { Success = true, Data = data.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("save-prop")]
        public async Task<IActionResult> SaveProp([FromBody] PropMaster model)
        {
            try
            {
                var (success, data) = await _4cmasterService.SavePropMasterAsync(model, GetUserTypeId(), GetUserId(), GetCompanyId());
                return Ok(new { Success = success, Data = data });
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

        [HttpPost("delete-prop/{id}")]
        public async Task<IActionResult> DeleteProp(int id)
        {
            try
            {
                bool result =
                    await _4cmasterService.DeletePropAsync(id, GetUserTypeId(), GetUserId(), GetCompanyId());

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

        #region Polish Master
        [HttpGet("list-polish")]
        public async Task<IActionResult> PolishMaster()
        {
            try
            {
                var result = await _4cmasterService.GetPolishMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex) { return Ok(new { Success = false, Data = ex.Message }); }
        }

        [HttpGet("get-polish/{id}")]
        public async Task<IActionResult> GetPolish(int id)
        {
            try
            {
                var data = await _4cmasterService.GetPolishMasterListAsync($" AND ID = {id}", GetCompanyId());
                return Ok(new { Success = true, Data = data.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("save-polish")]
        public async Task<IActionResult> SavePolish([FromBody] PolishMaster model)
        {
            try
            {
                var (success, data) = await _4cmasterService.SavePolishMasterAsync(model, GetUserTypeId(), GetUserId(), GetCompanyId());
                return Ok(new { Success = success, Data = data });
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

        [HttpPost("delete-polish/{id}")]
        public async Task<IActionResult> DeletePolish(int id)
        {
            try
            {
                bool result =
                    await _4cmasterService.DeletePolishAsync(id, GetUserTypeId(), GetUserId(), GetCompanyId());

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

        #region Sym Master
        [HttpGet("list-sym")]
        public async Task<IActionResult> SymMaster()
        {
            try
            {
                var result = await _4cmasterService.GetSymMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex) { return Ok(new { Success = false, Data = ex.Message }); }
        }

        [HttpGet("get-sym/{id}")]
        public async Task<IActionResult> GetSym(int id)
        {
            try
            {
                var data = await _4cmasterService.GetSymMasterListAsync($" AND ID = {id}", GetCompanyId());
                return Ok(new { Success = true, Data = data.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("save-sym")]
        public async Task<IActionResult> SaveSym([FromBody] SymMaster model)
        {
            try
            {
                var (success, data) = await _4cmasterService.SaveSymMasterAsync(model, GetUserTypeId(), GetUserId(), GetCompanyId());
                return Ok(new { Success = success, Data = data });
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

        [HttpPost("delete-sym/{id}")]
        public async Task<IActionResult> DeleteSym(int id)
        {
            try
            {
                bool result =
                    await _4cmasterService.DeleteSymAsync(id, GetUserTypeId(), GetUserId(), GetCompanyId());

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

        #region FL Master
        [HttpGet("list-fl")]
        public async Task<IActionResult> FLMaster()
        {
            try
            {
                var result = await _4cmasterService.GetFLMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex) { return Ok(new { Success = false, Data = ex.Message }); }
        }

        [HttpGet("get-fl/{id}")]
        public async Task<IActionResult> GetFL(int id)
        {
            try
            {
                var data = await _4cmasterService.GetFLMasterListAsync($" AND ID = {id}", GetCompanyId());
                return Ok(new { Success = true, Data = data.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("save-fl")]
        public async Task<IActionResult> SaveFL([FromBody] FLMaster model)
        {
            try
            {
                var (success, data) = await _4cmasterService.SaveFLMasterAsync(model, GetUserTypeId(), GetUserId(), GetCompanyId());
                return Ok(new { Success = success, Data = data });
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

        [HttpPost("delete-fl/{id}")]
        public async Task<IActionResult> DeleteFL(int id)
        {
            try
            {
                bool result =
                    await _4cmasterService.DeleteFLAsync(id, GetUserTypeId(), GetUserId(), GetCompanyId());

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

        #region FLColor Master
        [HttpGet("list-flcolor")]
        public async Task<IActionResult> FLColorMaster()
        {
            try
            {
                var result = await _4cmasterService.GetFLColorMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex) { return Ok(new { Success = false, Data = ex.Message }); }
        }

        [HttpGet("get-flcolor/{id}")]
        public async Task<IActionResult> GetFLColor(int id)
        {
            try
            {
                var data = await _4cmasterService.GetFLColorMasterListAsync($" AND ID = {id}", GetCompanyId());
                return Ok(new { Success = true, Data = data.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("save-flcolor")]
        public async Task<IActionResult> SaveFLColor([FromBody] FLColorMaster model)
        {
            try
            {
                var (success, data) = await _4cmasterService.SaveFLColorMasterAsync(model, GetUserTypeId(), GetUserId(), GetCompanyId());
                return Ok(new { Success = success, Data = data });
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

        [HttpPost("delete-flcolor/{id}")]
        public async Task<IActionResult> DeleteFLColor(int id)
        {
            try
            {
                bool result =
                    await _4cmasterService.DeleteFLColorAsync(id, GetUserTypeId(), GetUserId(), GetCompanyId());

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

        //Location Master
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

        //Invoice Master
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

        //Origin Master
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

        //Grading Master
        #region Brown Master
        [HttpGet("list-brown")]
        public async Task<IActionResult> BrownMaster()
        {
            try
            {
                var result = await _gradingService.GetBrownMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex) { return Ok(new { Success = false, Data = ex.Message }); }
        }

        [HttpGet("get-brown/{id}")]
        public async Task<IActionResult> GetBrown(int id)
        {
            try
            {
                var data = await _gradingService.GetBrownMasterListAsync($" AND ID = {id}", GetCompanyId());
                return Ok(new { Success = true, Data = data.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("save-brown")]
        public async Task<IActionResult> SaveBrown([FromBody] BrownMaster model)
        {
            try
            {
                var (success, data) = await _gradingService.SaveBrownMasterAsync(model, GetUserTypeId(), GetUserId(), GetCompanyId());
                return Ok(new { Success = success, Data = data });
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

        [HttpPost("delete-brown/{id}")]
        public async Task<IActionResult> DeleteBrown(int id)
        {
            try
            {
                bool result =
                    await _gradingService.DeleteBrownAsync(id, GetUserTypeId(), GetUserId(), GetCompanyId());

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

        #region Green Master
        [HttpGet("list-green")]
        public async Task<IActionResult> GreenMaster()
        {
            try
            {
                var result = await _gradingService.GetGreenMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex) { return Ok(new { Success = false, Data = ex.Message }); }
        }

        [HttpGet("get-green/{id}")]
        public async Task<IActionResult> GetGreen(int id)
        {
            try
            {
                var data = await _gradingService.GetGreenMasterListAsync($" AND ID = {id}", GetCompanyId());
                return Ok(new { Success = true, Data = data.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("save-green")]
        public async Task<IActionResult> SaveGreen([FromBody] GreenMaster model)
        {
            try
            {
                var (success, data) = await _gradingService.SaveGreenMasterAsync(model, GetUserTypeId(), GetUserId(), GetCompanyId());
                return Ok(new { Success = success, Data = data });
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

        [HttpPost("delete-green/{id}")]
        public async Task<IActionResult> DeleteGreen(int id)
        {
            try
            {
                bool result =
                    await _gradingService.DeleteGreenAsync(id, GetUserTypeId(), GetUserId(), GetCompanyId());

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

        #region Milky Master
        [HttpGet("list-milky")]
        public async Task<IActionResult> MilkyMaster()
        {
            try
            {
                var result = await _gradingService.GetMilkyMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex) { return Ok(new { Success = false, Data = ex.Message }); }
        }

        [HttpGet("get-milky/{id}")]
        public async Task<IActionResult> GetMilky(int id)
        {
            try
            {
                var data = await _gradingService.GetMilkyMasterListAsync($" AND ID = {id}", GetCompanyId());
                return Ok(new { Success = true, Data = data.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("save-milky")]
        public async Task<IActionResult> SaveMilky([FromBody] MilkyMaster model)
        {
            try
            {
                var (success, data) = await _gradingService.SaveMilkyMasterAsync(model, GetUserTypeId(), GetUserId(), GetCompanyId());
                return Ok(new { Success = success, Data = data });
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

        [HttpPost("delete-milky/{id}")]
        public async Task<IActionResult> DeleteMilky(int id)
        {
            try
            {
                bool result =
                    await _gradingService.DeleteMilkyAsync(id, GetUserTypeId(), GetUserId(), GetCompanyId());

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

        #region Luster Master
        [HttpGet("list-luster")]
        public async Task<IActionResult> LusterMaster()
        {
            try
            {
                var result = await _gradingService.GetLusterMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex) { return Ok(new { Success = false, Data = ex.Message }); }
        }

        [HttpGet("get-luster/{id}")]
        public async Task<IActionResult> GetLuster(int id)
        {
            try
            {
                var data = await _gradingService.GetLusterMasterListAsync($" AND ID = {id}", GetCompanyId());
                return Ok(new { Success = true, Data = data.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("save-luster")]
        public async Task<IActionResult> SaveLuster([FromBody] LusterMaster model)
        {
            try
            {
                var (success, data) = await _gradingService.SaveLusterMasterAsync(model, GetUserTypeId(), GetUserId(), GetCompanyId());
                return Ok(new { Success = success, Data = data });
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

        [HttpPost("delete-luster/{id}")]
        public async Task<IActionResult> DeleteLuster(int id)
        {
            try
            {
                bool result =
                    await _gradingService.DeleteLusterAsync(id, GetUserTypeId(), GetUserId(), GetCompanyId());

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

        #region Natts Master
        [HttpGet("list-natts")]
        public async Task<IActionResult> NattsMaster()
        {
            try
            {
                var result = await _gradingService.GetNattsMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex) { return Ok(new { Success = false, Data = ex.Message }); }
        }

        [HttpGet("get-natts/{id}")]
        public async Task<IActionResult> GetNatts(int id)
        {
            try
            {
                var data = await _gradingService.GetNattsMasterListAsync($" AND ID = {id}", GetCompanyId());
                return Ok(new { Success = true, Data = data.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("save-natts")]
        public async Task<IActionResult> SaveNatts([FromBody] NattsMaster model)
        {
            try
            {
                var (success, data) = await _gradingService.SaveNattsMasterAsync(model, GetUserTypeId(), GetUserId(), GetCompanyId());
                return Ok(new { Success = success, Data = data });
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

        [HttpPost("delete-natts/{id}")]
        public async Task<IActionResult> DeleteNatts(int id)
        {
            try
            {
                bool result =
                    await _gradingService.DeleteNattsAsync(id, GetUserTypeId(), GetUserId(), GetCompanyId());

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

        #region HA Master
        [HttpGet("list-ha")]
        public async Task<IActionResult> HAMaster()
        {
            try
            {
                var result = await _gradingService.GetHAMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex) { return Ok(new { Success = false, Data = ex.Message }); }
        }

        [HttpGet("get-ha/{id}")]
        public async Task<IActionResult> GetHA(int id)
        {
            try
            {
                var data = await _gradingService.GetHAMasterListAsync($" AND ID = {id}", GetCompanyId());
                return Ok(new { Success = true, Data = data.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("save-ha")]
        public async Task<IActionResult> SaveHA([FromBody] HAMaster model)
        {
            try
            {
                var (success, data) = await _gradingService.SaveHAMasterAsync(model, GetUserTypeId(), GetUserId(), GetCompanyId());
                return Ok(new { Success = success, Data = data });
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

        [HttpPost("delete-ha/{id}")]
        public async Task<IActionResult> DeleteHA(int id)
        {
            try
            {
                bool result =
                    await _gradingService.DeleteHAAsync(id, GetUserTypeId(), GetUserId(), GetCompanyId());

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

        #region Shade Master
        [HttpGet("list-shade")]
        public async Task<IActionResult> ShadeMaster()
        {
            try
            {
                var result = await _gradingService.GetShadeMasterListAsync("", GetCompanyId());
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex) { return Ok(new { Success = false, Data = ex.Message }); }
        }

        [HttpGet("get-shade/{id}")]
        public async Task<IActionResult> GetShade(int id)
        {
            try
            {
                var data = await _gradingService.GetShadeMasterListAsync($" AND ID = {id}", GetCompanyId());
                return Ok(new { Success = true, Data = data.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Data = ex.Message });
            }
        }

        [HttpPost("save-shade")]
        public async Task<IActionResult> SaveShade([FromBody] ShadeMaster model)
        {
            try
            {
                var (success, data) = await _gradingService.SaveShadeMasterAsync(model, GetUserTypeId(), GetUserId(), GetCompanyId());
                return Ok(new { Success = success, Data = data });
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

        [HttpPost("delete-shade/{id}")]
        public async Task<IActionResult> DeleteShade(int id)
        {
            try
            {
                bool result =
                    await _gradingService.DeleteShadeAsync(id, GetUserTypeId(), GetUserId(), GetCompanyId());

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

        //Jewellery Master
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
