using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace Agava.Wink
{
    internal class HelloSubscribeWindowPresenter : WindowPresenter
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _subscribeButton;
        [SerializeField] private Button _closeButton;

        Action _onClose;

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveListener(Disable);
        }

        private void Awake()
        {
            _closeButton.onClick.AddListener(Disable);
        }

        public void Enable(Action onClose = null)
        {
            _onClose = onClose;
            EnableCanvasGroup(_canvasGroup);
        }

        public override void Enable() { }

        public override void Disable()
        {
            _onClose?.Invoke();
            DisableCanvasGroup(_canvasGroup);
        }
    }
}
