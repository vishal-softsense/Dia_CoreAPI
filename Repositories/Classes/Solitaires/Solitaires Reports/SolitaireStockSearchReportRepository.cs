using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Solitaires.Solitaires_Reports;
using Dia_CoreAPI.DTOs.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports;

namespace Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Reports
{
    public class SolitaireStockSearchReportRepository : ISolitaireStockSearchReportRepository
    {
        private readonly ISqlDataAccess _db;

        public SolitaireStockSearchReportRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<IEnumerable<SolitaireStockSearchReportResponseDTO>> GetStockSearchReportAsync(SolitaireStockSearchReportDTO filter, string companyId)
        {
            var (dateVal, where, askingQry) = BuildQueryParams(filter);
            
            var dbParams = new Dictionary<string, object>
            {
                { "@Date", $"'{dateVal}'" },
                { "@AskingQry", askingQry },
                { "@Where", where.Replace("''", "'") },
                { "@ActionType", "StockReport" }
            };

            return await _db.SelectDataFromSP<SolitaireStockSearchReportResponseDTO>("GetStockReportData", dbParams, companyId);
        }



        public async Task<IEnumerable<BasicDataDto>> GetStockSearchDropdownAsync(string companyId)
        {
            return await _db.SelectDataFromSP<BasicDataDto>("AllMasterDataForAdminStockSearch", new Dictionary<string, object>(), companyId);
        }

        private (string dateVal, string where, string askingQry) BuildQueryParams(SolitaireStockSearchReportDTO filter)
        {
            string dateVal = string.IsNullOrEmpty(filter.Date) ? DateTime.Now.ToString("yyyy-MM-dd") : filter.Date;
            string where = "";

            // Show Filter
            string? show = filter.Show;
            if (!string.IsNullOrEmpty(show) && show != "All")
            {
                if (show == "Sold")
                {
                    where += " And (CurStatus = ''Sold'' or Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) = 0) ";
                }
                else if (show == "Unsold")
                {
                    where += " And (totcarats  + memoIn > 0.05 OR OnHandCarats > 0.05) ";
                }
            }

            // Hold/Unhold Filter
            string? holdUnHold = filter.HoldUnHold;
            if (!string.IsNullOrEmpty(holdUnHold) && holdUnHold != "All")
            {
                if (holdUnHold == "Hold")
                {
                    where += " And NotShow = 1 ";
                }
                else if (holdUnHold == "Unhold")
                {
                    where += " And NotShow = 0 ";
                }
            }

            // StoneTypes Filter
            string? stoneTypes = filter.StoneTypes;
            if (!string.IsNullOrEmpty(stoneTypes) && stoneTypes != "All")
            {
                if (stoneTypes == "Single")
                {
                    where += " And  singlepcs = 1 ";
                }
                else if (stoneTypes == "Parcel")
                {
                    where += " And  singlepcs = 0 ";
                }
            }

            // Status Filter
            string? status = filter.Status;
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                if (status == "OnHand")
                {
                    where += " And onhand>0.01 And CurStatus in (''S'') ";
                }
                else if (status == "OnMemo")
                {
                    where += " And CurStatus in (''M'',''I'',''L'',''PS'') ";
                }
                else if (status == "OnHand&Int.Memo")
                {
                    where += " And CurStatus in (''S'',''I'') ";
                }
                else if (status == "OnHand + Memo In")
                {
                    where += " And CurStatus in (''S'',''MI'') ";
                }
                else if (status == "OnHand + Party Hold")
                {
                    where += " And onhand>0.01 And CurStatus in (''S'',''PH'') ";
                }
                else if (status == "Memo In")
                {
                    where += " And CurStatus in (''MI'') ";
                }
                else if (status == "Party Hold")
                {
                    where += " And CurStatus in (''PH'') ";
                }
                else if (status == "All - Party Hold")
                {
                    where += " And CurStatus not in (''PH'') ";
                }
            }

            // Asking Query Construction
            float backPer = filter.BackPer;
            float backPlusMinus = filter.BackPlusMinus;
            float ratePer = filter.RatePer;
            float ratePlusMinus = filter.RatePlusMinus;
            string askingQry = "";

