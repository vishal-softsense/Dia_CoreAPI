using Dia_CoreAPI.DTOs.Parcel.Parcel_Reports;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Reports;
using Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Reports;

namespace Dia_CoreAPI.Services.Classes.Parcel.Parcel_Reports
{
    public class ParcelStockSearchReportService : IParcelStockSearchReportService
    {
        private readonly IParcelStockSearchReportRepository _repository;

        public ParcelStockSearchReportService(IParcelStockSearchReportRepository repository)
        {
            _repository = repository;
        }

        #region Permission

        public async Task<int> CheckUserPermissionAsync(
            string userTypeId,
            string formName,
            string actionType,
            string companyId)
        {
            return await _repository.CheckUserPermissionAsync(
                userTypeId,
                formName,
                actionType,
                companyId);
        }

        #endregion

        #region Load Report

        public async Task<ParcelStockSearchReportFilterDto> LoadReportDataAsync(string companyId)
        {
            var data = await _repository.GetParcelStockSearchDropdownAsync(companyId);

            return new ParcelStockSearchReportFilterDto
            {
                Currency      = data.Where(x => x.Type == "Currency").Select(x => x.Name).ToList(),
                ShapeGroup    = data.Where(x => x.Type == "ShapeGroup").Select(x => x.Name).ToList(),
                Shape         = data.Where(x => x.Type == "Shape").Select(x => x.Name).ToList(),
                ColorGroup    = data.Where(x => x.Type == "ColorGroup").Select(x => x.Name).ToList(),
                Color         = data.Where(x => x.Type == "Color").Select(x => x.Name).ToList(),
                Size          = data.Where(x => x.Type == "Size").Select(x => x.Name).ToList(),
                Clarity       = data.Where(x => x.Type == "Clarity").Select(x => x.Name).ToList(),
                Proportion    = data.Where(x => x.Type == "Proportion").Select(x => x.Name).ToList(),
                Polish        = data.Where(x => x.Type == "Polish").Select(x => x.Name).ToList(),
                Symmetry      = data.Where(x => x.Type == "Symmetry").Select(x => x.Name).ToList(),
                LAB           = data.Where(x => x.Type == "LAB").Select(x => x.Name).ToList(),
                Rough         = data.Where(x => x.Type == "Rough").Select(x => x.Name).ToList(),
                HA            = data.Where(x => x.Type == "HA").Select(x => x.Name).ToList(),
                Location      = data.Where(x => x.Type == "Location").Select(x => x.Name).ToList(),
                Fluorescence  = data.Where(x => x.Type == "Fluorescence").Select(x => x.Name).ToList(),
                Milky         = data.Where(x => x.Type == "Milky").Select(x => x.Name).ToList(),
                Luster        = data.Where(x => x.Type == "Luster").Select(x => x.Name).ToList(),
                Natts         = data.Where(x => x.Type == "Natts").Select(x => x.Name).ToList(),
                Shade         = data.Where(x => x.Type == "Shade").Select(x => x.Name).ToList(),
                FlColor       = data.Where(x => x.Type == "FlColor").Select(x => x.Name).ToList(),
                ItemGroup     = data.Where(x => x.Type == "ItemGroup").Select(x => x.Name).ToList(),
                Pair          = data.Where(x => x.Type == "Pair").Select(x => x.Name).ToList(),
                City          = data.Where(x => x.Type == "City").Select(x => x.Name).ToList(),
                State         = data.Where(x => x.Type == "State").Select(x => x.Name).ToList(),
                Country       = data.Where(x => x.Type == "Country").Select(x => x.Name).ToList(),
                BoxName       = data.Where(x => x.Type == "BoxName").Select(x => x.Name).ToList(),
                Sieve         = data.Where(x => x.Type == "Sieve").Select(x => x.Name).ToList()
            };
        }

        #endregion

        #region Dropdown (raw list for dropdowns endpoint)

        public async Task<List<BasicDataDto>> GetStockSearchDropdownAsync(string companyId)
        {
            return await _repository.GetParcelStockSearchDropdownAsync(companyId);
        }

        #endregion

        #region Report

        public async Task<List<ParcelStockrpt>> GetParcelStockSearchReportAsync(
            ParcelStockSearchReportFilterDto filter,
            string companyId)
        {
            // Validate date; fall back to today if missing or invalid
            string date = string.IsNullOrWhiteSpace(filter.Date) || !DateTime.TryParse(filter.Date, out _)
                ? DateTime.Now.ToString("yyyy-MM-dd")
                : filter.Date;

            string where     = BuildWhere(filter);
            string askingQry = BuildAskingQuery(filter);

            return await _repository.GetParcelStockSearchDataAsync(date, where, askingQry, companyId);
        }

