using System.Globalization;

namespace Agava.Wink
{
    /// <summary>
    ///     Access extention.
    /// </summary>
    public static class WinkAcceessHelper
    {
        /// <summary>
        ///     Return formated number after enter.
        /// </summary>
        public static string GetNumber(string phone_number, int minNumberCount, int maxNumberCount, bool additivePlusChar)
        {
            phone_number.Replace(" ", string.Empty);

            bool isCorrectNumber = ulong.TryParse(phone_number, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out ulong resultNumber);

            int countNumber = resultNumber.ToString().Length;

            if (isCorrectNumber == false || string.IsNullOrEmpty(phone_number)
                || (countNumber < minNumberCount || countNumber > maxNumberCount))
            {
                return string.Empty;
            }

            string plus = additivePlusChar == true ? "+" : "";
            string number = $"{plus}{resultNumber}";
            return number;
        }
    }
}
