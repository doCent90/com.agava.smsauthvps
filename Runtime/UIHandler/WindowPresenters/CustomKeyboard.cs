using System;
using UnityEngine;

namespace Agava.Wink
{
    internal class CustomKeyboard : WindowPresenter
    {
        [SerializeField] private KeyboardButton[] _buttons;
        [SerializeField] private CanvasGroup _groupKeysCanvas;

        public event Action<KeyCode> Clicked;

        private void OnDestroy()
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                KeyboardButton btn = _buttons[i];
                btn.Clicked -= OnClicked;
            }
        }

        private void Awake()
        {
            DisableCanvasGroup(_groupKeysCanvas);

            for (int i = 0; i < _buttons.Length; i++)
            {
                KeyboardButton btn = _buttons[i];
                btn.Clicked += OnClicked;
            }
        }

        public override void Enable() => EnableCanvasGroup(_groupKeysCanvas);

        public override void Disable() => DisableCanvasGroup(_groupKeysCanvas);

        private void OnClicked(KeyCode code) => Clicked?.Invoke(code);
    }
}
