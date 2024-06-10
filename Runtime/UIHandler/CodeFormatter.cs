using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class CodeFormatter : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private TMP_Text[] _textCells;

    private int _codeLength;

    private void Start()
    {
        _codeLength = _textCells.Length;
    }

    private void OnEnable()
    {
        _inputField.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDisable()
    {
        _inputField.onValueChanged.RemoveListener(OnValueChanged);
    }

    private void OnValueChanged(string newValue)
    {
        if (newValue.Length > _codeLength)
        {
            _inputField.text = newValue.Substring(0, _codeLength);
        }
        else
        {
            for (int i = 0; i < _codeLength; i++)
            {
                _textCells[i].text = i >= newValue.Length ? string.Empty : newValue[i].ToString();
            }
        }
    }
}
