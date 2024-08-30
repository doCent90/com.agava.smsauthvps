using System;
using System.Collections;
using SmsAuthAPI.DTO;
using SmsAuthAPI.Program;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    [Preserve]
    internal class TextTimer : MonoBehaviour
    {
        private const string RemoteName = "sms-delay-seconds";
        private const string SavedTime = nameof(SavedTime);
        private const int DefaultTime = 190;
        private const int AdditiveTime = 10;

        [SerializeField] private TextPlaceholder _timePlaceholder;

        private int _seconds = 0;
        private Coroutine _coroutine;

        public event Action TimerExpired;
        public bool Expired = true;

        private IEnumerator Start()
        {
            if (_seconds <= 0)
                _seconds = DefaultTime;

            yield return new WaitUntil(() => SmsAuthApi.Initialized);
            SetRemoteConfig();
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
                    _timePlaceholder.ReplaceValue(sec.ToString());
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

        private async void SetRemoteConfig()
        {
            var response = await SmsAuthApi.GetRemoteConfig(RemoteName);

            if (response.statusCode == UnityWebRequest.Result.Success)
            {
                if (string.IsNullOrEmpty(response.body))
                {
                    _seconds = DefaultTime;
                    Debug.LogError($"Fail to recieve remote config '{RemoteName}': value is NULL");
                }
                else
                {
                    SetTime(response.body);
                }
            }
            else
            {
                Debug.LogError($"Fail to recieve remote config '{RemoteName}': BAD REQUEST");
            }
        }

        private void SetTime(string timeStr)
        {
            bool success = int.TryParse(timeStr, out int time);

            if (success)
                _seconds = time + AdditiveTime;
            else
                _seconds = DefaultTime;
        }
    }
}
