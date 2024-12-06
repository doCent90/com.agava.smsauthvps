using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace Agava.Wink
{
    [Preserve]
    internal class RedirectWindowPresenter : WindowPresenter
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private ImagesCarousel _imagesCarousel;
        [SerializeField] private Button _yesButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _signInButton;
        [SerializeField] private bool _closeOnYesClicked = true;

        private void Awake()
        {
            _closeButton?.onClick.AddListener(Disable);
            _yesButton.onClick.AddListener(OnYesClicked);
        }

        private void OnDestroy()
        {
            _closeButton?.onClick.RemoveAllListeners();
            _yesButton.onClick.RemoveAllListeners();
        }

        public void Enable(bool closeButton)
        {
            if (_closeButton != null)
                _closeButton.gameObject.SetActive(closeButton);

            _imagesCarousel.Enable();
            EnableCanvasGroup(_canvasGroup);
        }

        public override void Enable() => Enable(true);

        public override void Disable()
        {
            DisableCanvasGroup(_canvasGroup);
            _imagesCarousel.Disable();
        }

        private void OnYesClicked()
        {
            Application.OpenURL(Links.Subscription);
            AnalyticsWinkService.SendPayWallRedirect();

            if (_closeOnYesClicked)
                Disable();
        }
    }
}
