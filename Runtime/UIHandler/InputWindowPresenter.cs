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

        private Action<uint> _onInputDone;

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

        public void Enable(Action<uint> onInputDone)
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

            bool isCorrectCode = uint.TryParse(_inputField.text, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out uint resultCode);

            if (isCorrectCode == false)
            {
                _failWindow.Enable();
                return;
            }

            _onInputDone?.Invoke(resultCode);
        }

        internal void ResetInputText()
        {
            _inputField.text = string.Empty;
            _enterCodeShaking.StartAnimation();
        }

        internal void Clear() => _inputField.text = string.Empty;

        private void OnNewCodeTimerExpired()
        {
            _sendRepeatCodeButton.gameObject.SetActive(true);
        }
    }
}
