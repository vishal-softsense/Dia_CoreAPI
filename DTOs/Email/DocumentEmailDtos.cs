namespace Dia_CoreAPI.DTOs.Email
{
    public class DocumentEmailComposeResponse
    {
        public string DocType { get; set; } = string.Empty; // "MixMemoOut" | "MixSale"
        public string Id { get; set; } = string.Empty;
        public string InvNo { get; set; } = string.Empty;
        public string PartyName { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Cc { get; set; } = string.Empty;
        public string Bcc { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string PdfFileName { get; set; } = string.Empty;
        public bool HasSmtpConfig { get; set; }
        public string? SmtpWarning { get; set; }
    }

    public class DocumentEmailSendRequest
    {
        public string DocType { get; set; } = string.Empty; // "MixMemoOut" | "MixSale"
        public string Id { get; set; } = string.Empty;      // Code or InvNo
        public string To { get; set; } = string.Empty;
        public string? Cc { get; set; }
        public string? Bcc { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string? Body { get; set; }
        public bool IncludePdf { get; set; } = true;
    }

    public class DocumentEmailSendResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

