using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SmsAuthAPI.DTO;

namespace Agava.Wink
{
    /// <summary>
    ///     Handler UI. Input data and view auth process.
    /// </summary>
    public class WinkSignInHandlerUI : MonoBehaviour, IWinkSignInHandlerUI, ICoroutine
    {
        [SerializeField] private DemoTimer _demoTimer;
        [SerializeField] private NotifyWindowHandler _notifyWindowHandler;
        [Header("UI Input")]
        [SerializeField] private TMP_InputField _numbersInputField;
        [Header("UI Buttons")]
        [SerializeField] private Button _signInButton;
        [SerializeField] private Button _openSignInButton;
        [SerializeField] private Button _unlinkButtonTemplate;
        [Header("UI Test Buttons")]
        [SerializeField] private Button _testSignInButton;
        [SerializeField] private Button _testDeleteButton;
        [Header("Phone Number Check Settings")]
        [SerializeField] private int _maxNumberCount = 30;
        [SerializeField] private int _minNumberCount = 5;
        [SerializeField] private bool _additivePlusChar = false;
        [Header("Factory components")]
        [SerializeField] private Transform _containerButtons;
        [Header("Placeholders")]
        [SerializeField] private TextPlaceholder[] _phoneNumberPlaceholders;
        [SerializeField] private TextPlaceholder[] _idPlaceholders;

        private SignInFuctionsUI _signInFuctionsUI;
        private WinkAccessManager _winkAccessManager;
        private readonly List<Button> _devicesIdButtons = new();

        public static WinkSignInHandlerUI Instance { get; private set; }

        public bool IsAnyWindowEnabled => _notifyWindowHandler.IsAnyWindowEnabled;

        public event Action AllWindowsClosed;
        public event Action SignInWindowClosed;
        public event Action HelloWindowsClosed;

        public void Dispose()
        {
            _signInButton.onClick.RemoveAllListeners();

            if (_winkAccessManager == null) return;

            _winkAccessManager.ResetLogin -= OpenSignWindow;
            _winkAccessManager.LimitReached -= OnLimitReached;
            _winkAccessManager.AuthenficationSuccessfully -= OnAuthenficationSuccessfully;
            _winkAccessManager.AuthorizationSuccessfully -= OnAuthorizationSuccessfully;
            _demoTimer.Dispose();
        }

        public void Construct(WinkAccessManager winkAccessManager)
        {
            if (Instance == null)
                Instance = this;

            _signInFuctionsUI = new(_notifyWindowHandler, _demoTimer, winkAccessManager, this, this);
#if UNITY_EDITOR || TEST
            _testSignInButton.onClick.AddListener(OnTestSignInClicked);
            _testDeleteButton.onClick.AddListener(OnTestDeleteClicked);
            _testSignInButton.gameObject.SetActive(true);
            _testDeleteButton.gameObject.SetActive(true);
#else
            _testDeleteButton.gameObject.SetActive(false);
            _testSignInButton.gameObject.SetActive(false);
#endif
            _winkAccessManager = winkAccessManager;
            _signInButton.onClick.AddListener(OnSignInClicked);
            _openSignInButton.onClick.AddListener(OpenSignWindow);
            CloseAllWindows();

            _winkAccessManager.ResetLogin += OpenSignWindow;
            _winkAccessManager.LimitReached += OnLimitReached;
            _winkAccessManager.AuthenficationSuccessfully += OnAuthenficationSuccessfully;
            _winkAccessManager.AuthorizationSuccessfully += OnAuthorizationSuccessfully;
            _demoTimer.TimerExpired += OnTimerExpired;
        }

        public IEnumerator Initialize()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                _notifyWindowHandler.OpenWindow(WindowType.NoEnternet);
                yield return new WaitWhile(() => Application.internetReachability == NetworkReachability.NotReachable);
            }
            else
            {
                _notifyWindowHandler.CloseWindow(WindowType.NoEnternet);
            }

