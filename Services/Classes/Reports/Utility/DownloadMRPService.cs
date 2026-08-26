using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Utility;
using Dia_CoreAPI.Services.Interfaces.Reports.Utility;

namespace Dia_CoreAPI.Services.Classes.Reports.Utility
{
    public partial class DownloadMRPService : IDownloadMRPService
    {
        private readonly IDownloadMRPRepository _repository;
        private readonly HttpClient _httpClient;

        public DownloadMRPService(
            IDownloadMRPRepository repository,
            IHttpClientFactory httpClientFactory)
        {
            _repository = repository;
            _httpClient = httpClientFactory.CreateClient(nameof(DownloadMRPService));
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
        {
            return _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);
        }

        /// <summary>
        /// Matches MVC MRPDownloadController.DownloadMRP (POST data/downloadMRP).
        /// </summary>
        public async Task<(bool Success, string Data)> DownloadMRPAsync(string companyId)
        {
            var credentials = await _repository.GetRapaportCredentialsAsync(companyId);
            if (string.IsNullOrWhiteSpace(credentials.RapUsername) || string.IsNullOrWhiteSpace(credentials.RapPassword))
            {
                return (false, "Rapaport credentials (RapUsername / RapPassword) are not configured in settings.");
            }

            var ticket = await RapaportLoginAsync(credentials.RapUsername!, credentials.RapPassword!);
            if (string.IsNullOrWhiteSpace(ticket))
            {
                return (false, "Rapaport login failed. Please verify RapUsername and RapPassword.");
            }

            try
            {
                // Round → BR (same as MVC)
                var (dateBr, _, pricesBr) = await GetRapaportPriceSheetAsync(ticket, "Round");
                await _repository.DeleteTabPriceIndexRAsync("BR", companyId);
                for (int i = 0; i < pricesBr.Rows.Count; i++)
                {
                    var row = MapRapaportPriceRow(pricesBr.Rows[i]);
                    await _repository.InsertTabPriceIndexRAsync("BR", row, dateBr, companyId);
                }

                // Pear → PS (same as MVC)
                var (datePs, _, pricesPs) = await GetRapaportPriceSheetAsync(ticket, "Pear");
                await _repository.DeleteTabPriceIndexRAsync("PS", companyId);
                for (int i = 0; i < pricesPs.Rows.Count; i++)
                {
                    var row = MapRapaportPriceRow(pricesPs.Rows[i]);
                    await _repository.InsertTabPriceIndexRAsync("PS", row, datePs, companyId);
                }

                await _repository.UpdatePriceToCaratAsync(companyId);
            }
            catch (Exception)
            {
                // MVC swallowed exceptions and still returned success; surface failure here.
                throw;
            }

            // Keep MVC success message text (including spelling)
            return (true, "MRP Download Succesfull.");
        }

        /// <summary>
        /// Matches MVC MRPDownloadController.updateMRP (POST data/updateMRP).
        /// </summary>
        public async Task<(bool Success, string Data)> UpdateMRPAsync(string companyId)
        {
            await _repository.UpdateMRPAsync(companyId);
            return (true, "MRP Updated Successfully In Stock. If You Want To Update Per/Cts Price Then You Have To Apply Import Pricing Utility.");
        }
    }
}
