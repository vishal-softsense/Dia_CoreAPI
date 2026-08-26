using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.DTOs.Reports.Utility;

namespace Dia_CoreAPI.Services.Classes.Reports.Utility
{
    public partial class ImportGradingService
    {
        private async Task<ImportGradingResult> InsUpdparcelitemInternalAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
        {
            var tableOrError = ReadExcel(dto.ImportFile);
            if (!tableOrError.Success) return tableOrError.Error;
            var dt = tableOrError.Table!;
            if (dt.Columns.Count < 1 || dt.Columns[0].ColumnName.Trim() != "LotNo")
                return new ImportGradingResult(false, "Invalid file format. First column must be LotNo....!");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (string.IsNullOrEmpty(Cell(dt.Rows[i], 0).Trim()))
                    return new ImportGradingResult(false, "LotNo missing Please check the file.");
            }

            int companyid = ParseCompanyId(companyId);
            using var sql = await CreateSqlTempAsync(companyId);
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (string.IsNullOrEmpty(Cell(dt.Rows[i], 0)))
                        continue;

                    sql.StartTransaction();
                    var itemCode = sql.GetLotNo("And LotNo In(''" + Cell(dt.Rows[i], 0).Replace("'", "''") + "'')");
                    var category = Cell(dt.Rows[i], 2);
                    var productType = Cell(dt.Rows[i], 3);
                    var itemGroupId = Cell(dt.Rows[i], 4);
                    float pcs = CellFlt(dt.Rows[i], 10);
                    var shapeId = Cell(dt.Rows[i], 5);
                    var sieveId = Cell(dt.Rows[i], 6);
                    var sizeId = Cell(dt.Rows[i], 7);
                    var colorId = Cell(dt.Rows[i], 8);
                    var clarityId = Cell(dt.Rows[i], 9);
                    var action = string.IsNullOrEmpty(itemCode) ? "insert" : "update";
                    var code = string.IsNullOrEmpty(itemCode) ? 0 : Convert.ToInt32(itemCode);

