using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        [Header("Phone Number Placeholders")]
        [SerializeField] private PhoneNumberPlaceholder[] _phoneNumberPlaceholders;

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
            _winkAccessManager.Successfully -= OnSuccessfully;
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
            _winkAccessManager.SetWinkSubsEvent(_signInFuctionsUI.OnSubsDenied);
            _signInButton.onClick.AddListener(OnSignInClicked);
            _openSignInButton.onClick.AddListener(OpenSignWindow);
            CloseAllWindows();

            _winkAccessManager.ResetLogin += OpenSignWindow;
            _winkAccessManager.LimitReached += OnLimitReached;
            _winkAccessManager.Successfully += OnSuccessfully;
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

        public void OpenSignWindow() => _notifyWindowHandler.OpenSignInWindow(() => SignInWindowClosed?.Invoke());
        public void OpenWindow(WindowType type) => _notifyWindowHandler.OpenWindow(type);
        public void CloseWindow(WindowType type) => _notifyWindowHandler.CloseWindow(type);
        public void CloseAllWindows() => _notifyWindowHandler.CloseAllWindows(AllWindowsClosed);

        private void OnSignInClicked()
        {
            string formattedNumber = _numbersInputField.text;

            foreach (PhoneNumberPlaceholder placeholder in _phoneNumberPlaceholders)
                placeholder.ReplaceValue(formattedNumber);

            string number = WinkAcceessHelper.GetNumber(formattedNumber, _minNumberCount, _maxNumberCount, _additivePlusChar);
            _signInFuctionsUI.OnSignInClicked(number, OnSuccessfully);
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

        private void OnSuccessfully()
        {
            _openSignInButton.gameObject.SetActive(false);

            bool subscribed = true;

            if (_winkAccessManager.HasAccess)
            {
                _notifyWindowHandler.OpenHelloWindow(onEnd: () =>
                {
                    if (subscribed == false)
                        _notifyWindowHandler.OpenHelloSubscribeWindow(null);
                });
            }

            _notifyWindowHandler.OpenWindow(WindowType.Hello);
        }

        private void OnTimerExpired() => _notifyWindowHandler.OpenWindow(WindowType.DemoTimerExpired);
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
