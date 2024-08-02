using UnityEngine;
using UnityEngine.UI;

namespace Agava.Wink
{
    internal class DeleteAccountButton : MonoBehaviour
    {
        [SerializeField] private Button _button;

        private WinkSignInHandlerUI _winkSignInHandlerUI;
        private WinkAccessManager _winkAccessManager;

        private void Start()
        {
            _winkSignInHandlerUI = WinkSignInHandlerUI.Instance;
            _winkAccessManager = WinkAccessManager.Instance;
            UpdateButton();
        }

        private void Update()
        {
            UpdateButton();
        }

        private void OnEnable() => _button.onClick.AddListener(OnButtonClick);

        private void OnDisable() => _button.onClick.RemoveListener(OnButtonClick);

        private void OnButtonClick() => _winkSignInHandlerUI.OnDeleteAccountButtonClick();

        private void UpdateButton()
        {
            if (PreloadService.Instance == null)
            {
                _button.gameObject.SetActive(false);
                return;
            }

            if (PreloadService.Instance.IsPluginAwailable == false)
                _button.gameObject.SetActive(false);
            else
                _button.gameObject.SetActive(_winkSignInHandlerUI != null && (_winkAccessManager == null ? false : _winkAccessManager.Authenficated));
        }
    }
}
