using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Agava.Wink
{
    internal static class PhoneNumber
    {
        public const string PlaceholderText = "+7 (999) 000-00-00";

        private static List<SymbolIndex> _symbols;

        static PhoneNumber()
        {
            _symbols = new();

            try
            {
                foreach (Match match in Regex.Matches(PlaceholderText, @"[^0-9]", RegexOptions.None, TimeSpan.FromSeconds(1)))
                    _symbols.Add(new SymbolIndex(char.Parse(match.Value), match.Index));
            }
            catch (RegexMatchTimeoutException exception)
            {
                Debug.LogError(exception.Message);
                _symbols.Clear();
            }
        }

        static public string FormatNumber(string number)
        {
            string symbol;
            int index;

            foreach (SymbolIndex symbolIndex in _symbols)
            {
                symbol = symbolIndex.Symbol.ToString();
                index = symbolIndex.Index;

                if (number.Length > symbolIndex.Index)
                {
                    if (number[index] != symbolIndex.Symbol)
                    {
                        number = number.Insert(symbolIndex.Index, symbol);
                    }
                }
                else
                {
                    break;
                }
            }

            return number;
        }

        private struct SymbolIndex
        {
            public char Symbol { get; private set; }
            public int Index { get; private set; }

            public SymbolIndex(char symbol, int index)
            {
                Symbol = symbol;
                Index = index;
            }
        }
    }
}
