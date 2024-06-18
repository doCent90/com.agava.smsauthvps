using System;
using System.Collections;
using UnityEngine;

namespace Agava.Wink
{
    internal class TextTimer : MonoBehaviour
    {
        [SerializeField] private int _time;
        [SerializeField] private TextPlaceholder _timePlaceholder;

        private int _seconds;
        private Coroutine _coroutine;

        public event Action TimerExpired;

        internal void Enable()
        {
            _timePlaceholder.gameObject.SetActive(true);
            _seconds = _time;
            _coroutine = StartCoroutine(Ticking());

            IEnumerator Ticking()
            {
                var tick = new WaitForSecondsRealtime(1);

                while (_seconds > 0)
                {
                    _seconds--;
                    _timePlaceholder.ReplaceValue(_seconds.ToString());

                    yield return tick;
                }

                if (_seconds <= 0)
                    TimerExpired?.Invoke();
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
    }
}
