using System;
using System.Collections;
using UnityEngine;
using SmsAuthAPI.Program;
using UnityEngine.Scripting;
using SmsAuthAPI.DTO;

namespace Agava.Wink
{
    /// <summary>
    ///     Demo timer Handler. Block app after expired allowed time.
    /// </summary>
    [Serializable, Preserve]
    internal class DemoTimer
    {
        private const string FirstTimeSave = nameof(FirstTimeSave);
        private const float Delay = 5f;

        [Min(0)]
        [SerializeField] private int _defaultTimerSeconds = 1800;

        private IWinkAccessManager _winkAccessManager;
        private IWinkSignInHandlerUI _winkSignInHandlerUI;
        private ICoroutine _coroutine;

        private DateTime _savedDemoTime;
        private Coroutine _current;

        public event Action TimerExpired;

        public bool Expired { get; private set; }

        internal void Construct(IWinkAccessManager winkAccessManager, int remoteCfgSeconds, IWinkSignInHandlerUI winkSignInHandlerUI, ICoroutine coroutine)
        {
            _winkSignInHandlerUI = winkSignInHandlerUI;
            _winkAccessManager = winkAccessManager;
            _coroutine = coroutine;

            if (remoteCfgSeconds <= 0)
                remoteCfgSeconds = _defaultTimerSeconds;

            if (UnityEngine.PlayerPrefs.HasKey(FirstTimeSave) == false)
            {
                _savedDemoTime = DateTime.UtcNow.AddSeconds(remoteCfgSeconds);
                UnityEngine.PlayerPrefs.SetString(FirstTimeSave, _savedDemoTime.ToString());
            }
            else
            {
                string time = UnityEngine.PlayerPrefs.GetString(FirstTimeSave);
                _savedDemoTime = DateTime.Parse(time);
            }

            _winkAccessManager.AuthorizationSuccessfully += Stop;
            _winkAccessManager.AccountDeleted += Start;
        }

        internal void Dispose()
        {
            if (_winkAccessManager != null)
            {
                _winkAccessManager.AuthorizationSuccessfully -= Stop;
                _winkAccessManager.AccountDeleted -= Start;
            }
        }

        internal void Start()
        {
            _current = _coroutine.StartCoroutine(Ticking());
            Expired = false;

            IEnumerator Ticking()
            {
#if UNITY_EDITOR || TEST
                Debug.Log("Demo activated");
#endif
                var tick = new WaitForSecondsRealtime(1f);
                var waitBeforeStart = new WaitForSecondsRealtime(Delay);
                var waitInitialize = new WaitWhile(() => SmsAuthApi.Initialized == false);
                var waitWindowClosed = new WaitUntil(() => _winkSignInHandlerUI.IsAnyWindowEnabled == false);

                yield return waitInitialize;
                yield return waitBeforeStart;
                yield return waitWindowClosed;

                if (WinkAccessManager.Instance.HasAccess)
                    Stop();

                while (true)
                {
                    if (_savedDemoTime <= DateTime.UtcNow && WinkAccessManager.Instance.HasAccess == false)
                    {
                        TimerExpired?.Invoke();
                        Expired = true;
                    }

                    yield return tick;
                }
            }
        }

        internal void Stop()
        {
            Expired = false;

            if (_current != null)
            {
#if UNITY_EDITOR || TEST
                Debug.Log("Demo Stoped");
#endif
                _coroutine.StopCoroutine(_current);
                _current = null;
            }
        }
    }
}
