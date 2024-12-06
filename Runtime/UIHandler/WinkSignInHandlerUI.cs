using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    /// <summary>
    ///     Handler UI. Input data and view auth process.
    /// </summary>
    [Preserve]
    public class WinkSignInHandlerUI : MonoBehaviour, IWinkSignInHandlerUI, ICoroutine
    {
        [SerializeField] private DemoTimer _demoTimer;
        [SerializeField] private NotifyWindowHandler _notifyWindowHandler;
        [Header("UI Input")]
        [SerializeField] private PhoneNumberFormatting _numbersInputField;
        [Header("UI Buttons")]
        [SerializeField] private Button _signInContinueButton;
        [SerializeField] private Button _enterCodeContinueButton;
        [SerializeField] private Button[] _signInButtons;
        [SerializeField] private Button _unlinkContinueButton;
        [Header("UI Test Buttons")]
        [SerializeField] private Button _testSignInButton;
        [SerializeField] private Button _testDeleteButton;
        [Header("Factory components")]
        [SerializeField] private UnlinkDeviceViewContainer _unlinkDeviceViewContainer;
        [Header("Placeholders")]
        [SerializeField] private TextPlaceholder[] _phoneNumberPlaceholders;

        private SignInFuctionsUI _signInFuctionsUI;
        private WinkAccessManager _winkAccessManager;

        public static WinkSignInHandlerUI Instance { get; private set; }

        public bool IsAnyWindowEnabled => _notifyWindowHandler.IsAnyWindowEnabled;

        public event Action AllWindowsClosed;

        private void Awake()
        {
            _notifyWindowHandler.OpenWindow(WindowType.ProccessOn);
        }

        private void OnApplicationFocus(bool focus) => _signInFuctionsUI?.OnAppFocus(focus);

        private void Update() => _signInFuctionsUI?.Update();

        public void Dispose()
        {
            if (_signInFuctionsUI == null) return;

            _enterCodeContinueButton.onClick.RemoveListener(OnEnterCodeContinueClicked);
            _signInContinueButton.onClick.RemoveListener(OnSignInContinueClicked);

            foreach (var button in _signInButtons)
                button.onClick.RemoveAllListeners();

            _unlinkContinueButton.onClick.RemoveListener(OnUnlinkContinueClicked);

            _unlinkDeviceViewContainer.Closed -= OnUnlinkButtonClicked;

            if (_winkAccessManager == null) return;

            _winkAccessManager.ResetLogin -= OpenSignWindow;
            _winkAccessManager.LimitReached -= OnLimitReached;
            _winkAccessManager.SignInSuccessfully -= OnSignInSuccessfully;
            _demoTimer.Dispose();
        }

        public IEnumerator Initialize()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                _notifyWindowHandler.OpenWindow(WindowType.NoEnternet);
                yield return new WaitWhile(() => Application.internetReachability == NetworkReachability.NotReachable);
            }

            _notifyWindowHandler.CloseWindow(WindowType.NoEnternet);
        }

        public void Construct()
        {
            StartCoroutine(EnternetChecking());
            _signInFuctionsUI.SetRemoteConfig();
        }

        public void StartSevice(WinkAccessManager winkAccessManager)
        {
            if (Instance == null)
                Instance = this;

            _signInFuctionsUI = new(_notifyWindowHandler, _demoTimer, winkAccessManager, this, this);

#if TEST
            _testSignInButton.onClick.AddListener(OnTestSignInClicked);
            _testDeleteButton.onClick.AddListener(OnTestDeleteClicked);
            _testSignInButton.gameObject.SetActive(true);
            _testDeleteButton.gameObject.SetActive(true);
#else
            _testDeleteButton.gameObject.SetActive(false);
            _testSignInButton.gameObject.SetActive(false);
#endif

            _winkAccessManager = winkAccessManager;

            _enterCodeContinueButton.onClick.AddListener(OnEnterCodeContinueClicked);
            _signInContinueButton.onClick.AddListener(OnSignInContinueClicked);

            foreach (var button in _signInButtons)
                button.onClick.AddListener(OpenSignWindow);

            _unlinkContinueButton.onClick.AddListener(OnUnlinkContinueClicked);
            _unlinkDeviceViewContainer.Closed += OnUnlinkButtonClicked;

            CloseAllWindows();

            _winkAccessManager.ResetLogin += OpenSignWindow;
            _winkAccessManager.LimitReached += OnLimitReached;
            _winkAccessManager.SignInSuccessfully += OnSignInSuccessfully;
            _winkAccessManager.AuthorizationSuccessfully += OnAuthorizationSuccessfully;
            _demoTimer.TimerExpired += OnTimerExpired;
        }

        public void OpenStartWindow()
        {
            OpenSubscriptionWindow();
        }

        public void OpenSignWindow()
        {
            _notifyWindowHandler.OpenSignInWindow();
            AnalyticsWinkService.SendEnterPhoneWindow();
        }

        public void OpenSubscriptionWindow()
        {
            _notifyWindowHandler.OpenWindow(WindowType.Redirect);
            AnalyticsWinkService.SendSubscribeOfferWindow();
        }

        public void OpenWindow(WindowType type) => _notifyWindowHandler.OpenWindow(type);

        public void CloseAllWindows() => _notifyWindowHandler.CloseAllWindows(AllWindowsClosed);

        public void OnWinkButtonClick()
        {
            if (_winkAccessManager.Authenficated)
            {
                if (_winkAccessManager.HasAccess)
                {
                    _notifyWindowHandler.OpenWindow(WindowType.WinkProfile);
                }
                else
                {
                    OpenSubscriptionWindow();
                }
            }
            else
            {
                OpenSignWindow();
            }
        }

        public void OnDeleteAccountButtonClick()
        {
            _notifyWindowHandler.OpenDeleteAccountWindow(
                onDeleteAccount: () =>
                {
                    _winkAccessManager.DeleteAccount(
                    onComplete: (resultSuccess) =>
                    {
                        if (resultSuccess == false)
                        {
                            _notifyWindowHandler.OpenWindow(WindowType.Fail);
                        }
                        else
                        {
                            AnalyticsWinkService.SendDeleteWindow();
                        }
                    });
                });
        }

        private void OnSignInContinueClicked()
        {
            string number = _numbersInputField.Number;
            string formattedNumber = PhoneNumber.FormatNumber(number);

            foreach (TextPlaceholder placeholder in _phoneNumberPlaceholders)
                placeholder.ReplaceValue(formattedNumber);

            if (_notifyWindowHandler.HasCodeDelayExpired == false)
                _notifyWindowHandler.OpenInputOtpCodeWhileReapetWindow(number);
            else
                _signInFuctionsUI.OnSignInClicked(number);
        }

        private void OnLimitReached(IReadOnlyList<string> devicesList)
        {
            CloseAllWindows();
            _notifyWindowHandler.OpenWindow(WindowType.Unlink);
            _unlinkDeviceViewContainer.Initialize(devicesList);
        }

        void OnUnlinkButtonClicked(UnlinkDeviceView unlinkDeviceView)
        {
            _signInFuctionsUI.OnUnlinkClicked(unlinkDeviceView.DeviceId);
        }

        private void OnUnlinkContinueClicked()
        {
            _notifyWindowHandler.CloseWindow(WindowType.Unlink);
            _winkAccessManager.Login();
        }

        private void OnAuthorizationSuccessfully() => _signInFuctionsUI.OnAuthorizationSuccessfully();

        private void OnEnterCodeContinueClicked()
        {
            _notifyWindowHandler.CloseWindow(WindowType.Redirect);
            _notifyWindowHandler.CloseWindow(WindowType.EnterOtpCode);
        }

        private void OnSignInSuccessfully(bool hasAccess)
        {
            _numbersInputField.Clear();
            _signInFuctionsUI.OnSignInSuccesfully(hasAccess);

            if (hasAccess)
            {
                SetPhone();
                AnalyticsWinkService.SendHelloWindow();
                _notifyWindowHandler.OpenWindow(WindowType.Hello);
                _notifyWindowHandler.CloseWindow(WindowType.Redirect);
            }
        }

        private void SetPhone()
        {
            string number = "N/A";

            if (UnityEngine.PlayerPrefs.HasKey(_winkAccessManager.PhoneNumber))
                number = PhoneNumber.FormatNumber(UnityEngine.PlayerPrefs.GetString(_winkAccessManager.PhoneNumber));

            foreach (TextPlaceholder placeholder in _phoneNumberPlaceholders)
                placeholder.ReplaceValue(number);
        }

        private void OnTimerExpired() => _notifyWindowHandler.OpenDemoExpiredWindow(false);

        private IEnumerator EnternetChecking()
        {
            var wait = new WaitForSecondsRealtime(1f);

            while (true)
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    _notifyWindowHandler.OpenWindow(WindowType.NoEnternet);
                    Debug.LogError("NO CONNECTION");
                }
                else
                {
                    if (_notifyWindowHandler.HasOpenedWindow(WindowType.NoEnternet))
                        _notifyWindowHandler.CloseWindow(WindowType.NoEnternet);
                }

                yield return wait;
            }
        }
        #region TEST_METHODS
#if UNITY_EDITOR || TEST
        private void OnTestSignInClicked()
        {
            _winkAccessManager.TestEnableSubsription();
            _testSignInButton.gameObject.SetActive(false);
        }

        private void OnTestDeleteClicked()
        {
            if (WinkAccessManager.Instance.HasAccess == false)
            {
                Debug.LogError("Wink not authorizated!");
                return;
            }

            SmsAuthAPI.Utility.PlayerPrefs.DeleteAll();
            SmsAuthAPI.Utility.PlayerPrefs.Save();
        }
#endif
        #endregion
    }
}
