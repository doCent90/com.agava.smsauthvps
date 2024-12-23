using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    [Preserve]
    internal class EnterCodeShaking : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private int _amplitude;
        [SerializeField] private float _duration = 1.0f;

        private Coroutine _coroutine;

        public bool Shaking => _coroutine != null;

        internal void StartAnimation()
        {
            float y = _rectTransform.anchoredPosition.y;
            _coroutine = StartCoroutine(Shaking());

            IEnumerator Shaking()
            {
                float cycleDuration = _duration / 4;

                yield return Move(_amplitude, cycleDuration);
                yield return Move(-_amplitude, cycleDuration);
                yield return Move(-_amplitude, cycleDuration);
                yield return Move(_amplitude, cycleDuration);

                IEnumerator Move(float moveX, float duration)
                {
                    float delta = moveX / duration;
                    float distance = 0.0f;
                    WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

                    while (Mathf.Abs(distance) < Mathf.Abs(moveX))
                    {
                        distance += delta * Time.deltaTime;
                        float x = _rectTransform.anchoredPosition.x + delta * Time.deltaTime;
                        _rectTransform.anchoredPosition = new Vector2(x, y);
                        yield return waitForEndOfFrame;
                    }

                    _coroutine = null;
                }
            }
        }

        internal void Dispose()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }
    }
}
