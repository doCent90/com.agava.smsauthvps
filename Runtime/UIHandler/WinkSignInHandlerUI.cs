using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SmsAuthAPI.Program;
using UnityEngine.Scripting;
using System.Threading.Tasks;

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
        [SerializeField] private TMP_InputField _numbersInputField;
        [Header("UI Buttons")]
        [SerializeField] private Button _signInButton;
        [SerializeField] private Button _openSignInButton;
        [SerializeField] private Button _openSignInDemoButton;
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
            if (_signInFuctionsUI == null) return;

            _signInButton.onClick.RemoveAllListeners();
            _openSignInDemoButton.onClick.RemoveAllListeners();

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
            _openSignInDemoButton.onClick.AddListener(OpenSignWindow);
            CloseAllWindows();

            _winkAccessManager.ResetLogin += OpenSignWindow;
            _winkAccessManager.LimitReached += OnLimitReached;
            _winkAccessManager.SignInSuccessfully += OnSignInSuccessfully;
            _winkAccessManager.AuthorizationSuccessfully += OnAuthorizationSuccessfully;
            _demoTimer.TimerExpired += OnTimerExpired;
        }

        public void OpenSignWindow()
        {
            _numbersInputField.text = string.Empty;
            _notifyWindowHandler.OpenSignInWindow(() => SignInWindowClosed?.Invoke());
            AnalyticsWinkService.SendEnterPhoneWindow();
        }

        public void OpenWindow(WindowType type) => _notifyWindowHandler.OpenWindow(type);
        public void CloseWindow(WindowType type) => _notifyWindowHandler.CloseWindow(type);
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
                    AnalyticsWinkService.SendSubscribeProfileWindow();
                    _notifyWindowHandler.OpenWindow(WindowType.Redirect);
                }
            }
            else
            {
                OpenSignWindow();
            }
        }

        public void OnDeleteAccountButtonClick()
        {
            _notifyWindowHandler.OpenDeleteAccountWindow(_winkAccessManager.DeleteAccount);
        }

        private void OnSignInClicked()
        {
            string number = WinkAcceessHelper.GetNumber(_numbersInputField.text, _minNumberCount, _maxNumberCount, _additivePlusChar);
            string formattedNumber = PhoneNumber.FormatNumber(number);

            foreach (TextPlaceholder placeholder in _phoneNumberPlaceholders)
                placeholder.ReplaceValue(formattedNumber);

            _signInFuctionsUI.OnSignInClicked(number);
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

                Destroy(button.gameObject);
            }

            _devicesIdButtons.Clear();
            _signInFuctionsUI.OnUnlinkClicked(device);
        }

        private void OnAuthorizationSuccessfully() => _signInFuctionsUI.OnAuthorizationSuccessfully();

        private async void OnSignInSuccessfully(bool hasAccess)
        {
            _signInFuctionsUI.OnSignInSuccesfully(hasAccess);
            _openSignInButton.gameObject.SetActive(false);
            _signInButton.gameObject.SetActive(false);

            SetPhone();
            bool hasId = await SetId(hasAccess);

            StartCoroutine(OpeningHelloWindow());

            IEnumerator OpeningHelloWindow()
            {
                yield return new WaitUntil(() => hasId);

                _notifyWindowHandler.OpenHelloWindow(onEnd: () =>
                {
                    AnalyticsWinkService.SendHelloWindow();

                    if (hasAccess == false)
                    {
                        if (_demoTimer.Expired == false)
                        {
                            _notifyWindowHandler.OpenWindow(WindowType.Redirect);
                            AnalyticsWinkService.SendPayWallWindow();
                        }
                    }
                });

                _notifyWindowHandler.CloseWindow(WindowType.ProccessOn);
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

        private async Task<bool> SetId(bool hasAccess)
        {
            string id = null;

            if (hasAccess)
                id = await TryGetId();

            if (string.IsNullOrEmpty(id))
                id = "N/A";

            foreach (TextPlaceholder placeholder in _idPlaceholders)
                placeholder.ReplaceValue(id);

            return true;
        }

        private async Task<string> TryGetId()
        {
            string sanId = null;

            if (UnityEngine.PlayerPrefs.HasKey(_winkAccessManager.SanId) == false && UnityEngine.PlayerPrefs.HasKey(_winkAccessManager.PhoneNumber))
            {
                var phone = UnityEngine.PlayerPrefs.GetString(_winkAccessManager.PhoneNumber);
                var responseGetSanId = await SmsAuthApi.GetSanId(phone);

                if (responseGetSanId.statusCode == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    UnityEngine.PlayerPrefs.SetString(_winkAccessManager.SanId, responseGetSanId.body);
            }

            if (UnityEngine.PlayerPrefs.HasKey(_winkAccessManager.SanId))
                sanId = UnityEngine.PlayerPrefs.GetString(_winkAccessManager.SanId);

            return sanId;
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
