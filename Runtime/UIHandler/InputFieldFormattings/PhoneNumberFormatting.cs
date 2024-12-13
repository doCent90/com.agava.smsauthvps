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
        [SerializeField] private TMP_Text _hiddenText;
        [SerializeField] private bool _usePlaceholder;

        private int _placeholderLength;
        private string _colorCode;
        private int _maxNumbersCount;
        private int _visibleTextLength = 0;
        private int _length = 0;

        public string Number { get; private set; }
        public bool InputDone { get; private set; } = false;

        private void Awake()
        {
            _inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            _inputField.resetOnDeActivation = false;
            _inputField.restoreOriginalTextOnEscape = false;

            if (_usePlaceholder)
                _placeholder.text = PhoneNumber.PlaceholderText;

            _placeholderLength = PhoneNumber.PlaceholderText.Length;
            _colorCode = ColorUtility.ToHtmlStringRGB((_usePlaceholder ? _placeholder : _hiddenText).color);
            _maxNumbersCount = Regex.Replace(PhoneNumber.PlaceholderText, "[^0-9]", string.Empty).Length;
            _visibleText.text = ColorText(_colorCode, PhoneNumber.PlaceholderText);
        }

        private void OnEnable() => _inputField.onValueChanged.AddListener(OnValueChanged);

        private void OnDisable() => _inputField.onValueChanged.RemoveListener(OnValueChanged);

        private void Update()
        {
            if (_usePlaceholder == false)
                _visibleText.enabled = _visibleTextLength != 0;

            _inputField.caretPosition = _visibleTextLength;
        }

        public void Clear()
        {
            _inputField.text = string.Empty;
        }

        private void OnValueChanged(string newValue)
        {
            string numbers = Regex.Replace(newValue, "[^0-9]", string.Empty);

            Number = numbers;

            _length = numbers.Length;

            if (_length > _maxNumbersCount)
            {
                _length = _maxNumbersCount;
                _inputField.text = numbers.Substring(0, _length);
                return;
            }

            InputDone = _length == _maxNumbersCount;

            numbers = PhoneNumber.FormatNumber(numbers);

            Number = Regex.Replace(numbers, "[^0-9]", string.Empty);

            _visibleTextLength = numbers.Length;

            string coloredString = _visibleTextLength >= _placeholderLength ? string.Empty : PhoneNumber.PlaceholderText.Substring(_visibleTextLength);

            _visibleText.text = string.Concat(
                numbers,
                ColorText(_colorCode, coloredString)
                );
        }

        private string ColorText(string colorCode, string text) => $"<color=#{colorCode}>{text}</color>";
    }
}
