using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Email;

namespace Dia_CoreAPI.Services.Interfaces
{
    public interface IDocumentEmailService
    {
        Task<DocumentEmailComposeResponse?> ComposeAsync(string docType, string id, string companyId);
        Task<(byte[] PdfBytes, string FileName)?> GeneratePdfAsync(string docType, string id, string companyId);
        Task<DocumentEmailSendResult> SendAsync(DocumentEmailSendRequest request, string companyId);
    }
}

