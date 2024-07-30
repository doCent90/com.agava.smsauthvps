﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmsAuthAPI.DTO;
using SmsAuthAPI.Program;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    /// <summary>
    ///     Auth process logic.
    /// </summary>
    [Preserve]
    public class WinkAccessManager : MonoBehaviour, IWinkAccessManager, ICoroutine
    {
        private const string FirstRegist = nameof(FirstRegist);
        private const string UniqueId = nameof(UniqueId);

        [SerializeField] private string _ip;
        [SerializeField] private string _additiveId;

        private RequestHandler _requestHandler;
        private TimespentService _timespentService;
        private SubscriptionSearchSystem _subscribeSearchSystem;
        private Action<bool> _winkSubscriptionAccessRequest;
        private Action<bool> _otpCodeAccepted;
        private string _uniqueId;

        public readonly string PhoneNumber = nameof(PhoneNumber);
        public readonly string SanId = nameof(SanId);
        public LoginData LoginData { get; private set; }
        public bool Authenficated { get; private set; } = false;
        public bool HasAccess { get; private set; } = false;
        public string AppId => Application.identifier;
        public static WinkAccessManager Instance { get; private set; }

        public event Action<IReadOnlyList<string>> LimitReached;
        public event Action ResetLogin;
        public event Action<bool> SignInSuccessfully;
        public event Action AuthorizationSuccessfully;
        public event Action AccountDeleted;

        private void OnApplicationFocus(bool focus)
        {
            if (focus == false && _timespentService != null)
                _timespentService.OnFinishedApp();
            else if (focus && _timespentService != null)
                _timespentService.OnStartedApp();
        }

        public void Initialize()
        {
            if (SmsAuthApi.Initialized == false)
                SmsAuthApi.Initialize(_ip, AppId);

            if (Instance == null)
                Instance = this;
        }

        public IEnumerator Construct()
        {
            _requestHandler = new();

            DontDestroyOnLoad(this);

            if (UnityEngine.PlayerPrefs.HasKey(UniqueId) == false)
                _uniqueId = SystemInfo.deviceUniqueIdentifier + _additiveId;
            else
                _uniqueId = UnityEngine.PlayerPrefs.GetString(UniqueId);

            if (UnityEngine.PlayerPrefs.HasKey(PhoneNumber))
                LoginData = new LoginData() { phone = UnityEngine.PlayerPrefs.GetString(PhoneNumber), device_id = _uniqueId, app_id = AppId };

            if (LoginData != null)
                StartTimespentAnalytics();

            yield return null;

            StartCoroutine(DelayedSendStatistic());
        }

        public void TryQuickAccess()
        {
            if (UnityEngine.PlayerPrefs.HasKey(TokenLifeHelper.Tokens))
                QuickAccess();
        }

        public void SendOtpCode(string enteredOtpCode)
        {
            LoginData.otp_code = enteredOtpCode;
            Login(LoginData);
        }

        public async void Regist(string phoneNumber, Action<bool> otpCodeRequest, Action<bool> otpCodeAccepted)
        {
            _winkSubscriptionAccessRequest = OnSignInSuccessfully;
            _otpCodeAccepted = otpCodeAccepted;
            UnityEngine.PlayerPrefs.SetString(PhoneNumber, phoneNumber);
            LoginData = await _requestHandler.Regist(phoneNumber, _uniqueId, AppId, otpCodeRequest);

            if (_timespentService == null)
                StartTimespentAnalytics();
        }

        public void Unlink(string deviceId) => _requestHandler.Unlink(new UnlinkData() { device_id = deviceId, app_id = AppId }, ResetLogin);

#if UNITY_EDITOR || TEST
        public void TestEnableSubsription()
        {
            HasAccess = true;
            Authenficated = true;
            AuthorizationSuccessfully?.Invoke();
            Debug.Log("Test Access succesfully. No cloud saves");
        }
#endif

        private void Login(LoginData data) => _requestHandler.Login(data, LimitReached, _winkSubscriptionAccessRequest, _otpCodeAccepted);

        public void QuickAccess()
            => _requestHandler.QuickAccess(LoginData.phone, ResetLogin, null, OnSignInSuccessfully);

        public void DeleteAccount()
        {
            if (_timespentService != null)
            {
                _timespentService.OnFinishedApp();
                _timespentService = null;
            }

            _requestHandler.UnlinkDevices(AppId, _uniqueId, () =>
            {
                HasAccess = false;
                Authenficated = false;
                UnityEngine.PlayerPrefs.DeleteKey(TokenLifeHelper.Tokens);
                AccountDeleted?.Invoke();
            });
        }

        private void OnSignInSuccessfully(bool hasAccess)
        {
            Authenficated = true;
            SignInSuccessfully?.Invoke(hasAccess);
            SearchSubscription(LoginData.phone);

#if UNITY_EDITOR || TEST
            Debug.Log("Authenfication succesfully");
#endif

            if (hasAccess)
            {
                OnSubscriptionExist();
            }
        }

        private void OnSubscriptionExist()
        {
            _subscribeSearchSystem?.Stop();
            HasAccess = true;
            AuthorizationSuccessfully?.Invoke();

            if (PlayerPrefs.HasKey(FirstRegist))
                AnalyticsWinkService.SendHasActiveAccountUser(hasActiveAcc: true);

#if UNITY_EDITOR || TEST
            Debug.Log("Wink access succesfully");
#endif
        }

        private void SearchSubscription(string phone)
        {
            if (_subscribeSearchSystem != null)
                return;

            _subscribeSearchSystem = new(phone);
            _subscribeSearchSystem.StartSearching(onSubscriptionExist: () =>
            {
                OnSubscriptionExist();
                TrySendAnalyticsData(LoginData.phone);
            });
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
                        AnalyticsWinkService.SendSanId(responseGetSanId.body);
                        AnalyticsWinkService.SendHasActiveAccountNewUser(hasActiveAcc: true);
                        SmsAuthApi.OnUserAddApp(LoginData.phone, responseGetSanId.body, AppId);
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
            _timespentService = new(this, LoginData.phone, _uniqueId, AppId);
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
