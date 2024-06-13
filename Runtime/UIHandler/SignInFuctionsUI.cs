using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using SmsAuthAPI.Program;

namespace Agava.Wink
{
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

        internal void OnSignInClicked(string number, Action onSuccessfully)
        {
            if (string.IsNullOrEmpty(number))
            {
                _notifyWindowHandler.OpenWindow(WindowType.WrongNumber);
                return;
            }

            _notifyWindowHandler.OpenWindow(WindowType.ProccessOn);

            _winkAccessManager.Regist(phoneNumber: number,
            otpCodeRequest: (hasOtpCode) =>
            {
                if (hasOtpCode)
                {
                    _notifyWindowHandler.CloseWindow(WindowType.ProccessOn);

                    _notifyWindowHandler.OpenInputWindow(onInputDone: (code) =>
                    {
                        _notifyWindowHandler.OpenWindow(WindowType.ProccessOn);
                        _winkAccessManager.SendOtpCode(code);
                    });
                }
                else
                {
                    _notifyWindowHandler.CloseWindow(WindowType.ProccessOn);
                    _notifyWindowHandler.OpenWindow(WindowType.Fail);
                }
            },
            winkSubscriptionAccessRequest: (hasAccess) =>
            {
                OnSignInDone(onSuccessfully, hasAccess);

                //if (hasAccess)
                //{
                //    OnSignInDone(onSuccessfully);
                //}
                //else
                //{
                //    _notifyWindowHandler.CloseWindow(WindowType.ProccessOn);
                //    _notifyWindowHandler.OpenWindow(WindowType.Redirect);
                //}
            });
        }

        internal void OnSubsDenied(bool hasAccess)
        {
            if (hasAccess == false)
            {
                _notifyWindowHandler.CloseWindow(WindowType.ProccessOn);
                _notifyWindowHandler.OpenWindow(WindowType.Redirect);
            }
        }

        internal void OnUnlinkClicked(string device)
        {
            _winkAccessManager.Unlink(device);
            _notifyWindowHandler.CloseWindow(WindowType.Unlink);
            _notifyWindowHandler.OpenSignInWindow();
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

        private void OnSignInDone(Action onSuccessfully, bool hasAccess)
        {
            onSuccessfully?.Invoke();
            OnSuccessfully();

            _notifyWindowHandler.CloseWindow(WindowType.SignIn);
            _notifyWindowHandler.CloseWindow(WindowType.ProccessOn);

            _notifyWindowHandler.OpenHelloWindow(onEnd: () =>
            {
                if (hasAccess == false)
                    _notifyWindowHandler.OpenHelloSubscribeWindow(null);
            });

            //_notifyWindowHandler.OpenWindow(WindowType.Successfully);
            //_notifyWindowHandler.CloseWindow(WindowType.SignIn);
            //_notifyWindowHandler.CloseWindow(WindowType.ProccessOn);

            //onSuccessfully?.Invoke();
            //OnSuccessfully();

            //_coroutine.StartCoroutine(Waiting());
            //IEnumerator Waiting()
            //{
            //    yield return new WaitForSecondsRealtime(2f);

            //    if (_notifyWindowHandler.HasOpenedWindow(WindowType.Successfully))
            //        _notifyWindowHandler.CloseWindow(WindowType.Successfully);
            //}
        }

        private void OnSuccessfully()
        {
            _demoTimer.Stop();
            _notifyWindowHandler.CloseWindow(WindowType.DemoTimerExpired);
        }
    }
}