        #endregion

        #region Private Helpers

        private string BuildWhere(ParcelStockSearchReportFilterDto filter)
        {
            string where = string.Empty;

            // --- Show ---
            if (!string.IsNullOrEmpty(filter.Show) && filter.Show != "All")
            {
                if (filter.Show == "Sold")
                    where += " And (CurStatus = 'Sold' or Round((CONVERT(decimal(9,3),TotCarats + MemoIn)),3,1) = 0) ";
                else if (filter.Show == "Unsold")
                    where += " And (totcarats + memoIn > 0.05 OR OnHandCarats > 0.05) ";
            }

            // --- Hold / Unhold ---
            if (!string.IsNullOrEmpty(filter.HoldUnHold) && filter.HoldUnHold != "All")
            {
                if (filter.HoldUnHold == "Hold")
                    where += " And NotShow = 1 ";
                else if (filter.HoldUnHold == "Unhold")
                    where += " And NotShow = 0 ";
            }

            // --- Stone Types ---
            if (!string.IsNullOrEmpty(filter.StoneTypes) && filter.StoneTypes != "All")
            {
                if (filter.StoneTypes == "Single")
                    where += " And singlepcs = 1 ";
                else if (filter.StoneTypes == "Parcel")
                    where += " And singlepcs = 0 ";
            }

            // --- Status ---
            if (!string.IsNullOrEmpty(filter.Status) && filter.Status != "All")
            {
                switch (filter.Status)
                {
                    case "OnHand":
                        where += " And onhand>0.01 And CurStatus in ('S') "; break;
                    case "OnMemo":
                        where += " And CurStatus in ('M','I','L','PS') "; break;
                    case "OnHand + OnMemo":
                        where += " And (CurStatus in ('M','I','L','PS') OR ISNULL(MemoOut, 0) > 0) "; break;
                    case "OnHand&Int.Memo":
                        where += " And CurStatus in ('S','I') "; break;
                    case "OnHand + Memo In":
                        where += " And CurStatus in ('S','MI') "; break;
                    case "OnHand + Party Hold":
                        where += " And onhand>0.01 And CurStatus in ('S','PH') "; break;
                    case "Memo In":
                        where += " And CurStatus in ('MI') "; break;
                    case "Party Hold":
                        where += " And CurStatus in ('PH') "; break;
                    case "All - Party Hold":
                        where += " And CurStatus not in ('PH') "; break;
                }
            }

            // --- List / Multi-select filters ---
            AddInClause(ref where, "ShapeGroupId",  filter.ShapeGroup);
            AddInClause(ref where, "Shp",           filter.Shape);
            AddInClause(ref where, "ColorGroupId",  filter.ColorGroup);
            AddInClause(ref where, "Col",           filter.Color);
            AddInClause(ref where, "Size",          filter.Size);
            AddInClause(ref where, "Cla",           filter.Clarity);
            AddInClause(ref where, "Prop",          filter.Proportion);
            AddInClause(ref where, "Pol",           filter.Polish);
            AddInClause(ref where, "Sym",           filter.Symmetry);
            AddInClause(ref where, "Cert",          filter.LAB);
            AddInClause(ref where, "HA",            filter.HA);
            AddInClause(ref where, "City",          filter.Location);

            // Fluorescence (special: null check)
            if (filter.Fluorescence != null && filter.Fluorescence.Count > 0)
            {
                if (filter.Fluorescence.Contains("IS_NULL"))
                    where += " And FL IS NULL ";
                else
                {
                    string flVal = BuildInClauseString(filter.Fluorescence);
                    if (!string.IsNullOrEmpty(flVal))
                        where += " And FL In (" + flVal + ")";
                }
            }

            AddInClause(ref where, "[Ml.]",       filter.Milky);
            AddInClause(ref where, "[LS.]",        filter.Luster);
            AddInClause(ref where, "[Nts.]",       filter.Natts);
            AddInClause(ref where, "Shade",        filter.Shade);
            AddInClause(ref where, "FLColor",      filter.FlColor);
            AddInClause(ref where, "ItemgroupId",  filter.ItemGroup);
            AddInClause(ref where, "BoxName",      filter.BoxNm);
            AddInClause(ref where, "BoxName",      filter.BoxName);
            AddInClause(ref where, "City",         filter.City);
            AddInClause(ref where, "State",        filter.State);
            AddInClause(ref where, "Country",      filter.Country);

            // --- Numeric range filters ---
            if (filter.CtsFrm != 0 && filter.CtsTo != 0)
                where += $" And Round((CONVERT(decimal(9,3),TotCarats + MemoIn)),3,1) between {filter.CtsFrm} And {filter.CtsTo}";

            if (filter.BackFrm != 0 && filter.BackTo != 0)
                where += $" And (Back between {filter.BackFrm} And {filter.BackTo})";

            if (filter.PriceFrm != 0 && filter.PriceTo != 0)
                where += $" And (Rate between {filter.PriceFrm} And {filter.PriceTo})";

            if (filter.LotFrm != 0 && filter.LotTo != 0)
                where += $" And (ItemSrno between {filter.LotFrm} And {filter.LotTo})";

            if (filter.TDepthFrm != 0 && filter.TDepthTo != 0)
                where += $" And (TDep between {filter.TDepthFrm} And {filter.TDepthTo})";

            if (filter.TablePerFrm != 0 && filter.TablePerTo != 0)
                where += $" And ([Tab%] between {filter.TablePerFrm} And {filter.TablePerTo})";

            if (filter.CAFrm != 0 && filter.CATo != 0)
                where += $" And (CA between {filter.CAFrm} And {filter.CATo})";

            if (filter.CHFrm != 0 && filter.CHTo != 0)
                where += $" And (CH between {filter.CHFrm} And {filter.CHTo})";

            if (filter.PHFrm != 0 && filter.PHTo != 0)
                where += $" And (PH between {filter.PHFrm} And {filter.PHTo})";

            if (filter.PAFrm != 0 && filter.PATo != 0)
                where += $" And (PA between {filter.PAFrm} And {filter.PATo})";

            // --- Date range filters (only when checkbox is true AND dates are valid) ---
            if (filter.PurchaseChkBox && IsValidDate(filter.FrmPurchaseDate) && IsValidDate(filter.ToPurchaseDate))
                where += $" And (convert(date,PDate,111) Between '{filter.FrmPurchaseDate}' And '{filter.ToPurchaseDate}') ";

            if (filter.ReviseCheckBox && IsValidDate(filter.FrmReviseDate) && IsValidDate(filter.ToReviseDate))
                where += $" And (convert(date,RDate,111) Between '{filter.FrmReviseDate}' And '{filter.ToReviseDate}') ";

            if (filter.NewArrivalChkBox && IsValidDate(filter.FrmNewArrivalDate) && IsValidDate(filter.ToNewArrivalDate))
                where += $" And (convert(date,NewDate,111) Between '{filter.FrmNewArrivalDate}' And '{filter.ToNewArrivalDate}') ";

            if (filter.WebRevisedChkBox && IsValidDate(filter.FrmWebRevisedDate) && IsValidDate(filter.ToWebRevisedDate))
                where += $" And (convert(date,WebRDate,111) Between '{filter.FrmWebRevisedDate}' And '{filter.ToWebRevisedDate}') ";

            if (filter.UnholdDateChkBox && IsValidDate(filter.FrmUnholdDate) && IsValidDate(filter.ToUnholdDate))
                where += $" And (convert(date,UnholdDate,111) Between '{filter.FrmUnholdDate}' And '{filter.ToUnholdDate}') ";

            if (filter.LabDateChkBox && IsValidDate(filter.FrmLabDate) && IsValidDate(filter.ToLabDate))
                where += $" And (convert(date,GRecDate,111) Between '{filter.FrmLabDate}' And '{filter.ToLabDate}') ";

            // --- LotNo / CertNo / SrNo / KapanNo / PacketNo text filter ---
            if (!string.IsNullOrWhiteSpace(filter.LotNo))
            {
                string lotNoClean = filter.LotNo.Replace("\r\n", ",").Replace("\n", ",").Trim();
                var items = lotNoClean
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => i.Trim())
                    .Where(i => !string.IsNullOrEmpty(i))
                    .ToList();

                if (items.Count > 0)
                {
                    string inList = string.Join("','", items.Select(i => i.Replace("'", "''")));

                    switch (filter.Filters)
                    {
                        case "LotNo":
                            where += $" AND (LotNo IN ('{inList}'))"; break;
                        case "LotNo Not":
                        case "LotNoNot":
                            where += $" AND (LotNo NOT IN ('{inList}'))"; break;
                        case "LotNo Like":
                        case "LotNoLike":
                            where += $" AND (LotNo Like ('{inList}%'))"; break;
                        case "CertNo":
                            where += $" AND ([C.No.] IN ('{inList}'))"; break;
                        case "CertNo Not":
                        case "CertNoNot":
                            where += $" AND ([C.No.] NOT IN ('{inList}'))"; break;
                        case "SrNo":
                            where += $" AND (ItemSrNo IN ('{inList}'))"; break;
                        case "SrNo Not":
                        case "SrNoNot":
                            where += $" AND (ItemSrNo NOT IN ('{inList}'))"; break;
                        case "KapanNo":
                            where += $" AND (Kapan IN ('{inList}'))"; break;
                        case "KapanNo Not":
                        case "KapanNoNot":
                            where += $" AND (Kapan NOT IN ('{inList}'))"; break;
                        case "PacketNo":
                            where += $" AND (Packet IN ('{inList}'))"; break;
                        case "PacketNo Not":
                        case "PacketNoNot":
                            where += $" AND (Packet NOT IN ('{inList}'))"; break;
                    }
                }
            }

            

