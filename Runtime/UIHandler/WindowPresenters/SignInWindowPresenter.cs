using System;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace Agava.Wink
{
    [Preserve]
    internal class SignInWindowPresenter : WindowPresenter
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TMP_InputField _inputField;

        private Action Closed;

        private void OnDestroy() => _closeButton.onClick.RemoveListener(Disable);

        private void Awake() => _closeButton.onClick.AddListener(Disable);

        public override void Enable() => EnableCanvasGroup(_canvasGroup);

        private void Update()
        {
            if (Enabled == false)
                return;
        }

        public void Enable(Action closeCallback)
        {
            Enable();
            Closed = closeCallback;
            _inputField.Select();
        }

        public override void Disable()
        {
            DisableCanvasGroup(_canvasGroup);
            Closed?.Invoke();
        }
    }
}
