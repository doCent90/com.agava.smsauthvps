using TMPro;
using UnityEngine;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    [Preserve]
    internal class TextPlaceholder : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private string _placeholderText;

        private string _startText;

        private void Awake()
        {
            _startText = _text.text;
        }

        public void ReplaceValue(string value)
        {
            _text.text = _startText.Replace($"{{{_placeholderText}}}", value);
        }
    }
}
