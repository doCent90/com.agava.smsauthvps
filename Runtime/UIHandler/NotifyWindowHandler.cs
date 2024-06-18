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
        [SerializeField] private NotifyWindowPresenter _unlinkWindow;
        [SerializeField] private RedirectWindowPresenter _demoTimerExpiredWindow;
        [SerializeField] private NotifyWindowPresenter _noEnternetWindow;
        [SerializeField] private RedirectWindowPresenter _redirectToWebsiteWindow;
        [SerializeField] private InputWindowPresenter _enterCodeWindow;
        [SerializeField] private WinkProfileWindow _winkProfileWindow;
        [Header("All UI Windows")]
        [SerializeField] private List<WindowPresenter> _windows;

        public bool IsAnyWindowEnabled => _windows.Any(window => window.HasOpened);

        internal void OpenSignInWindow(Action closeCallback = null) => _signInWindow.Enable(closeCallback);
        internal void OpenWindow(WindowType type) => GetWindowByType(type).Enable();
        internal void CloseWindow(WindowType type) => GetWindowByType(type).Disable();
        internal void OpenInputWindow(Action<string> onInputDone) => _enterCodeWindow.Enable(onInputDone);
        internal void OpenHelloWindow(Action onEnd) => _helloWindow.Enable(onEnd);
        internal void OpenDemoExpiredWindow(bool closeButton) => _demoTimerExpiredWindow.Enable(closeButton);

        internal void ResetInputWindow() => _enterCodeWindow.ResetInputText();

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
