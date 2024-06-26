using System;
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
        [SerializeField] private WindowScalerPresenter _scalerPresenter;

        private Action Closed;

        private void OnDestroy() => _closeButton.onClick.RemoveListener(Disable);

        private void Awake()
        {
            _closeButton.onClick.AddListener(Disable);
            _scalerPresenter.Construct();
        }

        public override void Enable() => EnableCanvasGroup(_canvasGroup);

        private void Update()
        {
            if (HasOpened == false)
                return;

            _scalerPresenter.Update();
        }

        public void Enable(Action closeCallback)
        {            
            Enable();
            Closed = closeCallback;
        }

        public override void Disable()
        {
            DisableCanvasGroup(_canvasGroup);
            Closed?.Invoke();
        }
    }
}
