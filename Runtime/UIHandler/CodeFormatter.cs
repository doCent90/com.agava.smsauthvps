using UnityEngine;
using TMPro;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    [Preserve]
    internal class CodeFormatter : MonoBehaviour, IInputFieldFormatting
    {
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TMP_Text[] _textCells;

        private int _codeLength;

        public bool InputDone { get; private set; } = false;

        private void Start() => _codeLength = _textCells.Length;

        private void OnEnable() => _inputField.onValueChanged.AddListener(OnValueChanged);

        private void OnDisable() => _inputField.onValueChanged.RemoveListener(OnValueChanged);

        public void Clear()
        {
            foreach (var cell in _textCells)
                cell.text = string.Empty;
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

            InputDone = string.IsNullOrEmpty(_textCells[_textCells.Length - 1].text) == false;
        }
    }
}
