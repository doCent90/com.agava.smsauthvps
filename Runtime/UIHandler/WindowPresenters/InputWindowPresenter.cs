using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SmsAuthAPI.Program;
using SmsAuthAPI.DTO;
using UnityEngine.Networking;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    [Preserve]
    internal class InputWindowPresenter : WindowPresenter
    {
        [SerializeField] private NotifyWindowPresenter _failWindow;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TextTimer _repeatCodeTimer;
        [SerializeField] private EnterCodeShaking _enterCodeShaking;
        [SerializeField] private CodeFormatter _codeFormatter;
        [SerializeField] private GameObject _wrongCodeText;
        [Header("Buttons")]
        [SerializeField] private Button _sendRepeatCodeButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _continueButton;

        private Action<string> _onInputDone;
        private Action _onBackClicked;
        private string _phone;
        private bool _checkedInputDone = false;

        public bool HasExpired => _repeatCodeTimer.Expired;

        private void Awake()
        {
            _continueButton.onClick.AddListener(OnContinue);
            _sendRepeatCodeButton.onClick.AddListener(OnRepeatClicked);
            _backButton.onClick.AddListener(OnBackClicked);
            _repeatCodeTimer.TimerExpired += OnNewCodeTimerExpired;
        }

        private void OnDestroy()
        {
            _continueButton.onClick.RemoveListener(OnContinue);
            _sendRepeatCodeButton.onClick.RemoveListener(OnRepeatClicked);
            _backButton.onClick.RemoveListener(OnBackClicked);
            _repeatCodeTimer.TimerExpired -= OnNewCodeTimerExpired;
        }

        private void Update()
        {
            if (Enabled == false)
                return;

            if (_codeFormatter.InputDone)
            {
                if (_checkedInputDone == false)
                {
                    _checkedInputDone = true;
                    OnInputDone();
                }
            }
            else
            {
                _checkedInputDone = false;
            }
        }

        public void Enable(string phone, Action<string> onInputDone, Action onBackClicked)
        {
            _phone = phone;
            _repeatCodeTimer.Enable();
            _onInputDone = onInputDone;
            _onBackClicked = onBackClicked;
            EnableCanvasGroup(_canvasGroup);
            _inputField.Select();
        }

        public void Enable(string phone)
        {
            _phone = phone;
            EnableCanvasGroup(_canvasGroup);
        }

        public override void Enable() { }

        public override void Disable()
        {
            DisableCanvasGroup(_canvasGroup);
            Clear();
        }

        public void OnInputDone()
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

        public void Response(bool codeAccepted)
        {
            if (codeAccepted)
            {
                _continueButton.gameObject.SetActive(true);
            }
            else
            {
                _repeatCodeTimer.Disable();
                _repeatCodeTimer.SetSmsDelayConfig();
                _repeatCodeTimer.Enable();
                _wrongCodeText.SetActive(true);
                _inputField.text = string.Empty;
                _codeFormatter.Clear();
                _enterCodeShaking.StartAnimation();
            }
        }

        public void Clear()
        {
            _wrongCodeText.gameObject.SetActive(false);
            _sendRepeatCodeButton.gameObject.SetActive(false);
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
            Clear();
            _sendRepeatCodeButton.gameObject.SetActive(true);
            _inputField.interactable = false;
            _codeFormatter.SetInteractable(false);
            _repeatCodeTimer.Disable();
        }

        private async void OnRepeatClicked()
        {
            _sendRepeatCodeButton.gameObject.SetActive(false);
            _inputField.interactable = true;
            _codeFormatter.SetInteractable(true);
            _repeatCodeTimer.SetCodeLifespanConfig();
            _repeatCodeTimer.Enable();

            Response response = await SmsAuthApi.Regist(_phone);

            if (response.statusCode != UnityWebRequest.Result.Success)
                Debug.LogError("Repeat send sms Error : " + response.statusCode);
        }

        private void OnContinue()
        {
            Disable();
        }
    }
}
