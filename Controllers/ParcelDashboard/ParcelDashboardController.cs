using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Dia_CoreAPI.Services.Interfaces.ParcelDashboard;

namespace Dia_CoreAPI.Controllers.ParcelDashboard
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ParcelDashboardController : ControllerBase
    {
        private readonly IParcelDashboardService _service;

        public ParcelDashboardController(IParcelDashboardService service)
        {
            _service = service;
        }

        private string GetCompanyId()
        {
            return User.FindFirst("CompanyID")?.Value ?? throw new UnauthorizedAccessException("CompanyID missing from token.");
        }

        [HttpGet("GetBestSeller")]
        public async Task<IActionResult> GetBestSeller(string month = "", string year = "", string tdate = "")
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetBestSellerAsync(month, year, tdate, companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetTopBrokers")]
        public async Task<IActionResult> GetTopBrokers(string month = "", string year = "", string tdate = "")
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetTopBrokersAsync(month, year, tdate, companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetAllBrokers")]
        public async Task<IActionResult> GetAllBrokers(string month = "", string year = "", string tdate = "")
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetAllBrokersAsync(month, year, tdate, companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetTopCustomers")]
        public async Task<IActionResult> GetTopCustomers(string month = "", string year = "", string tdate = "")
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetTopCustomersAsync(month, year, tdate, companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetAllCustomers")]
        public async Task<IActionResult> GetAllCustomers(string month = "", string year = "", string tdate = "")
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetAllCustomersAsync(month, year, tdate, companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetOverDueSaleCount")]
        public async Task<IActionResult> GetOverDueSaleCount()
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetOverDueSaleCountAsync(companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetOverDuePurCount")]
        public async Task<IActionResult> GetOverDuePurCount()
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetOverDuePurCountAsync(companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetSalesByCountry")]
        public async Task<IActionResult> GetSalesByCountry(string month = "", string year = "", string tdate = "")
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetSalesByCountryAsync(month, year, tdate, companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetTopPayable")]
        public async Task<IActionResult> GetTopPayable(string month = "", string year = "", string tdate = "")
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetTopPayableAsync(month, year, tdate, companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetAllPayables")]
        public async Task<IActionResult> GetAllPayables(string month = "", string year = "", string tdate = "")
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetAllPayablesAsync(month, year, tdate, companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetTopOutstanding")]
        public async Task<IActionResult> GetTopOutstanding(string month = "", string year = "", string tdate = "")
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetTopOutstandingAsync(month, year, tdate, companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetAllOutstandings")]
        public async Task<IActionResult> GetAllOutstandings(string month = "", string year = "", string tdate = "")
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetAllOutstandingsAsync(month, year, tdate, companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetTopOpenMemo")]
        public async Task<IActionResult> GetTopOpenMemo(string month = "", string year = "", string tdate = "")
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetTopOpenMemoAsync(month, year, tdate, companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetAllOpenMemos")]
        public async Task<IActionResult> GetAllOpenMemos(string month = "", string year = "", string tdate = "")
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetAllOpenMemosAsync(month, year, tdate, companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetTopOverdueSales")]
        public async Task<IActionResult> GetTopOverdueSales(string month = "", string year = "", string tdate = "")
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetTopOverdueSalesAsync(month, year, tdate, companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetAllOverdueSales")]
        public async Task<IActionResult> GetAllOverdueSales(string month = "", string year = "", string tdate = "")
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _service.GetAllOverdueSalesAsync(month, year, tdate, companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
