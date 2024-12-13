using System;
using System.Collections;
using System.Threading.Tasks;
using SmsAuthAPI.Program;
using UnityEngine;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    [Preserve]
    internal class TextTimer : MonoBehaviour
    {
        private const string SmsDelay = "sms-delay-seconds";
        private const string ExpirationTime = nameof(ExpirationTime);
        private const int SmsDelayDefaultTime = 60;

        [SerializeField] private TextPlaceholder _timePlaceholder;

        private int _smsDelaySeconds;
        private DateTime _expirationDate;
        private int _seconds;
        private bool _active = false;

        public event Action TimerExpired;

        private DateTime Now => DateTime.Now;

        public bool Initialized { get; private set; } = false;
        public bool ZeroSeconds => _seconds <= 0;

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => SmsAuthApi.Initialized);

            Task task = SetRemoteConfigs();
            yield return new WaitUntil(() => task.IsCompleted);

            Initialized = true;

            _seconds = 0;

            if (TryLoadSave())
            {
                _active = true;
            }
        }

        private void Update()
        {
            if (_active)
            {
                if (_seconds > 0)
                {
                    _seconds = SubtractSeconds(_expirationDate);
                    _timePlaceholder.ReplaceValue(TimeString(_seconds));
                }
                else
                {
                    TimerExpired?.Invoke();
                    ResetTimer();
                    StopTimer();
                }
            }

            _timePlaceholder.gameObject.SetActive(_active);
        }

        internal void StartTimer()
        {
            TryLoadSave();

            if (_seconds <= 0)
            {
                NewSave();
            }

            _active = true;
        }

        internal void StopTimer()
        {
            _active = false;
        }

        internal void ResetTimer()
        {
            UnityEngine.PlayerPrefs.DeleteKey(ExpirationTime);
        }

        private async Task SetRemoteConfigs()
        {
            _smsDelaySeconds = await RemoteConfig.IntRemoteConfig(SmsDelay, SmsDelayDefaultTime);
        }

        private string TimeString(int seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
            return $"{timeSpan.Minutes}:{timeSpan.Seconds:00}";
        }

        private bool TryLoadSave()
        {
            if (UnityEngine.PlayerPrefs.HasKey(ExpirationTime))
            {
                if (DateTime.TryParse(UnityEngine.PlayerPrefs.GetString(ExpirationTime), out _expirationDate))
                {
                    _seconds = SubtractSeconds(_expirationDate);

                    if (_seconds > _smsDelaySeconds)
                        NewSave();

                    return true;
                }
            }

            return false;
        }

        private void NewSave()
        {
            _expirationDate = Now.AddSeconds(_smsDelaySeconds);
            _seconds = _smsDelaySeconds;
            UnityEngine.PlayerPrefs.SetString(ExpirationTime, _expirationDate.ToString());
        }

        private int SubtractSeconds(DateTime expirationDate) => expirationDate.Subtract(Now).Seconds;
    }
}
