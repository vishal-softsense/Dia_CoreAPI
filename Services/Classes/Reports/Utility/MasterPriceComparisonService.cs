using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DTOs.Reports.Utility;
using Dia_CoreAPI.Repositories.Interfaces.Reports.Utility;
using Dia_CoreAPI.Services.Interfaces.Reports.Utility;
using BasicDataDto = Dia_CoreAPI.DTOs.StockAccount.BasicDataDto;

namespace Dia_CoreAPI.Services.Classes.Reports.Utility
{
    public class MasterPriceComparisonService : IMasterPriceComparisonService
    {
        private readonly IMasterPriceComparisonRepository _repository;

        public MasterPriceComparisonService(IMasterPriceComparisonRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CheckUserPermissionAsync(string userTypeId, string formName, string actionType, string companyId)
            => _repository.CheckUserPermissionAsync(userTypeId, formName, actionType, companyId);

        /// <summary>Matches MVC GET data/MasterPriceComparison ViewBag LAB/Shape/Size/Fluorescence/CPS.</summary>
        public async Task<MasterPriceComparisonPageDto> LoadPageDataAsync(string companyId)
        {
            var data = await _repository.GetFilterDropdownAsync(companyId);

            return new MasterPriceComparisonPageDto
            {
                LAB = MapDropdown(data, "LAB"),
                Shape = MapDropdown(data, "Shape"),
                Size = MapDropdown(data, "Size"),
                Fluorescence = MapDropdown(data, "Flou"),
                CPS = MapDropdown(data, "CPS")
            };
        }

        /// <summary>Matches MVC POST data/MasterPriceComparison-List where + Type=Report.</summary>
        public async Task<List<MasterPriceComparisonReportDto>> GetReportAsync(
            MasterPriceComparisonFilterDto filter,
            string companyId)
        {
            string where = BuildWhere(filter);
            return await _repository.GetReportDataAsync(where, "Report", companyId);
        }

        private static List<MasterPriceComparisonDropdownOptionDto> MapDropdown(
            IEnumerable<BasicDataDto> data,
            string type)
        {
            return data
                .Where(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase))
                .Select(x => new MasterPriceComparisonDropdownOptionDto
                {
                    ID = x.ID,
                    Name = string.IsNullOrWhiteSpace(x.Name) ? x.ID : x.Name
                })
                .ToList();
        }

        /// <summary>Builds the same @where string as MVC MasterPriceComparisonController (double-quoted IN lists for SP).</summary>
        private static string BuildWhere(MasterPriceComparisonFilterDto? filter)
        {
            var where = "";

            if (filter?.LAB != null && filter.LAB.Any())
            {
                var labList = string.Join("','", filter.LAB);
                where += " AND b.LAB IN (''" + labList.Replace("'", "''") + "'')";
            }

            if (filter?.Fluorescence != null && filter.Fluorescence.Any())
            {
                var flouList = string.Join("','", filter.Fluorescence);
                where += " AND  b.FLId IN (''" + flouList.Replace("'", "''") + "'')";
            }

            if (filter?.Shape != null && filter.Shape.Any())
            {
                var shapeList = string.Join("','", filter.Shape);
                where += " AND b.Shape IN (''" + shapeList.Replace("'", "''") + "'')";
            }

            if (filter?.Size != null && filter.Size.Any())
            {
                var sizeConditions = new List<string>();
                foreach (var size in filter.Size)
                {
                    var parts = size.Split('-');
                    if (parts.Length == 2 &&
                        double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var fromVal) &&
                        double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var toVal))
                    {
                        sizeConditions.Add($"(b.FromSize = {fromVal} AND b.ToSize = {toVal})");
                    }
                }

                if (sizeConditions.Any())
                    where += " AND (" + string.Join(" OR ", sizeConditions) + ")";
            }

            if (filter?.CPS != null && filter.CPS.Any())
            {
                var cpsList = string.Join("','", filter.CPS);
                where += " AND b.CPSID IN (''" + cpsList.Replace("'", "''") + "'')";
            }

            return where;
        }
    }
}
