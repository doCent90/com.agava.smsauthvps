using System;
using UnityEngine;
using UnityEngine.Scripting;
using SmsAuthAPI.Program;

namespace Agava.Wink
{
    /// <summary>
    ///     Demo timer Handler. Block app after expired allowed time.
    /// </summary>
    [Serializable, Preserve]
    internal class DemoTimer
    {
        private const string FirstTimeSave = nameof(FirstTimeSave);

        [Min(0)]
        [SerializeField] private int _defaultTimerSeconds = 1800;

        private IWinkAccessManager _winkAccessManager;
        private IWinkSignInHandlerUI _winkSignInHandlerUI;

        private TimeSpan _savedDemoTime;
        private bool _focus = true;
        private bool _stoped;
        private float _second;
        private float _delay = 5f;

        public event Action TimerExpired;

        public bool Expired { get; private set; }

        internal void Construct(IWinkAccessManager winkAccessManager, int remoteCfgSeconds, IWinkSignInHandlerUI winkSignInHandlerUI)
        {
            _winkSignInHandlerUI = winkSignInHandlerUI;
            _winkAccessManager = winkAccessManager;

            if (remoteCfgSeconds <= 0)
                remoteCfgSeconds = _defaultTimerSeconds;

            if (UnityEngine.PlayerPrefs.HasKey(FirstTimeSave) == false)
            {
                _savedDemoTime = TimeSpan.FromSeconds(remoteCfgSeconds);
                UnityEngine.PlayerPrefs.SetString(FirstTimeSave, _savedDemoTime.ToString());
                Debug.LogError(_savedDemoTime);
            }
            else
            {
                string time = UnityEngine.PlayerPrefs.GetString(FirstTimeSave);
                _savedDemoTime = TimeSpan.Parse(time);
                Debug.LogError(_savedDemoTime);
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

        internal void OnAppFocus(bool focus) => _focus = focus;

        internal void Start()
        {
            Expired = false;
            _stoped = false;
#if UNITY_EDITOR || TEST
                Debug.Log("Demo activated");
#endif
        }

        internal void Stop()
        {
            Expired = false;
            _stoped = true;

#if UNITY_EDITOR || TEST
            Debug.Log("Demo Stoped");
#endif
        }

        internal void Update()
        {
            if (_focus == false || _stoped)
                return;

            if (_delay > 0)
            {
                _delay -= Time.deltaTime;
                return;
            }

            if (_winkSignInHandlerUI.IsAnyWindowEnabled || Expired || SmsAuthApi.Initialized == false)
                return;

            _second -= Time.deltaTime;

            if (_second <= 0)
            {
                if (_savedDemoTime <= TimeSpan.Zero && WinkAccessManager.Instance.HasAccess == false)
                {
                    TimerExpired?.Invoke();
                    Expired = true;
                }

                _savedDemoTime = _savedDemoTime.Subtract(TimeSpan.FromSeconds(1f));
                UnityEngine.PlayerPrefs.SetString(FirstTimeSave, _savedDemoTime.ToString());
                Debug.LogError(_savedDemoTime);
                _second = 1;
            }
        }
    }
}
