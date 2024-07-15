using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Scripting;
using UnityEngine.Networking;
using SmsAuthAPI.Program;

namespace Agava.Wink
{
    [Preserve]
    public class SubscriptionSearchSystem
    {
#if UNITY_EDITOR || TEST
        private const int CoolDown = 10000;
#else
        private const int CoolDown = 120000;
#endif
        private readonly string _phone;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private Action OnSubcriptionExist;

        public SubscriptionSearchSystem(string phone)
        {
            _phone = phone;
            _cancellationTokenSource = new();
        }

        public async void StartSearching(Action onSubscriptionExist)
        {
            OnSubcriptionExist = onSubscriptionExist;

            CancellationToken token = _cancellationTokenSource.Token;

            while (token.IsCancellationRequested == false)
            {
                await Task.Yield();

                if (token.IsCancellationRequested == false)
                {
                    await Task.Delay(CoolDown, token);
                    var response = await SmsAuthApi.HasActiveAccount(_phone);

                    if (response.statusCode == UnityWebRequest.Result.Success
                        && WinkAccessManager.Instance.HasAccess == false)
                    {
                        OnSubcriptionExist?.Invoke();
                        _cancellationTokenSource.Cancel();
                    }
                }
            }
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            OnSubcriptionExist = null;
        }
    }
}