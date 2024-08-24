using System.Collections;
using UnityEngine;
using SmsAuthAPI.Program;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Scripting;
using System;

namespace Agava.Wink
{
    [Preserve]
    internal class TimespentService
    {
        private const string SavedTime = nameof(SavedTime);

        private readonly ICoroutine _coroutine;
        private Coroutine _current;
        private readonly string _phone;
        private readonly string _deviceId;
        private readonly string _appId;

        private ulong _spentTimeMin = 0;
        private ulong _spentTimeSec = 0;
        private List<int> _savedTime = new();

        public TimespentService(ICoroutine coroutine, string phone, string deviceId, string appId)
        {
            _coroutine = coroutine;
            _phone = phone;
            _deviceId = deviceId;
            _appId = appId;

            if (PlayerPrefs.HasKey(SavedTime))
                _savedTime = JsonConvert.DeserializeObject<List<int>>(PlayerPrefs.GetString(SavedTime));
        }

        internal void OnStartedApp()
        {
            _current ??= _coroutine.StartCoroutine(Ticking());

            IEnumerator Ticking()
            {
                var wait = new WaitForSecondsRealtime(1f);
                int currentSec = 0;

                while (true)
                {
                    yield return wait;
                    _spentTimeSec += 1;
                    currentSec += 1;

                    if (currentSec >= 60f)
                    {
                        _spentTimeMin += 1;
                        currentSec = 0;
                        SendTimeAnalytics();
                        _spentTimeSec = 0;
                    }
                }
            }
        }

        internal void OnAppFocusFalse()
        {
            if (_current != null && _spentTimeSec != 0)
            {
                _coroutine.StopCoroutine(_current);
                _current = null;
            }
        }

        private void SendTimeAnalytics()
        {
            SmsAuthApi.SetTimespentAllApp(_phone, _appId, _spentTimeSec);
            SmsAuthApi.SetTimespentAllUsers(_appId, _spentTimeMin);
            SetAverageSessionTimespent(_spentTimeMin);
        }


        private void SetAverageSessionTimespent(ulong minutes)
        {
            _savedTime.Add((int)minutes);

            if (_savedTime.Count > 3)
            {
                double averageTime = _savedTime.Average();
                AnalyticsWinkService.SendAverageSessionLength((int)averageTime);

                if (PlayerPrefs.HasKey(SavedTime) == false)
                {
                    var json = JsonConvert.SerializeObject(_savedTime);
                    PlayerPrefs.SetString(SavedTime, json);
                }
            }
        }
    }
}
