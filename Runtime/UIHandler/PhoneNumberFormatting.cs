using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    [Preserve]
    internal class PhoneNumberFormatting : MonoBehaviour, IInputFieldFormatting
    {
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TMP_Text _placeholder;
        [SerializeField] private TMP_Text _visibleText;

        private int _placeholderLength;
        private string _colorCode;
        private int _maxNumbersCount;
        private int _visibleTextLength = 0;
        private int _length = 0;
        private bool _inputDone = true;

        public bool InputDone { get; private set; } = false;

        private void Awake()
        {
            _inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            _placeholder.text = PhoneNumber.PlaceholderText;
            _placeholderLength = PhoneNumber.PlaceholderText.Length;
            _colorCode = ColorUtility.ToHtmlStringRGB(_placeholder.color);
            _maxNumbersCount = Regex.Replace(PhoneNumber.PlaceholderText, "[^0-9]", string.Empty).Length;
            _visibleText.text = ColorText(_colorCode, PhoneNumber.PlaceholderText);
        }

        private void OnEnable() => _inputField.onValueChanged.AddListener(OnValueChanged);

        private void OnDisable() => _inputField.onValueChanged.RemoveListener(OnValueChanged);

        private void Update()
        {
            _inputField.caretColor = new Color(0, 0, 0, 0);
            _inputField.caretPosition = _length;
        }

        private void OnValueChanged(string newValue)
        {
            if (_inputDone == false)
                return;

            string numbers = Regex.Replace(newValue, "[^0-9]", string.Empty);

            _length = numbers.Length;

            if (_length > _maxNumbersCount)
            {
                _length = _maxNumbersCount;
                _inputField.text = numbers.Substring(0, _length);
                return;
            }

            _inputDone = false;

            InputDone = _length == _maxNumbersCount;

            numbers = PhoneNumber.FormatNumber(numbers);

            _visibleTextLength = numbers.Length;

            string coloredString = _visibleTextLength >= _placeholderLength ? string.Empty : PhoneNumber.PlaceholderText.Substring(_visibleTextLength);

            _visibleText.text = string.Concat(
                numbers,
                ColorText(_colorCode, coloredString)
                );

            _inputDone = true;
        }

        private string ColorText(string colorCode, string text) => $"<color=#{colorCode}>{text}</color>";
    }
}