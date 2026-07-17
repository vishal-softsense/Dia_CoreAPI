using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Dia_CoreAPI.Services.Interfaces.Dashboard;

namespace Dia_CoreAPI.Controllers.Dashboard
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requires JWT Token
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        private string GetCompanyId()
        {
            return User.FindFirst("CompanyID")?.Value ?? throw new System.UnauthorizedAccessException("CompanyID missing from token.");
        }

        [HttpGet("GetBestSeller")]
        public async Task<IActionResult> GetBestSeller(string month = "", string year = "", string tdate = "")
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _dashboardService.GetBestSellerAsync(month, year, tdate, companyId);
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
                var data = await _dashboardService.GetTopBrokersAsync(month, year, tdate, companyId);
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
                var data = await _dashboardService.GetTopCustomersAsync(month, year, tdate, companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetOverDueSale")]
        public async Task<IActionResult> GetOverDueSale()
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _dashboardService.GetOverDueSaleAsync(companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetOverDuePur")]
        public async Task<IActionResult> GetOverDuePur()
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _dashboardService.GetOverDuePurAsync(companyId);
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
                var data = await _dashboardService.GetSalesByCountryAsync(month, year, tdate, companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetSalePLPerLegend")]
        public async Task<IActionResult> GetSalePLPerLegend()
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _dashboardService.GetSalePLPerLegendAsync(companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetStoneAge")]
        public async Task<IActionResult> GetStoneAge()
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _dashboardService.GetStoneAgeAsync(companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetRevisedAge")]
        public async Task<IActionResult> GetRevisedAge()
        {
            try
            {
                var companyId = GetCompanyId();
                var data = await _dashboardService.GetRevisedAgeAsync(companyId);
                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