            return where;
        }

        private string BuildAskingQuery(ParcelStockSearchReportFilterDto filter)
        {
            if (filter.BackPer != 0)
                return $" Rapa+(Rapa*({filter.BackPer})/100) AS AskingRate,(Rapa+(Rapa*({filter.BackPer})/100)) * Round((CONVERT(decimal(9,3),TotCarats + MemoIn)),3,1) AS AskingAmount,{filter.BackPer} As AskingBack ";

            if (filter.BackPlusMinus != 0)
                return $" Rapa+(Rapa*(ViewStockReport.Back+{filter.BackPlusMinus})/100) AS AskingRate,(Rapa+(Rapa*(ViewStockReport.Back+{filter.BackPlusMinus})/100)) * Round((CONVERT(decimal(9,3),TotCarats + MemoIn)),3,1) AS AskingAmount,ViewStockReport.Back+{filter.BackPlusMinus} As AskingBack ";

            if (filter.RatePer != 0)
                return $" (ROUND(((Rate+(Rate*({filter.RatePer})/100))/10)+.49,0))*10 AS AskingRate,(ROUND(((Rate+(Rate*({filter.RatePer})/100))/10)+.49,0))*10 * Round((CONVERT(decimal(9,3),TotCarats + MemoIn)),3,1) AS AskingAmount,CASE WHEN Rapa = 0 OR Rapa IS NULL THEN 0 ELSE (ROUND(((Rate+(Rate*({filter.RatePer})/100))/10)+.49,0))*10 * 100 / Rapa - 100 END AS AskingBack ";

            if (filter.RatePlusMinus != 0)
                return $" Rate+{filter.RatePlusMinus} AS AskingRate,((Rate+{filter.RatePlusMinus}) * Round((CONVERT(decimal(9,3),TotCarats + MemoIn)),3,1)) AS AskingAmount,CASE WHEN Rapa = 0 OR Rapa IS NULL THEN 0 ELSE ((((Rate+{filter.RatePlusMinus})*100)/Rapa)-100) END As AskingBack ";

            return " Rate AS AskingRate,Back As AskingBack,Round((CONVERT(decimal(18,2),Round((CONVERT(decimal(18,3),TotCarats + MemoIn)),3,1) * ISNULL(Rate,0))),2,1) AS AskingAmount ";
        }

        private void AddInClause(ref string where, string columnName, List<string>? list)
        {
            if (list == null || list.Count == 0) return;
            string inClause = BuildInClauseString(list);
            if (!string.IsNullOrEmpty(inClause))
                where += $" And {columnName} In ({inClause})";
        }

        private string BuildInClauseString(List<string> list)
        {
            var escaped = list.Select(el => $"'{el.Replace("'", "''")}'");
            return string.Join(",", escaped);
        }

        private static bool IsValidDate(string? value)
            => !string.IsNullOrWhiteSpace(value) && DateTime.TryParse(value, out _);

        #endregion

        public async Task<IEnumerable<BasicDataDto>> GetParcelLotNoListAsync(string searchby, string companyId)
        {
            return await _repository.GetParcelLotNoListAsync(searchby, companyId);
        }

        public async Task<bool> UpdateLocationAsync(ParcelStockLocationUpdateDTO dto, string companyId)
        {
            return await _repository.UpdateLocationAsync(dto, companyId);
        }

        public async Task<bool> UpdateBoxAndGroupAsync(ParcelStockBoxGroupUpdateDTO dto, string companyId)
        {
            return await _repository.UpdateBoxAndGroupAsync(dto, companyId);
        }
    }
}