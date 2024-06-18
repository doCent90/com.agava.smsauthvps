using UnityEngine;
using UnityEngine.UI;

namespace Agava.Wink
{
    internal class WinkProfileWindow : WindowPresenter
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _profileButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private string _url;

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

        public override void Enable() => EnableCanvasGroup(_canvasGroup);

        public override void Disable() => DisableCanvasGroup(_canvasGroup);

        private void OnProfileButtonClicked()
        {
            Application.OpenURL(_url);
            AnalyticsWinkService.SendSubscribeProfileRemote();
            Disable();
        }
    }
}
