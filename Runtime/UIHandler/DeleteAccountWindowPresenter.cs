using System;
using UnityEngine;
using UnityEngine.UI;

namespace Agava.Wink
{
    internal class DeleteAccountWindowPresenter : WindowPresenter
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _okButton;
        [SerializeField] private Button _cancelButton;

        private Action _onDeleteAccount;

        private void Awake()
        {
            _okButton.onClick.AddListener(OnOkButtonClick);
            _cancelButton.onClick.AddListener(OnCancelButtonClick);
        }

        private void OnDestroy()
        {
            _okButton.onClick.RemoveListener(OnOkButtonClick);
            _cancelButton.onClick.RemoveListener(OnCancelButtonClick);
        }

        public override void Disable()
        {
            DisableCanvasGroup(_canvasGroup);
        }

        public override void Enable() { }

        public void Enable(Action onDeleteAccount)
        {
            EnableCanvasGroup(_canvasGroup);
            _onDeleteAccount = onDeleteAccount;
        }

        private void OnOkButtonClick()
        {
            _onDeleteAccount?.Invoke();
            Disable();
        }

        private void OnCancelButtonClick()
        {
            Disable();
        }
    }
}
