using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;
using SmsAuthAPI.Program;

namespace Agava.Wink
{
    [Preserve]
    public class PreloadService
    {
        private const string True = "true";
#if UNITY_STANDALONE
        private const string RemoteName = "-standalone";
#elif UNITY_ANDROID
        private const string RemoteName = "-android";
#elif UNITY_IOS
        private const string RemoteName = "-ios";
#endif
        private bool _isEndPrepare = false;
        private readonly WinkSignInHandlerUI _winkSignInHandlerUI;

        public PreloadService(WinkSignInHandlerUI winkSignInHandlerUI)
        {
            Instance ??= this;
            _winkSignInHandlerUI = winkSignInHandlerUI;
        }

        public static PreloadService Instance { get; private set; }
        public bool IsPluginAwailable { get; private set; } = false;

        public IEnumerator Preparing()
        {
            yield return _winkSignInHandlerUI.Initialize();
            yield return new WaitUntil(() => SmsAuthApi.Initialized);
            yield return null;

            SetPluginAwailable();
            yield return new WaitUntil(() => _isEndPrepare);
#if UNITY_EDITOR || TEST
            Debug.Log("Prepare is done. Start plugin " + IsPluginAwailable);
#endif
        }

        private async void SetPluginAwailable()
        {
            var response = await SmsAuthApi.GetRemoteConfig(Application.identifier + RemoteName);

            if (response.statusCode == UnityWebRequest.Result.Success)
            {
                if (string.IsNullOrEmpty(response.body))
                    IsPluginAwailable = false;
                else if (response.body == True)
                    IsPluginAwailable = true;
                else
                    IsPluginAwailable = false;
            }
            else
            {
                IsPluginAwailable = false;
                Debug.LogError($"Fail to recieve remote config '{RemoteName}': " + response.statusCode);
            }

            _isEndPrepare = true;
        }
    }
}
