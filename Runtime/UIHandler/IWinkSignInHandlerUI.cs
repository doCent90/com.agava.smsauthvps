using System;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    [Preserve]
    public interface IWinkSignInHandlerUI
    {
        bool IsAnyWindowEnabled { get; }

        event Action AllWindowsClosed;

        void OpenSignWindow();
        void OpenWindow(WindowType type);
        void CloseAllWindows();
    }
}
