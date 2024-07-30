using System;
using System.Collections;
using UnityEngine;
using SmsAuthAPI.Program;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    /// <summary>
    ///     Demo timer Handler. Block app after expired allowed time.
    /// </summary>
    [Serializable, Preserve]
    internal class DemoTimer
    {
        private const string TimerKey = nameof(TimerKey);
        private const float Delay = 5f;

        [Min(0)]
        [SerializeField] private int _defaultTimerSeconds = 1800;

        private IWinkAccessManager _winkAccessManager;
        private IWinkSignInHandlerUI _winkSignInHandlerUI;
        private ICoroutine _coroutine;

        private Coroutine _current;
        private int _seconds;

        public event Action TimerExpired;

        public bool Expired { get; private set; }

        internal void Construct(IWinkAccessManager winkAccessManager, int remoteCfgSeconds, IWinkSignInHandlerUI winkSignInHandlerUI, ICoroutine coroutine)
        {
            _winkSignInHandlerUI = winkSignInHandlerUI;
            _winkAccessManager = winkAccessManager;
            _coroutine = coroutine;

            if (remoteCfgSeconds <= 0)
                remoteCfgSeconds = _defaultTimerSeconds;

            if (UnityEngine.PlayerPrefs.HasKey(TimerKey))
                _seconds = UnityEngine.PlayerPrefs.GetInt(TimerKey);
            else
                _seconds = remoteCfgSeconds;

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

            UnityEngine.PlayerPrefs.SetInt(TimerKey, _seconds);
        }

        internal void Start()
        {
            _current = _coroutine.StartCoroutine(Ticking());
            Expired = false;

            IEnumerator Ticking()
            {
                var tick = new WaitForSecondsRealtime(1);
                var waitBeforeStart = new WaitForSecondsRealtime(Delay);
                var waitInitialize = new WaitWhile(() => SmsAuthApi.Initialized == false);

                yield return waitInitialize;
                yield return waitBeforeStart;

                if (WinkAccessManager.Instance.HasAccess)
                    Stop();

                while (_seconds > 0)
                {
                    if (_winkSignInHandlerUI.IsAnyWindowEnabled == false)
                    {
                        _seconds--;
                        UnityEngine.PlayerPrefs.SetInt(TimerKey, _seconds);
                    }

                    yield return tick;
                }

                if (_seconds <= 0 && WinkAccessManager.Instance.HasAccess == false)
                {
                    TimerExpired?.Invoke();
                    Expired = true;
                }
            }
        }

        internal void Stop()
        {
            Expired = false;

            if (_current != null)
            {
                _coroutine.StopCoroutine(_current);
                _current = null;
            }
        }
    }
}
