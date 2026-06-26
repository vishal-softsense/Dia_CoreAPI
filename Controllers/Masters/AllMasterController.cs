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
        private readonly IGradingService _gradingService;

        public AllMasterController(IAllMasterService masterService, I4CMasterService fourCMasterService, IGradingService gradingService)
        {
            _masterService = masterService;
            _4cmasterService = fourCMasterService;
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


        // Grading Master
        #region Grading master

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

        #endregion
    }
}