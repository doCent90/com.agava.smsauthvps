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
        [SerializeField] private Button _yesButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _signInButton;
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

        private void Update()
        {
            if (_signInButton != null)
            {
                bool authenticated = WinkAccessManager.Instance == null ? false : WinkAccessManager.Instance.Authenficated;
                _signInButton.gameObject.SetActive(authenticated == false);
                _yesButton.gameObject.SetActive(authenticated);
            }
        }

        public void Enable(bool closeButton)
        {
            if (_closeButton != null)
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
