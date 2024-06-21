using UnityEngine;
using UnityEngine.UI;

namespace Agava.Wink
{
    internal class RedirectWindowPresenter : WindowPresenter
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _yesButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private string _url;
        [SerializeField] private bool _closeOnYesClicked = true;

        private void OnDestroy()
        {
            _closeButton?.onClick.RemoveAllListeners();
            _yesButton.onClick.RemoveAllListeners();
        }

        private void Awake()
        {
            _closeButton?.onClick.AddListener(Disable);
            _yesButton.onClick.AddListener(OnYesClicked);
        }

        public void Enable(bool closeButton)
        {
            _closeButton.gameObject.SetActive(closeButton);
            EnableCanvasGroup(_canvasGroup);
        }

        public override void Enable() => Enable(true);

        public override void Disable() => DisableCanvasGroup(_canvasGroup);

        private void OnYesClicked()
        {
            Application.OpenURL(_url);
            AnalyticsWinkService.SendPayWallRedirect();

            if (_closeOnYesClicked)
                Disable();
        }
    }
}