            if (backPer != 0)
            {
                askingQry = " Rapa+(Rapa*(" + backPer + ")/100) AS AskingRate,(Rapa+(Rapa*(" + backPer + ")/100)) * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) AS AskingAmount," + backPer + " As AskingBack ";
            }
            else if (backPlusMinus != 0)
            {
                askingQry = " Rapa+(Rapa*(ViewStockReport.back+" + backPlusMinus + ")/100) AS AskingRate,(Rapa+(Rapa*(ViewStockReport.back+" + backPlusMinus + ")/100)) * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) AS AskingAmount,ViewStockReport.Back+" + backPlusMinus + " As AskingBack ";
            }
            else if (ratePer != 0)
            {
                askingQry = " (ROUND(((Rate+(Rate*(" + ratePer + ")/100))/10)+.49,0))*10 AS AskingRate,(ROUND(((Rate+(Rate*(" + ratePer + ")/100))/10)+.49,0))*10 * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) AS AskingAmount,CASE WHEN Rapa = 0 OR Rapa IS NULL THEN 0 ELSE (ROUND(((Rate+(Rate*(" + ratePer + ")/100))/10)+.49,0))*10 * 100 / Rapa - 100 END AS AskingBack ";
            }
            else if (ratePlusMinus != 0)
            {
                askingQry = " Rate+" + ratePlusMinus + " AS AskingRate, ((rate+" + ratePlusMinus + ") * Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1)) AS AskingAmount,CASE WHEN Rapa = 0 OR Rapa IS NULL THEN 0 ELSE ((((Rate+" + ratePlusMinus + " )*100) / Rapa) - 100) END As AskingBack ";
            }
            else
            {
                askingQry = " Rate AS AskingRate,Back As AskingBack,Round((CONVERT(decimal(18, 2), Round((CONVERT(decimal(18, 2), TotCarats + MemoIn)), 3, 1) * ISNULL(Rate, 0))), 2, 1) AS AskingAmount ";
            }

            // Multi-Select In Clause Builders
            AddInClause(ref where, "ShapeGroupId", filter.ShapeGroup);
            AddInClause(ref where, "Shp", filter.Shape);
            AddInClause(ref where, "ColorGroupId", filter.ColorGroup);
            AddInClause(ref where, "Col", filter.Color);
            AddInClause(ref where, "Size", filter.Size);
            AddInClause(ref where, "Cla", filter.Clarity);
            AddInClause(ref where, "Prop", filter.Proportion);
            AddInClause(ref where, "Pol", filter.Polish);
            AddInClause(ref where, "Sym", filter.Symmetry);
            AddInClause(ref where, "Cert", filter.LAB);
            AddInClause(ref where, "HA", filter.HA);
            AddInClause(ref where, "City", filter.Location);

            // Fluorescence Special Handling
            if (filter.Fluorescence != null && filter.Fluorescence.Count > 0)
            {
                if (filter.Fluorescence.Contains("IS_NULL"))
                {
                    where += " And FL IS NULL ";
                }
                else
                {
                    string fluorescenceVal = BuildInClauseString(filter.Fluorescence);
                    if (!string.IsNullOrEmpty(fluorescenceVal))
                    {
                        where += " And FL In ( " + fluorescenceVal + ")";
                    }
                }
            }

            AddInClause(ref where, "[Ml.]", filter.Milky);
            AddInClause(ref where, "[LS.]", filter.Luster);
            AddInClause(ref where, "[Nts.]", filter.Natts);
            AddInClause(ref where, "Shade", filter.Shade);
            AddInClause(ref where, "FLColor", filter.FlColor);
            AddInClause(ref where, "ItemgroupId", filter.ItemGroup);
            AddInClause(ref where, "BoxName", filter.BoxNm);

