using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SmsAuthAPI.Program;
using SmsAuthAPI.DTO;
using UnityEngine.Networking;

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
        private string _phone;

        private void Awake()
        {
            _sendButton.onClick.AddListener(OnSendCodeClicked);
            _sendRepeatCodeButton.onClick.AddListener(OnRepeatClicked);
            _repeatCodeTimer.TimerExpired += OnNewCodeTimerExpired;
        }

        private void OnDestroy()
        {
            _sendButton.onClick.RemoveListener(OnSendCodeClicked);
            _sendRepeatCodeButton.onClick.RemoveListener(OnRepeatClicked);
            _repeatCodeTimer.TimerExpired -= OnNewCodeTimerExpired;
        }

        public void Enable(string phone, Action<string> onInputDone)
        {
            _phone = phone;
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

        public void ResetInputText()
        {
            _inputField.text = string.Empty;
            _enterCodeShaking.StartAnimation();
        }

        public void Clear() => _inputField.text = string.Empty;

        private void OnNewCodeTimerExpired()
        {
            _sendRepeatCodeButton.gameObject.SetActive(true);
            _repeatCodeTimer.Disable();
        }

        private async void OnRepeatClicked()
        {
            _sendRepeatCodeButton.gameObject.SetActive(false);
            _repeatCodeTimer.Enable();

            Response response = await SmsAuthApi.Regist(_phone);

            if (response.statusCode != UnityWebRequest.Result.Success)
                Debug.LogError("Repeat send sms Error : " + response.statusCode);
        }
    }
}
