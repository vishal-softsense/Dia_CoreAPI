using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Utility;
using Dia_CoreAPI.Services.Interfaces.Reports.Utility;

namespace Dia_CoreAPI.Services.Classes.Reports.Utility
{
    public class MissingStockService : IMissingStockService
    {
        private readonly IMissingStockRepository _repository;

        public MissingStockService(IMissingStockRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);

        /// <summary>Matches MVC MissingStockController.MissingStockSearch WHERE + Sp_MissingStock.</summary>
        public async Task<List<MissingStockReportDto>> GetReportAsync(
            MissingStockFilterDto filter,
            string companyId)
        {
            if (string.Equals(filter?.Filters, "LotNo", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(filter?.LotNo))
            {
                return new List<MissingStockReportDto>();
            }

            string where = BuildWhere(filter);
            string stockType = filter?.StoneTypes ?? "All";
            return await _repository.GetMissingStockDataAsync(stockType, where, companyId);
        }

        private static string BuildWhere(MissingStockFilterDto? filter)
        {
            var conditions = new List<string>();
            string stoneTypes = filter?.StoneTypes ?? "All";
            string status = filter?.Status ?? "";
            string filters = filter?.Filters ?? "";
            string lotNo = filter?.LotNo ?? "";

            if (stoneTypes != "All")
            {
                if (stoneTypes == "Single")
                    conditions.Add("singlepcs = 1");
                else if (stoneTypes == "Parcel")
                    conditions.Add("singlepcs = 0");
            }

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                if (status == "OnHand")
                    conditions.Add("onhand>0.01 And CurStatus in (''S'')");
                else if (status == "OnMemo")
                    conditions.Add("CurStatus in (''M'',''I'',''L'',''PS'')");
                else if (status == "OnHand&Int.Memo")
                    conditions.Add("CurStatus in (''S'',''I'')");
                else if (status == "OnHand + Memo In")
                    conditions.Add("CurStatus in (''S'',''MI'')");
                else if (status == "OnHand + Party Hold")
                    conditions.Add("onhand>0.01 And CurStatus in (''S'',''PH'')");
                else if (status == "Memo In")
                    conditions.Add("CurStatus in (''MI'')");
                else if (status == "Party Hold")
                    conditions.Add("CurStatus in (''PH'')");
                else if (status == "All - Party Hold")
                    conditions.Add("CurStatus not in (''PH'')");
            }

            if (filters == "LotNo" && !string.IsNullOrWhiteSpace(lotNo))
            {
                var lots = lotNo
                    .Replace("\r\n", "\n")
                    .Split('\n')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x));

                string strTmpNewLine = string.Join("'',''", lots);
                conditions.Add("(LotNo IN (''" + strTmpNewLine + "''))");
            }
            else if (filters == "LotNo Not" && !string.IsNullOrWhiteSpace(lotNo))
            {
                string strTmpNewLine = string.Join("'',''",
                    lotNo
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrWhiteSpace(x)));

                conditions.Add("(LotNo NOT IN (''" + strTmpNewLine + "''))");
            }
            else if (filters == "LotNo Like" && !string.IsNullOrWhiteSpace(lotNo))
            {
                string strTmpNewLine = string.Join("'',''",
                    lotNo
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrWhiteSpace(x)));

                conditions.Add("(LotNo Like (''" + strTmpNewLine + "%''))");
            }
            else if (filters == "LotNo")
            {
                conditions.Add("(1=0)");
            }

            if (filters == "LotNo")
            {
                // No extra where — JS categorises by CurStatus
            }
            else if (filters == "LotNo Not" || filters == "LotNo Like")
            {
                if (stoneTypes == "Jewellery")
                    conditions.Add("(totcarats  + memoIn > 0.05 OR OnHandCarats > 0.05) And CurStatus in (''S'',''MI'')");
                else
                    conditions.Add("(totcarats  + memoIn > 0.05 OR OnHandCarats > 0.05)  And NotShow = 0 And CurStatus in (''S'',''MI'')");
            }
            else if (stoneTypes == "Single" || stoneTypes == "Parcel")
            {
                conditions.Add("(totcarats  + memoIn > 0.05 OR OnHandCarats > 0.05)  And NotShow = 0 And CurStatus in (''S'',''MI'')");
            }
            else
            {
                conditions.Add("(totcarats + memoIn > 0.05 OR OnHandCarats > 0.05) And CurStatus in (''S'',''MI'')");
            }

            return string.Join(" And ", conditions);
        }
    }
}
