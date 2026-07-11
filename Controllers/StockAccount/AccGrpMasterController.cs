using System;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Services.Interfaces.StockAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.StockAccount
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccGrpMasterController : ControllerBase
    {
        private readonly IAccGrpMasterService _service;

        public AccGrpMasterController(IAccGrpMasterService service)
        {
            _service = service;
        }

        private string GetCompanyId()
        {
            return User.FindFirst("CompanyID")?.Value ?? throw new UnauthorizedAccessException("CompanyID missing from token.");
        }

        //[HttpGet("list")]
        //public async Task<IActionResult> GetList([FromQuery] string? filter)
        //{
        //    try
        //    {
        //        var companyId = GetCompanyId();
        //        string whereClause = "";
        //        if (!string.IsNullOrEmpty(filter))
        //        {
        //            whereClause += $" AND (TAG.AccGrpName LIKE '%{filter}%' OR TAG.SName LIKE '%{filter}%' OR TAG.UnderGroupName LIKE '%{filter}%')";
        //        }
        //        var list = await _service.GetAccGrpMasterListAsync(companyId, whereClause);
        //        return Ok(new { Success = true, Data = list });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { Success = false, Message = ex.Message });
        //    }
        //}

        [HttpGet("list")]
        public async Task<IActionResult> GetList([FromQuery] string? filter)
        {
            try
            {
                var companyId = GetCompanyId();
                string whereClause = "";

                if (!string.IsNullOrEmpty(filter))
                {
                    whereClause += $" AND (TAG.AccGrpName LIKE '%{filter}%' OR TAG.SName LIKE '%{filter}%')";
                }

                var result = await _service.GetAccGrpMasterListAsync(companyId, whereClause);

                List<AccGrpMasterListDto> data = new List<AccGrpMasterListDto>();

                foreach (var row in result)
                {
                    // Skip duplicate AccGrpCode
                    if (!data.Any(x => x.AccGrpCode == row.AccGrpCode))
                    {
                        data.Add(new AccGrpMasterListDto
                        {
                            AccGrpCode = row.AccGrpCode,
                            AccGrpName = row.AccGrpName,
                            Currency = row.Currency,
                            ODrCr = row.ODrCr,
                            UnderGroupName = row.UnderGroupName,
                            AccountType = row.AccountType,
                            OBalance = row.OBalance,
                            Country = row.Country
                        });
                    }
                }

                return Ok(new
                {
                    Success = true,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }


        [HttpGet("dropdowns")]
        public async Task<IActionResult> GetDropdowns()
        {
            try
            {
                var companyId = GetCompanyId();
                var list = await _service.GetAccGrpMasterDropdownAsync(companyId);
                var defaultCurrencyCode = await _service.GetDefaultCurrencyCodeAsync(companyId);

                var underGroup = list.Where(x => x.Type.Equals("UnderGroup", StringComparison.OrdinalIgnoreCase)).ToList();
                var accType = list.Where(x => x.Type.Equals("AccType", StringComparison.OrdinalIgnoreCase)).ToList();
                var currency = list.Where(x => x.Type.Equals("Currency", StringComparison.OrdinalIgnoreCase)).ToList();
                var area = list.Where(x => x.Type.Equals("Area", StringComparison.OrdinalIgnoreCase)).ToList();
                var city = list.Where(x => x.Type.Equals("City", StringComparison.OrdinalIgnoreCase)).ToList();
                var state = list.Where(x => x.Type.Equals("State", StringComparison.OrdinalIgnoreCase)).ToList();
                var country = list.Where(x => x.Type.Equals("Country", StringComparison.OrdinalIgnoreCase)).ToList();
                var term = list.Where(x => x.Type.Equals("Term", StringComparison.OrdinalIgnoreCase)).ToList();
                var through = list.Where(x => x.Type.Equals("Through", StringComparison.OrdinalIgnoreCase)).ToList();
                var broker = list.Where(x => x.Type.Equals("Broker", StringComparison.OrdinalIgnoreCase)).ToList();
                var agent = list.Where(x => x.Type.Equals("Agent", StringComparison.OrdinalIgnoreCase)).ToList();
                var accEffCode = list.Where(x => x.Type.Equals("AccEffCode", StringComparison.OrdinalIgnoreCase)).ToList();

                return Ok(new
                {
                    Success = true,
                    Data = new
                    {
                        UnderGroup = underGroup,
                        AccType = accType,
                        Currency = currency,
                        Area = area,
                        City = city,
                        State = state,
                        Country = country,
                        Term = term,
                        Through = through,
                        Broker = broker,
                        Agent = agent,
                        AccEffCode = accEffCode,
                        DefaultCurrencyCode = defaultCurrencyCode
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            try
            {
                var companyId = GetCompanyId();
                var detail = await _service.GetAccGrpMasterByIdAsync(id, companyId);
                if (detail == null)
                {
                    return NotFound(new { Success = false, Message = "Account Group not found." });
                }
                return Ok(new { Success = true, Data = detail });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("currency-rates/{code}")]
        public async Task<IActionResult> GetCurrencyRates(string code)
        {
            try
            {
                var companyId = GetCompanyId();
                var baseRate = await _service.GetMcurBaseRateAsync(code, companyId);
                var sBaseRate = await _service.GetScurBaseRateAsync(code, companyId);
                return Ok(new { Success = true, BaseRate = baseRate, SBaseRate = sBaseRate });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("check-name")]
        public async Task<IActionResult> CheckName([FromQuery] string name, [FromQuery] int? excludeId = null)
        {
            try
            {
                var companyId = GetCompanyId();
                bool exists = await _service.CheckAccGrpNameExistsAsync(name, companyId, excludeId);
                return Ok(new { Success = true, Exists = exists });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] AccGrpMasterDto model)
        {
            try
            {
                var companyId = GetCompanyId();
                var (success, message) = await _service.AddAccGrpMasterAsync(model, companyId);
                if (success)
                {
                    return Ok(new { Success = true, Message = message });
                }
                return BadRequest(new { Success = false, Message = message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] AccGrpMasterDto model)
        {
            try
            {
                var companyId = GetCompanyId();
                if (id != model.AccGrpCode)
                {
                    return BadRequest(new { Success = false, Message = "ID mismatch." });
                }

                var (success, message) = await _service.UpdateAccGrpMasterAsync(model, companyId);
                if (success)
                {
                    return Ok(new { Success = true, Message = message });
                }
                return BadRequest(new { Success = false, Message = message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var companyId = GetCompanyId();
                var (success, message) = await _service.DeleteAccGrpMasterAsync(id, companyId);
                if (success)
                {
                    return Ok(new { Success = true, Message = message });
                }
                return BadRequest(new { Success = false, Message = message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("eff-code/{accType}")]
        public async Task<IActionResult> GetAccEffCode(int accType)
        {
            try
            {
                var companyId = GetCompanyId();
                var effCode = await _service.GetAccEffCodeAsync(accType, companyId);
                return Ok(new { Success = true, Data = effCode });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("eff-side/{accType}")]
        public async Task<IActionResult> GetAccEffSide(int accType)
        {
            try
            {
                var companyId = GetCompanyId();
                var effSide = await _service.GetAccEffSideAsync(accType, companyId);
                return Ok(new { Success = true, Data = effSide });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
    }
}
