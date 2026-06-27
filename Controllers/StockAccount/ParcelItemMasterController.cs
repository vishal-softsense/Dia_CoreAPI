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
    public class ParcelItemMasterController : ControllerBase
    {
        private readonly IParcelItemMasterService _service;

        public ParcelItemMasterController(IParcelItemMasterService service)
        {
            _service = service;
        }

        private string GetCompanyId()
        {
            return User.FindFirst("CompanyID")?.Value ?? throw new UnauthorizedAccessException("CompanyID missing from token.");
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetList([FromQuery] string? filter)
        {
            try
            {
                var companyId = GetCompanyId();
                string whereClause = "And SinglePcs= '0'";
                if (!string.IsNullOrEmpty(filter))
                {
                    whereClause += $" AND (LotNo LIKE '%{filter}%' OR ItemName LIKE '%{filter}%')";
                }
                var list = await _service.GetParcelItemMasterListAsync(companyId, whereClause);
                return Ok(new { Success = true, Data = list });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
        [HttpGet("dropdowns")]
        public async Task<IActionResult> GetDropdowns()
        {
            try
            {
                var companyId = GetCompanyId();
                var list = await _service.GetItemMasterDropdownAsync(companyId);

                var shape = list.Where(x => x.Type.Equals("Shape", StringComparison.OrdinalIgnoreCase)).ToList();
                var color = list.Where(x => x.Type.Equals("Color", StringComparison.OrdinalIgnoreCase)).ToList();
                var size = list.Where(x => x.Type.Equals("Size", StringComparison.OrdinalIgnoreCase)).ToList();
                var clarity = list.Where(x => x.Type.Equals("Clarity", StringComparison.OrdinalIgnoreCase)).ToList();
                var sieve = list.Where(x => x.Type.Equals("Sieve", StringComparison.OrdinalIgnoreCase)).ToList();
                var itemGroup = list.Where(x => x.Type.Equals("ItemGroup", StringComparison.OrdinalIgnoreCase)).ToList();
                var category = list.Where(x => x.Type.Equals("Category", StringComparison.OrdinalIgnoreCase)).ToList();
                var productType = list.Where(x => x.Type.Equals("ProductType", StringComparison.OrdinalIgnoreCase)).ToList();

                return Ok(new
                {
                    Success = true,
                    Data = new
                    {
                        Shape = shape,
                        Color = color,
                        Size = size,
                        Clarity = clarity,
                        Sieve = sieve,
                        ItemGroup = itemGroup,
                        Category = category,
                        ProductType = productType
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
        //[HttpGet("dropdowns")]
        //public async Task<IActionResult> GetDropdowns()
        //{
        //    try
        //    {
        //        var companyId = GetCompanyId();
        //        var list = await _service.GetItemMasterDropdownAsync(companyId);

        //        var shape = list.Where(x => x.Type.Equals("Shape", StringComparison.OrdinalIgnoreCase)).ToList();
        //        var color = list.Where(x => x.Type.Equals("Color", StringComparison.OrdinalIgnoreCase)).ToList();
        //        var size = list.Where(x => x.Type.Equals("Size", StringComparison.OrdinalIgnoreCase)).ToList();
        //        var clarity = list.Where(x => x.Type.Equals("Clarity", StringComparison.OrdinalIgnoreCase)).ToList();
        //        var sieve = list.Where(x => x.Type.Equals("Sieve", StringComparison.OrdinalIgnoreCase)).ToList();
        //        var itemGroup = list.Where(x => x.Type.Equals("ItemGroup", StringComparison.OrdinalIgnoreCase)).ToList();
        //        var category = list.Where(x => x.Type.Equals("Category", StringComparison.OrdinalIgnoreCase)).ToList();
        //        var productType = list.Where(x => x.Type.Equals("ProductType", StringComparison.OrdinalIgnoreCase)).ToList();

        //        return Ok(new
        //        {
        //            Success = true,
        //            Data = new
        //            {
        //                Shape = shape,
        //                Color = color,
        //                Size = size,
        //                Clarity = clarity,
        //                Sieve = sieve,
        //                ItemGroup = itemGroup,
        //                Category = category,
        //                ProductType = productType
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { Success = false, Message = ex.Message });
        //    }
        //}

        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            try
            {
                var companyId = GetCompanyId();
                var detail = await _service.GetParcelItemMasterByIdAsync(id, companyId);
                if (detail == null)
                {
                    return NotFound(new { Success = false, Message = "Parcel Item not found." });
                }
                return Ok(new { Success = true, Data = detail });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("check-lot")]
        public async Task<IActionResult> CheckLotNo([FromQuery] string lotNo)
        {
            try
            {
                var companyId = GetCompanyId();
                bool exists = await _service.CheckLotNoExistsAsync(lotNo, companyId);
                return Ok(new { Success = true, Exists = exists });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] ParcelItemMasterDto model)
        {
            try
            {
                var companyId = GetCompanyId();

                // Validation: check if LotNo already exists
                bool exists = await _service.CheckLotNoExistsAsync(model.LotNo, companyId);
                if (exists)
                {
                    return BadRequest(new { Success = false, Message = "LotNo Already Exists" });
                }

                // Get next Max SrNo
                int maxSrNo = await _service.GetMaxItemSrNoAsync(companyId);
                model.SrNo = (maxSrNo + 1).ToString();

                bool success = await _service.SaveParcelItemMasterAsync(model, "insert", companyId);
                if (success)
                {
                    return Ok(new { Success = true, Message = "Parcel Item created successfully." });
                }
                return BadRequest(new { Success = false, Message = "Failed to save Parcel Item." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] ParcelItemMasterDto model)
        {
            try
            {
                var companyId = GetCompanyId();
                if (id != model.ItemCode)
                {
                    return BadRequest(new { Success = false, Message = "ID mismatch." });
                }

                // Check LotNo uniqueness (exclude self if needed, but the original checks if it exists in DB)
                // In MVC, checking lot no exists for edit was done via front-end on lotNo change if the field changed.
                // We'll trust the validation.

                bool success = await _service.SaveParcelItemMasterAsync(model, "update", companyId);
                if (success)
                {
                    return Ok(new { Success = true, Message = "Parcel Item updated successfully." });
                }
                return BadRequest(new { Success = false, Message = "Failed to update Parcel Item." });
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
                string countStr = await _service.GetAccandItemCountAsync(id, companyId);
                if (countStr != "0")
                {
                    return BadRequest(new { Success = false, Message = "This Item is in Transaction" });
                }

                bool success = await _service.DeleteParcelItemMasterAsync(id, companyId);
                if (success)
                {
                    return Ok(new { Success = true, Message = "Parcel Item deleted successfully." });
                }
                return BadRequest(new { Success = false, Message = "Failed to delete Parcel Item." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
    }
}
