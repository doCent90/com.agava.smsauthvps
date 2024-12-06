using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace Agava.Wink
{
    [Preserve]
    internal class HelloWindowPresenter : WindowPresenter
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private ImagesCarousel _imagesCarousel;
        [SerializeField] private Button _startButton;

        public override void Enable()
        {
            _imagesCarousel.Enable();
            _startButton.onClick.AddListener(OnStartButtonClick);
            EnableCanvasGroup(_canvasGroup);
        }

        public override void Disable()
        {
            DisableCanvasGroup(_canvasGroup);
            _imagesCarousel.Disable();
            _startButton.onClick.RemoveListener(OnStartButtonClick);
        }

        private void OnStartButtonClick()
        {
            Disable();
        }
    }
}
