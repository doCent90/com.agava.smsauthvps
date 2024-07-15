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

        private void Start() => _winkSignInHandlerUI = WinkSignInHandlerUI.Instance;

        private void Update()
        {
            if (PreloadService.Instance == null)
                return;

            if (PreloadService.Instance.IsPluginAwailable == false)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(_winkSignInHandlerUI != null);
        }

        private void OnEnable() => _button.onClick.AddListener(OnButtonClick);

        private void OnDisable() => _button.onClick.RemoveListener(OnButtonClick);

        private void OnButtonClick() => _winkSignInHandlerUI.OnWinkButtonClick();
    }
}
