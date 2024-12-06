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
        private const string CodeLifespan = "code-lifespan-seconds";
        private const string SavedTime = nameof(SavedTime);
        private const int SmsDelayDefaultTime = 60;
        private const int CodeLifespanDefaultTime = 600;

        [SerializeField] private TextPlaceholder _timePlaceholder;

        private int _smsDelaySeconds;
        private int _codeLifespan;

        private int _seconds = 0;
        private Coroutine _coroutine;

        public event Action TimerExpired;
        public bool Expired = true;

        private IEnumerator Start()
        {
            if (_seconds <= 0)
                _seconds = SmsDelayDefaultTime;

            yield return new WaitUntil(() => SmsAuthApi.Initialized);

            Task task = SetRemoteConfigs();
            yield return new WaitUntil(() => task.IsCompleted);

            _seconds = _codeLifespan;
        }

        public void SetSmsDelayConfig()
        {
            UnityEngine.PlayerPrefs.DeleteKey(SavedTime);
            _seconds = _smsDelaySeconds;
        }

        public void SetCodeLifespanConfig()
        {
            UnityEngine.PlayerPrefs.DeleteKey(SavedTime);
            _seconds = _codeLifespan;
        }

        internal void Enable()
        {
            _timePlaceholder.gameObject.SetActive(true);
            _coroutine ??= StartCoroutine(Ticking());

            IEnumerator Ticking()
            {
                int sec = _seconds;

                if (UnityEngine.PlayerPrefs.HasKey(SavedTime))
                    sec = UnityEngine.PlayerPrefs.GetInt(SavedTime);

                Expired = false;
                var tick = new WaitForSecondsRealtime(1);

                while (sec > 0)
                {
                    sec--;
                    _timePlaceholder.ReplaceValue(TimeString(sec));
                    UnityEngine.PlayerPrefs.SetInt(SavedTime, sec);

                    yield return tick;
                }

                if (sec <= 0)
                {
                    TimerExpired?.Invoke();
                    Expired = true;
                    UnityEngine.PlayerPrefs.DeleteKey(SavedTime);
                    _timePlaceholder.gameObject.SetActive(false);
                }
            }
        }

        internal void Disable()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
                _timePlaceholder.gameObject.SetActive(false);
            }
        }

        private async Task SetRemoteConfigs()
        {
            _smsDelaySeconds = await RemoteConfig.IntRemoteConfig(SmsDelay, SmsDelayDefaultTime);
            _codeLifespan = await RemoteConfig.IntRemoteConfig(CodeLifespan, CodeLifespanDefaultTime);
        }

        private string TimeString(int seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
            return $"{timeSpan.Minutes}:{timeSpan.Seconds:00}";
        }
    }
}