            // Numeric Ranges
            if (filter.CtsFrm != 0 && filter.CtsTo != 0)
            {
                where += " And Round((CONVERT(decimal(9,3),TotCarats + MemoIn )),3,1) between " + filter.CtsFrm + " And " + filter.CtsTo + "";
            }
            if (filter.BackFrm != 0 && filter.BackTo != 0)
            {
                where += " And (Back between " + filter.BackFrm + " And " + filter.BackTo + ")";
            }
            if (filter.PriceFrm != 0 && filter.PriceTo != 0)
            {
                where += " And (Rate between " + filter.PriceFrm + " And " + filter.PriceTo + ")";
            }
            if (filter.LotFrm != 0 && filter.LotTo != 0)
            {
                where += " And (ItemSrno between " + filter.LotFrm + " And " + filter.LotTo + ")";
            }
            if (filter.TDepthFrm != 0 && filter.TDepthTo != 0)
            {
                where += " And (TDep between " + filter.TDepthFrm + " And " + filter.TDepthTo + ")";
            }
            if (filter.TablePerFrm != 0 && filter.TablePerTo != 0)
            {
                where += " And ([Tab%] between " + filter.TablePerFrm + " And " + filter.TablePerTo + ")";
            }
            if (filter.CAFrm != 0 && filter.CATo != 0)
            {
                where += " And (CA between " + filter.CAFrm + " And " + filter.CATo + ")";
            }
            if (filter.CHFrm != 0 && filter.CHTo != 0)
            {
                where += " And (CH between " + filter.CHFrm + " And " + filter.CHTo + ")";
            }
            if (filter.PHFrm != 0 && filter.PHTo != 0)
            {
                where += " And (PH between " + filter.PHFrm + " And " + filter.PHTo + ")";
            }
            if (filter.PAFrm != 0 && filter.PATo != 0)
            {
                where += " And (PA between " + filter.PAFrm + " And " + filter.PATo + ")";
            }

            // Date Ranges
            if (filter.PurchaseChkBox)
            {
                where += " And (convert(date,PDate,111) Between ''" + filter.FrmPurchaseDate + "'' And ''" + filter.ToPurchaseDate + "'' ) ";
            }
            if (filter.ReviseCheckBox)
            {
                where += " And (convert(date,RDate,111) Between ''" + filter.FrmReviseDate + "'' And ''" + filter.ToReviseDate + "'' ) ";
            }
            if (filter.NewArrivalChkBox)
            {
                where += " And (convert(date,NewDate,111) Between ''" + filter.FrmNewArrivalDate + "'' And ''" + filter.ToNewArrivalDate + "'' ) ";
            }
            if (filter.WebRevisedChkBox)
            {
                where += " And (convert(date,WebRDate,111) Between ''" + filter.FrmWebRevisedDate + "'' And ''" + filter.ToWebRevisedDate + "'' ) ";
            }
            if (filter.UnholdDateChkBox)
            {
                where += " And (convert(date,UnholdDate,111) Between ''" + filter.FrmUnholdDate + "'' And ''" + filter.ToUnholdDate + "'' ) ";
            }
            if (filter.LabDateChkBox)
            {
                where += " And (convert(date,GRecDate,111) Between ''" + filter.FrmLabDate + "'' And ''" + filter.ToLabDate + "'' ) ";
            }

            // Text LotNo / CertNo Filters
            if (!string.IsNullOrEmpty(filter.LotNo))
            {
                string lotNoClean = filter.LotNo.Replace("\r\n", ",").Replace("\n", ",").Trim();
                var items = lotNoClean.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(i => i.Trim())
                                      .Where(i => !string.IsNullOrEmpty(i));
                
                if (items.Any())
                {
                    string strTmpNewLine = string.Join("'',''", items.Select(i => i.Replace("'", "''")));

                    if (filter.Filters == "LotNo")
                    {
                        where += " AND (LotNo IN (''" + strTmpNewLine + "''))";
                    }
                    else if (filter.Filters == "LotNo Not" || filter.Filters == "LotNoNot")
                    {
                        where += " AND (LotNo NOT IN (''" + strTmpNewLine + "''))";
                    }
                    else if (filter.Filters == "LotNo Like" || filter.Filters == "LotNoLike")
                    {
                        where += " AND (LotNo Like (''" + strTmpNewLine + "%''))";
                    }
                    else if (filter.Filters == "CertNo")
                    {
                        where += " AND ([C.No.] IN (''" + strTmpNewLine + "''))";
                    }
                    else if (filter.Filters == "CertNo Not" || filter.Filters == "CertNoNot")
                    {
                        where += " AND ([C.No.] NOT IN (''" + strTmpNewLine + "''))";
                    }
                    else if (filter.Filters == "SrNo")
                    {
                        where += " AND (ItemSrNo IN (''" + strTmpNewLine + "''))";
                    }
                    else if (filter.Filters == "SrNo Not" || filter.Filters == "SrNoNot")
                    {
                        where += " AND (ItemSrNo NOT IN (''" + strTmpNewLine + "''))";
                    }
                    else if (filter.Filters == "KapanNo")
                    {
                        where += " AND (Kapan IN (''" + strTmpNewLine + "''))";
                    }
                    else if (filter.Filters == "KapanNo Not" || filter.Filters == "KapanNoNot")
                    {
                        where += " AND (Kapan NOT IN (''" + strTmpNewLine + "''))";
                    }
                    else if (filter.Filters == "PacketNo")
                    {
                        where += " AND (Packet IN (''" + strTmpNewLine + "''))";
                    }
                    else if (filter.Filters == "PacketNo Not" || filter.Filters == "PacketNoNot")
                    {
                        where += " AND (Packet NOT IN (''" + strTmpNewLine + "''))";
                    }
                }
            }

