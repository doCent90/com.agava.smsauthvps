using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    [Preserve]
    internal class LoadingProgressBar : MonoBehaviour
    {
        [SerializeField] private Utils.SlicedFilledImage _progressBar;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField, Tooltip("Passive progress per second")] private float _passiveProgress;

        private const float Max = 1.0f;
        private const float Min = 0.0f;

        private Coroutine _coroutine;
        private float _progress;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        internal void Enable()
        {
            _canvasGroup.alpha = 1.0f;
            _progress = 0.0f;
            SetProgress(_progress);

            StartCoroutine(Loading());

            IEnumerator Loading()
            {
                WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

                SetProgress(Mathf.Lerp(_progress, Max, Time.deltaTime * _passiveProgress));

                yield return waitForEndOfFrame;
            }
        }

        internal void SetProgress(float value, float min = 0.0f, float max = 1.0f)
        {
            float normalizedProgress = (max - min) * (value - Max) + max;

            if (_progress >= normalizedProgress)
                return;

            _progress = normalizedProgress;
            _progressBar.fillAmount = _progress;
        }

        internal void Disable()
        {
            _canvasGroup.alpha = 0;

            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }
    }
}
