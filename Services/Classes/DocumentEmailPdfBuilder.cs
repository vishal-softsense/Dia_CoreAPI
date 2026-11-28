using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Dia_CoreAPI.DTOs.Parcel.Parcel_Transaction;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Dia_CoreAPI.Services.Classes
{
    public partial class DocumentEmailService
    {
        private const string FallbackMemoInformation =
            "The goods described and valued as below are delivered to you for examination and inspection only and remain our property and shall be returned to us on demand and in any event, such merchandise, until returned to us and actually received by us, is at your risk from all hazards, no right or power is given to you to sell, pledge, hypothetical or otherwise dispose of this merchandise regardless of prior transactions. A sale of this merchandise can only be effected and title will pass only if as and when we the said owners shall agree to such sale in writing and shall have billed you for the merchandise. All money received by you on the sale of the merchandise shall be held in trust for us until the full amount invoiced has been paid to us. The undersigned personally guarantee the below obligations on behalf of the company.";

        private const string FallbackMemoConditions =
            "The diamonds invoiced herein are purchased from legitimate sources, not involved in funding conflict & in compliance with the United Nations resolutions. The seller hereby agrees the assurances are based on personal knowledge and /or written assurances given by the supplier of these diamonds";

        private static string CollapseKey(string? value) =>
            Regex.Replace((value ?? string.Empty).Trim(), @"[\s_]+", "").ToLowerInvariant();

        private static string FormatDateSlash(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
                || DateTime.TryParse(raw, out dt))
                return dt.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            return raw.Trim();
        }

        private static string FormatDateLong(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
                || DateTime.TryParse(raw, out dt))
                return dt.ToString("d-MMMM-yyyy", CultureInfo.InvariantCulture);
            return raw.Trim();
        }

        private static string FormatDateMmm(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
                || DateTime.TryParse(raw, out dt))
                return dt.ToString("d-MMM-yyyy", CultureInfo.InvariantCulture);
            return raw.Trim();
        }

        private static string FormatMoney(double value, int decimals) =>
            value.ToString($"N{Math.Clamp(decimals, 0, 4)}", CultureInfo.InvariantCulture);

        private static List<List<T>> PaginateFixed<T>(List<T> items, int pageSize)
        {
            var pages = new List<List<T>>();
            if (items.Count == 0)
            {
                pages.Add(new List<T>());
                return pages;
            }
            for (int i = 0; i < items.Count; i += pageSize)
                pages.Add(items.GetRange(i, Math.Min(pageSize, items.Count - i)));
            return pages;
        }

        // =========================================================================
        // MIX MEMO OUT — resolves DEFAULT MIX MEMO PRINT FORMAT (registry.js rules)
        // =========================================================================
        private static byte[] BuildMixMemoPdf(MixMemoOutPrintViewDto data, CompanyEmailSettings company, byte[]? logoBytes)
        {
            var items = data.Items ?? new List<MixMemoOutPrintItemDto>();
            var rawFmt = !string.IsNullOrWhiteSpace(company.MixMemoPrintFormat)
                ? company.MixMemoPrintFormat
                : (!string.IsNullOrWhiteSpace(company.ParcelMemoPrintFormat) ? company.ParcelMemoPrintFormat : "MixMemoUSA");

            var key = CollapseKey(rawFmt);

            // Match registry.js lines 111-114:
            // - "MixMemoDual" -> Dual copy
            // - "MixMemoUSA" with < 11 items -> Dual copy (OFFICE COPY + CUSTOMER COPY on one Letter page)
            // - "MixMemoUSA_Stella" or "MixMemoUSA" with >= 11 items -> Full-page GENERAL MEMO
            bool useDual = key.Contains("dual") || (key == "mixmemousa" && items.Count < 11);

            return useDual
                ? BuildMixMemoDualPdf(data, items, company, logoBytes)
                : BuildMixMemoUsaPdf(data, items, company, logoBytes);
        }

        /// <summary>Matches React MixMemoDualFormat.js — OFFICE COPY + CUSTOMER COPY on one Letter page (10 rows/page).</summary>
        private static byte[] BuildMixMemoDualPdf(
            MixMemoOutPrintViewDto data,
            List<MixMemoOutPrintItemDto> items,
            CompanyEmailSettings company,
            byte[]? logoBytes)
        {
            const int pageSize = 10;
            var pages = PaginateFixed(items, pageSize);
            int dec = company.AmountDecimals;

            double totalWeight = items.Sum(i => i.Weight);
            double totalAmount = items.Sum(i => i.Amount > 0 ? i.Amount : i.MasterCurrencyAmount);
            double plusMinus = data.PlusMinusAmount;
            double finalAmount = totalAmount + plusMinus;
            string dateStr = FormatDateLong(data.TrnDate);
            string footerText = string.IsNullOrWhiteSpace(company.CompanyFooter) ? "Powered by SoftOnCloud" : company.CompanyFooter;

            void RenderCopy(ColumnDescriptor parent, string copyTitle, List<MixMemoOutPrintItemDto> pageItems, int pageIdx, bool isLastPage, bool isOffice)
            {
                int srNo = pageIdx * pageSize;

                parent.Item().PaddingBottom(isOffice ? 14 : 0).Column(copy =>
                {
                    // Header: Logo | Company Info | Copy Title + Date/Memo# table
                    copy.Item().Row(row =>
                    {
                        if (logoBytes != null && logoBytes.Length > 0)
                            row.ConstantItem(64).PaddingRight(6).AlignTop().Image(logoBytes).FitArea();

                        row.RelativeItem(5).Column(c =>
                        {
                            if (!string.IsNullOrWhiteSpace(company.CompanyName))
                                c.Item().Text(company.CompanyName).Bold().FontSize(10.5f);
                            if (!string.IsNullOrWhiteSpace(company.CompanyAddress))
                                c.Item().Text(company.CompanyAddress).FontSize(7.8f);
                            if (!string.IsNullOrWhiteSpace(company.CompanyAddress1))
                                c.Item().Text(company.CompanyAddress1).FontSize(7.8f);
                            if (!string.IsNullOrWhiteSpace(company.CompanyAddress2))
                                c.Item().Text(company.CompanyAddress2).FontSize(7.8f);
                            if (!string.IsNullOrWhiteSpace(company.CompanyPhone))
                                c.Item().Text(company.CompanyPhone).FontSize(7.8f);
                            if (!string.IsNullOrWhiteSpace(company.CompanyEmail))
                                c.Item().Text(company.CompanyEmail).FontSize(7.8f);
                        });

                        row.RelativeItem(4).AlignRight().Column(c =>
                        {
                            c.Item().AlignRight().Text(copyTitle).Bold().FontSize(14);
                            c.Item().PaddingTop(2).Table(t =>
                            {
                                t.ColumnsDefinition(cols => { cols.RelativeColumn(); cols.RelativeColumn(); });
                                t.Cell().Background("#CCCCCC").Border(0.6f).BorderColor("#A2A1A1").Padding(2).AlignCenter().Text("Date").Bold().FontSize(7.5f);
                                t.Cell().Background("#CCCCCC").Border(0.6f).BorderColor("#A2A1A1").Padding(2).AlignCenter().Text("Memo #").Bold().FontSize(7.5f);
                                t.Cell().Border(0.6f).BorderColor("#A2A1A1").Padding(2).AlignCenter().Text(string.IsNullOrWhiteSpace(dateStr) ? "-" : dateStr).FontSize(8f);
                                t.Cell().Border(0.6f).BorderColor("#A2A1A1").Padding(2).AlignCenter().Text(data.InvNo ?? "-").FontSize(8f);
                            });
                        });
                    });

                    // Memo To + Ship Via / Tracking No
                    copy.Item().PaddingTop(3).Row(row =>
                    {
                        row.RelativeItem(5).Border(0.6f).BorderColor("#A2A1A1").Column(c =>
                        {
                            c.Item().Background("#C0C0C0").BorderBottom(0.6f).BorderColor("#A2A1A1").Padding(2).AlignCenter().Text("Memo To").Bold().FontSize(8f);
                            c.Item().PaddingHorizontal(5).PaddingVertical(3).Column(p =>
                            {
                                p.Item().Text(data.PartyName ?? "-").Bold().FontSize(8.5f);
                                if (!string.IsNullOrWhiteSpace(data.AddressLine1)) p.Item().Text(data.AddressLine1).FontSize(7.5f);
                                if (!string.IsNullOrWhiteSpace(data.AddressLine2)) p.Item().Text(data.AddressLine2).FontSize(7.5f);
                                if (!string.IsNullOrWhiteSpace(data.AddressLine3)) p.Item().Text(data.AddressLine3).FontSize(7.5f);
                            });
                        });

                        row.ConstantItem(14);

                        row.RelativeItem(4).AlignTop().Table(t =>
                        {
                            t.ColumnsDefinition(cols => { cols.RelativeColumn(); cols.RelativeColumn(); });
                            t.Cell().Background("#CCCCCC").Border(0.6f).BorderColor("#A2A1A1").Padding(2).AlignCenter().Text("Ship Via").Bold().FontSize(7.5f);
                            t.Cell().Background("#CCCCCC").Border(0.6f).BorderColor("#A2A1A1").Padding(2).AlignCenter().Text("Tracking No").Bold().FontSize(7.5f);
                            t.Cell().Border(0.6f).BorderColor("#A2A1A1").Padding(2).MinHeight(14).AlignCenter().Text(data.ShipVia ?? "").FontSize(8f);
                            t.Cell().Border(0.6f).BorderColor("#A2A1A1").Padding(2).MinHeight(14).AlignCenter().Text(data.Tracking ?? "").FontSize(8f);
                        });
                    });

                    // 8-column Items table matching MixMemoDualFormat.js
                    copy.Item().PaddingTop(3).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(5);   // Sr No
                            cols.RelativeColumn(25);  // DESCRIPTION
                            cols.RelativeColumn(7);   // Pcs
                            cols.RelativeColumn(12);  // Carat / Qty.
                            cols.RelativeColumn(11);  // Price / Carat
                            cols.RelativeColumn(10);  // Return Cts
                            cols.RelativeColumn(10);  // Kept Cts
                            cols.RelativeColumn(20);  // Amount
                        });

                        IContainer Th(IContainer c) => c.Background("#C0C0C0").Border(0.6f).BorderColor("#A2A1A1").Padding(2).AlignCenter().AlignMiddle();
                        IContainer Td(IContainer c) => c.Border(0.5f).BorderColor("#A2A1A1").PaddingHorizontal(3).PaddingVertical(1.5f).MinHeight(12).AlignMiddle();
                        IContainer EmptyTd(IContainer c) => c.PaddingHorizontal(3).PaddingVertical(1.5f).MinHeight(12);

                        table.Header(h =>
                        {
                            h.Cell().Element(Th).Text("Sr No").Bold().FontSize(7.2f);
                            h.Cell().Element(Th).Text("DESCRIPTION").Bold().FontSize(7.2f);
                            h.Cell().Element(Th).Text("Pcs").Bold().FontSize(7.2f);
                            h.Cell().Element(Th).Text("Carat / Qty.").Bold().FontSize(7.2f);
                            h.Cell().Element(Th).Text("Price / Carat").Bold().FontSize(7.2f);
                            h.Cell().Element(Th).Text("Return Cts").Bold().FontSize(7.2f);
                            h.Cell().Element(Th).Text("Kept Cts").Bold().FontSize(7.2f);
                            h.Cell().Element(Th).Text("Amount").Bold().FontSize(7.2f);
                        });

                        foreach (var item in pageItems)
                        {
                            srNo++;
                            var desc = !string.IsNullOrWhiteSpace(item.Description) ? item.Description : (item.StockNo ?? "");
                            var amt = item.Amount > 0 ? item.Amount : item.MasterCurrencyAmount;
                            var rate = item.PricePerCarat > 0 ? item.PricePerCarat : item.Rate;

                            table.Cell().Element(Td).AlignCenter().Text(srNo.ToString(CultureInfo.InvariantCulture)).FontSize(7.5f);
                            table.Cell().Element(Td).AlignCenter().Text(desc).FontSize(7.5f);
                            table.Cell().Element(Td).AlignCenter().Text(item.Qty.ToString(CultureInfo.InvariantCulture)).FontSize(7.5f);
                            table.Cell().Element(Td).AlignRight().Text(FormatCarat(item.Weight, company.ParcelCaratFormat)).FontSize(7.5f);
                            table.Cell().Element(Td).AlignRight().Text(FormatMoney(rate, dec)).FontSize(7.5f);
                            table.Cell().Element(Td).AlignRight().Text(item.ReturnCarats > 0 ? FormatCarat(item.ReturnCarats, company.ParcelCaratFormat) : "").FontSize(7.5f);
                            table.Cell().Element(Td).AlignRight().Text(item.IssueCarats > 0 ? FormatCarat(item.IssueCarats, company.ParcelCaratFormat) : "").FontSize(7.5f);
                            table.Cell().Element(Td).AlignRight().Text(FormatMoney(amt, dec)).FontSize(7.5f);
                        }

                        // Pad to 10 rows per copy (matching MixMemoDualFormat.js)
                        int padCount = Math.Max(0, pageSize - pageItems.Count);
                        for (int i = 0; i < padCount; i++)
                        {
                            table.Cell().BorderLeft(0.5f).BorderColor("#A2A1A1").Element(EmptyTd).Text("");
                            table.Cell().Element(EmptyTd).Text("");
                            table.Cell().Element(EmptyTd).Text("");
                            table.Cell().Element(EmptyTd).Text("");
                            table.Cell().Element(EmptyTd).Text("");
                            table.Cell().Element(EmptyTd).Text("");
                            table.Cell().Element(EmptyTd).Text("");
                            table.Cell().BorderLeft(0.5f).BorderRight(0.5f).BorderColor("#A2A1A1").Element(EmptyTd).Text("");
                        }

                        if (!isLastPage)
                        {
                            table.Cell().ColumnSpan(8).BorderTop(0.6f).BorderColor("#A2A1A1").Text("");
                        }
                        else
                        {
                            IContainer SumCell(IContainer c) => c.Border(0.6f).BorderColor("#999999").PaddingHorizontal(4).PaddingVertical(2).AlignMiddle();

                            table.Cell().ColumnSpan(3).Element(SumCell).AlignLeft().Text("Total Carats :").Bold().FontSize(7.8f);
                            table.Cell().Element(SumCell).AlignRight().Text(FormatCarat(totalWeight, company.ParcelCaratFormat)).Bold().FontSize(7.8f);
                            table.Cell().ColumnSpan(3).Element(SumCell).AlignRight().Text("Total :").Bold().FontSize(7.8f);
                            table.Cell().Element(SumCell).AlignRight().Text(FormatMoney(totalAmount, dec)).Bold().FontSize(7.8f);

                            table.Cell().ColumnSpan(7).Text("");
                            table.Cell().Element(SumCell).Row(r =>
                            {
                                r.RelativeItem().AlignLeft().Text("Round Off").FontSize(7.5f);
                                r.RelativeItem().AlignRight().Text(FormatMoney(plusMinus, dec)).Bold().FontSize(7.5f);
                            });

                            table.Cell().ColumnSpan(7).Text("");
                            table.Cell().Element(SumCell).AlignRight().Text(FormatMoney(finalAmount, dec)).Bold().FontSize(8.2f);
                        }
                    });

                    // Copy footer + Signature
                    copy.Item().PaddingTop(6).Row(r =>
                    {
                        r.RelativeItem(4).AlignMiddle().Text(footerText).FontSize(7f).FontColor(Colors.Grey.Darken1);
                        r.RelativeItem(4).AlignRight().AlignMiddle().Text("Signature   ______________________").Bold().FontSize(8.5f);
                    });

                    if (isOffice)
                    {
                        copy.Item().PaddingTop(10).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                    }
                });
            }

            return Document.Create(container =>
            {
                for (int pageIdx = 0; pageIdx < pages.Count; pageIdx++)
                {
                    int idx = pageIdx;
                    var pageItems = pages[idx];
                    bool isLastPage = idx == pages.Count - 1;

                    container.Page(page =>
                    {
                        page.Size(PageSizes.Letter);
                        page.MarginHorizontal(20);
                        page.MarginVertical(14);
                        page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Lato"));

                        page.Content().Column(col =>
                        {
                            RenderCopy(col, "OFFICE COPY", pageItems, idx, isLastPage, true);
                            RenderCopy(col, "CUSTOMER COPY", pageItems, idx, isLastPage, false);
                        });
                    });
                }
            }).GeneratePdf();
        }

        /// <summary>Matches React MixMemoUsaFormat.js &amp; MixMemoStellaFormat.js — Full-page GENERAL MEMO (20 rows/page).</summary>
        private static byte[] BuildMixMemoUsaPdf(
            MixMemoOutPrintViewDto data,
            List<MixMemoOutPrintItemDto> items,
            CompanyEmailSettings company,
            byte[]? logoBytes)
        {
            const int pageSize = 20;
            var pages = PaginateFixed(items, pageSize);
            int dec = company.AmountDecimals;

            int totalQty = items.Sum(i => i.Qty);
            double totalWeight = items.Sum(i => i.Weight);
            double totalAmount = items.Sum(i => i.Amount > 0 ? i.Amount : i.MasterCurrencyAmount);
            double finalAmount = totalAmount + data.PlusMinusAmount;

            string trnDate = FormatDateSlash(data.TrnDate);
            string printedOn = FormatDateSlash(!string.IsNullOrWhiteSpace(data.RoundDate) ? data.RoundDate : DateTime.Now.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture));
            string information = !string.IsNullOrWhiteSpace(company.Information) ? company.Information : FallbackMemoInformation;
            string conditions = !string.IsNullOrWhiteSpace(company.MemoConditions)
                ? company.MemoConditions
                : (!string.IsNullOrWhiteSpace(company.SaleConditions) ? company.SaleConditions : FallbackMemoConditions);
            string footerText = string.IsNullOrWhiteSpace(company.CompanyFooter) ? "Powered by SoftOnCloud" : company.CompanyFooter;

            void RenderAddressBox(IContainer container, string legend)
            {
                container.Border(0.75f).BorderColor(Colors.Black).Padding(6).MinHeight(68).Column(c =>
                {
                    c.Item().Text(legend).Bold().FontSize(8.5f);
                    c.Item().PaddingTop(2).Text(data.PartyName ?? "").FontSize(8.5f);
                    if (!string.IsNullOrWhiteSpace(data.AddressLine1)) c.Item().Text(data.AddressLine1).FontSize(7.8f);
                    if (!string.IsNullOrWhiteSpace(data.AddressLine2)) c.Item().Text(data.AddressLine2).FontSize(7.8f);
                    if (!string.IsNullOrWhiteSpace(data.AddressLine3)) c.Item().Text(data.AddressLine3).FontSize(7.8f);
                });
            }

            return Document.Create(container =>
            {
                for (int pageIdx = 0; pageIdx < pages.Count; pageIdx++)
                {
                    int idx = pageIdx;
                    var pageItems = pages[idx];
                    bool isLastPage = idx == pages.Count - 1;
                    int fillTo = isLastPage ? Math.Max(pageItems.Count, 12) : pageSize;
                    int srNo = idx * pageSize;

                    container.Page(page =>
                    {
                        page.Size(PageSizes.Letter);
                        page.MarginHorizontal(22);
                        page.MarginVertical(18);
                        page.DefaultTextStyle(x => x.FontSize(8.5f).FontFamily("Lato"));

                        page.Content().Column(col =>
                        {
                            // Top Header
                            col.Item().Row(row =>
                            {
                                if (logoBytes != null && logoBytes.Length > 0)
                                    row.ConstantItem(68).PaddingRight(8).AlignTop().Image(logoBytes).FitArea();

                                row.RelativeItem(5).Column(c =>
                                {
                                    if (!string.IsNullOrWhiteSpace(company.CompanyName))
                                        c.Item().Text(company.CompanyName).Bold().FontSize(11.5f);
                                    if (!string.IsNullOrWhiteSpace(company.CompanyAddress))
                                        c.Item().Text(company.CompanyAddress).FontSize(8.2f);
                                    if (!string.IsNullOrWhiteSpace(company.CompanyAddress1))
                                        c.Item().Text(company.CompanyAddress1).FontSize(8.2f);
                                    if (!string.IsNullOrWhiteSpace(company.CompanyAddress2))
                                        c.Item().Text(company.CompanyAddress2).FontSize(8.2f);
                                    if (!string.IsNullOrWhiteSpace(company.CompanyPhone))
                                        c.Item().PaddingTop(2).Text(t => { t.Span("Tel:  ").Bold().FontSize(8.2f); t.Span(company.CompanyPhone).FontSize(8.2f); });
                                    if (!string.IsNullOrWhiteSpace(company.CompanyEmail))
                                        c.Item().Text(t => { t.Span("Email:  ").Bold().FontSize(8.2f); t.Span(company.CompanyEmail).FontSize(8.2f); });
                                });

                                row.RelativeItem(3.5f).Column(c =>
                                {
                                    c.Item().Text("GENERAL MEMO").Bold().FontSize(16);
                                    c.Item().PaddingTop(4).Table(t =>
                                    {
                                        t.ColumnsDefinition(cols => { cols.ConstantColumn(65); cols.RelativeColumn(); });
                                        t.Cell().PaddingVertical(1).Text("Date :").Bold().FontSize(8.5f);
                                        t.Cell().PaddingVertical(1).Text(trnDate).FontSize(8.5f);
                                        t.Cell().PaddingVertical(1).Text("Memo # :").Bold().FontSize(8.5f);
                                        t.Cell().PaddingVertical(1).Text(data.InvNo ?? "").FontSize(8.5f);
                                        t.Cell().PaddingVertical(1).Text("Printed On :").Bold().FontSize(8.5f);
                                        t.Cell().PaddingVertical(1).Text(printedOn).FontSize(8.5f);
                                    });
                                });
                            });

                            // Consigned To / Ship To
                            col.Item().PaddingTop(6).Row(row =>
                            {
                                row.RelativeItem().Element(e => RenderAddressBox(e, "Consigned To:"));
                                row.ConstantItem(14);
                                row.RelativeItem().Element(e => RenderAddressBox(e, "Ship To:"));
                            });

                            // Information legal text
                            if (!string.IsNullOrWhiteSpace(information))
                                col.Item().PaddingVertical(5).Text(information).FontSize(7.2f).Justify();

                            // 6-col Meta Table
                            col.Item().PaddingBottom(5).Table(t =>
                            {
                                t.ColumnsDefinition(cols =>
                                {
                                    for (int i = 0; i < 6; i++) cols.RelativeColumn();
                                });
                                IContainer MTh(IContainer c) => c.Background("#BFBFBF").Border(0.75f).BorderColor(Colors.Black).Padding(3).AlignCenter();
                                IContainer MTd(IContainer c) => c.Border(0.75f).BorderColor(Colors.Black).Padding(3).MinHeight(15).AlignCenter();

                                foreach (var h in new[] { "CONTACT", "SALES PERSON", "CUST ORDER #", "SHIP VIA", "TRACKING #", "REPORT BY" })
                                    t.Cell().Element(MTh).Text(h).Bold().FontSize(7.2f);

                                t.Cell().Element(MTd).Text("");
                                t.Cell().Element(MTd).Text(data.SalesPersons ?? data.SalesPerson ?? "").FontSize(7.8f);
                                t.Cell().Element(MTd).Text("");
                                t.Cell().Element(MTd).Text(data.ShipVia ?? "").FontSize(7.8f);
                                t.Cell().Element(MTd).Text(data.Tracking ?? "").FontSize(7.8f);
                                t.Cell().Element(MTd).Text("");
                            });

                            // 7-col Items Table
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(6);   // Sr No
                                    cols.RelativeColumn(12);  // Stock #
                                    cols.RelativeColumn(7);   // QTY
                                    cols.RelativeColumn(9);   // WT.
                                    cols.RelativeColumn(28);  // DESCRIPTION
                                    cols.RelativeColumn(16);  // Price/Carat
                                    cols.RelativeColumn(22);  // Amount
                                });

                                IContainer Th(IContainer c) => c.Background("#BFBFBF").Border(0.75f).BorderColor(Colors.Black).Padding(3.5f).AlignCenter().AlignMiddle();
                                IContainer Td(IContainer c) => c.BorderLeft(0.75f).BorderRight(0.75f).BorderColor(Colors.Black).PaddingHorizontal(4).PaddingVertical(2.5f).MinHeight(15).AlignMiddle();
                                IContainer TdFull(IContainer c) => c.Border(0.75f).BorderColor(Colors.Black).PaddingHorizontal(4).PaddingVertical(3).AlignTop();

                                table.Header(h =>
                                {
                                    foreach (var hdr in new[] { "Sr No", "Stock #", "QTY", "WT.", "DESCRIPTION", "Price/Carat", "Amount" })
                                        h.Cell().Element(Th).Text(hdr).Bold().FontSize(8f);
                                });

                                foreach (var item in pageItems)
                                {
                                    srNo++;
                                    var amt = item.Amount > 0 ? item.Amount : item.MasterCurrencyAmount;
                                    var rate = item.PricePerCarat > 0 ? item.PricePerCarat : item.Rate;

                                    table.Cell().Element(Td).AlignCenter().Text(srNo.ToString(CultureInfo.InvariantCulture)).FontSize(8f);
                                    table.Cell().Element(Td).AlignLeft().Text(item.StockNo ?? "").FontSize(8f);
                                    table.Cell().Element(Td).AlignRight().Text(item.Qty.ToString(CultureInfo.InvariantCulture)).FontSize(8f);
                                    table.Cell().Element(Td).AlignRight().Text(FormatCarat(item.Weight, company.ParcelCaratFormat)).FontSize(8f);
                                    table.Cell().Element(Td).AlignLeft().Text(item.Description ?? "").FontSize(8f);
                                    table.Cell().Element(Td).AlignRight().Text(FormatMoney(rate, dec)).FontSize(8f);
                                    table.Cell().Element(Td).AlignRight().Text(FormatMoney(amt, dec)).FontSize(8f);
                                }

                                for (int i = pageItems.Count; i < fillTo; i++)
                                {
                                    for (int c = 0; c < 7; c++)
                                        table.Cell().Element(Td).Text("");
                                }

                                if (!isLastPage)
                                {
                                    table.Cell().ColumnSpan(7).BorderTop(0.75f).BorderColor(Colors.Black).Text("");
                                }
                                else
                                {
                                    table.Cell().Element(TdFull).Text("");
                                    table.Cell().Element(TdFull).Text("");
                                    table.Cell().Element(TdFull).AlignRight().Text(totalQty.ToString(CultureInfo.InvariantCulture)).FontSize(8.5f);
                                    table.Cell().Element(TdFull).AlignRight().Text(FormatCarat(totalWeight, company.ParcelCaratFormat)).FontSize(8.5f);
                                    table.Cell().Element(TdFull).Text("");
                                    table.Cell().Element(TdFull).AlignRight().Column(sc =>
                                    {
                                        sc.Item().AlignRight().Text("Sub Total").FontSize(8f);
                                        if (Math.Abs(data.PlusMinusAmount) > 0.0001)
                                            sc.Item().PaddingTop(2).AlignRight().Text("+/- Amount").FontSize(8f);
                                        else
                                        {
                                            sc.Item().PaddingTop(2).AlignRight().Text("Charges").FontSize(8f);
                                            sc.Item().PaddingTop(2).AlignRight().Text("Discount").FontSize(8f);
                                        }
                                        sc.Item().PaddingTop(3).AlignRight().Text("TOTAL").Bold().FontSize(9.5f);
                                    });
                                    table.Cell().Element(TdFull).AlignRight().Column(vc =>
                                    {
                                        vc.Item().AlignRight().Text(FormatMoney(totalAmount, dec)).FontSize(8.5f);
                                        if (Math.Abs(data.PlusMinusAmount) > 0.0001)
                                            vc.Item().PaddingTop(2).AlignRight().Text(FormatMoney(data.PlusMinusAmount, dec)).FontSize(8.5f);
                                        else
                                        {
                                            vc.Item().PaddingTop(2).AlignRight().Text(" ").FontSize(8f);
                                            vc.Item().PaddingTop(2).AlignRight().Text(" ").FontSize(8f);
                                        }
                                        vc.Item().PaddingTop(3).AlignRight().Text(FormatMoney(finalAmount, dec)).Bold().FontSize(9.5f);
                                    });
                                }
                            });

                            // Signature + Conditions
                            col.Item().PaddingTop(14).Row(r =>
                            {
                                r.AutoItem().Text("Signature:   ").FontSize(8.5f);
                                r.ConstantItem(200).PaddingTop(10).LineHorizontal(0.75f).LineColor(Colors.Black);
                            });

                            if (!string.IsNullOrWhiteSpace(conditions))
                                col.Item().PaddingTop(8).Text(conditions).FontSize(7.2f).Justify();
                        });

                        page.Footer().Row(r =>
                        {
                            r.RelativeItem().AlignLeft().Text(footerText).FontSize(8f);
                            r.RelativeItem().AlignRight().Text($"{idx + 1}").FontSize(8f);
                        });
                    });
                }
            }).GeneratePdf();
        }

        // =========================================================================
        // MIX SALE — resolves DEFAULT SALE INVOICE PRINT FORMAT PARCEL
        // =========================================================================
        private static (string curSym, string amountHeader, double totalAmount, double finalTotal) ResolveSaleTotals(
            MixSalePrintViewDto data,
            List<MixSalePrintItemDto> items)
        {
            double totalAmount = items.Sum(i => i.Amount > 0 ? i.Amount : i.MasterCurrencyAmount);
            double charges = data.Charges;
            double discount = data.Discount;

            var rawCur = (data.Currency ?? data.Symbol ?? "").Trim();
            bool isUsd = rawCur.Equals("USD", StringComparison.OrdinalIgnoreCase)
                         || rawCur == "$"
                         || (!string.IsNullOrWhiteSpace(data.StockSymbol) && rawCur.Equals(data.StockSymbol, StringComparison.OrdinalIgnoreCase))
                         || string.IsNullOrWhiteSpace(rawCur);
            bool isInr = !isUsd && (rawCur.Equals("INR", StringComparison.OrdinalIgnoreCase) || rawCur == "₹");

            string curSym = !string.IsNullOrWhiteSpace(data.CurrencySymbol)
                ? data.CurrencySymbol
                : (isUsd ? "$" : (isInr ? "INR" : rawCur));
            string amountHeader = isUsd
                ? "Amount US $"
                : (!string.IsNullOrWhiteSpace(curSym) ? $"Amount ({curSym})" : "Amount");

            // Currency guard matching MixSalePrintUsaFormat.js L279-282
            double computedFinal = totalAmount + charges - discount;
            double finalTotal = data.FinalAmount > 0 && !(isUsd && computedFinal > 0 && data.FinalAmount > computedFinal * 5)
                ? data.FinalAmount
                : computedFinal;

            return (curSym, amountHeader, totalAmount, finalTotal);
        }

        private static byte[] BuildMixSalePdf(MixSalePrintViewDto data, CompanyEmailSettings company, byte[]? logoBytes)
        {
            var items = data.Items ?? new List<MixSalePrintItemDto>();
            // Match MixSalePrintView.js -> always renders MixSalePrintUsaFormat (GENERAL INVOICE)
            return BuildMixSaleGeneralUsaPdf(data, items, company, logoBytes);
        }

        /// <summary>Matches React ParcelSaleUsaFormat2.js — DEFAULT SALE INVOICE PRINT FORMAT PARCEL = Parcel Sale InvoiceUSA_Format2.</summary>
        private static byte[] BuildMixSaleParcelUsaFormat2Pdf(
            MixSalePrintViewDto data,
            List<MixSalePrintItemDto> items,
            CompanyEmailSettings company,
            byte[]? logoBytes)
        {
            const int pageSize = 15;
            var pages = PaginateFixed(items, pageSize);
            int dec = company.AmountDecimals;
            var (_, amountHeader, totalAmount, finalTotal) = ResolveSaleTotals(data, items);

            int totalQty = items.Sum(i => i.Pcs > 0 ? i.Pcs : i.Qty);
            double totalWeight = items.Sum(i => i.Carats > 0 ? i.Carats : i.Weight);
            string trnDate = FormatDateSlash(data.TrnDate);
            string printedOn = FormatDateSlash(!string.IsNullOrWhiteSpace(data.RoundDate) ? data.RoundDate : DateTime.Now.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture));
            string information = company.Information ?? string.Empty;
            string conditions = !string.IsNullOrWhiteSpace(company.SaleConditions) ? company.SaleConditions : FallbackMemoConditions;
            string footerText = string.IsNullOrWhiteSpace(company.CompanyFooter) ? "Powered by SoftOnCloud" : company.CompanyFooter;

            void RenderAddressBox(IContainer box, string legend)
            {
                box.Border(0.75f).BorderColor(Colors.Black).CornerRadius(8).Padding(8).MinHeight(68).Column(c =>
                {
                    c.Item().Text(legend).Bold().FontSize(8.5f);
                    c.Item().PaddingTop(3).Text(data.PartyName ?? "").FontSize(8.5f);
                    if (!string.IsNullOrWhiteSpace(data.AddressLine1)) c.Item().Text(data.AddressLine1).FontSize(7.8f);
                    if (!string.IsNullOrWhiteSpace(data.AddressLine2)) c.Item().Text(data.AddressLine2).FontSize(7.8f);
                    if (!string.IsNullOrWhiteSpace(data.AddressLine3)) c.Item().Text(data.AddressLine3).FontSize(7.8f);
                });
            }

            return Document.Create(container =>
            {
                for (int pageIdx = 0; pageIdx < pages.Count; pageIdx++)
                {
                    int idx = pageIdx;
                    var pageItems = pages[idx];
                    bool isLastPage = idx == pages.Count - 1;
                    int srNo = idx * pageSize;

                    container.Page(page =>
                    {
                        page.Size(PageSizes.Letter);
                        page.MarginHorizontal(20);
                        page.MarginVertical(18);
                        page.DefaultTextStyle(x => x.FontSize(8.2f).FontFamily("Lato"));

                        page.Content().Column(col =>
                        {
                            // Header: Logo + Company | INVOICE + Date/Memo#/Printed On
                            col.Item().Row(row =>
                            {
                                if (logoBytes != null && logoBytes.Length > 0)
                                    row.ConstantItem(66).PaddingRight(8).AlignTop().Image(logoBytes).FitArea();

                                row.RelativeItem(5).Column(c =>
                                {
                                    if (!string.IsNullOrWhiteSpace(company.CompanyName)) c.Item().Text(company.CompanyName).Bold().FontSize(11f);
                                    if (!string.IsNullOrWhiteSpace(company.CompanyAddress)) c.Item().Text(company.CompanyAddress).FontSize(8f);
                                    if (!string.IsNullOrWhiteSpace(company.CompanyAddress1)) c.Item().Text(company.CompanyAddress1).FontSize(8f);
                                    if (!string.IsNullOrWhiteSpace(company.CompanyAddress2)) c.Item().Text(company.CompanyAddress2).FontSize(8f);
                                    if (!string.IsNullOrWhiteSpace(company.CompanyPhone)) c.Item().PaddingTop(2).Text($"Tel:  {company.CompanyPhone}").FontSize(8f);
                                    if (!string.IsNullOrWhiteSpace(company.CompanyEmail)) c.Item().Text($"Email:  {company.CompanyEmail}").FontSize(8f);
                                });

                                row.RelativeItem(3.5f).Column(c =>
                                {
                                    c.Item().Text("INVOICE").Bold().FontSize(16);
                                    c.Item().PaddingTop(4).Table(t =>
                                    {
                                        t.ColumnsDefinition(cols => { cols.ConstantColumn(62); cols.RelativeColumn(); });
                                        t.Cell().PaddingVertical(1).Text("Date :").Bold().FontSize(8.2f);
                                        t.Cell().PaddingVertical(1).Text(trnDate).FontSize(8.2f);
                                        t.Cell().PaddingVertical(1).Text("Memo # :").Bold().FontSize(8.2f);
                                        t.Cell().PaddingVertical(1).Text(data.InvNo ?? "").FontSize(8.2f);
                                        t.Cell().PaddingVertical(1).Text("Printed On :").Bold().FontSize(8.2f);
                                        t.Cell().PaddingVertical(1).Text(printedOn).FontSize(8.2f);
                                    });
                                });
                            });

                            // Consigned To / Ship To
                            col.Item().PaddingTop(6).Row(row =>
                            {
                                row.RelativeItem().Element(e => RenderAddressBox(e, "Consigned To:"));
                                row.ConstantItem(14);
                                row.RelativeItem().Element(e => RenderAddressBox(e, "Ship To:"));
                            });

                            if (!string.IsNullOrWhiteSpace(information))
                                col.Item().PaddingVertical(5).Text(information).FontSize(7.2f).Justify();

                            // 5-col Meta table matching ParcelSaleUsaFormat2.js
                            col.Item().PaddingTop(3).PaddingBottom(5).Table(t =>
                            {
                                t.ColumnsDefinition(cols => { for (int i = 0; i < 5; i++) cols.RelativeColumn(); });
                                IContainer MTh(IContainer c) => c.Background("#BFBFBF").Border(0.75f).BorderColor(Colors.Black).Padding(3).AlignCenter();
                                IContainer MTd(IContainer c) => c.Border(0.75f).BorderColor(Colors.Black).Padding(3).MinHeight(14).AlignCenter();

                                foreach (var h in new[] { "CONTACT", "SALES PERSON", "CUST ORDER #", "SHIP VIA", "TRACKING #" })
                                    t.Cell().Element(MTh).Text(h).Bold().FontSize(7.2f);

                                t.Cell().Element(MTd).Text("");
                                t.Cell().Element(MTd).Text(data.SalesPersons ?? "").FontSize(7.5f);
                                t.Cell().Element(MTd).Text("");
                                t.Cell().Element(MTd).Text(data.ShipVia ?? "").FontSize(7.5f);
                                t.Cell().Element(MTd).Text(data.Tracking ?? "").FontSize(7.5f);
                            });

                            // 8-col Items Table matching ParcelSaleUsaFormat2.js
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(5);   // Sr No
                                    cols.RelativeColumn(9);   // MemoNo
                                    cols.RelativeColumn(10);  // Stock #
                                    cols.RelativeColumn(7);   // QTY
                                    cols.RelativeColumn(10);  // WT.
                                    cols.RelativeColumn(28);  // DESCRIPTION
                                    cols.RelativeColumn(14);  // Price/Carat
                                    cols.RelativeColumn(17);  // Amount
                                });

                                IContainer Th(IContainer c) => c.Background("#BFBFBF").Border(0.75f).BorderColor(Colors.Black).Padding(3).AlignCenter().AlignMiddle();
                                IContainer Td(IContainer c) => c.Border(0.75f).BorderColor(Colors.Black).PaddingHorizontal(3).PaddingVertical(2.5f).MinHeight(15).AlignMiddle();
                                IContainer EmptyTd(IContainer c) => c.BorderLeft(0.75f).BorderRight(0.75f).BorderColor(Colors.Black).MinHeight(14);

                                table.Header(h =>
                                {
                                    foreach (var hdr in new[] { "Sr No", "MemoNo", "Stock #", "QTY", "WT.", "DESCRIPTION", "Price/Carat", amountHeader })
                                        h.Cell().Element(Th).Text(hdr).Bold().FontSize(7.5f);
                                });

                                foreach (var item in pageItems)
                                {
                                    srNo++;
                                    int pcs = item.Pcs > 0 ? item.Pcs : item.Qty;
                                    double wt = item.Carats > 0 ? item.Carats : item.Weight;
                                    double rate = item.Rate > 0 ? item.Rate : item.PricePerCarat;
                                    double amt = item.Amount > 0 ? item.Amount : item.MasterCurrencyAmount;

                                    table.Cell().Element(Td).AlignCenter().Text(srNo.ToString(CultureInfo.InvariantCulture)).FontSize(7.8f);
                                    table.Cell().Element(Td).AlignCenter().Text(item.MemoNo ?? "").FontSize(7.8f);
                                    table.Cell().Element(Td).AlignCenter().Text(item.Lotno ?? item.StockNo ?? "").FontSize(7.8f);
                                    table.Cell().Element(Td).AlignCenter().Text(pcs.ToString(CultureInfo.InvariantCulture)).FontSize(7.8f);
                                    table.Cell().Element(Td).AlignRight().Text(FormatCarat(wt, company.ParcelCaratFormat)).FontSize(7.8f);
                                    table.Cell().Element(Td).AlignLeft().Text(!string.IsNullOrWhiteSpace(item.ItemName) ? item.ItemName : (item.Description ?? "")).FontSize(7.8f);
                                    table.Cell().Element(Td).AlignRight().Text(FormatMoney(rate, dec)).FontSize(7.8f);
                                    table.Cell().Element(Td).AlignRight().Text(FormatMoney(amt, dec)).FontSize(7.8f);
                                }

                                int fillTo = isLastPage ? Math.Max(pageItems.Count, 10) : pageSize;
                                for (int i = pageItems.Count; i < fillTo; i++)
                                {
                                    for (int c = 0; c < 8; c++)
                                        table.Cell().Element(EmptyTd).Text("");
                                }

                                if (!isLastPage)
                                {
                                    table.Cell().ColumnSpan(8).BorderTop(0.75f).BorderColor(Colors.Black).Text("");
                                }
                                else
                                {
                                    table.Cell().Element(Td).Text("");
                                    table.Cell().Element(Td).Text("");
                                    table.Cell().Element(Td).Text("");
                                    table.Cell().Element(Td).AlignCenter().Text(totalQty.ToString(CultureInfo.InvariantCulture)).FontSize(8f);
                                    table.Cell().Element(Td).AlignRight().Text(FormatCarat(totalWeight, company.ParcelCaratFormat)).FontSize(8f);
                                    table.Cell().Element(Td).AlignLeft().Text("Terms: Due upon receipt").Bold().FontSize(7.8f);
                                    table.Cell().Element(Td).AlignLeft().Column(sc =>
                                    {
                                        sc.Item().Text("Sub Total").FontSize(7.8f);
                                        sc.Item().PaddingTop(2).Text("Charges").FontSize(7.8f);
                                        sc.Item().PaddingTop(2).Text("Discount").FontSize(7.8f);
                                        sc.Item().PaddingTop(3).Text("TOTAL").Bold().FontSize(9f);
                                    });
                                    table.Cell().Element(Td).AlignRight().Column(vc =>
                                    {
                                        vc.Item().AlignRight().Text(FormatMoney(totalAmount, dec)).FontSize(8f);
                                        vc.Item().PaddingTop(2).AlignRight().Text(data.Charges > 0 ? FormatMoney(data.Charges, dec) : " ").FontSize(8f);
                                        vc.Item().PaddingTop(2).AlignRight().Text(data.Discount > 0 ? FormatMoney(data.Discount, dec) : " ").FontSize(8f);
                                        vc.Item().PaddingTop(3).AlignRight().Text(FormatMoney(finalTotal, dec)).Bold().FontSize(9f);
                                    });
                                }
                            });

                            if (isLastPage)
                            {
                                bool hasBank = !string.IsNullOrWhiteSpace(data.bnm) || !string.IsNullOrWhiteSpace(data.BankAccNo)
                                               || !string.IsNullOrWhiteSpace(data.ABARoutingNumber) || !string.IsNullOrWhiteSpace(data.SwiftCode)
                                               || !string.IsNullOrWhiteSpace(data.ZelleID);
                                if (hasBank)
                                {
                                    col.Item().PaddingTop(8).Column(c =>
                                    {
                                        c.Item().Text("Payment Instruction:").Bold().FontSize(8.5f);
                                        if (!string.IsNullOrWhiteSpace(company.CompanyName)) c.Item().Text(company.CompanyName).FontSize(8f);
                                        if (!string.IsNullOrWhiteSpace(company.CompanyAddress)) c.Item().Text(company.CompanyAddress).FontSize(8f);
                                        c.Item().PaddingTop(3).Table(bt =>
                                        {
                                            bt.ColumnsDefinition(cols => { cols.ConstantColumn(105); cols.RelativeColumn(); });
                                            void BankRow(string k, string? v)
                                            {
                                                if (string.IsNullOrWhiteSpace(v)) return;
                                                bt.Cell().PaddingVertical(1).Text(k).Bold().FontSize(8f);
                                                bt.Cell().PaddingVertical(1).Text(v).Bold().FontSize(8f);
                                            }
                                            BankRow("Bank Name :", data.bnm);
                                            BankRow("Account No :", data.BankAccNo);
                                            BankRow("Routing Number :", data.ABARoutingNumber);
                                            BankRow("Swift Code :", data.SwiftCode);
                                            BankRow("ZELLE Id :", data.ZelleID);
                                        });
                                    });
                                }

                                col.Item().PaddingTop(12).Row(r =>
                                {
                                    r.AutoItem().Text("Signature:   ").FontSize(8.5f);
                                    r.ConstantItem(200).PaddingTop(10).LineHorizontal(0.75f).LineColor(Colors.Black);
                                });

                                if (!string.IsNullOrWhiteSpace(conditions))
                                    col.Item().PaddingTop(8).Text(conditions).FontSize(7.2f).Justify();
                            }
                        });

                        page.Footer().Row(r =>
                        {
                            r.RelativeItem().AlignLeft().Text(footerText).FontSize(8f);
                            r.RelativeItem().AlignRight().Text($"{idx + 1}").FontSize(8f);
                        });
                    });
                }
            }).GeneratePdf();
        }

        /// <summary>Matches React ParcelSaleUsaFormat.js — DEFAULT SALE INVOICE PRINT FORMAT PARCEL = Parcel Sale InvoiceUSA_Format / Format2.</summary>
        private static byte[] BuildMixSaleParcelUsaPdf(
            MixSalePrintViewDto data,
            List<MixSalePrintItemDto> items,
            CompanyEmailSettings company,
            byte[]? logoBytes)
        {
            const int pageSize = 22;
            var pages = PaginateFixed(items, pageSize);
            int dec = company.AmountDecimals;
            var (curSym, amountHeader, totalAmount, finalTotal) = ResolveSaleTotals(data, items);
            double totalWeight = items.Sum(i => i.Carats > 0 ? i.Carats : i.Weight);
            double adjAmt = data.Charges - data.Discount;

            string trnDate = FormatDateMmm(data.TrnDate);
            string dueDate = FormatDateMmm(data.DueDate);
            string printedOn = FormatDateMmm(!string.IsNullOrWhiteSpace(data.RoundDate) ? data.RoundDate : DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            string conditions = !string.IsNullOrWhiteSpace(company.SaleConditions) ? company.SaleConditions : company.MemoConditions;
            string footerText = string.IsNullOrWhiteSpace(company.CompanyFooter) ? "Powered by SoftOnCloud" : company.CompanyFooter;

            void RenderPartyBox(IContainer box, string title)
            {
                box.Border(1f).BorderColor("#A2A1A1").Column(c =>
                {
                    c.Item().Background("#BFBFBF").BorderBottom(0.75f).BorderColor("#A2A1A1").Padding(3).AlignCenter().Text(title).Bold().FontSize(9f);
                    c.Item().Padding(6).MinHeight(62).Column(p =>
                    {
                        p.Item().PaddingBottom(3).Text(data.PartyName ?? "-").Bold().FontSize(9.5f);
                        if (!string.IsNullOrWhiteSpace(data.AddressLine1)) p.Item().Text(data.AddressLine1).FontSize(8f);
                        if (!string.IsNullOrWhiteSpace(data.AddressLine2)) p.Item().Text(data.AddressLine2).FontSize(8f);
                        if (!string.IsNullOrWhiteSpace(data.AddressLine3)) p.Item().Text(data.AddressLine3).FontSize(8f);
                    });
                });
            }

            return Document.Create(container =>
            {
                for (int pageIdx = 0; pageIdx < pages.Count; pageIdx++)
                {
                    int idx = pageIdx;
                    var pageItems = pages[idx];
                    bool isLastPage = idx == pages.Count - 1;
                    int srNo = idx * pageSize;

                    container.Page(page =>
                    {
                        page.Size(PageSizes.Letter);
                        page.MarginHorizontal(22);
                        page.MarginVertical(18);
                        page.DefaultTextStyle(x => x.FontSize(8.5f).FontFamily("Lato"));

                        page.Content().Column(col =>
                        {
                            // Header: Logo + Company on left; INVOICE + bordered Date/Invoice#/Printed on table on right
                            col.Item().Row(row =>
                            {
                                row.RelativeItem(5.5f).Column(c =>
                                {
                                    if (logoBytes != null && logoBytes.Length > 0)
                                        c.Item().Width(80).MaxHeight(55).PaddingBottom(4).Image(logoBytes).FitArea();
                                    if (!string.IsNullOrWhiteSpace(company.CompanyName))
                                        c.Item().Text(company.CompanyName).Bold().FontSize(11f);
                                    if (!string.IsNullOrWhiteSpace(company.CompanyAddress))
                                        c.Item().Text(company.CompanyAddress).FontSize(8f);
                                    if (!string.IsNullOrWhiteSpace(company.CompanyAddress1))
                                        c.Item().Text(company.CompanyAddress1).FontSize(8f);
                                    if (!string.IsNullOrWhiteSpace(company.CompanyAddress2))
                                        c.Item().Text(company.CompanyAddress2).FontSize(8f);
                                    if (!string.IsNullOrWhiteSpace(company.CompanyPhone))
                                        c.Item().Text(t => { t.Span("Phone : ").Bold().FontSize(8f); t.Span(company.CompanyPhone).FontSize(8f); });
                                    if (!string.IsNullOrWhiteSpace(company.CompanyEmail))
                                        c.Item().Text(t => { t.Span("E-Mail : ").Bold().FontSize(8f); t.Span(company.CompanyEmail).FontSize(8f); });
                                });

                                row.RelativeItem(4.5f).AlignRight().Column(c =>
                                {
                                    c.Item().AlignRight().Text("INVOICE").Bold().FontSize(16);
                                    c.Item().PaddingTop(4).Table(t =>
                                    {
                                        t.ColumnsDefinition(cols => { cols.RelativeColumn(4.5f); cols.RelativeColumn(5.5f); });
                                        IContainer Lbl(IContainer x) => x.Border(1f).BorderColor("#A2A1A1").PaddingHorizontal(8).PaddingVertical(3.5f).AlignLeft();
                                        IContainer Val(IContainer x) => x.Border(1f).BorderColor("#A2A1A1").PaddingHorizontal(6).PaddingVertical(3.5f).AlignCenter();

                                        t.Cell().Element(Lbl).Text("Date").Bold().FontSize(8.5f);
                                        t.Cell().Element(Val).Text(trnDate).FontSize(8.5f);
                                        t.Cell().Element(Lbl).Text("Invoice #").Bold().FontSize(8.5f);
                                        t.Cell().Element(Val).Text(data.InvNo ?? "-").FontSize(8.5f);
                                        t.Cell().Element(Lbl).Text("Printed on").Bold().FontSize(8.5f);
                                        t.Cell().Element(Val).Text(printedOn).FontSize(8.5f);
                                    });
                                });
                            });

                            // Bill To / Ship To
                            col.Item().PaddingTop(8).Row(row =>
                            {
                                row.RelativeItem().Element(e => RenderPartyBox(e, "Bill To"));
                                row.ConstantItem(24);
                                row.RelativeItem().Element(e => RenderPartyBox(e, "Ship To"));
                            });

                            // 6-column Meta Table
                            col.Item().PaddingTop(8).Table(t =>
                            {
                                t.ColumnsDefinition(cols => { for (int i = 0; i < 6; i++) cols.RelativeColumn(); });
                                IContainer MTh(IContainer c) => c.Background("#BFBFBF").Border(1f).BorderColor("#A2A1A1").Padding(3).AlignCenter();
                                IContainer MTd(IContainer c) => c.Border(1f).BorderColor("#A2A1A1").Padding(3).MinHeight(18).AlignCenter().AlignMiddle();

                                foreach (var h in new[] { "Sales Person", "Terms", "Due Date", "Ship Via", "Ship Date", "Tracking No" })
                                    t.Cell().Element(MTh).Text(h).Bold().FontSize(7.5f);

                                t.Cell().Element(MTd).Text(data.SalesPersons ?? "").FontSize(7.8f);
                                t.Cell().Element(MTd).Text(data.DueDays > 0 ? $"{data.DueDays} Days" : "0").FontSize(7.8f);
                                t.Cell().Element(MTd).Text(dueDate).FontSize(7.8f);
                                t.Cell().Element(MTd).Text(data.ShipVia ?? "").FontSize(7.8f);
                                t.Cell().Element(MTd).Text("").FontSize(7.8f);
                                t.Cell().Element(MTd).Text(data.Tracking ?? "").FontSize(7.8f);
                            });

                            // 7-column Items Table matching ParcelSaleUsaFormat.js
                            col.Item().PaddingTop(6).Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.ConstantColumn(32);    // Sr No
                                    cols.ConstantColumn(58);    // Memo No
                                    cols.RelativeColumn(3);     // ItemName
                                    cols.ConstantColumn(40);    // PCS
                                    cols.ConstantColumn(68);    // Carats/Qty
                                    cols.ConstantColumn(74);    // Price/Carat
                                    cols.ConstantColumn(86);    // Amount
                                });

                                IContainer Th(IContainer c) => c.Background("#D9D9D9").Border(0.75f).BorderColor("#999999").Padding(4).AlignCenter().AlignMiddle();
                                IContainer Td(IContainer c) => c.Border(0.6f).BorderColor("#999999").Padding(3.5f).MinHeight(16).AlignMiddle();

                                table.Header(h =>
                                {
                                    h.Cell().Element(Th).Text("Sr No").Bold().FontSize(7.8f);
                                    h.Cell().Element(Th).Text("Memo No").Bold().FontSize(7.8f);
                                    h.Cell().Element(Th).Text("ItemName").Bold().FontSize(7.8f);
                                    h.Cell().Element(Th).Text("PCS").Bold().FontSize(7.8f);
                                    h.Cell().Element(Th).Text("Carats/Qty").Bold().FontSize(7.8f);
                                    h.Cell().Element(Th).Text("Price/Carat").Bold().FontSize(7.8f);
                                    h.Cell().Element(Th).Text(amountHeader).Bold().FontSize(7.8f);
                                });

                                foreach (var item in pageItems)
                                {
                                    srNo++;
                                    int pcs = item.Pcs > 0 ? item.Pcs : item.Qty;
                                    double wt = item.Carats > 0 ? item.Carats : item.Weight;
                                    double rate = item.Rate > 0 ? item.Rate : item.PricePerCarat;
                                    double amt = item.Amount > 0 ? item.Amount : item.MasterCurrencyAmount;
                                    string desc = !string.IsNullOrWhiteSpace(item.ItemName) ? item.ItemName : (item.Description ?? item.Lotno ?? "-");

                                    table.Cell().Element(Td).AlignCenter().Text(srNo.ToString(CultureInfo.InvariantCulture)).FontSize(8f);
                                    table.Cell().Element(Td).AlignCenter().Text(item.MemoNo ?? "").FontSize(8f);
                                    table.Cell().Element(Td).AlignLeft().Text(desc).FontSize(8f);
                                    table.Cell().Element(Td).AlignCenter().Text(pcs == 0 ? "-" : pcs.ToString(CultureInfo.InvariantCulture)).FontSize(8f);
                                    table.Cell().Element(Td).AlignRight().Text(FormatCarat(wt, company.ParcelCaratFormat)).FontSize(8f);
                                    table.Cell().Element(Td).AlignRight().Text(FormatMoney(rate, dec)).FontSize(8f);
                                    table.Cell().Element(Td).AlignRight().Text(FormatMoney(amt, dec)).FontSize(8f);
                                }

                                if (isLastPage)
                                {
                                    table.Cell().ColumnSpan(4).Element(Td).AlignLeft().PaddingLeft(6).Text("Total Carats :").Bold().FontSize(8.5f);
                                    table.Cell().Element(Td).AlignRight().Text(FormatCarat(totalWeight, company.ParcelCaratFormat)).Bold().FontSize(9f);
                                    table.Cell().Element(Td).AlignCenter().Text($"{(string.IsNullOrWhiteSpace(curSym) ? "Total" : curSym)} :").Bold().FontSize(8f);
                                    table.Cell().Element(Td).AlignRight().Text(FormatMoney(totalAmount, dec)).Bold().FontSize(9f);

                                    table.Cell().ColumnSpan(6).Element(Td).Text("");
                                    table.Cell().Element(Td).Row(r =>
                                    {
                                        r.RelativeItem().AlignLeft().Text("+/- Amt").Bold().FontSize(7.2f);
                                        r.RelativeItem().AlignRight().Text(FormatMoney(adjAmt, dec)).Bold().FontSize(8f);
                                    });

                                    table.Cell().ColumnSpan(6).Element(Td).Text("");
                                    table.Cell().Element(Td).AlignRight().Text(FormatMoney(finalTotal, dec)).Bold().FontSize(9.5f);
                                }
                            });

                            if (isLastPage)
                            {
                                // Payment Instruction + Signature
                                col.Item().PaddingTop(18).Row(r =>
                                {
                                    r.RelativeItem(6).Column(c =>
                                    {
                                        bool hasBank = !string.IsNullOrWhiteSpace(data.bnm) || !string.IsNullOrWhiteSpace(data.BankAccNo)
                                                       || !string.IsNullOrWhiteSpace(data.ABARoutingNumber) || !string.IsNullOrWhiteSpace(data.SwiftCode)
                                                       || !string.IsNullOrWhiteSpace(data.ZelleID);
                                        if (hasBank)
                                        {
                                            c.Item().PaddingBottom(4).Text("Payment Instruction:").Bold().FontSize(9.5f);
                                            c.Item().Table(bt =>
                                            {
                                                bt.ColumnsDefinition(cols => { cols.ConstantColumn(115); cols.RelativeColumn(); });
                                                void BankRow(string k, string? v)
                                                {
                                                    if (string.IsNullOrWhiteSpace(v)) return;
                                                    bt.Cell().PaddingVertical(1).Text(k).Bold().FontSize(8f);
                                                    bt.Cell().PaddingVertical(1).Text(v).FontSize(8f);
                                                }
                                                BankRow("Bank Name :", data.bnm);
                                                BankRow("Account No :", data.BankAccNo);
                                                BankRow("ABA Routing Number :", data.ABARoutingNumber);
                                                BankRow("Swift Code :", data.SwiftCode);
                                                BankRow("ZELLE Id :", data.ZelleID);
                                            });
                                        }
                                    });

                                    r.RelativeItem(4).AlignBottom().AlignRight().Column(c =>
                                    {
                                        c.Item().AlignRight().Text("Signature   ______________________").Bold().FontSize(9.5f);
                                    });
                                });

                                if (!string.IsNullOrWhiteSpace(conditions))
                                    col.Item().PaddingTop(12).Text(conditions).FontSize(7.2f).Justify();
                            }
                        });

                        page.Footer().Row(r =>
                        {
                            r.RelativeItem().AlignLeft().Text(footerText).FontSize(8f);
                            r.RelativeItem().AlignRight().Text($"{idx + 1} / {pages.Count}").FontSize(8.5f);
                        });
                    });
                }
            }).GeneratePdf();
        }

        /// <summary>Matches React MixSalePrintUsaFormat.js — GENERAL INVOICE.</summary>
        private static byte[] BuildMixSaleGeneralUsaPdf(
            MixSalePrintViewDto data,
            List<MixSalePrintItemDto> items,
            CompanyEmailSettings company,
            byte[]? logoBytes)
        {
            const int pageSize = 20;
            var pages = PaginateFixed(items, pageSize);
            int dec = company.AmountDecimals;
            var (_, amountHeader, totalAmount, finalTotal) = ResolveSaleTotals(data, items);

            int totalQty = items.Sum(i => i.Pcs > 0 ? i.Pcs : i.Qty);
            double totalWeight = items.Sum(i => i.Carats > 0 ? i.Carats : i.Weight);
            double totalTdc = items.Sum(i => i.TotalDiamondCarat ?? 0);

            string trnDate = FormatDateSlash(data.TrnDate);
            string printedOn = FormatDateSlash(!string.IsNullOrWhiteSpace(data.RoundDate) ? data.RoundDate : DateTime.Now.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture));
            string conditions = !string.IsNullOrWhiteSpace(company.SaleConditions) ? company.SaleConditions : company.MemoConditions;
            string footerText = string.IsNullOrWhiteSpace(company.CompanyFooter) ? "Powered by SoftOnCloud" : company.CompanyFooter;

            void RenderAddressBox(IContainer box, string legend)
            {
                box.Border(0.75f).BorderColor(Colors.Black).CornerRadius(8).Padding(8).MinHeight(68).Column(c =>
                {
                    c.Item().Text(legend).Bold().FontSize(8.5f);
                    c.Item().PaddingTop(3).Text(data.PartyName ?? "").FontSize(8.5f);
                    if (!string.IsNullOrWhiteSpace(data.AddressLine1)) c.Item().Text(data.AddressLine1).FontSize(7.8f);
                    if (!string.IsNullOrWhiteSpace(data.AddressLine2)) c.Item().Text(data.AddressLine2).FontSize(7.8f);
                    if (!string.IsNullOrWhiteSpace(data.AddressLine3)) c.Item().Text(data.AddressLine3).FontSize(7.8f);
                });
            }

            return Document.Create(container =>
            {
                for (int pageIdx = 0; pageIdx < pages.Count; pageIdx++)
                {
                    int idx = pageIdx;
                    var pageItems = pages[idx];
                    bool isLastPage = idx == pages.Count - 1;
                    int srNo = idx * pageSize;

                    container.Page(page =>
                    {
                        page.Size(PageSizes.Letter);
                        page.MarginHorizontal(20);
                        page.MarginVertical(18);
                        page.DefaultTextStyle(x => x.FontSize(8.2f).FontFamily("Lato"));

                        page.Content().Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                if (logoBytes != null && logoBytes.Length > 0)
                                    row.ConstantItem(66).PaddingRight(8).AlignTop().Image(logoBytes).FitArea();

                                row.RelativeItem(5).Column(c =>
                                {
                                    if (!string.IsNullOrWhiteSpace(company.CompanyName)) c.Item().Text(company.CompanyName).Bold().FontSize(11f);
                                    if (!string.IsNullOrWhiteSpace(company.CompanyAddress)) c.Item().Text(company.CompanyAddress).FontSize(8f);
                                    if (!string.IsNullOrWhiteSpace(company.CompanyAddress1)) c.Item().Text(company.CompanyAddress1).FontSize(8f);
                                    if (!string.IsNullOrWhiteSpace(company.CompanyAddress2)) c.Item().Text(company.CompanyAddress2).FontSize(8f);
                                    if (!string.IsNullOrWhiteSpace(company.CompanyPhone)) c.Item().PaddingTop(2).Text($"Tel:  {company.CompanyPhone}").FontSize(8f);
                                    if (!string.IsNullOrWhiteSpace(company.CompanyEmail)) c.Item().Text($"Email:  {company.CompanyEmail}").FontSize(8f);
                                });

                                row.RelativeItem(3.5f).Column(c =>
                                {
                                    c.Item().Text("GENERAL INVOICE").Bold().FontSize(15);
                                    c.Item().PaddingTop(4).Table(t =>
                                    {
                                        t.ColumnsDefinition(cols => { cols.ConstantColumn(62); cols.RelativeColumn(); });
                                        t.Cell().PaddingVertical(1).Text("Date :").Bold().FontSize(8.2f);
                                        t.Cell().PaddingVertical(1).Text(trnDate).FontSize(8.2f);
                                        t.Cell().PaddingVertical(1).Text("Memo # :").Bold().FontSize(8.2f);
                                        t.Cell().PaddingVertical(1).Text(data.InvNo ?? "").FontSize(8.2f);
                                        t.Cell().PaddingVertical(1).Text("Printed On :").Bold().FontSize(8.2f);
                                        t.Cell().PaddingVertical(1).Text(printedOn).FontSize(8.2f);
                                    });
                                });
                            });

                            col.Item().PaddingTop(6).Row(row =>
                            {
                                row.RelativeItem().Element(e => RenderAddressBox(e, "Bill To:"));
                                row.ConstantItem(14);
                                row.RelativeItem().Element(e => RenderAddressBox(e, "Ship To:"));
                            });

                            if (!string.IsNullOrWhiteSpace(company.Information))
                                col.Item().PaddingVertical(4).Text(company.Information).FontSize(7f).Justify();

                            // 5-col Meta table
                            col.Item().PaddingTop(3).PaddingBottom(5).Table(t =>
                            {
                                t.ColumnsDefinition(cols => { for (int i = 0; i < 5; i++) cols.RelativeColumn(); });
                                IContainer MTh(IContainer c) => c.Background("#BFBFBF").Border(0.75f).BorderColor(Colors.Black).Padding(3).AlignCenter();
                                IContainer MTd(IContainer c) => c.Border(0.75f).BorderColor(Colors.Black).Padding(3).MinHeight(14).AlignCenter();

                                foreach (var h in new[] { "DESC", "SALES PERSON", "CUST ORDER #", "SHIP VIA", "TRACKING #" })
                                    t.Cell().Element(MTh).Text(h).Bold().FontSize(7.2f);

                                t.Cell().Element(MTd).AlignLeft().Text(data.InvDisc ?? "").FontSize(7.5f);
                                t.Cell().Element(MTd).Text(data.SalesPersons ?? "").FontSize(7.5f);
                                t.Cell().Element(MTd).Text("");
                                t.Cell().Element(MTd).Text(data.ShipVia ?? "").FontSize(7.5f);
                                t.Cell().Element(MTd).Text(data.Tracking ?? "").FontSize(7.5f);
                            });

                            // 10-col Items Table matching MixSalePrintUsaFormat.js
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(5);   // Sr No
                                    cols.RelativeColumn(7);   // ITEM
                                    cols.RelativeColumn(9);   // MemoNo
                                    cols.RelativeColumn(10);  // Stock #
                                    cols.RelativeColumn(7);   // QTY
                                    cols.RelativeColumn(9);   // WT.
                                    cols.RelativeColumn(9);   // TDC
                                    cols.RelativeColumn(20);  // DESCRIPTION
                                    cols.RelativeColumn(12);  // Price/Carat
                                    cols.RelativeColumn(12);  // Amount
                                });

                                IContainer Th(IContainer c) => c.Background("#BFBFBF").Border(0.75f).BorderColor(Colors.Black).Padding(3.5f).AlignCenter().AlignMiddle();
                                IContainer Td(IContainer c) => c.BorderLeft(0.75f).BorderRight(0.75f).BorderColor(Colors.Black).PaddingHorizontal(3).PaddingVertical(4).MinHeight(30).AlignMiddle();
                                IContainer TdFull(IContainer c) => c.Border(0.75f).BorderColor(Colors.Black).PaddingHorizontal(3).PaddingVertical(3).AlignTop();

                                table.Header(h =>
                                {
                                    foreach (var hdr in new[] { "Sr No", "ITEM", "MemoNo", "Stock #", "QTY", "WT.", "TDC", "DESCRIPTION", "Price/Carat", amountHeader })
                                        h.Cell().Element(Th).Text(hdr).Bold().FontSize(7.4f);
                                });

                                foreach (var item in pageItems)
                                {
                                    srNo++;
                                    int pcs = item.Pcs > 0 ? item.Pcs : item.Qty;
                                    double wt = item.Carats > 0 ? item.Carats : item.Weight;
                                    double rate = item.PricePerCarat > 0 ? item.PricePerCarat : item.Rate;
                                    double amt = item.Amount > 0 ? item.Amount : item.MasterCurrencyAmount;

                                    table.Cell().Element(Td).AlignCenter().Text(srNo.ToString(CultureInfo.InvariantCulture)).FontSize(7.6f);
                                    table.Cell().Element(Td).AlignCenter().Element(imgCell =>
                                    {
                                        if (item.ImageFullPath != null && item.ImageFullPath.Length > 0)
                                        {
                                            try { imgCell.MaxWidth(36).MaxHeight(26).Image(item.ImageFullPath).FitArea(); }
                                            catch { imgCell.Text(""); }
                                        }
                                        else
                                        {
                                            imgCell.Text("");
                                        }
                                    });
                                    table.Cell().Element(Td).AlignCenter().Text(item.MemoNo ?? "").FontSize(7.6f);
                                    table.Cell().Element(Td).AlignLeft().Text(item.Lotno ?? item.StockNo ?? "").FontSize(7.6f);
                                    table.Cell().Element(Td).AlignCenter().Text(pcs.ToString(CultureInfo.InvariantCulture)).FontSize(7.6f);
                                    table.Cell().Element(Td).AlignRight().Text(FormatCarat(wt, company.ParcelCaratFormat)).FontSize(7.6f);
                                    table.Cell().Element(Td).AlignRight().Text(item.TotalDiamondCarat.HasValue ? FormatCarat(item.TotalDiamondCarat.Value, company.ParcelCaratFormat) : "").FontSize(7.6f);
                                    table.Cell().Element(Td).AlignLeft().Text(!string.IsNullOrWhiteSpace(item.ItemName) ? item.ItemName : (item.Description ?? "")).FontSize(7.6f);
                                    table.Cell().Element(Td).AlignRight().Text(FormatMoney(rate, dec)).FontSize(7.6f);
                                    table.Cell().Element(Td).AlignRight().Text(FormatMoney(amt, dec)).FontSize(7.6f);
                                }

                                if (!isLastPage)
                                {
                                    table.Cell().ColumnSpan(10).BorderTop(0.75f).BorderColor(Colors.Black).Text("");
                                }
                                else
                                {
                                    table.Cell().Element(TdFull).Text("");
                                    table.Cell().Element(TdFull).Text("");
                                    table.Cell().Element(TdFull).Text("");
                                    table.Cell().Element(TdFull).Text("");
                                    table.Cell().Element(TdFull).AlignRight().Text(totalQty.ToString(CultureInfo.InvariantCulture)).FontSize(8f);
                                    table.Cell().Element(TdFull).AlignRight().Text(FormatCarat(totalWeight, company.ParcelCaratFormat)).FontSize(8f);
                                    table.Cell().Element(TdFull).AlignRight().Text(FormatCarat(totalTdc, company.ParcelCaratFormat)).FontSize(8f);
                                    table.Cell().Element(TdFull).AlignLeft().Text("Terms: Due upon receipt").Bold().FontSize(7.8f);
                                    table.Cell().Element(TdFull).AlignRight().Column(sc =>
                                    {
                                        sc.Item().AlignRight().Text("Sub Total").FontSize(7.8f);
                                        sc.Item().PaddingTop(2).AlignRight().Text("Charges").FontSize(7.8f);
                                        sc.Item().PaddingTop(2).AlignRight().Text("Discount").FontSize(7.8f);
                                        sc.Item().PaddingTop(3).AlignRight().Text("TOTAL").Bold().FontSize(9f);
                                    });
                                    table.Cell().Element(TdFull).AlignRight().Column(vc =>
                                    {
                                        vc.Item().AlignRight().Text(FormatMoney(totalAmount, dec)).FontSize(8f);
                                        vc.Item().PaddingTop(2).AlignRight().Text(data.Charges > 0 ? FormatMoney(data.Charges, dec) : " ").FontSize(8f);
                                        vc.Item().PaddingTop(2).AlignRight().Text(data.Discount > 0 ? FormatMoney(data.Discount, dec) : " ").FontSize(8f);
                                        vc.Item().PaddingTop(3).AlignRight().Text(FormatMoney(finalTotal, dec)).Bold().FontSize(9f);
                                    });
                                }
                            });

                            if (isLastPage)
                            {
                                col.Item().PaddingTop(10).Column(c =>
                                {
                                    if (!string.IsNullOrWhiteSpace(data.InvFeesDescription))
                                        c.Item().PaddingBottom(4).Text(data.InvFeesDescription).Bold().FontColor(Colors.Red.Medium).FontSize(8f);

                                    bool hasBank = !string.IsNullOrWhiteSpace(data.bnm) || !string.IsNullOrWhiteSpace(data.BankAccNo)
                                                   || !string.IsNullOrWhiteSpace(data.ABARoutingNumber) || !string.IsNullOrWhiteSpace(data.SwiftCode)
                                                   || !string.IsNullOrWhiteSpace(data.ZelleID);
                                    if (hasBank)
                                    {
                                        c.Item().Text("Payment Instruction:").Bold().FontSize(8.5f);
                                        if (!string.IsNullOrWhiteSpace(company.CompanyName)) c.Item().Text(company.CompanyName).FontSize(8f);
                                        if (!string.IsNullOrWhiteSpace(company.CompanyAddress)) c.Item().Text(company.CompanyAddress).FontSize(8f);
                                        c.Item().PaddingTop(3).Table(bt =>
                                        {
                                            bt.ColumnsDefinition(cols => { cols.ConstantColumn(105); cols.RelativeColumn(); });
                                            void BankRow(string k, string? v)
                                            {
                                                if (string.IsNullOrWhiteSpace(v)) return;
                                                bt.Cell().PaddingVertical(1).Text(k).Bold().FontSize(8f);
                                                bt.Cell().PaddingVertical(1).Text(v).Bold().FontSize(8f);
                                            }
                                            BankRow("Bank Name :", data.bnm);
                                            BankRow("Account No :", data.BankAccNo);
                                            BankRow("Routing Number :", data.ABARoutingNumber);
                                            BankRow("Swift Code :", data.SwiftCode);
                                            BankRow("ZELLE Id :", data.ZelleID);
                                        });
                                    }
                                });

                                col.Item().PaddingTop(14).Row(r =>
                                {
                                    r.AutoItem().Text("Signature:   ").FontSize(8.5f);
                                    r.ConstantItem(200).PaddingTop(10).LineHorizontal(0.75f).LineColor(Colors.Black);
                                });

                                if (!string.IsNullOrWhiteSpace(conditions))
                                    col.Item().PaddingTop(8).Text(conditions).FontSize(7.2f).Justify();
                            }
                        });

                        page.Footer().Row(r =>
                        {
                            r.RelativeItem().AlignLeft().Text(footerText).FontSize(8f);
                            r.RelativeItem().AlignRight().Text($"{idx + 1} / {pages.Count}").FontSize(8f);
                        });
                    });
                }
            }).GeneratePdf();
        }

        /// <summary>Matches React ParcelSaleDubaiLocalFormat.js — TAX INVOICE.</summary>
        private static byte[] BuildMixSaleDubaiPdf(
            MixSalePrintViewDto data,
            List<MixSalePrintItemDto> items,
            CompanyEmailSettings company,
            byte[]? logoBytes)
        {
            const int pageSize = 20;
            var pages = PaginateFixed(items, pageSize);
            int dec = company.AmountDecimals;
            var (_, amountHeader, totalAmount, finalTotal) = ResolveSaleTotals(data, items);
            double totalWeight = items.Sum(i => i.Carats > 0 ? i.Carats : i.Weight);
            int totalQty = items.Sum(i => i.Pcs > 0 ? i.Pcs : i.Qty);
            string conditions = !string.IsNullOrWhiteSpace(company.SaleConditions) ? company.SaleConditions : FallbackMemoConditions;
            string footerText = string.IsNullOrWhiteSpace(company.CompanyFooter) ? "Powered by SoftOnCloud" : company.CompanyFooter;

            return Document.Create(container =>
            {
                for (int pageIdx = 0; pageIdx < pages.Count; pageIdx++)
                {
                    int idx = pageIdx;
                    var pageItems = pages[idx];
                    bool isLastPage = idx == pages.Count - 1;
                    int srNo = idx * pageSize;

                    container.Page(page =>
                    {
                        page.Size(PageSizes.Letter);
                        page.MarginHorizontal(22);
                        page.MarginVertical(18);
                        page.DefaultTextStyle(x => x.FontSize(8.5f).FontFamily("Lato"));

                        page.Content().Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                if (logoBytes != null && logoBytes.Length > 0)
                                    row.ConstantItem(78).PaddingRight(8).AlignMiddle().Image(logoBytes).FitArea();

                                row.RelativeItem(5).Column(c =>
                                {
                                    if (!string.IsNullOrWhiteSpace(company.CompanyName)) c.Item().Text(company.CompanyName).Bold().FontSize(14);
                                    if (!string.IsNullOrWhiteSpace(company.CompanyAddress)) c.Item().Text(company.CompanyAddress).FontSize(8.2f);
                                    if (!string.IsNullOrWhiteSpace(company.CompanyAddress1)) c.Item().Text(company.CompanyAddress1).FontSize(8.2f);
                                });

                                row.RelativeItem(3.5f).Column(c =>
                                {
                                    if (!string.IsNullOrWhiteSpace(company.CompanyPhone)) c.Item().Text($"Tel : {company.CompanyPhone}").Bold().FontSize(8f);
                                    if (!string.IsNullOrWhiteSpace(company.CompanyEmail)) c.Item().Text($"Email : {company.CompanyEmail}").Bold().FontSize(8f);
                                });
                            });

                            col.Item().PaddingVertical(6).Background(Colors.Black).PaddingVertical(3).AlignCenter()
                                .Text("Diamond & Diamond Studded Jewellery Exporters, Importers & Manufacturers")
                                .Bold().FontSize(8.5f).FontColor(Colors.White);

                            col.Item().AlignCenter().PaddingBottom(4).Text("TAX INVOICE").Bold().FontSize(11);

                            col.Item().PaddingBottom(6).Row(row =>
                            {
                                row.RelativeItem(6).Column(c =>
                                {
                                    c.Item().Text(t => { t.Span("Invoice No : ").FontSize(8.2f); t.Span(data.InvNo ?? "-").Bold().FontSize(8.2f); });
                                    c.Item().Text(t => { t.Span("To : ").FontSize(8.2f); t.Span(data.PartyName ?? "-").Bold().FontSize(8.2f); });
                                    var addr = string.Join(", ", new[] { data.AddressLine1, data.AddressLine2, data.AddressLine3 }.Where(s => !string.IsNullOrWhiteSpace(s)));
                                    if (!string.IsNullOrWhiteSpace(addr))
                                        c.Item().Text(t => { t.Span("Address : ").FontSize(8.2f); t.Span(addr).Bold().FontSize(8.2f); });
                                });
                                row.RelativeItem(4).Column(c =>
                                {
                                    c.Item().Text(t => { t.Span("Invoice Date : ").FontSize(8.2f); t.Span(FormatDateMmm(data.TrnDate)).Bold().FontSize(8.2f); });
                                    c.Item().Text(t => { t.Span("Terms : ").FontSize(8.2f); t.Span(data.DueDays > 0 ? $"{data.DueDays} Days" : "0").Bold().FontSize(8.2f); });
                                    c.Item().Text(t => { t.Span("Due Date : ").FontSize(8.2f); t.Span(FormatDateMmm(data.DueDate)).Bold().FontSize(8.2f); });
                                });
                            });

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.ConstantColumn(30);
                                    cols.ConstantColumn(75);
                                    cols.RelativeColumn(3);
                                    cols.ConstantColumn(42);
                                    cols.ConstantColumn(65);
                                    cols.ConstantColumn(75);
                                    cols.ConstantColumn(85);
                                });

                                IContainer Th(IContainer c) => c.Background("#C0C0C0").Border(1f).BorderColor("#454545").Padding(3.5f).AlignCenter().AlignMiddle();
                                IContainer Td(IContainer c) => c.Border(0.75f).BorderColor("#454545").Padding(3.5f).AlignMiddle();

                                table.Header(h =>
                                {
                                    foreach (var hdr in new[] { "Sr", "Lot No", "Description", "Pcs", "Carats", "Rate", amountHeader })
                                        h.Cell().Element(Th).Text(hdr).Bold().FontSize(8f);
                                });

                                foreach (var item in pageItems)
                                {
                                    srNo++;
                                    int pcs = item.Pcs > 0 ? item.Pcs : item.Qty;
                                    double wt = item.Carats > 0 ? item.Carats : item.Weight;
                                    double rate = item.Rate > 0 ? item.Rate : item.PricePerCarat;
                                    double amt = item.Amount > 0 ? item.Amount : item.MasterCurrencyAmount;

                                    table.Cell().Element(Td).AlignCenter().Text(srNo.ToString(CultureInfo.InvariantCulture)).FontSize(8f);
                                    table.Cell().Element(Td).AlignCenter().Text(item.Lotno ?? item.StockNo ?? "-").FontSize(8f);
                                    table.Cell().Element(Td).AlignLeft().Text(!string.IsNullOrWhiteSpace(item.Description) ? item.Description : (item.ItemName ?? "-")).FontSize(8f);
                                    table.Cell().Element(Td).AlignRight().Text(pcs.ToString(CultureInfo.InvariantCulture)).FontSize(8f);
                                    table.Cell().Element(Td).AlignRight().Text(FormatCarat(wt, company.ParcelCaratFormat)).FontSize(8f);
                                    table.Cell().Element(Td).AlignRight().Text(FormatMoney(rate, dec)).FontSize(8f);
                                    table.Cell().Element(Td).AlignRight().Text(FormatMoney(amt, dec)).FontSize(8f);
                                }

                                if (isLastPage)
                                {
                                    table.Cell().ColumnSpan(3).Element(Td).AlignRight().Text("TOTAL").Bold().FontSize(8.5f);
                                    table.Cell().Element(Td).AlignRight().Text(totalQty.ToString(CultureInfo.InvariantCulture)).Bold().FontSize(8.5f);
                                    table.Cell().Element(Td).AlignRight().Text(FormatCarat(totalWeight, company.ParcelCaratFormat)).Bold().FontSize(8.5f);
                                    table.Cell().Element(Td).AlignRight().Text(totalWeight > 0 ? FormatMoney(totalAmount / totalWeight, dec) : "0.00").Bold().FontSize(8.5f);
                                    table.Cell().Element(Td).AlignRight().Text(FormatMoney(finalTotal, dec)).Bold().FontSize(8.5f);
                                }
                            });

                            if (isLastPage)
                            {
                                col.Item().PaddingTop(10).Text(conditions).FontSize(7.2f).Justify();
                                col.Item().PaddingTop(24).Row(r =>
                                {
                                    r.RelativeItem().Column(c =>
                                    {
                                        c.Item().Width(160).LineHorizontal(0.75f).LineColor(Colors.Black);
                                        c.Item().PaddingTop(3).Text("Receiver's Signature").FontSize(8f);
                                    });
                                    r.RelativeItem().AlignRight().Column(c =>
                                    {
                                        c.Item().AlignRight().Width(160).LineHorizontal(0.75f).LineColor(Colors.Black);
                                        c.Item().PaddingTop(3).AlignRight().Text($"For {company.CompanyName}").Bold().FontSize(8f);
                                    });
                                });
                            }
                        });

                        page.Footer().Row(r =>
                        {
                            r.RelativeItem().AlignLeft().Text(footerText).FontSize(8f);
                            r.RelativeItem().AlignRight().Text($"{idx + 1} / {pages.Count}").FontSize(8f);
                        });
                    });
                }
            }).GeneratePdf();
        }
    }
}

