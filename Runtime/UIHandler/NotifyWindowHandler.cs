using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Agava.Wink
{
    [Serializable]
    internal class NotifyWindowHandler
    {
        [Header("UI Windows")]
        [SerializeField] private SignInWindowPresenter _signInWindow;
        [SerializeField] private NotifyWindowPresenter _failWindow;
        [SerializeField] private NotifyWindowPresenter _wrongNumberWindow;
        [SerializeField] private NotifyWindowPresenter _proccesOnWindow;
        [SerializeField] private NotifyWindowPresenter _successfullyWindow;
        [SerializeField] private HelloWindowPresenter _helloWindow;
        [SerializeField] private HelloSubscribeWindowPresenter _helloSubscribeWindow;
        [SerializeField] private NotifyWindowPresenter _unlinkWindow;
        [SerializeField] private NotifyWindowPresenter _demoTimerExpiredWindow;
        [SerializeField] private NotifyWindowPresenter _noEnternetWindow;
        [SerializeField] private RedirectWindowPresenter _redirectToWebsiteWindow;
        [SerializeField] private InputWindowPresenter _enterCodeWindow;
        [Header("All UI Windows")]
        [SerializeField] private List<WindowPresenter> _windows;

        public bool IsAnyWindowEnabled => _windows.Any(window => window.HasOpened);

        internal void OpenSignInWindow(Action closeCallback = null) => _signInWindow.Enable(closeCallback);
        internal void OpenWindow(WindowType type) => GetWindowByType(type).Enable();
        internal void CloseWindow(WindowType type) => GetWindowByType(type).Disable();
        internal void OpenInputWindow(Action<uint> onInputDone) => _enterCodeWindow.Enable(onInputDone);
        internal void OpenHelloWindow(Action onEnd) => _helloWindow.Enable(onEnd);
        internal void OpenHelloSubscribeWindow(Action onClose) => _helloWindow.Enable(onClose);

        internal void CloseAllWindows(Action onClosed)
        {
            _windows.ForEach(window => window.Disable());
            onClosed?.Invoke();
        }

        internal void OnLimitReached()
        {
            _enterCodeWindow.Clear();
            _unlinkWindow.Enable();
        }

        internal bool HasOpenedWindow(WindowType type)
            => _windows.Any(window => window.Type == type && window.isActiveAndEnabled == true);

        private WindowPresenter GetWindowByType(WindowType type)
            => _windows.FirstOrDefault(window => window.Type == type);
    }
}