            // Ordering / Sorting Wise
            string? filterSize = filter.FilterSize;
            if (!string.IsNullOrEmpty(filterSize))
            {
                if (filterSize == "Sort On LAB-Shape-Size-Color-Clarity-LotNo")
                {
                    where += " Order By CertPriority,ShapePriority,SizePriority,ColorPriority,ClarityPriority,PropPriority,PolishPriority,SymPriority,FLPriority,TotCarats+MemoIn Desc,LotNo";
                }
                else if (filterSize == "Sort On LAB-Shape-Color-Size-Clarity-LotNo")
                {
                    where += " Order By CertPriority,ShapePriority,ColorPriority,SizePriority,ClarityPriority,PropPriority,PolishPriority,SymPriority,TotCarats+MemoIn Desc,LotNo";
                }
                else if (filterSize == "Sort On LAB-Shape-Size-Clarity-Color-LotNo")
                {
                    where += " Order By CertPriority,ShapePriority,SizePriority,ClarityPriority,ColorPriority,PropPriority,PolishPriority,SymPriority,TotCarats+MemoIn Desc,LotNo";
                }
                else if (filterSize == "Sort On Shape-LAB-Size-Color-Clarity-LotNo")
                {
                    where += " Order By ShapePriority,CertPriority,SizePriority,ColorPriority,ClarityPriority,PropPriority,PolishPriority,SymPriority,TotCarats+MemoIn Desc,LotNo";
                }
                else if (filterSize == "Sort On Shape-LAB-Color-Size-Clarity-LotNo")
                {
                    where += " Order By ShapePriority,CertPriority,ColorPriority,SizePriority,ClarityPriority,PropPriority,PolishPriority,SymPriority,TotCarats+MemoIn Desc,LotNo";
                }
                else if (filterSize == "Sort On Shape-LAB-Size-Clarity-Color-LotNo")
                {
                    where += " Order By ShapePriority,CertPriority,SizePriority,ClarityPriority,ColorPriority,PropPriority,PolishPriority,SymPriority,TotCarats+MemoIn Desc,LotNo";
                }
                else if (filterSize == "Sort On Shape-Size-Clarity-Color-LotNo")
                {
                    where += " Order By ShapePriority,SizePriority,ClarityPriority,ColorPriority,PropPriority,PolishPriority,SymPriority,TotCarats+MemoIn Desc,LotNo";
                }
                else if (filterSize == "Sort On Shape-Size-Color-Clarity-LotNo")
                {
                    where += " Order By ShapePriority,SizePriority,ColorPriority,ClarityPriority,PropPriority,PolishPriority,SymPriority,FLPriority,TotCarats+MemoIn Desc,LotNo";
                }
                else if (filterSize == "Sort On Shape-Size-Color-Clarity-Cut-Polish-Sym-Fl")
                {
                    where += " Order By ShapePriority,SizePriority,ColorPriority,ClarityPriority,PropPriority,PolishPriority,SymPriority,FLPriority,TotCarats+MemoIn Asc,LotNo ";
                }
                else if (filterSize == "Sort On Kapan-LotNo")
                {
                    where += " Order By Kapan,LotNo";
                }
                else if (filterSize == "Sort On ItemCode")
                {
                    where += " Order By ItemCode";
                }
                else if (filterSize == "Sort On Item Serial No")
                {
                    where += " Order By ItemSrNo";
                }
                else if (filterSize == "Sort On Stone Status & Lotno")
                {
                    where += " Order By Lotno";
                }
                else if (filterSize == "Sort On Size Wise")
                {
                    where += " Order By SizePriority,TotCarats+MemoIn Asc,LotNo";
                }
                else if (filterSize == "Sort On Size-Color-Clarity-Lotno")
                {
                    where += " Order By SizePriority desc,ClarityPriority,ColorPriority";
                }
                else if (filterSize == "Sort On ItemGroup Wise")
                {
                    where += " Order By ItemGroupId desc";
                }
                else if (filterSize == "Sort On Shape-Size-Clarity-Color")
                {
                    where += " Order By ShapePriority,SizePriority,ClarityPriority,ColorPriority";
                }
            }

