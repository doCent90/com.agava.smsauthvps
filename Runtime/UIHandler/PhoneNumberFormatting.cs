using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class PhoneNumberFormatting : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private TMP_Text _placeholder;
    [SerializeField] private SymbolIndex[] _symbols;

    private string _placeholderText;
    private string _colorCode;
    private int _placeholderLength;
    private int _length = 0;

    public bool NumberFilled { get; private set; } = false;

    private void Awake()
    {
        _placeholderText = _placeholder.text;
        _colorCode = ColorUtility.ToHtmlStringRGB(_placeholder.color);
        _placeholderLength = _placeholderText.Length;
        _inputField.text = ColorText(_colorCode, _placeholderText);
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

[Serializable]
internal struct SymbolIndex
{
    [field: SerializeField] public char Symbol { get; private set; }
    [field: SerializeField] public int Index { get; private set; }
}