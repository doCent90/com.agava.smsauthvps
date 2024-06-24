using System;
using System.Collections.Generic;
using UnityEngine;
using SmsAuthAPI.DTO;
using SmsAuthAPI.Program;
using System.Collections;

namespace Agava.Wink
{
    /// <summary>
    ///     Auth process logic.
    /// </summary>
    public class WinkAccessManager : MonoBehaviour, IWinkAccessManager, ICoroutine
    {
        private const string FirstRegist = nameof(FirstRegist);
        private const string UniqueId = nameof(UniqueId);

        [SerializeField] private string _ip;
        [SerializeField] private string _additiveId;

        private RequestHandler _requestHandler;
        private TimespentService _timespentService;
        private Action<bool> _winkSubscriptionAccessRequest;
        private Action<bool> _otpCodeAccepted;
        private string _uniqueId;

        public readonly string PhoneNumber = nameof(PhoneNumber);
        public readonly string SanId = nameof(SanId);
        public LoginData LoginData { get; private set; }
        public bool Authenficated { get; private set; } = false;
        public bool HasAccess { get; private set; } = false;
        public static WinkAccessManager Instance { get; private set; }

        public event Action<IReadOnlyList<string>> LimitReached;
        public event Action ResetLogin;
        public event Action AuthorizationSuccessfully;
        public event Action AuthenficationSuccessfully;

        private void OnApplicationFocus(bool focus)
        {
            if (focus == false && _timespentService != null)
                _timespentService.OnFinishedApp();
            else if (focus && _timespentService != null)
                _timespentService.OnStartedApp();
        }

        public IEnumerator Construct()
        {
            if (Instance == null)
                Instance = this;

            _requestHandler = new();

            DontDestroyOnLoad(this);

            if (UnityEngine.PlayerPrefs.HasKey(UniqueId) == false)
                _uniqueId = SystemInfo.deviceName + Application.identifier + _additiveId;
            else
                _uniqueId = UnityEngine.PlayerPrefs.GetString(UniqueId);

            if (UnityEngine.PlayerPrefs.HasKey(PhoneNumber))
                LoginData = new LoginData() { phone = UnityEngine.PlayerPrefs.GetString(PhoneNumber), device_id = _uniqueId };

            if (LoginData != null)
                StartTimespentAnalytics();

            if (SmsAuthApi.Initialized == false)
                SmsAuthApi.Initialize(_ip, _uniqueId);

            yield return null;

            if (UnityEngine.PlayerPrefs.HasKey(TokenLifeHelper.Tokens))
                QuickAccess();

            StartCoroutine(DelayedSendStatistic());
        }

        public void SendOtpCode(string enteredOtpCode)
        {
            LoginData.otp_code = enteredOtpCode;
            Login(LoginData);
        }

        public async void Regist(string phoneNumber, Action<bool> otpCodeRequest, Action<bool> winkSubscriptionAccessRequest, Action<bool> otpCodeAccepted)
        {
            _winkSubscriptionAccessRequest = winkSubscriptionAccessRequest;
            _otpCodeAccepted = otpCodeAccepted;
            UnityEngine.PlayerPrefs.SetString(PhoneNumber, phoneNumber);
            LoginData = await _requestHandler.Regist(phoneNumber, _uniqueId, otpCodeRequest);

            if (_timespentService == null)
                StartTimespentAnalytics();
        }

        public void Unlink(string deviceId) => _requestHandler.Unlink(deviceId, ResetLogin);

#if UNITY_EDITOR || TEST
        public void TestEnableSubsription()
        {
            HasAccess = true;
            Authenficated = true;
            AuthorizationSuccessfully?.Invoke();
            Debug.Log("Test Access succesfully. No cloud saves");
        }
#endif

        private void Login(LoginData data)
        {
            _requestHandler.Login(data, LimitReached, _winkSubscriptionAccessRequest, _otpCodeAccepted,
            onAuthenficationSuccessfully: () =>
            {
                OnAuthenficationSuccessfully();
            },
            onAuthorizationSuccessfully: () =>
            {
                OnSubscriptionExist();
                TrySendAnalyticsData(LoginData.phone);
            });
        }

        private void QuickAccess() =>
            _requestHandler.QuickAccess(LoginData.phone, OnSubscriptionExist, ResetLogin, _winkSubscriptionAccessRequest, OnAuthenficationSuccessfully);

        private void OnAuthenficationSuccessfully()
        {
            Authenficated = true;
            AuthenficationSuccessfully?.Invoke();
#if UNITY_EDITOR || TEST
            Debug.Log("Authenfication succesfully");
#endif
        }

        private void OnSubscriptionExist()
        {
            HasAccess = true;
            Authenficated = true;
            AuthorizationSuccessfully?.Invoke();

            if (PlayerPrefs.HasKey(FirstRegist))
                AnalyticsWinkService.SendHasActiveAccountUser(hasActiveAcc: true);
#if UNITY_EDITOR || TEST
            Debug.Log("Wink access succesfully");
#endif
        }

        private async void TrySendAnalyticsData(string phone)
        {
            if (PlayerPrefs.HasKey(FirstRegist) == false)
            {
                var responseActiveAccount = await SmsAuthApi.HasActiveAccount(phone);

                if (responseActiveAccount.statusCode == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    AnalyticsWinkService.SendFirstOpen();

                    var responseGetSanId = await SmsAuthApi.GetSanId(phone);

                    if (responseGetSanId.statusCode == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        UnityEngine.PlayerPrefs.SetString(SanId, responseGetSanId.body);
                        AnalyticsWinkService.SendSanId(responseGetSanId.body);
                        AnalyticsWinkService.SendHasActiveAccountNewUser(hasActiveAcc: true);
                        SmsAuthApi.OnUserAddApp(LoginData.phone, responseGetSanId.body);
                        PlayerPrefs.SetString(FirstRegist, "done");
                    }
                }
                else
                {
                    AnalyticsWinkService.SendHasActiveAccountNewUser(hasActiveAcc: false);
                }
            }
        }

        private void StartTimespentAnalytics()
        {
            _timespentService = new(this, LoginData.phone, _uniqueId, Application.identifier);
            _timespentService.OnStartedApp();
        }

        private IEnumerator DelayedSendStatistic()
        {
            yield return new WaitForSecondsRealtime(time: 120f);

            if (HasAccess == false)
                AnalyticsWinkService.SendHasActiveAccountUser(hasActiveAcc: false);
        }
    }
}
