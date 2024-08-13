using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    /// <summary>
    ///     Access extention.
    /// </summary>
    [Preserve]
    public static class WinkAcceessHelper
    {
        /// <summary>
        ///     Return formated number after enter
        /// </summary>
        public static string GetNumber(string phone_number, int minNumberCount, int maxNumberCount, bool additivePlusChar)
        {
            phone_number = Regex.Replace(phone_number, "[^0-9]", string.Empty);

            bool isCorrectNumber = ulong.TryParse(phone_number, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out ulong resultNumber);

            int countNumber = resultNumber.ToString().Length;

            if (isCorrectNumber == false || string.IsNullOrEmpty(phone_number)
                || (countNumber < minNumberCount || countNumber > maxNumberCount))
            {
                return string.Empty;
            }

            string correctedNumber = resultNumber.ToString();

            if (correctedNumber.Length >= 1 && correctedNumber[0] != '7')
            {
                List<char> chars = new();
                chars.AddRange(correctedNumber);
                chars[0] = '7';
                correctedNumber = new string(chars.ToArray());
            }

            string plus = additivePlusChar == true ? "+" : "";
            string number = $"{plus}{correctedNumber}";
            return number;
        }
    }
}
