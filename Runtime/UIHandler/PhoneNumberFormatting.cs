using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class PhoneNumberFormatting : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private TMP_Text _placeholder;

    private List<SymbolIndex> _symbols = new();
    private string _placeholderText;
    private string _colorCode;
    private int _placeholderLength;
    private int _length = 0;

    public bool NumberFilled { get; private set; } = false;

    private void Awake()
    {
        _inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        _placeholderText = _placeholder.text;
        _colorCode = ColorUtility.ToHtmlStringRGB(_placeholder.color);
        _placeholderLength = _placeholderText.Length;
        _inputField.text = ColorText(_colorCode, _placeholderText);

        try
        {
            foreach (Match match in Regex.Matches(_placeholderText, @"[^0-9]", RegexOptions.None, TimeSpan.FromSeconds(1)))
                _symbols.Add(new SymbolIndex(char.Parse(match.Value), match.Index));
        }
        catch (RegexMatchTimeoutException)
        {

        }
    }

    private void OnEnable()
    {
        _inputField.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDisable()
    {
        _inputField.onValueChanged.RemoveListener(OnValueChanged);
    }

    private void Update()
    {
        _inputField.caretPosition = _length;
    }

    private void OnValueChanged(string newValue)
    {
        newValue = Regex.Replace(newValue, @"<[^>]*>", string.Empty);

        int diff = _placeholderLength - newValue.Length;

        if (diff == 0)
            return;

        _length -= diff;

        if (_length >= _placeholderLength)
        {
            _length = _placeholderLength;
        }
        else if (_length <= 0)
        {
            _length = 0;
        }

        NumberFilled = _length == _placeholderLength;

        newValue = newValue.Substring(0, _length);

        string symbol;
        int index;

        foreach (SymbolIndex symbolIndex in _symbols)
        {
            symbol = symbolIndex.Symbol.ToString();
            index = symbolIndex.Index;

            if (newValue.Length > symbolIndex.Index)
            {
                if (newValue[index] != symbolIndex.Symbol)
                {
                    newValue = newValue.Insert(symbolIndex.Index, symbol);
                    break;
                }
            }
            else
            {
                break;
            }
        }

        _inputField.text = string.Concat(
            newValue,
            ColorText(_colorCode, _placeholderText.Substring(_length))
            );
    }

    private string ColorText(string colorCode, string text) => $"<color=#{colorCode}>{text}</color>";
}

internal struct SymbolIndex
{
    public char Symbol { get; private set; }
    public int Index { get; private set; }

    public SymbolIndex(char symbol, int index)
    {
        Symbol = symbol;
        Index = index;
    }
}