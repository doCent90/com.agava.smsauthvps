using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    [Serializable, Preserve]
    internal class NotifyWindowHandler
    {
        [Header("UI Windows")]
        [SerializeField] private SignInWindowPresenter _signInWindow;
        [SerializeField] private NotifyWindowPresenter _failWindow;
        [SerializeField] private ProcessWindowPresenter _proccesOnWindow;
        [SerializeField] private HelloWindowPresenter _helloWindow;
        [SerializeField] private UnlinkWindowPresenter _unlinkWindow;
        [SerializeField] private RedirectWindowPresenter _demoTimerExpiredWindow;
        [SerializeField] private NotifyWindowPresenter _noEnternetWindow;
        [SerializeField] private RedirectWindowPresenter _redirectToWebsiteWindow;
        [SerializeField] private InputWindowPresenter _enterCodeWindow;
        [SerializeField] private WinkProfileWindow _winkProfileWindow;
        [SerializeField] private DeleteAccountWindowPresenter _deleteAccountWindow;
        [Header("All UI Windows")]
        [SerializeField] private List<WindowPresenter> _windows;

        public bool IsAnyWindowEnabled => _windows.Any(window => window.Enabled);
        public bool ZeroSecondsCodeTimer => _enterCodeWindow.ZeroSeconds;
        public bool EnterCodeWindowInitialized => _enterCodeWindow.Initialized;

        internal void OpenSignInWindow(Action closeCallback = null) => _signInWindow.Enable(closeCallback);
        internal void OpenWindow(WindowType type) => GetWindowByType(type).Enable();
        internal void CloseWindow(WindowType type) => GetWindowByType(type).Disable();
        internal void OpenInputOtpCodeWindow(string phone, Action<string> onInputDone = null, Action onBackClicked = null)
        {
            _enterCodeWindow.Enable(phone, onInputDone, onBackClicked);
            _signInWindow.Clear();
        }
        internal void OpenDemoExpiredWindow(bool closeButton) => _demoTimerExpiredWindow.Enable(closeButton);
        internal void OpenDeleteAccountWindow(Action onDeleteAccount) => _deleteAccountWindow.Enable(onDeleteAccount);

        internal void Response(bool accepted) => _enterCodeWindow.Response(accepted);

        internal void CloseAllWindows(Action onClosed)
        {
            _windows.ForEach(window => window.Disable());
            onClosed?.Invoke();
        }

        internal bool HasOpenedWindow(WindowType type)
            => _windows.Any(window => window.Type == type && window.isActiveAndEnabled == true);

        private WindowPresenter GetWindowByType(WindowType type)
            => _windows.FirstOrDefault(window => window.Type == type);
    }
}
