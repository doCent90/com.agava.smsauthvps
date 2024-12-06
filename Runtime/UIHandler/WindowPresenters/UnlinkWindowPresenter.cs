using UnityEngine;
using UnityEngine.UI;

namespace Agava.Wink
{
    internal class UnlinkWindowPresenter : WindowPresenter
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [Header("First panel")]
        [SerializeField] private GameObject _firstPanel;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _unlinkButton;
        [Header("Second panel buttons")]
        [SerializeField] private GameObject _secondPanel;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private UnlinkDeviceViewContainer _unlinkDeviceViewsContainer;

        private void Awake()
        {
            _closeButton?.onClick.AddListener(Disable);
            _unlinkButton?.onClick.AddListener(OnUnlinkButtonClicked);
            _backButton?.onClick.AddListener(OnBackButtonClicked);
            _continueButton?.onClick.AddListener(OnContinueButtonClicked);
        }

        private void Update()
        {
            bool deviceUnlinked = _unlinkDeviceViewsContainer.HasFreePlaces;

            _backButton.gameObject.SetActive(deviceUnlinked == false);
            _continueButton.gameObject.SetActive(deviceUnlinked);
        }

        private void OnDestroy()
        {
            _closeButton?.onClick.RemoveListener(Disable);
            _unlinkButton?.onClick.RemoveListener(OnUnlinkButtonClicked);
            _backButton?.onClick.RemoveListener(OnBackButtonClicked);
            _continueButton?.onClick.RemoveListener(OnContinueButtonClicked);
        }

        public override void Enable()
        {
            SetPanel(true);
            EnableCanvasGroup(_canvasGroup);
        }

        public override void Disable()
        {
            DisableCanvasGroup(_canvasGroup);
            _unlinkDeviceViewsContainer.Clear();
        }

        private void OnUnlinkButtonClicked() => SetPanel(false);
        private void OnBackButtonClicked() => SetPanel(true);
        private void OnContinueButtonClicked() => Debug.Log("r");

        private void SetPanel(bool first)
        {
            _firstPanel.SetActive(first);
            _secondPanel.SetActive(first == false);
        }
    }
}