                    await _repository.SaveItemMasterData(
                        itemCode: code,
                        itemSrno: 0,
                        itemId: Cell(dt.Rows[i], 0),
                        itemName: Cell(dt.Rows[i], 1),
                        itemGroupId: itemGroupId,
                        category: category,
                        productType: productType,
                        pcs: pcs,
                        weight: CellDbl(dt.Rows[i], 11),
                        shapeId: shapeId,
                        colorId: colorId,
                        sizeId: sizeId,
                        clarityId: clarityId,
                        mrp: 0,
                        back: 0,
                        askingRate: CellFlt(dt.Rows[i], 12),
                        askingAmount: CellFlt(dt.Rows[i], 13),
                        boxName: "",
                        boxRate: 0,
                        certId: "",
                        certificateNo: "",
                        haid: "",
                        propId: "",
                        polishId: "",
                        symId: "",
                        flId: "",
                        flColor: "",
                        fancyColor: "",
                        fancyColorIntensity: "",
                        fancyColorOvertone: "",
                        l: 0,
                        w: 0,
                        d: 0,
                        diameter: "",
                        measurement: "",
                        lwRatio: "",
                        totDepth: 0,
                        tablePer: 0,
                        crownHeight: 0,
                        crownAngle: 0,
                        pavilionDepth: 0,
                        pavilionHeight: 0,
                        pavilionAngle: 0,
                        girdlePer: 0,
                        girdleCondition: "",
                        girdleMin: 0,
                        girdleMax: 0,
                        girdleMinMs: 0,
                        girdleMaxMs: 0,
                        culetId: "",
                        culetCondition: "",
                        strLn: 0,
                        lrHalf: 0,
                        laserInscription: false,
                        laserInscriptionText: "",
                        instruction: "",
                        keyToSymbols: "",
                        labComments: "",
                        memberComments: "",
                        brownId: "",
                        greenId: "",
                        milkyId: "",
                        lusterId: "",
                        buthId: "",
                        nattsId: "",
                        eyeClean: "",
                        shadeId: "",
                        tableInclusion: "",
                        crownInclusion: "",
                        oit: "",
                        oic: "",
                        oip: "",
                        oig: "",
                        polishFeatures: "",
                        symFeatures: "",
                        colorDescription: "",
                        clarityDescription: "",
                        clarityStatus: "",
                        graining: "",
                        painting: "",
                        idealImagePath: "",
                        assetImagePath: "",
                        heartsImagePath: "",
                        arrowsImagePath: "",
                        imagePath: "",
                        videoPath: "",
                        b2bDnaPagePath: "",
                        b2cDnaPagePath: "",
                        certificatePath: "",
                        plottingImagePath: "",
                        purchaseDate: DateTime.Now,
                        rateReviseDate: DateTime.Now,
                        newArrivalDate: DateTime.Now,
                        stoneLastUnHoldDate: DateTime.Now,
                        publicRemarks: "",
                        privateRemarks: "",
                        adminRemarks: "",
                        purchaseBack: 0,
                        singlePcs: 0,
                        purchaseRate: CellFlt(dt.Rows[i], 14),
                        purchaseAmount: CellFlt(dt.Rows[i], 15),
                        actualBack: 0,
                        actualRate: CellFlt(dt.Rows[i], 20),
                        actualAmount: CellFlt(dt.Rows[i], 21),
                        bottomBack: 0,
                        bottomRate: 0,
                        b2bBack: 0,
                        b2bRate: 0,
                        b2cBack: 0,
                        b2cRate: 0,
                        mfgSeriesId: "",
                        mfgPacketId: "",
                        roughId: "",
                        minesId: "",
                        actionType: action,
                        companyId: companyid,
                        sieveId: sieveId,
                        masterCurrencyRate: CellFlt(dt.Rows[i], 16),
                        masterCurrencyAmount: CellFlt(dt.Rows[i], 17),
                        stockCurrencyRate: CellFlt(dt.Rows[i], 18),
                        stockCurrencyAmount: CellFlt(dt.Rows[i], 19),
                        companyIdStr: companyId);
                    sql.CommitTransaction();
                }
            }
            catch
            {
                sql.RollbackTransaction();
            }

            return new ImportGradingResult(true, "Parcel Item Data Upload Succesfull.");
        }

        private async Task<ImportGradingResult> InsUpdsolitaireitemInternalAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
        {
            var tableOrError = ReadExcel(dto.ImportFile);
            if (!tableOrError.Success) return tableOrError.Error;
            var dt = tableOrError.Table!;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (string.IsNullOrEmpty(Cell(dt.Rows[i], 0).Trim()))
                    return new ImportGradingResult(false, "LotNo missing Please check the file.");
            }

            int companyid = ParseCompanyId(companyId);
            using var sql = await CreateSqlTempAsync(companyId);
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var row = dt.Rows[i];
                    if (string.IsNullOrEmpty(Cell(row, 0)))
                        continue;

                    sql.StartTransaction();
                    var itemCode = sql.GetLotNo("And LotNo In(''" + Cell(row, 0).Replace("'", "''") + "'')");
                    var action = string.IsNullOrEmpty(itemCode) ? "insert" : "update";
                    var code = string.IsNullOrEmpty(itemCode) ? 0 : Convert.ToInt32(itemCode);
                    float weight = CellFlt(row, 11);
                    float actualRate = CellFlt(row, 119);
                    await SaveSolitaireRowAsync(row, code, action, companyid, companyId, weight, actualRate);
                    sql.CommitTransaction();
                }
            }
            catch
            {
                sql.RollbackTransaction();
            }

            return new ImportGradingResult(true, "Solitaire Item Data Upload Succesfull.");
        }

        private Task<bool> SaveSolitaireRowAsync(DataRow row, int itemCode, string action, int companyid, string companyIdStr, float weight, float actualRate)
        {
            return _repository.SaveItemMasterData(
                itemCode: itemCode,
                itemSrno: 0,
                itemId: Cell(row, 0),
                itemName: Cell(row, 1),
                itemGroupId: Cell(row, 4),
                category: Cell(row, 2),
                productType: Cell(row, 3),
                pcs: CellFlt(row, 10),
                weight: weight,
                shapeId: Cell(row, 5),
                colorId: Cell(row, 8),
                sizeId: Cell(row, 7),
                clarityId: Cell(row, 9),
                mrp: CellFlt(row, 12),
                back: CellFlt(row, 13),
                askingRate: CellFlt(row, 14),
                askingAmount: CellFlt(row, 15),
                boxName: Cell(row, 16),
                boxRate: CellFlt(row, 17),
                certId: Cell(row, 18),
                certificateNo: Cell(row, 19),
                haid: Cell(row, 25),
                propId: Cell(row, 20),
                polishId: Cell(row, 21),
                symId: Cell(row, 22),
                flId: Cell(row, 23),
                flColor: Cell(row, 24),
                fancyColor: Cell(row, 26),
                fancyColorIntensity: Cell(row, 27),
                fancyColorOvertone: Cell(row, 28),
                l: CellFlt(row, 29),
                w: CellFlt(row, 30),
                d: CellFlt(row, 31),
                diameter: Cell(row, 32),
                measurement: Cell(row, 33),
                lwRatio: Cell(row, 34),
                totDepth: CellFlt(row, 35),
                tablePer: CellFlt(row, 36),
                crownHeight: CellFlt(row, 37),
                crownAngle: CellFlt(row, 38),
                pavilionDepth: CellFlt(row, 39),
                pavilionHeight: CellFlt(row, 40),
                pavilionAngle: CellFlt(row, 41),
                girdlePer: CellFlt(row, 47),
                girdleCondition: Cell(row, 46),
                girdleMin: CellFlt(row, 42),
                girdleMax: CellFlt(row, 43),
                girdleMinMs: CellFlt(row, 44),
                girdleMaxMs: CellFlt(row, 45),
                culetId: Cell(row, 48),
                culetCondition: Cell(row, 49),
                strLn: CellFlt(row, 50),
                lrHalf: CellFlt(row, 51),
                laserInscription: CellBool(row, 55),
                laserInscriptionText: Cell(row, 56),
                instruction: Cell(row, 54),
                keyToSymbols: Cell(row, 52),
                labComments: Cell(row, 53),
                memberComments: Cell(row, 60),
                brownId: Cell(row, 61),
                greenId: Cell(row, 62),
                milkyId: Cell(row, 63),
                lusterId: Cell(row, 65),
                buthId: Cell(row, 64),
                nattsId: Cell(row, 72),
                eyeClean: Cell(row, 75),
                shadeId: "",
                tableInclusion: "",
                crownInclusion: Cell(row, 68),
                oit: "",
                oic: Cell(row, 71),
                oip: Cell(row, 69),
                oig: Cell(row, 73),
                polishFeatures: Cell(row, 74),
                symFeatures: "",
                colorDescription: "",
                clarityDescription: "",
                clarityStatus: "",
                graining: "",
                painting: Cell(row, 76),
                idealImagePath: Cell(row, 77),
                assetImagePath: Cell(row, 78),
                heartsImagePath: Cell(row, 79),
                arrowsImagePath: Cell(row, 80),
                imagePath: Cell(row, 81),
                videoPath: Cell(row, 82),
                b2bDnaPagePath: Cell(row, 83),
                b2cDnaPagePath: Cell(row, 84),
                certificatePath: Cell(row, 85),
                plottingImagePath: "",
                purchaseDate: DateTime.Now,
                rateReviseDate: DateTime.Now,
                newArrivalDate: DateTime.Now,
                stoneLastUnHoldDate: DateTime.Now,
                publicRemarks: Cell(row, 89),
                privateRemarks: Cell(row, 90),
                adminRemarks: Cell(row, 91),
                purchaseBack: CellFlt(row, 105),
                singlePcs: 1,
                purchaseRate: CellFlt(row, 106),
                purchaseAmount: CellFlt(row, 110),
                actualBack: CellFlt(row, 118),
                actualRate: actualRate,
                actualAmount: actualRate * weight,
                bottomBack: CellFlt(row, 125),
                bottomRate: CellFlt(row, 126),
                b2bBack: CellFlt(row, 121),
                b2bRate: CellFlt(row, 122),
                b2cBack: CellFlt(row, 123),
                b2cRate: CellFlt(row, 124),
                mfgSeriesId: Cell(row, 92),
                mfgPacketId: Cell(row, 93),
                roughId: "",
                minesId: Cell(row, 131),
                actionType: action,
                companyId: companyid,
                sieveId: Cell(row, 6),
                masterCurrencyRate: CellFlt(row, 112),
                masterCurrencyAmount: CellFlt(row, 113),
                stockCurrencyRate: CellFlt(row, 114),
                stockCurrencyAmount: CellFlt(row, 115),
                companyIdStr: companyIdStr);
        }

        private async Task<ImportGradingResult> InsUpdaccountitemInternalAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
        {
            var tableOrError = ReadExcel(dto.ImportFile);
            if (!tableOrError.Success) return tableOrError.Error;
            var dt = tableOrError.Table!;
            if (dt.Columns.Count < 1 || dt.Columns[0].ColumnName.Trim() != "Account Name")
                return new ImportGradingResult(false, "Account File is not valid.");

            var regex = new Regex(@"^[A-Za-z0-9 &()'._|-]*$");
            int companyid = ParseCompanyId(companyId);
            using var sql = await CreateSqlTempAsync(companyId);
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var accName = Cell(dt.Rows[i], 0).Trim();
                    if (string.IsNullOrEmpty(accName) || !regex.IsMatch(accName))
                        continue;

                    sql.StartTransaction();
                    var ugrcode = sql.GetAccountCode("And AccGrpName=''" + Cell(dt.Rows[i], 2).Replace("'", "''") + "''");
                    var accTypeCode = sql.GetAccountTypeCode("And AccDisc=''" + Cell(dt.Rows[i], 3).Replace("'", "''") + "''");
                    var aclevel = sql.GetAccountLevel("And AccGrpName=''" + Cell(dt.Rows[i], 2).Replace("'", "''") + "''");
                    int level = (int.TryParse(aclevel, out var lvl) ? lvl : 0) + 1;
                    long curcode = await _repository.GetCurCodeAsync("Symbol='" + Cell(dt.Rows[i], 6).ToString() + "'", companyId);
                    var accountcode = sql.GetAccountCode(" And AccGrpName IN(''" + accName.Replace("'", "''") + "'')");
                    var action = string.IsNullOrEmpty(accountcode) ? "insert" : "update";
                    var code = string.IsNullOrEmpty(accountcode) ? 0 : Convert.ToInt32(accountcode);

                    await _repository.SaveAccGrpMasterData(
                        code, accName, Cell(dt.Rows[i], 1),
                        int.TryParse(ugrcode, out var ug) ? ug : 0,
                        int.TryParse(accTypeCode, out var at) ? at : 0,
                        level, Cell(dt.Rows[i], 4), CellDbl(dt.Rows[i], 5),
                        (int)curcode, CellFlt(dt.Rows[i], 7), CellDbl(dt.Rows[i], 8),
                        CellFlt(dt.Rows[i], 9), CellDbl(dt.Rows[i], 10), Cell(dt.Rows[i], 11), null, Cell(dt.Rows[i], 15),
                        Cell(dt.Rows[i], 16), Cell(dt.Rows[i], 17), Cell(dt.Rows[i], 18), null, Cell(dt.Rows[i], 20), Cell(dt.Rows[i], 22), Cell(dt.Rows[i], 23),
                        Cell(dt.Rows[i], 24), Cell(dt.Rows[i], 65), Cell(dt.Rows[i], 28), Cell(dt.Rows[i], 29), Cell(dt.Rows[i], 30),
                        Cell(dt.Rows[i], 31), Cell(dt.Rows[i], 32), Cell(dt.Rows[i], 33), Cell(dt.Rows[i], 34), Cell(dt.Rows[i], 35),
                        Cell(dt.Rows[i], 36), 0, 0, 0, 0, 0, CellInt(dt.Rows[i], 40), CellInt(dt.Rows[i], 41),
                        CellDbl(dt.Rows[i], 44), CellInt(dt.Rows[i], 45), CellInt(dt.Rows[i], 46), null, null, 0, 0, null, Cell(dt.Rows[i], 42),
                        CellInt(dt.Rows[i], 43), Cell(dt.Rows[i], 47), Cell(dt.Rows[i], 56), Cell(dt.Rows[i], 58),
                        Cell(dt.Rows[i], 53), null, null, Cell(dt.Rows[i], 60), CellDbl(dt.Rows[i], 51), 0, Cell(dt.Rows[i], 21), Cell(dt.Rows[i], 25),
                        Cell(dt.Rows[i], 52), null, Cell(dt.Rows[i], 12), Cell(dt.Rows[i], 13), Cell(dt.Rows[i], 14), Cell(dt.Rows[i], 62),
                        Cell(dt.Rows[i], 63), Cell(dt.Rows[i], 64), null, Cell(dt.Rows[i], 54), Cell(dt.Rows[i], 55), null,
                        null, null, false, false, null, null, null, null, Cell(dt.Rows[i], 26), Cell(dt.Rows[i], 59),
                        false, null, companyid, null, Cell(dt.Rows[i], 66), action, companyId);
                    sql.CommitTransaction();
                }
            }
            catch
            {
                sql.RollbackTransaction();
            }

            return new ImportGradingResult(true, "Account Data Upload Succesfull.");
        }

        private async Task<ImportGradingResult> UploadPurchaseDataInternalAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
        {
            var tableOrError = ReadExcel(dto.ImportFile);
            if (!tableOrError.Success) return tableOrError.Error;
            var dt = tableOrError.Table!;
            if (dt.Columns.Count < 1 || dt.Columns[0].ColumnName.Trim() != "BillNo")
                return new ImportGradingResult(false, "The provided Purchase  data file is invalid....! ");

            int companyid = ParseCompanyId(companyId);
            int usercode = int.TryParse(userId, out var uid) ? uid : 0;
            using var sql = await CreateSqlTempAsync(companyId);
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sql.StartTransaction();
                    var partyAccGrp = Cell(dt.Rows[i], 8).Trim();
                    var adatAccGrp = Cell(dt.Rows[i], 15).Trim();
                    var brokerAccGrp = Cell(dt.Rows[i], 16).Trim();
                    if (partyAccGrp == "")
                        return new ImportGradingResult(false, $"Row {i + 2}: Party name is missing in Excel.");

                    var accountcode = sql.GetAccountCode(" And AccGrpName IN(''" + partyAccGrp.Replace("'", "''") + "'')");
                    var commcode = string.IsNullOrEmpty(adatAccGrp) ? null : sql.GetAccountCode(" And AccGrpName IN(''" + adatAccGrp.Replace("'", "''") + "'')");
                    var brokercode = string.IsNullOrEmpty(brokerAccGrp) ? null : sql.GetAccountCode(" And AccGrpName IN(''" + brokerAccGrp.Replace("'", "''") + "'')");

                    if (!string.IsNullOrEmpty(partyAccGrp) && string.IsNullOrEmpty(accountcode))
                        return new ImportGradingResult(false, $"Account '{partyAccGrp}' not found in master.");
                    if (!string.IsNullOrEmpty(adatAccGrp) && string.IsNullOrEmpty(commcode))
                        return new ImportGradingResult(false, $"Adat account '{adatAccGrp}' not found in master.");
                    if (!string.IsNullOrEmpty(brokerAccGrp) && string.IsNullOrEmpty(brokercode))
                        return new ImportGradingResult(false, $"Broker account '{brokerAccGrp}' not found in master.");

                    long accountCodeLong = !string.IsNullOrEmpty(accountcode) ? Convert.ToInt64(accountcode) : 0;
                    long brokerCodeLong = !string.IsNullOrEmpty(brokercode) ? Convert.ToInt64(brokercode) : 0;
                    long commCodeLong = !string.IsNullOrEmpty(commcode) ? Convert.ToInt64(commcode) : 0;
                    int curcode = CellInt(dt.Rows[i], 21);
                    float mCurBaseRate = CellFlt(dt.Rows[i], 13);
                    float sCurBaseRate = 1;
                    string mCurOperator = await _repository.GetMCurOperatorAsync(" and ID=" + curcode, companyId);
                    string sCurOperator = await _repository.GetSCurOperatorAsync(" and ID=" + curcode, companyId);
                    float totCarats = CellFlt(dt.Rows[i], 9);
                    double usdAmount = CellDbl(dt.Rows[i], 10);
                    double plusMinusAmount = CellDbl(dt.Rows[i], 11);
                    decimal netAmount = plusMinusAmount == 0 ? Convert.ToDecimal(CellFlt(dt.Rows[i], 12)) : Convert.ToDecimal(usdAmount + plusMinusAmount);
                    var amountResponse = await _repository.GetConvertAmountAsync(curcode, (float)netAmount, mCurBaseRate, mCurOperator, sCurBaseRate, sCurOperator, companyId);
                    double mCurNetAmt = Convert.ToDouble(amountResponse[0]);
                    double sCurNeTAmt = Convert.ToDouble(amountResponse[1]);
                    float mToSCurRate = sCurNeTAmt == 0 ? 0 : (float)mCurNetAmt / (float)sCurNeTAmt;
                    float finalMAmount = CellFlt(dt.Rows[i], 10);
                    float mCurCode = await _repository.GetMCurIdAsync(companyId);
                    float sCurCode = await _repository.GetSCurIdAsync(companyId);
                    string mCurOperatorM = await _repository.GetMCurOperatorAsync(" and ID=" + mCurCode, companyId);
                    string sCurOperatorM = await _repository.GetSCurOperatorAsync(" and ID=" + mCurCode, companyId);
                    var finalAmountResponse = GetConvertAmount(mCurCode, (float)mCurNetAmt, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, sCurCode);
                    double.TryParse(finalAmountResponse[0], out double mCurFinalAmt);
                    double.TryParse(finalAmountResponse[1], out double sCurFinalAmt);

                    var trnType = Cell(dt.Rows[i], 1);
                    var invno = sql.GetMaxInvNo(" And TrnType=''" + trnType.Replace("'", "''") + "'' ");
                    int maxInvNo = (int.TryParse(invno, out var inv) ? inv : 0) + 1;
                    double adatPer = CellDbl(dt.Rows[i], 17);
                    double brokerPer = CellDbl(dt.Rows[i], 18);
                    double adatamount = mCurFinalAmt * adatPer / 100;
                    double brokeramount = (mCurFinalAmt - adatamount) * brokerPer / 100;

                    if (!DateTime.TryParse(Cell(dt.Rows[i], 5), out var parsedDate) || !DateTime.TryParse(Cell(dt.Rows[i], 7), out var parsedDate1))
                        return new ImportGradingResult(false, $"Row {i + 2}: Invalid date format in DATE or Duedate column.");

                    if (!string.IsNullOrEmpty(Cell(dt.Rows[i], 0)))
                    {
                        sql.InsertSalePur(maxInvNo, Cell(dt.Rows[i], 0), trnType, Cell(dt.Rows[i], 2), Cell(dt.Rows[i], 4),
                            parsedDate, CellInt(dt.Rows[i], 6), parsedDate1, accountCodeLong, 0, "", 0, brokerCodeLong, (float)brokerPer,
                            (float)brokeramount, commCodeLong, (float)adatPer, (float)adatamount, totCarats,
                            usdAmount, Convert.ToDouble(netAmount), plusMinusAmount, 0,
                            Math.Round(finalMAmount, 4), Math.Round(mCurBaseRate, 4), "", curcode.ToString(), true, false, false, false, false,
                            usercode, Math.Round(mCurBaseRate, 4), Math.Round(mCurFinalAmt, 4), Math.Round(sCurBaseRate, 4),
                            Math.Round(sCurFinalAmt, 4), 0, parsedDate1, 0, 0, 0, 0, "", 0, Cell(dt.Rows[i], 3), "", "",
                            "", "", "", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, userId, companyid, mCurFinalAmt, Convert.ToDouble(netAmount));
                    }
                    sql.CommitTransaction();
                }
                return new ImportGradingResult(true, "Purchase Data Upload Successful.");
            }
            catch (Exception ex)
            {
                sql.RollbackTransaction();
                return new ImportGradingResult(false, "Error occurred during upload: " + ex.Message);
            }
        }

        private async Task<ImportGradingResult> UploadPurchaseDetailsInternalAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
        {
            var tableOrError = ReadExcel(dto.ImportFile);
            if (!tableOrError.Success) return tableOrError.Error;
            var dt = tableOrError.Table!;
            if (dt.Columns.Count < 1 || dt.Columns[0].ColumnName != "BillNo")
                return new ImportGradingResult(false, "The purchase file format is invalid.");

            int companyid = ParseCompanyId(companyId);
            int usercode = int.TryParse(userId, out var uid) ? uid : 0;
            using var sql = await CreateSqlTempAsync(companyId);
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sql.StartTransaction();
                    int curcode = CellInt(dt.Rows[i], 14);
                    float mCurBaseRate = CellFlt(dt.Rows[i], 17);
                    float sCurBaseRate = 85;
                    string mCurOperator = await _repository.GetMCurOperatorAsync(" and ID=" + curcode, companyId);
                    string sCurOperator = await _repository.GetSCurOperatorAsync(" and ID=" + curcode, companyId);
                    var billNo = Cell(dt.Rows[i], 0).Trim();
                    var trnType = Cell(dt.Rows[i], 1).Trim();
                    if (string.IsNullOrEmpty(billNo) || string.IsNullOrEmpty(trnType))
                        return new ImportGradingResult(false, $"Row {i + 1}: BillNo or TrnType is missing.");

                    var t = sql.GetCheckTrntype("and BillNo=''" + billNo.Replace("'", "''") + "'' and TrnType=''" + trnType.Replace("'", "''") + "'' and DeleteFlag=0 ");
                    if (string.IsNullOrEmpty(t))
                        return new ImportGradingResult(false, "Invalid purchase details file. Please check the file format and try again.");

                    var maxSPCode = sql.GetMaxSPCode("and BillNo=''" + billNo.Replace("'", "''") + "'' and TrnType=''" + trnType.Replace("'", "''") + "'' and DeleteFlag=0  ");
                    float purAmount = CellFlt(dt.Rows[i], 13);
                    double less1 = CellDbl(dt.Rows[i], 10);
                    double less2 = CellDbl(dt.Rows[i], 11);
                    double less3 = CellDbl(dt.Rows[i], 12);
                    if (less1 != 0) purAmount = purAmount * (100 + (float)less1) / 100;
                    if (less2 != 0) purAmount = purAmount * (100 + (float)less2) / 100;
                    if (less3 != 0) purAmount = purAmount * (100 + (float)less3) / 100;
                    var purAmountResponse = await _repository.GetConvertAmountAsync(curcode, purAmount, mCurBaseRate, mCurOperator, sCurBaseRate, sCurOperator, companyId);
                    double purMCurAmt = Convert.ToDouble(purAmountResponse[0]);
                    double purSCurAmt = Convert.ToDouble(purAmountResponse[1]);
                    var itemCode = sql.GetMaxItemCode(" AND LotNo = ''" + Cell(dt.Rows[i], 2).Replace("'", "''") + "''");
                    int maxItemCode = int.TryParse(itemCode, out var ic) ? ic : 0;
                    if (!DateTime.TryParse(Cell(dt.Rows[i], 15), out var parsedDate))
                        return new ImportGradingResult(false, $"Row {i + 2}: Invalid date format in DATE or Duedate column.");

                    sql.InsertSalePurTran(parsedDate, trnType, Cell(dt.Rows[i], 16), Convert.ToInt32(maxSPCode), maxItemCode, Cell(dt.Rows[i], 3),
                        CellFlt(dt.Rows[i], 7), CellFlt(dt.Rows[i], 4), CellDbl(dt.Rows[i], 9), CellDbl(dt.Rows[i], 5), CellDbl(dt.Rows[i], 6),
                        0, 0, Convert.ToDouble(purAmount.ToString("N2")), Cell(dt.Rows[i], 18), less1, less2, less3,
                        curcode.ToString(), 0, false, false, 0, 0, 0, 0, 0, 0, 0, 0, "", usercode, Math.Round(mCurBaseRate, 4),
                        Convert.ToDouble(purMCurAmt.ToString("N2")), Math.Round(sCurBaseRate, 4),
                        Convert.ToDouble(purSCurAmt.ToString("N2")), userId, companyid);
                    sql.CommitTransaction();
                }
                return new ImportGradingResult(true, "Purchase Details Data Upload Successful.");
            }
            catch (Exception ex)
            {
                sql.RollbackTransaction();
                return new ImportGradingResult(false, "Error occurred during upload: " + ex.Message);
            }
        }

