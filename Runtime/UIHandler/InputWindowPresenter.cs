using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Agava.Wink
{
    internal class InputWindowPresenter : WindowPresenter
    {
        [SerializeField] private NotifyWindowPresenter _failWindow;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _sendButton;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TextTimer _repeatCodeTimer;
        [SerializeField] private Button _sendRepeatCodeButton;
        [SerializeField] private EnterCodeShaking _enterCodeShaking;

        private Action<string> _onInputDone;

        private void Awake()
        {
            _sendButton.onClick.AddListener(OnSendCodeClicked);
            _repeatCodeTimer.TimerExpired += OnNewCodeTimerExpired;
        }

        private void OnDestroy()
        {
            _sendButton.onClick.RemoveListener(OnSendCodeClicked);
            _repeatCodeTimer.TimerExpired -= OnNewCodeTimerExpired;
        }

        public void Enable(Action<string> onInputDone)
        {
            _repeatCodeTimer.Enable();
            _onInputDone = onInputDone;
            EnableCanvasGroup(_canvasGroup);
        }

        public override void Enable() { }

        public override void Disable()
        {
            _repeatCodeTimer.Disable();
            DisableCanvasGroup(_canvasGroup);
        }

        public void OnSendCodeClicked()
        {
            if (string.IsNullOrEmpty(_inputField.text))
                return;

            string code = _inputField.text;

            bool isCorrectCode = uint.TryParse(code, System.Globalization.NumberStyles.Integer, CultureInfo.InvariantCulture, out uint _);

            if (isCorrectCode == false)
            {
                _failWindow.Enable();
                return;
            }

            _onInputDone?.Invoke(code);
        }

        internal void ResetInputText()
        {
            _inputField.text = string.Empty;
            _enterCodeShaking.StartAnimation();
        }

        internal void Clear() => _inputField.text = string.Empty;

        private void OnNewCodeTimerExpired() => _sendRepeatCodeButton.gameObject.SetActive(true);
    }
}
