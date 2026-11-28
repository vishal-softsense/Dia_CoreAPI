using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Email;
using Dia_CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.Common
{
    [ApiController]
    [Route("api/DocumentEmail")]
    [Authorize]
    public class DocumentEmailController : ControllerBase
    {
        private readonly IDocumentEmailService _emailService;

        public DocumentEmailController(IDocumentEmailService emailService)
        {
            _emailService = emailService;
        }

        private string CompanyId => User.FindFirstValue("CompanyID") ?? string.Empty;

        [HttpGet("compose")]
        public async Task<IActionResult> Compose([FromQuery] string docType, [FromQuery] string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CompanyId))
                    return Ok(new { success = false, Data = "Company not found in token." });

                var data = await _emailService.ComposeAsync(docType, id, CompanyId);
                if (data == null)
                    return Ok(new { success = false, Data = "Document not found." });

                return Ok(new { success = true, Data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] DocumentEmailSendRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CompanyId))
                    return Ok(new { success = false, Data = "Company not found in token." });

                var result = await _emailService.SendAsync(request, CompanyId);
                return Ok(new { success = result.Success, Data = result.Message });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Data = ex.Message });
            }
        }

        [HttpGet("preview-pdf")]
        public async Task<IActionResult> PreviewPdf([FromQuery] string docType, [FromQuery] string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CompanyId))
                    return BadRequest("Company not found in token.");

                var pdf = await _emailService.GeneratePdfAsync(docType, id, CompanyId);
                if (pdf == null)
                    return NotFound("Document not found.");

                return File(pdf.Value.PdfBytes, "application/pdf", pdf.Value.FileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

