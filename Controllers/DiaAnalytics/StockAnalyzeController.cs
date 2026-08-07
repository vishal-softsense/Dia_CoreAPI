using Dia_CoreAPI.Services.Interfaces.DiaAnalytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.DiaAnalytics
{
    /// <summary>
    /// MVC equivalent: StockAnalyzeController → View with Diasense iframe.
    /// Route in MVC: data/StockAnalyze
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StockAnalyzeController : ControllerBase
    {
        private readonly IStockAnalyzeService _service;

        public StockAnalyzeController(IStockAnalyzeService service)
        {
            _service = service;
        }

        /// <summary>
        /// Returns iframe URL config for Diasense Stock Analyze AI.
        /// </summary>
        [HttpGet("config")]
        public IActionResult GetConfig()
        {
            try
            {
                var data = _service.GetStockAnalyzeConfig();

                if (string.IsNullOrWhiteSpace(data.IframeUrl))
                {
                    return Ok(new
                    {
                        Success = false,
                        Message = "Diasense Stock Analyze is not configured. Set Diasense:StockAnalyze in appsettings.",
                        Data = data
                    });
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
    }
}
