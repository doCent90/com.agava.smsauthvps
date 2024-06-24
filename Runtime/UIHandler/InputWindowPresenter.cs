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
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TextTimer _repeatCodeTimer;
        [SerializeField] private EnterCodeShaking _enterCodeShaking;
        [SerializeField] private CodeFormatter _codeFormatter;
        [Header("Buttons")]
        [SerializeField] private Button _sendButton;
        [SerializeField] private Button _sendRepeatCodeButton;
        [SerializeField] private Button _backButton;

        private Action<string> _onInputDone;
        private Action _onBackClicked;
        private string _phone;

        private void Awake()
        {
            _sendButton.onClick.AddListener(OnSendCodeClicked);
            _sendRepeatCodeButton.onClick.AddListener(OnRepeatClicked);
            _backButton.onClick.AddListener(OnBackClicked);
            _repeatCodeTimer.TimerExpired += OnNewCodeTimerExpired;
        }

        private void OnDestroy()
        {
            _sendButton.onClick.RemoveListener(OnSendCodeClicked);
            _sendRepeatCodeButton.onClick.RemoveListener(OnRepeatClicked);
            _backButton.onClick.RemoveListener(OnBackClicked);
            _repeatCodeTimer.TimerExpired -= OnNewCodeTimerExpired;
        }

        public void Enable(string phone, Action<string> onInputDone, Action onBackClicked)
        {
            _phone = phone;
            _repeatCodeTimer.Enable();
            _onInputDone = onInputDone;
            _onBackClicked = onBackClicked;
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
            Clear();
        }

        public void ResetInputText()
        {
            _inputField.text = string.Empty;
            _enterCodeShaking.StartAnimation();
        }

        public void Clear()
        {
            _inputField.text = string.Empty;
            _codeFormatter.Clear();
        }

        private void OnBackClicked()
        {
            Clear();
            _onBackClicked?.Invoke();
        }

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
