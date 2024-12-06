using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace Agava.Wink
{
    [Preserve]
    internal class WinkProfileWindow : WindowPresenter
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private ImagesCarousel _imagesCarousel;
        [SerializeField] private Button _profileButton;
        [SerializeField] private Button _closeButton;

        private void OnDestroy()
        {
            _closeButton?.onClick.RemoveAllListeners();
            _profileButton.onClick.RemoveAllListeners();
        }

        private void Awake()
        {
            _closeButton?.onClick.AddListener(Disable);
            _profileButton.onClick.AddListener(OnProfileButtonClicked);
        }

        public override void Enable()
        {
            _imagesCarousel.Enable();
            EnableCanvasGroup(_canvasGroup);
        }

        public override void Disable()
        {
            DisableCanvasGroup(_canvasGroup);
            _imagesCarousel.Disable();
        }

        private void OnProfileButtonClicked()
        {
            Application.OpenURL(Links.Subscription);
            Disable();
        }
    }
}