            return (dateVal, where, askingQry);
        }

        private void AddInClause(ref string where, string columnName, List<string>? list)
        {
            if (list == null || list.Count == 0) return;
            string inClause = BuildInClauseString(list);
            if (!string.IsNullOrEmpty(inClause))
            {
                where += $" And {columnName} In ( {inClause})";
            }
        }

        private string BuildInClauseString(List<string> list)
        {
            var escaped = list.Select(el => $"''{el.Replace("'", "''")}''");
            return string.Join(",", escaped);
        }

        public async Task<bool> UpdateLocationAsync(LocationUpdateDTO dto, string companyId)
        {
            if (dto.LotNumbers == null || dto.LotNumbers.Count == 0)
                return false;

            foreach (var lot in dto.LotNumbers)
            {
                var param = new Dictionary<string, object>
                {
                    { "@City", dto.City ?? "" },
                    { "@Country", dto.Country ?? "" },
                    { "@State", dto.State ?? "" },
                    { "@Lot", lot },
                    { "@Type", "location" }
                };

                await _db.ExecuteSP("SP_InsertUpdateLocation", param, companyId);
            }
            return true;
        }

        public async Task<bool> UpdateBoxAndGroupAsync(BoxGroupUpdateDTO dto, string companyId)
        {
            if (dto.LotNumbers == null || dto.LotNumbers.Count == 0)
                return false;

            foreach (var lot in dto.LotNumbers)
            {
                var param = new Dictionary<string, object>
                {
                    { "@boxname", dto.BoxName ?? "" },
                    { "@groupname", dto.Group ?? "" },
                    { "@Lot", lot },
                    { "@Type", "box-group-update" }
                };

                await _db.ExecuteSP("SP_InsertUpdateLocation", param, companyId);
            }
            return true;
        }

        public async Task<IEnumerable<BasicDataDto>> GetSolitaireLotNoListAsync(string searchby, string companyId)
        {
            string strWhere = " AND ISNULL(SinglePcs,0) = 1 ";
            if (!string.IsNullOrEmpty(searchby))
            {
                var cleaned = searchby.Replace("\r\n", ",").Replace("\n", ",").Trim();
                var items = cleaned.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(i => i.Trim())
                                   .Where(i => !string.IsNullOrEmpty(i))
                                   .ToList();

                if (items.Any())
                {
                    if (items.Count == 1)
                    {
                        string safeItem = items[0].Replace("'", "''");
                        strWhere += $" AND (Name LIKE ''%{safeItem}%'' OR TextName LIKE ''%{safeItem}%'') ";
                    }
                    else
                    {
                        string joined = string.Join("','", items.Select(i => i.Replace("'", "''")));
                        strWhere += " AND Name IN ('" + joined + "') ";
                    }
                }
            }

            var dbParams = new Dictionary<string, object>
            {
                { "@strWhere", strWhere }
            };

            return await _db.SelectDataFromSP<BasicDataDto>("GetDiamondLotDropdown", dbParams, companyId);
        }

        public async Task<bool> SendStockListEmailAsync(SendSolitaireStockEmailDTO dto, string companyId)
        {
            string email = await _db.ExecuteQuerySingle<string>(
                "SELECT Value FROM TabSettingMaster WHERE Name = 'Email'",
                new { },
                companyId) ?? "";

            string password = await _db.ExecuteQuerySingle<string>(
                "SELECT Value FROM TabSettingMaster WHERE Name = 'Password'",
                new { },
                companyId) ?? "";

            if (string.IsNullOrEmpty(email))
            {
                throw new Exception("Sender email is not configured in TabSettingMaster.");
            }

            try
            {
                using (var client = new System.Net.Mail.SmtpClient("smtpout.secureserver.net", 587))
                {
                    client.EnableSsl = true;
                    client.Timeout = 100000;
                    client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential(email, password);

                    using (var mailMessage = new System.Net.Mail.MailMessage(email, dto.To ?? ""))
                    {
                        mailMessage.Subject = dto.Subject;
                        string formattedBody = dto.Body ?? "";
                        formattedBody = System.Text.RegularExpressions.Regex.Replace(formattedBody, @"\r?\n", "<br />");
                        mailMessage.Body = formattedBody;
                        mailMessage.IsBodyHtml = true;

                        if (!string.IsNullOrEmpty(dto.CC))
                        {
                            foreach (var cc in dto.CC.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                mailMessage.CC.Add(cc.Trim());
                            }
                        }

                        if (!string.IsNullOrEmpty(dto.BCC))
                        {
                            foreach (var bcc in dto.BCC.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                mailMessage.Bcc.Add(bcc.Trim());
                            }
                        }

                        if (!string.IsNullOrEmpty(dto.AttachmentBase64))
                        {
                            byte[] fileBytes = Convert.FromBase64String(dto.AttachmentBase64);
                            var stream = new System.IO.MemoryStream(fileBytes);
                            var attachment = new System.Net.Mail.Attachment(stream, dto.AttachmentFileName ?? "StockList.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                            mailMessage.Attachments.Add(attachment);
                        }

                        await client.SendMailAsync(mailMessage);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send email: {ex.Message}", ex);
            }
        }

        public async Task<EmailSettingsDTO> GetEmailSettingsAsync(string companyId)
        {
            string email = await _db.ExecuteQuerySingle<string>(
                "SELECT Value FROM TabSettingMaster WHERE Name = 'Email'",
                new { },
                companyId) ?? "";

            string companyName = await _db.ExecuteQuerySingle<string>(
                "SELECT Value FROM TabSettingMaster WHERE Name = 'CompanyNameforInvoice'",
                new { },
                companyId) ?? "";

            string body = $"Please refer attached Stock List as attached .pdf file.\r\n\r\nPlease feel free to connect us for any query.\r\n\r\nRegards.\r\n\r\n{companyName}\r\n\r\nPowered By www.SoftOnCloud.com";

            return new EmailSettingsDTO
            {
                FromEmail = email,
                Subject = "StockList as per Inquiry",
                Body = body
            };
        }

        public async Task<IEnumerable<SolitaireStockSearchHistoryRowDto>> GetStockSearchHistoryAsync(
            SolitaireStockSearchHistoryFilterDto filter,
            string companyId)
        {
            var (stkInStr, stkOutStr) = BuildStockSearchHistoryTrnTypes(ResolveSelectedTypes(filter));
            var whereNew = BuildStockSearchHistoryWhere(filter);

            var fromDate = DateTime.Parse(filter.FromDate ?? "2020-01-01").ToString("yyyy-MM-dd");
            var toDate = DateTime.Parse(filter.ToDate ?? "2026-12-31").ToString("yyyy-MM-dd");

            // MVC ReportProcessor.ParcelGetStockRegisterReport — inline EXEC with doubled-quote params
            var sql =
                " EXEC [SP_StockRegisterReport] @Type = 'SalePurAvgPrice', @StkIn = '" + stkInStr +
                "', @StkOut = '" + stkOutStr +
                "', @WhereNew = '" + whereNew +
                "', @FromDate= '" + fromDate +
                "',@ToDate= '" + toDate + "'";

            var rows = await _db.SelectDataDapperDynamic(sql, companyId);
            return rows.Select(MapStockSearchHistoryRow);
        }

        private static List<string> ResolveSelectedTypes(SolitaireStockSearchHistoryFilterDto filter)
        {
            if (filter.SelectedTypes != null && filter.SelectedTypes.Count > 0)
                return filter.SelectedTypes;

            return new List<string> { "ALL" };
        }

        private static SolitaireStockSearchHistoryRowDto MapStockSearchHistoryRow(dynamic row)
        {
            var dict = (IDictionary<string, object>)row;

            string GetString(params string[] keys)
            {
                foreach (var key in keys)
                {
                    foreach (var pair in dict)
                    {
                        if (!string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
                            continue;

                        if (pair.Value == null || pair.Value == DBNull.Value)
                            continue;

                        return Convert.ToString(pair.Value, CultureInfo.InvariantCulture) ?? "";
                    }
                }

                return "";
            }

            decimal GetDecimal(params string[] keys)
            {
                foreach (var key in keys)
                {
                    foreach (var pair in dict)
                    {
                        if (!string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
                            continue;

                        if (pair.Value == null || pair.Value == DBNull.Value)
                            continue;

                        return Convert.ToDecimal(pair.Value, CultureInfo.InvariantCulture);
                    }
                }

                return 0;
            }

            var trnType = GetString("TrnType");
            var particulars = GetString(
                "Particulars",
                "PartyName",
                "AccName",
                "RelatedRemark",
                "Perticulors");

            if (string.IsNullOrWhiteSpace(particulars) && trnType == "OpStk")
                particulars = "OpStk";

            return new SolitaireStockSearchHistoryRowDto
            {
                TrnDate = GetString("TrnDate"),
                TrnType = trnType,
                Particulars = particulars,
                StockIn = GetDecimal("StockIn"),
                StockInPrice = GetDecimal("StockInPrice"),
                StockInAmount = GetDecimal("StockInAmount"),
                StockOut = GetDecimal("StockOut"),
                StockOutPrice = GetDecimal("StockOutPrice"),
                StockOutAmount = GetDecimal("StockOutAmount"),
                BalanceStock = GetDecimal("BalanceStock"),
                BalancePrice = GetDecimal("BalancePrice"),
                BalanceAmount = GetDecimal("BalanceAmount")
            };
        }

        private static string BuildStockSearchHistoryWhere(SolitaireStockSearchHistoryFilterDto filter)
        {
            var strWhere = " ";

            if (!string.IsNullOrWhiteSpace(filter.LotNo))
            {
                var lotList = filter.LotNo
                    .Replace("\r\n", ",")
                    .Replace("\n", ",")
                    .Replace("'", "''")
                    .Replace(",", "'',''");

                strWhere += " AND (v.StockID IN (''" + lotList + "''))";
            }

            if (filter.StkEff == false)
                strWhere += " AND StockEffected = 1 ";

            return strWhere;
        }

        private static (string stkInStr, string stkOutStr) BuildStockSearchHistoryTrnTypes(List<string>? selectedTypes)
        {
            var stkIn = new List<string> { "OpStk" };
            var stkOut = new List<string>();

            if (selectedTypes == null || selectedTypes.Count == 0)
                selectedTypes = new List<string> { "ALL" };

            foreach (var type in selectedTypes)
            {
                switch (type)
                {
                    case "ALL":
                        stkIn.AddRange(new[]
                        {
                            "OpStk", "PPUR", "CPUR", "PSaleRtn", "CSaleRtn",
                            "PMemoIn", "CMemoIn", "MemoRtnP", "StkTrn", "MixMemoRtnP"
                        });
                        stkOut.AddRange(new[]
                        {
                            "PSALE", "CSALE", "PPurRtn", "CPurrtn",
                            "PMemoOut", "CMemoOut", "MemoInRtnP", "StkTrn", "MixMemoInRtn"
                        });
                        break;

                    case "PUR": stkIn.Add("PPUR"); break;
                    case "PURRTN": stkOut.Add("PPurRtn"); break;
                    case "SALE": stkOut.Add("PSALE"); break;
                    case "SALERTN": stkIn.Add("PSaleRtn"); break;
                    case "MEMOIN": stkIn.Add("PMemoIn"); break;
                    case "MEMOINRTN": stkOut.Add("MemoInRtnP"); break;
                    case "MEMOOUT": stkOut.Add("PMemoOut"); break;
                    case "MEMOOUTRTN": stkIn.Add("MemoRtnP"); break;
                    case "GPUR": stkIn.Add("CPUR"); break;
                    case "GPURRTN": stkOut.Add("CPurrtn"); break;
                    case "GSALE": stkOut.Add("CSALE"); break;
                    case "GSALERTN": stkIn.Add("CSaleRtn"); break;
                    case "GMEMOIN": stkIn.Add("CMemoIn"); break;
                    case "GMEMOOUT": stkOut.Add("CMemoOut"); break;
                    case "GMEMOINRTN": stkOut.Add("MixMemoInRtn"); break;
                    case "GMEMOOUTRTN": stkIn.Add("MixMemoRtnP"); break;
                    case "STKTRN":
                        stkIn.Add("StkTrn");
                        stkOut.Add("StkTrn");
                        break;
                }
            }

            if (!stkIn.Any())
                stkIn.Add("__NA__");
            if (!stkOut.Any())
                stkOut.Add("__NA__");

            stkIn = stkIn.Distinct().ToList();
            stkOut = stkOut.Distinct().ToList();

            var stkInStr = string.Join(", ", stkIn.Select(x => $"''{x}''"));
            var stkOutStr = string.Join(", ", stkOut.Select(x => $"''{x}''"));
            return (stkInStr, stkOutStr);
        }
    }
}
