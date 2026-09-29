using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Dia_CoreAPI.Utilities
{
    public static class MixLotNoHelper
    {
        public static string GetNextLotNo(string? latestLotNo, string defaultLotNo)
        {
            if (string.IsNullOrWhiteSpace(latestLotNo))
                return defaultLotNo;

            var match = Regex.Match(latestLotNo, @"([A-Za-z]+)(\d+)$");
            if (!match.Success || match.Groups.Count != 3)
                return defaultLotNo;

            string prefix = match.Groups[1].Value;
            string numPart = match.Groups[2].Value;

            if (!int.TryParse(numPart, out int number))
                return defaultLotNo;

            number++;
            return prefix + number.ToString("D3");
        }

        public static string? ReadLatestLotNo(IEnumerable<dynamic>? rows)
        {
            if (rows == null)
                return null;

            foreach (var row in rows)
            {
                if (row is not IDictionary<string, object> dict)
                    continue;

                var lotNo = ReadDictString(dict, "LotNo")
                    ?? ReadDictString(dict, "Stock_ID")
                    ?? ReadDictString(dict, "Name");

                if (!string.IsNullOrWhiteSpace(lotNo))
                    return lotNo.Trim();

                foreach (var kv in dict)
                {
                    var val = kv.Value?.ToString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(val))
                        return val;
                }
            }

            return null;
        }

        public static string? ReadLatestLotNoFromRow(MixLotNoRow? row)
        {
            if (row == null)
                return null;

            if (!string.IsNullOrWhiteSpace(row.LotNo))
                return row.LotNo.Trim();

            if (!string.IsNullOrWhiteSpace(row.Stock_ID))
                return row.Stock_ID.Trim();

            return null;
        }

        public static async Task<string> GetNextAvailableLotNoAsync(
            string? latestLotNo,
            string defaultLotNo,
            Func<string, Task<bool>> existsChecker)
        {
            var next = GetNextLotNo(latestLotNo, defaultLotNo);

            for (int i = 0; i < 1000; i++)
            {
                if (!await existsChecker(next))
                    return next;

                next = GetNextLotNo(next, defaultLotNo);
            }

            return next;
        }

        private static string? ReadDictString(IDictionary<string, object> dict, string key)
        {
            foreach (var kv in dict)
            {
                if (string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase))
                    return kv.Value?.ToString();
            }

            return null;
        }
    }
}
