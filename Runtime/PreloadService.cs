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
#if UNITY_STANDALONE
        private const string RemoteName = "skip-auth-standalone";
#elif UNITY_ANDROID
        private const string RemoteName = "skip-auth-android";
#elif UNITY_IOS
        private const string RemoteName = "skip-auth-ios";
#endif
        private bool _isEndPrepare = false;

        public static PreloadService Instance { get; private set; }
        public bool IsPluginAwailable { get; private set; } = false;

        public PreloadService() => Instance ??= this;

        public IEnumerator Preparing()
        {
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
            var response = await SmsAuthApi.GetRemoteConfig(RemoteName);

            if (response.statusCode == UnityWebRequest.Result.Success)
            {
                if (string.IsNullOrEmpty(response.body))
                    IsPluginAwailable = false;
                else if (response.body == "true")
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
