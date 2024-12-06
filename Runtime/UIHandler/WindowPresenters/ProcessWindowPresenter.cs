using UnityEngine;
using UnityEngine.UI;

namespace Agava.Wink
{
    internal class ProcessWindowPresenter : WindowPresenter
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _loadingImage;

        private void Update()
        {
            if (Enabled)
            {
                _loadingImage.transform.localEulerAngles += new Vector3(0, 0, 2f);
            }
        }

        public override void Enable() => EnableCanvasGroup(_canvasGroup);

        public override void Disable() => DisableCanvasGroup(_canvasGroup);
    }
}