            _signInFuctionsUI.SetRemoteConfig();
        }

        public void OpenSignWindow()
        {
            _notifyWindowHandler.OpenSignInWindow(() => SignInWindowClosed?.Invoke());
            AnalyticsWinkService.SendEnterPhoneWindow();
        }

        public void OpenWindow(WindowType type) => _notifyWindowHandler.OpenWindow(type);
        public void CloseWindow(WindowType type) => _notifyWindowHandler.CloseWindow(type);
        public void CloseAllWindows() => _notifyWindowHandler.CloseAllWindows(AllWindowsClosed);

        internal void OnWinkButtonClick()
        {
            if (_winkAccessManager.Authorized)
            {
                if (_winkAccessManager.HasAccess)
                {
                    _notifyWindowHandler.OpenWindow(WindowType.WinkProfile);
                }
                else
                {
                    AnalyticsWinkService.SendSubscribeProfileWindow();
                    _notifyWindowHandler.OpenDemoExpiredWindow(true);
                }
            }
            else
            {
                OpenSignWindow();
            }
        }

        private void OnSignInClicked()
        {
            string formattedNumber = _numbersInputField.text;

            foreach (TextPlaceholder placeholder in _phoneNumberPlaceholders)
                placeholder.ReplaceValue(formattedNumber);

            string number = WinkAcceessHelper.GetNumber(formattedNumber, _minNumberCount, _maxNumberCount, _additivePlusChar);
            _signInFuctionsUI.OnSignInClicked(number, OnAuthenficationSuccessfully);
        }

        private void OnLimitReached(IReadOnlyList<string> devicesList)
        {
            CloseAllWindows();
            _notifyWindowHandler.OnLimitReached();

            foreach (string device in devicesList)
            {
                Button button = Instantiate(_unlinkButtonTemplate, _containerButtons);
                button.GetComponentInChildren<TMP_Text>().text = device;
                button.onClick.AddListener(()
                    => OnUnlinkClicked(button.GetComponentInChildren<TMP_Text>().text));
                _devicesIdButtons.Add(button);
            }
        }

        private void OnUnlinkClicked(string device)
        {
            foreach (Button button in _devicesIdButtons)
            {
                button.onClick.RemoveListener(()
                    => OnUnlinkClicked(button.GetComponentInChildren<TMP_Text>().text));
            }

            _devicesIdButtons.Clear();
            _signInFuctionsUI.OnUnlinkClicked(device);
        }

        private void OnAuthenficationSuccessfully()
        {
            _openSignInButton.gameObject.SetActive(false);
            string phone = "N/A";

            if (UnityEngine.PlayerPrefs.HasKey(_winkAccessManager.PhoneNumber))
                phone = UnityEngine.PlayerPrefs.GetString(_winkAccessManager.PhoneNumber);

            foreach (TextPlaceholder placeholder in _phoneNumberPlaceholders)
                placeholder.ReplaceValue(phone);

            _notifyWindowHandler.OpenHelloWindow(onEnd: () =>
            {
                AnalyticsWinkService.SendHelloWindow();

                if (WinkAccessManager.Instance.HasAccess == false)
                {
                    _notifyWindowHandler.OpenWindow(WindowType.Redirect);
                    AnalyticsWinkService.SendPayWallWindow();
                }
            });
        }

        private void OnAuthorizationSuccessfully()
        {
            string sanId = "N/A";
            var wait = new WaitUntil(() => UnityEngine.PlayerPrefs.HasKey(_winkAccessManager.SanId) == true);

            StartCoroutine(HellowWindowOpening());
            IEnumerator HellowWindowOpening()
            {
                yield return wait;

                if (UnityEngine.PlayerPrefs.HasKey(_winkAccessManager.SanId))
                    sanId = UnityEngine.PlayerPrefs.GetString(_winkAccessManager.SanId);

                foreach (TextPlaceholder placeholder in _idPlaceholders)
                    placeholder.ReplaceValue(sanId);
            }
        }

        private void OnTimerExpired() => _notifyWindowHandler.OpenDemoExpiredWindow(false);
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