#if false // Disabled: Import Sale commented in MVC UI
        public async Task<ImportGradingResult> InsertSaleDataAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
        {
            var tableOrError = ReadExcel(dto.ImportFile);
            if (!tableOrError.Success) return tableOrError.Error;
            var dt = tableOrError.Table!;
            if (dt.Rows.Count == 0)
                return new ImportGradingResult(false, "Excel is empty");

            int companyid = ParseCompanyId(companyId);
            int usercode = int.TryParse(userId, out var uid) ? uid : 0;
            using var sql = await CreateSqlTempAsync(companyId);
            try
            {
                sql.StartTransaction();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    int curcode = 4;
                    var partyName = Cell(dt.Rows[i], 2).Replace("'", "''");
                    var partyCode = sql.GetAccountCode("And AccGrpName=''" + partyName + "''");
                    if (string.IsNullOrWhiteSpace(partyCode))
                        return new ImportGradingResult(false, $"Row {i + 2}: Party '{Cell(dt.Rows[i], 2)}' not found in master.");

                    float mCurBaseRate = 1;
                    float sCurBaseRate = 85;
                    string mCurOperator = await _repository.GetMCurOperatorAsync(" and ID=" + curcode, companyId);
                    string sCurOperator = await _repository.GetSCurOperatorAsync(" and ID=" + curcode, companyId);
                    float netAmount = CellFlt(dt.Rows[i], 3);

                    var amountResponse = await _repository.GetConvertAmountAsync(curcode, netAmount, mCurBaseRate, mCurOperator, sCurBaseRate, sCurOperator, companyId);
                    double mCurNetAmt = Convert.ToDouble(amountResponse[0]);
                    double sCurNeTAmt = Convert.ToDouble(amountResponse[1]);
                    float mToSCurRate = sCurNeTAmt == 0 ? 0 : Convert.ToSingle(mCurNetAmt / sCurNeTAmt);

                    float finalMAmount = CellFlt(dt.Rows[i], 3);
                    float mCurCode = await _repository.GetMCurIdAsync(companyId);
                    float sCurCode = await _repository.GetSCurIdAsync(companyId);
                    string mCurOperatorM = await _repository.GetMCurOperatorAsync(" and ID=" + mCurCode, companyId);
                    string sCurOperatorM = await _repository.GetSCurOperatorAsync(" and ID=" + mCurCode, companyId);
                    var finalAmountResponse = GetConvertAmount(mCurCode, finalMAmount, 1, mCurOperatorM, mToSCurRate, sCurOperatorM, mCurCode, sCurCode);
                    double mCurFinalAmt = Convert.ToDouble(finalAmountResponse[0]);
                    double sCurFinalAmt = Convert.ToDouble(finalAmountResponse[1]);

                    var invno = sql.GetMaxInvNo(" And TrnType=''PSALE'' ");
                    int maxInvNo = (int.TryParse(invno, out var inv) ? inv : 0) + 1;

                    var excelDateStr = Cell(dt.Rows[i], 0).Trim();
                    if (!DateTime.TryParseExact(excelDateStr, "dd/MMM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                        return new ImportGradingResult(false, $"Row {i + 2}: Invalid date format in column 1. Expected dd/MMM/yyyy.");

                    var excelDueDateStr = Cell(dt.Rows[i], 4).Trim();
                    if (!DateTime.TryParseExact(excelDueDateStr, "dd/MMM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dueDate))
                        return new ImportGradingResult(false, $"Row {i + 2}: Invalid due date format in column 5. Expected dd/MMM/yyyy.");

                    sql.InsertSalePur(maxInvNo, Cell(dt.Rows[i], 1), "PSALE", "", "", parsedDate, CellInt(dt.Rows[i], 5), dueDate,
                        Convert.ToInt64(partyCode), 0, "", 0, 0, 0, 0, 0, 0, 0, 0,
                        CellDbl(dt.Rows[i], 3), CellDbl(dt.Rows[i], 3), 0, 0,
                        Math.Round(finalMAmount, 4), Math.Round(mCurBaseRate, 4), "", curcode.ToString(), false, true, false, false, false,
                        usercode, Math.Round(mCurBaseRate, 4), Math.Round(mCurFinalAmt, 4), Math.Round(sCurBaseRate, 4),
                        Math.Round(sCurFinalAmt, 4), 0, dueDate, 0, 0, 0, 0, "", 0, "", "", "",
                        "", "", "", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, userId, companyid, 0, 0);
                }
                sql.CommitTransaction();
                return new ImportGradingResult(true, "Parcel Item Data Upload Succesfull.");
            }
            catch (Exception ex)
            {
                sql.RollbackTransaction();
                return new ImportGradingResult(false, "Error occurred during upload: " + ex.Message);
            }
        }
#endif

#if false // Disabled: legacy UploadJewData() commented in MVC UI
        public async Task<ImportGradingResult> UploadJewelleryStockAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
        {
            var tableOrError = ReadExcel(dto.ImportFile);
            if (!tableOrError.Success) return tableOrError.Error;
            var dt = tableOrError.Table!;
            if (dt.Columns.Count < 1 || dt.Columns[0].ColumnName != "Stock ID")
                return new ImportGradingResult(false, "Invalid jewellery stock file. Please check the file format and try again.");

            int companyid = ParseCompanyId(companyId);
            using var sql = await CreateSqlTempAsync(companyId);
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sql.StartTransaction();
                    if (!string.IsNullOrEmpty(Cell(dt.Rows[i], 0)))
                    {
                        sql.SaveJewStockMasterData(0, "", Cell(dt.Rows[i], 0), 0, null, null, 0, userId, userId,
                            companyid, Cell(dt.Rows[i], 1), Cell(dt.Rows[i], 2), Cell(dt.Rows[i], 4), Cell(dt.Rows[i], 5), "", "",
                            Cell(dt.Rows[i], 3), 0, CellDbl(dt.Rows[i], 11), CellDbl(dt.Rows[i], 10),
                            0, "", 0, 0, 0, 0, 0,
                            0, Cell(dt.Rows[i], 4), 0, 0, 0, 0,
                            0, CellFlt(dt.Rows[i], 7), CellFlt(dt.Rows[i], 9), 0, 0, "", Array.Empty<byte>(), "", "", 0,
                            CellFlt(dt.Rows[i], 15), CellFlt(dt.Rows[i], 13), CellFlt(dt.Rows[i], 12),
                            CellFlt(dt.Rows[i], 14), CellFlt(dt.Rows[i], 16), CellFlt(dt.Rows[i], 16),
                            CellInt(dt.Rows[i], 6), CellInt(dt.Rows[i], 8), 1, Cell(dt.Rows[i], 17), "", "", 0, "insert");
                    }
                    sql.CommitTransaction();
                }
                return new ImportGradingResult(true, "Jewellery Stock Upload Succesfull.");
            }
            catch (Exception ex)
            {
                sql.RollbackTransaction();
                return new ImportGradingResult(false, "Error occurred during upload: " + ex.Message);
            }
        }
#endif

        private async Task<ImportGradingResult> UploadJewelleryDiamondStockInternalAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
        {
            var tableOrError = ReadExcel(dto.ImportFile);
            if (!tableOrError.Success) return tableOrError.Error;
            var dt = tableOrError.Table!;
            if (dt.Columns.Count < 2 || dt.Columns[1].ColumnName != "DiamondLotNo")
                return new ImportGradingResult(false, "Jewellery Stock Diamond Details file format is invalid");

            int companyid = ParseCompanyId(companyId);
            using var sql = await CreateSqlTempAsync(companyId);
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sql.StartTransaction();
                    if (!string.IsNullOrEmpty(Cell(dt.Rows[i], 0)))
                    {
                        var stockid = Cell(dt.Rows[i], 1);
                        var itemCode = sql.GetMaxItemCode(" AND LotNo = ''" + stockid.Replace("'", "''") + "''");
                        int maxItemCode = int.TryParse(itemCode, out var ic) ? ic : 0;
                        long maxJewCode = sql.GetJewCode("and Stock_ID=''" + Cell(dt.Rows[i], 0).Replace("'", "''") + "''");
                        sql.SaveStockDiamondDetailsData(null, Convert.ToInt32(maxJewCode), maxItemCode, stockid,
                            Cell(dt.Rows[i], 2), Cell(dt.Rows[i], 3), Cell(dt.Rows[i], 4), Cell(dt.Rows[i], 5),
                            Cell(dt.Rows[i], 6), Cell(dt.Rows[i], 7), CellInt(dt.Rows[i], 8), CellDbl(dt.Rows[i], 9),
                            CellFlt(dt.Rows[i], 10), CellDbl(dt.Rows[i], 11), Cell(dt.Rows[i], 12), "InsertStockDiamond", companyid);
                    }
                    sql.CommitTransaction();
                }
                return new ImportGradingResult(true, "Jewellery Stock Diamond Details Upload Successful.");
            }
            catch (Exception ex)
            {
                sql.RollbackTransaction();
                return new ImportGradingResult(false, "Error occurred during upload: " + ex.Message);
            }
        }

        private async Task<ImportGradingResult> UploadJewelleryFullInternalAsync(ImportGradingDTO dto, string companyId, string userId, string userTypeId)
        {
            var tableOrError = ReadExcel(dto.ImportFile);
            if (!tableOrError.Success) return tableOrError.Error;
            var dt = tableOrError.Table!;
            if (dt.Rows.Count == 0)
                return new ImportGradingResult(false, "Excel is empty");

            string[] requiredColumns = { "StockId", "Category", "MetalType", "GrossWeight", "Asking Rate" };
            var missingColumns = requiredColumns.Where(c => !dt.Columns.Contains(c)).ToList();
            if (missingColumns.Count > 0)
                return new ImportGradingResult(false, "Invalid Excel format....!");

            foreach (DataRow row in dt.Rows)
            {
                if (string.IsNullOrWhiteSpace(Col(row, "StockId")))
                    return new ImportGradingResult(false, "Stock ID cannot be empty.");
            }

            int companyid = ParseCompanyId(companyId);
            using var sql = await CreateSqlTempAsync(companyId);
            sql.StartTransaction();
            try
            {
                var groups = dt.AsEnumerable().GroupBy(r => Col(r, "StockId"));
                foreach (var group in groups)
                {
                    var first = group.First();
                    string stockId = Col(first, "StockId");
                    string stockWhere = " AND Stock_ID = ''" + stockId.Replace("'", "''") + "''";
                    string jewCodeWhere = "and Stock_ID=''" + stockId.Replace("'", "''") + "''";
                    bool isNewStock = !sql.JewStockExists(stockWhere);
                    long jewCode;

                    if (isNewStock)
                    {
                        sql.SaveJewStockMasterData(
                            id: 0,
                            Trndate: "",
                            stockid: stockId,
                            serialno: 0,
                            DesignId: "",
                            DesignName: "",
                            DesignPriority: 0,
                            CreatedBy: userId,
                            ModifyBy: userId,
                            CompanyID: companyid,
                            category: Col(first, "Category"),
                            subcategory: Col(first, "SubCategory"),
                            jewsize: Col(first, "JewSize"),
                            JewCertificateNo: Col(first, "JewCertNo"),
                            metaltype: Col(first, "MetalType"),
                            color: Col(first, "Color"),
                            purity: Col(first, "Purity"),
                            pureratio: ColDbl(first, "PureRatio"),
                            grossweight: ColDbl(first, "GrossWeight"),
                            netweight: ColDbl(first, "NetWeight"),
                            metalpricegm: ColDbl(first, "MetalRate"),
                            makingtype: Col(first, "MakingType"),
                            makingrate: ColFlt(first, "MakingRate"),
                            totalmakingcharges: ColFlt(first, "MakingCharges"),
                            totalsettingcharges: ColFlt(first, "SettingCharges"),
                            handlingchargespercarat: 0,
                            handlingcarat: 0,
                            totalhandlingcharges: ColFlt(first, "HandlingCharges"),
                            jewcertificate: Col(first, "JewCert"),
                            certificatecharges: ColFlt(first, "CertificateCharges"),
                            othercharges: ColFlt(first, "OtherCharges"),
                            hallmarkcharges: ColFlt(first, "HallmarkCharges"),
                            totmetalamount: ColDbl(first, "MetalAmount"),
                            totdiamondamt: (float)group.Sum(x => ColDbl(x, "DiamondAmount")),
                            totdiamondcarat: (float)group.Sum(x => ColDbl(x, "DiamondCarat")),
                            totstonecarat: (float)group.Sum(x => ColDbl(x, "StoneCarat")),
                            totstoneamount: (float)group.Sum(x => ColDbl(x, "StoneAmount")),
                            totmakingcharges: ColFlt(first, "MakingCharges"),
                            Image: Col(first, "ImagePath"),
                            ImageBinary: Array.Empty<byte>(),
                            Video: Col(first, "VideoPath"),
                            Cert: "",
                            finalamount: ColDbl(first, "FinalAmount"),
                            askingrate: ColFlt(first, "Asking Rate"),
                            actualrate: ColFlt(first, "Asking Rate"),
                            purchaserate: ColFlt(first, "Asking Rate"),
                            bottomrate: ColFlt(first, "Asking Rate"),
                            b2brate: ColFlt(first, "Asking Rate"),
                            b2crate: ColFlt(first, "Asking Rate"),
                            totaldiamondqty: group.Sum(x => ColInt(x, "DiamondPcs")),
                            totalstoneqty: group.Sum(x => ColInt(x, "StonePcs")),
                            pcs: ColDbl(first, "Pcs"),
                            Description: Col(first, "ItemName"),
                            MountCategory: Col(first, "JewCategory"),
                            UseMounting: "",
                            costamount: 0,
                            ActionType: "insert");
                        jewCode = sql.GetJewCode(jewCodeWhere);
                    }
                    else
                    {
                        jewCode = sql.GetJewCode(jewCodeWhere);
                        sql.SaveJewStockMasterData(
                            id: Convert.ToInt32(jewCode),
                            Trndate: "",
                            stockid: stockId,
                            serialno: 0,
                            DesignId: "",
                            DesignName: "",
                            DesignPriority: 0,
                            CreatedBy: userId,
                            ModifyBy: userId,
                            CompanyID: companyid,
                            category: Col(first, "Category"),
                            subcategory: Col(first, "SubCategory"),
                            jewsize: Col(first, "JewSize"),
                            JewCertificateNo: Col(first, "JewCertNo"),
                            metaltype: Col(first, "MetalType"),
                            color: Col(first, "Color"),
                            purity: Col(first, "Purity"),
                            pureratio: ColDbl(first, "PureRatio"),
                            grossweight: ColDbl(first, "GrossWeight"),
                            netweight: ColDbl(first, "NetWeight"),
                            metalpricegm: ColDbl(first, "MetalRate"),
                            makingtype: Col(first, "MakingType"),
                            makingrate: ColFlt(first, "MakingRate"),
                            totalmakingcharges: ColFlt(first, "MakingCharges"),
                            totalsettingcharges: ColFlt(first, "SettingCharges"),
                            handlingchargespercarat: 0,
                            handlingcarat: 0,
                            totalhandlingcharges: ColFlt(first, "HandlingCharges"),
                            jewcertificate: Col(first, "JewCert"),
                            certificatecharges: ColFlt(first, "CertificateCharges"),
                            othercharges: ColFlt(first, "OtherCharges"),
                            hallmarkcharges: ColFlt(first, "HallmarkCharges"),
                            totmetalamount: ColDbl(first, "MetalAmount"),
                            totdiamondamt: (float)group.Sum(x => ColDbl(x, "DiamondAmount")),
                            totdiamondcarat: (float)group.Sum(x => ColDbl(x, "DiamondCarat")),
                            totstonecarat: (float)group.Sum(x => ColDbl(x, "StoneCarat")),
                            totstoneamount: (float)group.Sum(x => ColDbl(x, "StoneAmount")),
                            totmakingcharges: ColFlt(first, "MakingCharges"),
                            Image: Col(first, "ImagePath"),
                            ImageBinary: Array.Empty<byte>(),
                            Video: Col(first, "VideoPath"),
                            Cert: "",
                            finalamount: ColDbl(first, "FinalAmount"),
                            askingrate: ColFlt(first, "Asking Rate"),
                            actualrate: ColFlt(first, "Asking Rate"),
                            purchaserate: ColFlt(first, "Asking Rate"),
                            bottomrate: ColFlt(first, "Asking Rate"),
                            b2brate: ColFlt(first, "Asking Rate"),
                            b2crate: ColFlt(first, "Asking Rate"),
                            totaldiamondqty: group.Sum(x => ColInt(x, "DiamondPcs")),
                            totalstoneqty: group.Sum(x => ColInt(x, "StonePcs")),
                            pcs: ColDbl(first, "Pcs"),
                            Description: Col(first, "ItemName"),
                            MountCategory: Col(first, "JewCategory"),
                            UseMounting: "",
                            costamount: 0,
                            ActionType: "update");
                        sql.Deleteboth(Convert.ToInt32(jewCode), "deletestockdiamondandsetting");
                    }

                    foreach (var row in group)
                    {
                        string diamondLotNo = Col(row, "DiamondLotNo");
                        if (string.IsNullOrWhiteSpace(diamondLotNo))
                            return FailJewelleryUpload(sql, $"Diamond Lot Number is missing for Stock ID : {stockId}");

                        var itemCode = sql.GetMaxItemCode(" AND LotNo = ''" + diamondLotNo.Replace("'", "''") + "''");
                        if (string.IsNullOrWhiteSpace(itemCode) || itemCode == "0")
                            return FailJewelleryUpload(sql, $"Diamond Lot Number '{diamondLotNo}' does not exist in Master.");

                        int maxItemCode = Convert.ToInt32(itemCode);

                        if (!string.IsNullOrWhiteSpace(Col(row, "DiamondLotNo")))
                        {
                            sql.SaveStockDiamondDetailsData(null, Convert.ToInt32(jewCode), maxItemCode, Col(row, "DiamondLotNo"),
                                Col(row, "DiamondType"), Col(row, "DiamondShape"), Col(row, "DiamondSize"), Col(row, "DiamondSieve"),
                                Col(row, "DiamondColor"), Col(row, "DiamondClarity"),
                                ColInt(row, "DiamondPcs"), ColDbl(row, "DiamondCarat"), ColFlt(row, "DiamondPrice"), ColDbl(row, "DiamondAmount"),
                                Col(row, "DiamondLab"), "InsertStockDiamond", companyid);
                        }

                        if (!string.IsNullOrWhiteSpace(Col(row, "StoneLotNo")))
                        {
                            sql.SaveStockDiamondDetailsData(null, Convert.ToInt32(jewCode), maxItemCode, Col(row, "StoneLotNo"),
                                Col(row, "StoneShape"), Col(row, "StoneShape"), "", "",
                                Col(row, "StoneColor"), Col(row, "StoneClarity"),
                                ColInt(row, "StonePcs"), ColDbl(row, "StoneCarat"), ColFlt(row, "StonePrice"), ColDbl(row, "StoneAmount"),
                                Col(row, "StoneLab"), "InsertStockStone", companyid);
                        }
                    }
                }

                sql.CommitTransaction();
                return new ImportGradingResult(true, "Upload Successful");
            }
            catch (Exception ex)
            {
                sql.RollbackTransaction();
                return new ImportGradingResult(false, ex.Message);
            }
        }

        private static ImportGradingResult FailJewelleryUpload(SqlTempAccess sql, string message)
        {
            sql.RollbackTransaction();
            return new ImportGradingResult(false, message);
        }
    }
}
