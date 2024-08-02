using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using SmsAuthAPI.Program;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    [Preserve]
    internal class SignInFuctionsUI
    {
        private const string RemoteName = "max-demo-minutes";
        private const int MinutesFactor = 60;

        private readonly DemoTimer _demoTimer;
        private readonly WinkAccessManager _winkAccessManager;
        private readonly NotifyWindowHandler _notifyWindowHandler;

        private readonly ICoroutine _coroutine;
        private readonly IWinkSignInHandlerUI _winkSignInHandlerUI;

        public SignInFuctionsUI(NotifyWindowHandler notifyWindowHandler, DemoTimer demoTimer, WinkAccessManager winkAccessManager,
            IWinkSignInHandlerUI winkSignInHandlerUI, ICoroutine coroutine)
        {
            _demoTimer = demoTimer;
            _coroutine = coroutine;
            _winkAccessManager = winkAccessManager;
            _notifyWindowHandler = notifyWindowHandler;
            _winkSignInHandlerUI = winkSignInHandlerUI;
        }

        internal void OnSignInClicked(string phone)
        {
            _notifyWindowHandler.OpenWindow(WindowType.ProccessOn);
            AnalyticsWinkService.SendOnEnteredPhoneWindow();

            _winkAccessManager.Regist(phoneNumber: phone,
            otpCodeRequest: (hasOtpCode) =>
            {
                OnOtpCodeRequested(phone, hasOtpCode);
            },
            otpCodeAccepted: (accepted) =>
            {
                OnOtpCodeAccepted(accepted);
            });
        }

        internal void OnUnlinkClicked(string device)
        {
            _notifyWindowHandler.OpenWindow(WindowType.ProccessOn);

            _winkAccessManager.Unlink(device, () =>
            {
                _notifyWindowHandler.CloseWindow(WindowType.Redirect);
                _notifyWindowHandler.CloseWindow(WindowType.Unlink);
                _winkAccessManager.Login();
            });
        }

        internal async void SetRemoteConfig()
        {
            await Task.Yield();

            var response = await SmsAuthApi.GetRemoteConfig(RemoteName);

            if (response.statusCode == UnityWebRequest.Result.Success)
            {
                int seconds;

                if (string.IsNullOrEmpty(response.body))
                    seconds = 0;
                else
                    seconds = Convert.ToInt32(response.body) * MinutesFactor;

                _demoTimer.Construct(_winkAccessManager, seconds, _winkSignInHandlerUI, _coroutine);
                _demoTimer.Start();
#if UNITY_EDITOR || TEST
                Debug.Log("Remote setted: " + response.body);
#endif
            }
            else
            {
                Debug.LogError("Fail to recieve remote config: " + response.statusCode);
            }
        }

        private void OnOtpCodeRequested(string phone, bool hasOtpCode)
        {
            if (hasOtpCode)
            {
                _notifyWindowHandler.CloseWindow(WindowType.ProccessOn);
                AnalyticsWinkService.SendEnterOtpCodeWindow();

                _notifyWindowHandler.OpenInputOtpCodeWindow(phone,
                onInputDone: (code) =>
                {
                    _winkAccessManager.SendOtpCode(code);
                    AnalyticsWinkService.SendOnEnteredOtpCodeWindow();
                },
                onBackClicked: () =>
                {
                    _notifyWindowHandler.CloseWindow(WindowType.EnterOtpCode);
                    _notifyWindowHandler.OpenSignInWindow();
                });
            }
            else
            {
                _notifyWindowHandler.CloseWindow(WindowType.ProccessOn);
                _notifyWindowHandler.OpenWindow(WindowType.Fail);
            }
        }

        private void OnOtpCodeAccepted(bool accepted)
        {
            if (accepted == false)
            {
                _notifyWindowHandler.ResetInputWindow();
            }
            else
            {
                _notifyWindowHandler.OpenWindow(WindowType.ProccessOn);
                _notifyWindowHandler.CloseWindow(WindowType.EnterOtpCode);
            }
        }

        internal void OnSignInSuccesfully(bool hasAccess)
        {
            if (hasAccess)
                OnAuthorizationSuccessfully();

            _notifyWindowHandler.CloseWindow(WindowType.SignIn);
        }

        internal void OnAuthorizationSuccessfully()
        {
            _demoTimer.Stop();
            _notifyWindowHandler.CloseWindow(WindowType.DemoTimerExpired);
        }
    }
}
