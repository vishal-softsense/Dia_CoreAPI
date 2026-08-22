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
            string where = BuildWhere(filter);
            string stockType = filter?.StoneTypes ?? "All";
            return await _repository.GetMissingStockDataAsync(stockType, where, companyId);
        }

        private static string BuildWhere(MissingStockFilterDto? filter)
        {
            string where = "";
            string stoneTypes = filter?.StoneTypes ?? "All";
            string status = filter?.Status ?? "";
            string filters = filter?.Filters ?? "";
            string lotNo = filter?.LotNo ?? "";

            if (stoneTypes != "All")
            {
                if (stoneTypes == "Single")
                    where += "singlepcs = 1  And ";
                else if (stoneTypes == "Parcel")
                    where += "singlepcs = 0 And ";
            }

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                if (status == "OnHand")
                    where += " And onhand>0.01 And CurStatus in (''S'') ";
                else if (status == "OnMemo")
                    where += " And CurStatus in (''M'',''I'',''L'',''PS'') ";
                else if (status == "OnHand&Int.Memo")
                    where += " And CurStatus in (''S'',''I'') ";
                else if (status == "OnHand + Memo In")
                    where += " And CurStatus in (''S'',''MI'') ";
                else if (status == "OnHand + Party Hold")
                    where += " And onhand>0.01 And CurStatus in (''S'',''PH'') ";
                else if (status == "Memo In")
                    where += " And CurStatus in (''MI'') ";
                else if (status == "Party Hold")
                    where += " And CurStatus in (''PH'') ";
                else if (status == "All - Party Hold")
                    where += " And CurStatus not in (''PH'') ";
            }

            if (filters == "LotNo" && !string.IsNullOrWhiteSpace(lotNo))
            {
                var lots = lotNo
                    .Replace("\r\n", "\n")
                    .Split('\n')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x));

                string strTmpNewLine = string.Join("'',''", lots);
                where += " (LotNo IN (''" + strTmpNewLine + "''))";
            }
            else if (filters == "LotNo Not")
            {
                string strTmpNewLine = string.Join("'',''",
                    lotNo
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrWhiteSpace(x)));

                where += " (LotNo NOT IN (''" + strTmpNewLine + "'')) ";
            }
            else if (filters == "LotNo Like")
            {
                string strTmpNewLine = string.Join("'',''",
                    lotNo
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrWhiteSpace(x)));

                where += " (LotNo Like (''" + strTmpNewLine + "%''))";
            }

            if (filters == "LotNo")
            {
                // No extra where — JS categorises by CurStatus
            }
            else if (filters == "LotNo Not" || filters == "LotNo Like")
            {
                if (stoneTypes == "Jewellery")
                    where += " And (totcarats  + memoIn > 0.05 OR OnHandCarats > 0.05) And CurStatus in (''S'',''MI'')";
                else
                    where += " And (totcarats  + memoIn > 0.05 OR OnHandCarats > 0.05)  And NotShow = 0 And CurStatus in (''S'',''MI'')";
            }
            else if (stoneTypes == "Single" || stoneTypes == "Parcel")
            {
                where += " (totcarats  + memoIn > 0.05 OR OnHandCarats > 0.05)  And NotShow = 0 And CurStatus in (''S'',''MI'')";
            }
            else
            {
                where += " (totcarats + memoIn > 0.05 OR OnHandCarats > 0.05) And CurStatus in (''S'',''MI'')";
            }

            return where;
        }
    }
}
