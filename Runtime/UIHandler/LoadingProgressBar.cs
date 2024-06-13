using UnityEngine;

namespace Agava.Wink
{
    internal class LoadingProgressBar : MonoBehaviour
    {
        [SerializeField] private Utils.SlicedFilledImage _progressBar;

        internal void Enable()
        {
            gameObject.SetActive(true);
            SetProgress(0.0f);
        }

        internal void SetProgress(float value)
        {
            _progressBar.fillAmount = value;
        }

        internal void Disable()
        {
            gameObject.SetActive(false);
        }
    }
}
