using System.Collections;
using UnityEngine;
using SmsAuthAPI.Program;

namespace Agava.Wink
{
    internal class TimespentService
    {
        private readonly ICoroutine _coroutine;
        private Coroutine _current;

        private readonly string _phone;
        private readonly string _deviceId;
        private readonly string _appId;

        private int _spentTimeMin = 0;

        public TimespentService(ICoroutine coroutine, string phone, string deviceId, string appId)
        {
            _coroutine = coroutine;
            _phone = phone;
            _deviceId = deviceId;
            _appId = appId;
        }

        internal void OnStartedApp()
        {
            _current = _coroutine.StartCoroutine(Ticking());
            IEnumerator Ticking()
            {
                var wait = new WaitForSecondsRealtime(60f);

                while (true)
                {
                    yield return wait;
                    _spentTimeMin += 1;
                }
            }
        }

        internal void OnFinishedApp()
        {
            if (_current != null)
            {
                _coroutine.StopCoroutine(_current);
                _current = null;
                SmsAuthApi.SetTimespentAllApp(_phone, _deviceId, _spentTimeMin);
                SmsAuthApi.SetTimespentAllUsers(_appId, _spentTimeMin);
                _spentTimeMin = 0;
            }
        }
    }
}
