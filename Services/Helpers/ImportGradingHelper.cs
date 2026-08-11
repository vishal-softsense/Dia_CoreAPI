using System;
using System.Globalization;

namespace Dia_CoreAPI.Services.Helpers
{
    /// <summary>Matches MVC BasicFunctionController.GetConvertAmount</summary>
    public static class ImportGradingHelper
    {
        public static string[] GetConvertAmount(
            float curCode,
            float amount,
            float mCurBaseRate,
            string mCurOperator,
            float sCurBaseRate,
            string sCurOperator,
            float mCurId,
            float sCurId)
        {
            var amountResponse = new string[2];
            try
            {
                float mCurAmt = 0;
                float sCurAmt = 0;

                if (curCode == mCurId)
                {
                    mCurAmt = amount;
                    if (sCurOperator == "*")
                        sCurAmt = amount * sCurBaseRate;
                    else
                        sCurAmt = sCurBaseRate != 0 ? amount / sCurBaseRate : 0f;
                }
                else if (curCode == sCurId)
                {
                    sCurAmt = amount;
                    if (mCurOperator == "*")
                        mCurAmt = amount * mCurBaseRate;
                    else
                        mCurAmt = mCurBaseRate != 0 ? amount / mCurBaseRate : 0f;
                }
                else
                {
                    if (mCurOperator == "*")
                        mCurAmt = amount * mCurBaseRate;
                    else
                        mCurAmt = mCurBaseRate != 0 ? amount / mCurBaseRate : 0f;

                    if (sCurOperator == "*")
                        sCurAmt = amount * sCurBaseRate;
                    else
                        sCurAmt = sCurBaseRate != 0 ? amount / sCurBaseRate : 0f;
                }

                if (float.IsNaN(mCurAmt) || float.IsInfinity(mCurAmt)) mCurAmt = 0;
                if (float.IsNaN(sCurAmt) || float.IsInfinity(sCurAmt)) sCurAmt = 0;

                amountResponse[0] = mCurAmt.ToString(CultureInfo.InvariantCulture);
                amountResponse[1] = sCurAmt.ToString(CultureInfo.InvariantCulture);
            }
            catch
            {
                amountResponse[0] = "0";
                amountResponse[1] = "0";
            }

            return amountResponse;
        }
    }
}
