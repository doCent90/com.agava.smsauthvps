using System.Threading.Tasks;
using SmsAuthAPI.Program;
using UnityEngine;
using UnityEngine.Networking;

namespace Agava.Wink
{
    public static class RemoteConfig
    {
        public static async Task<int> IntRemoteConfig(string configName, int defaultValue)
        {
            var response = await SmsAuthApi.GetRemoteConfig(configName);

            if (response.statusCode == UnityWebRequest.Result.Success)
            {
                if (string.IsNullOrEmpty(response.body) == false)
                {
                    return ParseIntConfig(response.body, defaultValue);
                }
                else
                {
                    Debug.LogError($"Fail to recieve remote config '{configName}': value is NULL");
                }
            }
            else
            {
                Debug.LogError($"Fail to recieve remote config '{configName}': BAD REQUEST");
            }

            return defaultValue;
        }

        private static int ParseIntConfig(string timeStr, int defaultValue)
        {
            bool success = int.TryParse(timeStr, out int time);
            return success ? time : defaultValue;
        }

        public static async Task<string> StringRemoteConfig(string configName, string defaultValue)
        {
            var response = await SmsAuthApi.GetRemoteConfig(configName);

            if (response.statusCode == UnityWebRequest.Result.Success)
            {
                if (string.IsNullOrEmpty(response.body) == false)
                {
                    return response.body;
                }
                else
                {
                    Debug.LogError($"Fail to recieve remote config '{configName}': value is NULL");
                }
            }
            else
            {
                Debug.LogError($"Fail to recieve remote config '{configName}': BAD REQUEST");
            }

            return defaultValue;
        }
    }
}
