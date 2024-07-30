using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    [Preserve]
    public interface IWinkAccessManager
    {
        bool HasAccess { get; }

        event Action AuthorizationSuccessfully;
        event Action ResetLogin;
        event Action<IReadOnlyList<string>> LimitReached;
        event Action AccountDeleted;
    }
}
