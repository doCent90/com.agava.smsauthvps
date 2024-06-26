using UnityEngine.Scripting;

namespace Agava.Wink
{
    [Preserve]
    public enum WindowType
    {
        None,
        SignIn,
        Fail,
        WrongNumber,
        ProccessOn,
        Successfully,
        Unlink,
        DemoTimerExpired,
        NoEnternet,
        Redirect,
        EnterOtpCode,
        Hello,
        WinkProfile,
    }
}
