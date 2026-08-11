using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dia_CoreAPI.DTOs.Reports.Utility;

namespace Dia_CoreAPI.Services.Helpers
{
    /// <summary>
    /// ASP.NET Core SOAP client for Rapaport Prices ASMX
    /// (replaces MVC Connected Service MRPPrices.RapaportPricesSoapClient).
    /// </summary>
    public class RapaportPricesSoapClient
    {
        private const string Endpoint = "https://technet.rapaport.com:449/webservices/prices/rapaportprices.asmx";
        private const string RapNs = "http://technet.rapaport.com/";

        private readonly HttpClient _httpClient;

        public RapaportPricesSoapClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> LoginAsync(string username, string password)
        {
            var body = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <Login xmlns=""{RapNs}"">
      <Username>{EscapeXml(username)}</Username>
      <Password>{EscapeXml(password)}</Password>
    </Login>
  </soap:Body>
</soap:Envelope>";

            var responseXml = await PostSoapAsync("http://technet.rapaport.com/Login", body);
            var doc = XDocument.Parse(responseXml);

            var ticket = doc.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "AuthenticationTicketHeader")
                ?.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "Ticket")
                ?.Value;

            return string.IsNullOrWhiteSpace(ticket) ? null : ticket;
        }

        public async Task<(DateTime Date, string Shape, DataTable Prices)> GetPriceSheetAsync(string ticket, string shape)
        {
            var body = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Header>
    <AuthenticationTicketHeader xmlns=""{RapNs}"">
      <Ticket>{EscapeXml(ticket)}</Ticket>
    </AuthenticationTicketHeader>
  </soap:Header>
  <soap:Body>
    <GetPriceSheet xmlns=""{RapNs}"">
      <shape>{EscapeXml(shape)}</shape>
    </GetPriceSheet>
  </soap:Body>
</soap:Envelope>";

            var responseXml = await PostSoapAsync("http://technet.rapaport.com/GetPriceSheet", body);
            var doc = XDocument.Parse(responseXml);

            var result = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "GetPriceSheetResult")
                ?? throw new InvalidOperationException("GetPriceSheetResult not found in Rapaport response.");

            var dateAttr = result.Attribute("Date")?.Value;
            var shapeAttr = result.Attribute("Shape")?.Value ?? shape;
            if (!DateTime.TryParse(dateAttr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var rapDate))
                rapDate = DateTime.UtcNow.Date;

            var pricesElement = result.Elements().FirstOrDefault(e => e.Name.LocalName == "Prices")
                ?? throw new InvalidOperationException("Prices element not found in Rapaport price sheet.");

            var ds = new DataSet();
            using (var reader = pricesElement.CreateReader())
            {
                ds.ReadXml(reader, XmlReadMode.InferSchema);
            }

            if (ds.Tables.Count == 0)
            {
                // Fallback: strip schema and read DiffGram NewDataSet rows
                ds = ReadPriceDataSetFallback(pricesElement);
            }

            if (ds.Tables.Count == 0)
                throw new InvalidOperationException("Rapaport price sheet contained no price rows.");

            return (rapDate, shapeAttr, ds.Tables[0]);
        }

        public static RapaportPriceRowDTO MapPriceRow(DataRow row)
        {
            // Match MVC MRPDownloadController ordinals:
            // InsertTabPriceIndexR(shape, [0] Color, [1] Clarity, [3] CaratsFrom, [2] CaratsTo, [4] Rate, date)
            return new RapaportPriceRowDTO
            {
                Color = row[0]?.ToString() ?? string.Empty,
                Clarity = row[1]?.ToString() ?? string.Empty,
                CaratsFrom = row[3]?.ToString() ?? string.Empty,
                CaratsTo = row[2]?.ToString() ?? string.Empty,
                Rate = row[4]?.ToString() ?? string.Empty
            };
        }

        private async Task<string> PostSoapAsync(string soapAction, string body)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
            request.Content = new StringContent(body, Encoding.UTF8, "text/xml");
            request.Headers.TryAddWithoutValidation("SOAPAction", $"\"{soapAction}\"");

            using var response = await _httpClient.SendAsync(request);
            var responseXml = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Rapaport SOAP call failed ({(int)response.StatusCode}): {responseXml}");

            if (responseXml.Contains("soap:Fault", StringComparison.OrdinalIgnoreCase)
                || responseXml.Contains("soap12:Fault", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Rapaport SOAP fault: {responseXml}");

            return responseXml;
        }

        private static DataSet ReadPriceDataSetFallback(XElement pricesElement)
        {
            var ds = new DataSet();
            var tableNodes = pricesElement.Descendants()
                .Where(e => e.Name.LocalName == "Table" || e.Name.LocalName == "RapaportPrice" || e.Parent?.Name.LocalName == "NewDataSet")
                .Where(e => e.HasElements)
                .ToList();

            if (tableNodes.Count == 0)
                return ds;

            var table = new DataTable("Prices");
            var first = tableNodes[0];
            foreach (var child in first.Elements())
                table.Columns.Add(child.Name.LocalName, typeof(string));

            foreach (var node in tableNodes)
            {
                var row = table.NewRow();
                foreach (DataColumn col in table.Columns)
                {
                    var val = node.Elements().FirstOrDefault(e => e.Name.LocalName == col.ColumnName)?.Value;
                    row[col.ColumnName] = val ?? string.Empty;
                }
                table.Rows.Add(row);
            }

            ds.Tables.Add(table);
            return ds;
        }

        private static string EscapeXml(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }
    }
}
