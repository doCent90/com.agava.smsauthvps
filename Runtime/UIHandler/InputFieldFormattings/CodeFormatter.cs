using UnityEngine;
using TMPro;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    [Preserve]
    internal class CodeFormatter : MonoBehaviour, IInputFieldFormatting
    {
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TextCell[] _textCells;

        private int _codeLength;

        public bool InputDone { get; private set; } = false;
        public string InputText => _inputField.text;

        private void Start()
        {
            _codeLength = _textCells.Length;
            _inputField.resetOnDeActivation = false;
            _inputField.restoreOriginalTextOnEscape = false;
            Clear();
        }

        private void OnEnable() => _inputField.onValueChanged.AddListener(OnValueChanged);

        private void OnDisable() => _inputField.onValueChanged.RemoveListener(OnValueChanged);

        public void SetInteractable(bool interactable)
        {
            _inputField.interactable = interactable;
        }

        public void Clear()
        {
            _inputField.text = string.Empty;
            _textCells[0].SetActive(true);
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
                    _textCells[i].SetText(i >= newValue.Length ? string.Empty : newValue[i].ToString());

                    if (i != _codeLength - 1)
                        _textCells[i + 1].SetActive(_textCells[i].Empty == false);
                }
            }

            InputDone = _textCells[_textCells.Length - 1].Empty == false;
        }
    }
}
