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
    public class SolitaireItemMasterController : ControllerBase
    {
        private readonly ISolitaireItemMasterService _service;

        public SolitaireItemMasterController(ISolitaireItemMasterService service)
        {
            _service = service;
        }

        private string GetCompanyId()
        {
            return User.FindFirst("CompanyID")?.Value ?? throw new UnauthorizedAccessException("CompanyID missing from token.");
        }

        #region Solitaire

        [HttpGet("list")]
        public async Task<IActionResult> ListSolitaireItemMaster([FromQuery] string? filter)
        {
            try
            {
                var companyId = GetCompanyId();
                string whereClause = "And SinglePcs= '1'";
                if (!string.IsNullOrEmpty(filter))
                {
                    whereClause += $" AND (LotNo LIKE '%{filter}%' OR ItemName LIKE '%{filter}%')";
                }
                var itemData = await _service.GetItemMasterListAsync(companyId, whereClause);
                return Ok(new { Success = true, Data = itemData });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("dropdowns")]
        public async Task<IActionResult> SolitaireItemMasterAdd()
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.ItemMasterDropDownAsync(companyId);

                var shape = data.Where(x => x.Type.Equals("Shape", StringComparison.OrdinalIgnoreCase)).ToList();
                var color = data.Where(x => x.Type.Equals("Color", StringComparison.OrdinalIgnoreCase)).ToList();
                var size = data.Where(x => x.Type.Equals("Size", StringComparison.OrdinalIgnoreCase)).ToList();
                var clarity = data.Where(x => x.Type.Equals("Clarity", StringComparison.OrdinalIgnoreCase)).ToList();
                var sieve = data.Where(x => x.Type.Equals("Sieve", StringComparison.OrdinalIgnoreCase)).ToList();
                var category = data.Where(x => x.Type.Equals("Category", StringComparison.OrdinalIgnoreCase)).ToList();
                var productType = data.Where(x => x.Type.Equals("ProductType", StringComparison.OrdinalIgnoreCase)).ToList();

                var lab = data.Where(x => x.Type.Equals("LAB", StringComparison.OrdinalIgnoreCase)).ToList();
                var cps = data.Where(x => x.Type.Equals("CPS", StringComparison.OrdinalIgnoreCase)).ToList();
                var ha = data.Where(x => x.Type.Equals("HA", StringComparison.OrdinalIgnoreCase)).ToList();
                var proportion = data.Where(x => x.Type.Equals("Proportion", StringComparison.OrdinalIgnoreCase)).ToList();
                var polish = data.Where(x => x.Type.Equals("Polish", StringComparison.OrdinalIgnoreCase)).ToList();
                var symmetry = data.Where(x => x.Type.Equals("Symmetry", StringComparison.OrdinalIgnoreCase)).ToList();
                var fluorescence = data.Where(x => x.Type.Equals("Fluorescence", StringComparison.OrdinalIgnoreCase)).ToList();
                var flColor = data.Where(x => x.Type.Equals("FlColor", StringComparison.OrdinalIgnoreCase)).ToList();
                var brown = data.Where(x => x.Type.Equals("Brown", StringComparison.OrdinalIgnoreCase)).ToList();
                var green = data.Where(x => x.Type.Equals("Green", StringComparison.OrdinalIgnoreCase)).ToList();
                var milky = data.Where(x => x.Type.Equals("Milky", StringComparison.OrdinalIgnoreCase)).ToList();
                var luster = data.Where(x => x.Type.Equals("Luster", StringComparison.OrdinalIgnoreCase)).ToList();
                var natts = data.Where(x => x.Type.Equals("Natts", StringComparison.OrdinalIgnoreCase)).ToList();
                var shade = data.Where(x => x.Type.Equals("Shade", StringComparison.OrdinalIgnoreCase)).ToList();
                var rough = data.Where(x => x.Type.Equals("Rough", StringComparison.OrdinalIgnoreCase)).ToList();
                var mines = data.Where(x => x.Type.Equals("Mines", StringComparison.OrdinalIgnoreCase)).ToList();

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
                        Category = category,
                        ProductType = productType,
                        LAB = lab,
                        CPS = cps,
                        HA = ha,
                        Proportion = proportion,
                        Polish = polish,
                        Symmetry = symmetry,
                        Fluorescence = fluorescence,
                        FlColor = flColor,
                        Brown = brown,
                        Green = green,
                        Milky = milky,
                        Luster = luster,
                        Natts = natts,
                        Shade = shade,
                        Rough = rough,
                        Mines = mines
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("edit-dropdowns")]
        public async Task<IActionResult> SolitaireItemMasterEditDropdowns()
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.ItemMasterDropDownAsync(companyId);
                var data1 = await _service.FillCityStateCountryItemMasterDropDownAsync(companyId);

                var categoryList = data.Where(x => x.Type.Equals("Category", StringComparison.OrdinalIgnoreCase)).ToList();
                var productTypeList = data.Where(x => x.Type.Equals("ProductType", StringComparison.OrdinalIgnoreCase)).ToList();
                var city = data1.Where(x => x.Type.Equals("City", StringComparison.OrdinalIgnoreCase)).ToList();

                return Ok(new
                {
                    Success = true,
                    Data = new
                    {
                        CategoryList = categoryList,
                        ProductTypeList = productTypeList,
                        City = city,
                        // And all other normal dropdowns...
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("detail/{id}")]
        public async Task<IActionResult> SolitaireItemMasterEdit(string id)
        {
            try
            {
                var companyId = GetCompanyId();
                var editData = await _service.GetItemMasterFullListAsync(id, companyId);
                if (editData == null)
                {
                    return NotFound(new { Success = false, Message = "Solitaire Item not found." });
                }
                return Ok(new { Success = true, Data = editData });
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
                 
                if (string.IsNullOrWhiteSpace(lotNo))
                    return BadRequest(new { Success = false, Message = "LotNo is required." });

                var companyId = GetCompanyId();
                string lotAlready = await _service.GetLotExistAsync(lotNo, companyId);

                if (lotAlready == "0")
                {
                    return Ok(new { Success = true, Exists = false });
                }
                return Ok(new { Success = true, Exists = true, Message = "LotNo already exists." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddItem([FromBody] SolitaireItemMasterDto item)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var companyId = GetCompanyId();
                    bool response = await _service.AddItemAsync(item, companyId);
                    return Ok(new { Success = true, Message = "LotNo Created Successfully." });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { Success = false, Message = ex.Message });
                }
            }
            else
            {
                string errorMsg = string.Join("<br>", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return BadRequest(new { Success = false, Message = errorMsg });
            }
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> EditItem(int id, [FromBody] SolitaireItemMasterDto item)
        {
            if (id != item.ItemCode)
            {
                return BadRequest(new { Success = false, Message = "ID mismatch." });
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    var companyId = GetCompanyId();
                    bool response = await _service.EditItemAsync(item, companyId);
                    return Ok(new { Success = true, Message = "Record Updated Successfully." });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { Success = false, Message = ex.Message });
                }
            }
            else
            {
                string errorMsg = string.Join("\n\n", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return BadRequest(new { Success = false, Message = errorMsg });
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            try
            {
                var companyId = GetCompanyId();
                bool response = await _service.DeleteItemAsync(id, companyId);
                return Ok(new { Success = true, Message = "Record Deleted." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("preview-full-item")]
        public async Task<IActionResult> PreviewFullItem([FromBody] ImportCertExcel model)
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.PreviewFullItemAsync(model, companyId);
                return Ok(new { Success = true, Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        #endregion
    }
}
