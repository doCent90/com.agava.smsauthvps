using UnityEngine;

namespace Agava.Wink
{
    internal class LoadingProgressBar : MonoBehaviour
    {
        [SerializeField] private Utils.SlicedFilledImage _progressBar;
        [SerializeField] private CanvasGroup _canvasGroup;

        private const float Max = 1.0f;
        private const float Min = 0.0f;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        internal void Enable()
        {
            _canvasGroup.alpha = 1.0f;
            SetProgress(0.0f);
        }

        internal void SetProgress(float value, float min = 0.0f, float max = 1.0f)
        {
            _progressBar.fillAmount = (max - min) * (value - Max) + max;
        }

        internal void Disable()
        {
            _canvasGroup.alpha = 0;
        }
    }
}
