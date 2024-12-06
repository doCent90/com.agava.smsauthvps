using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace Agava.Wink
{
    [Preserve]
    internal class WinkButton : MonoBehaviour
    {
        [SerializeField] private Button _button;

        private WinkSignInHandlerUI _winkSignInHandlerUI;

        private void Start()
        {
            _winkSignInHandlerUI = WinkSignInHandlerUI.Instance;
            UpdateButtons();
        }

        private void Update()
        {
            UpdateButtons();
        }

        private void OnEnable() => _button.onClick.AddListener(OnButtonClick);

        private void OnDisable() => _button.onClick.RemoveListener(OnButtonClick);

        private void OnButtonClick() => _winkSignInHandlerUI.OnWinkButtonClick();

        private void UpdateButtons()
        {
            if (PreloadService.Instance == null)
            {
                _button.gameObject.SetActive(false);
                return;
            }

            if (PreloadService.Instance.IsPluginAwailable == false)
                _button.gameObject.SetActive(false);
            else
                _button.gameObject.SetActive(_winkSignInHandlerUI != null);
        }
    }
}
