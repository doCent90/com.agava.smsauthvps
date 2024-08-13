using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using SmsAuthAPI.DTO;
using SmsAuthAPI.Utility;
using SmsAuthAPI.Program;
using UnityEngine.Networking;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    /// <summary>
    ///    Requests/response to/from VPS server.
    /// </summary>
    [Preserve]
    internal class RequestHandler
    {
        private const string UnlinkProcess = nameof(UnlinkProcess);

        internal async Task<LoginData> Regist(string phoneNumber, string uniqueId, string appId, Action<bool> otpCodeRequest)
        {
            LoginData data = new()
            {
                phone = phoneNumber,
                otp_code = "0",
                device_id = uniqueId,
                app_id = appId,
            };

            Response response = await SmsAuthApi.Regist(phoneNumber);

            if (response.statusCode != UnityWebRequest.Result.Success)
            {
                otpCodeRequest?.Invoke(false);
                Debug.LogError("Regist Error : " + response.statusCode);
                return null;
            }
            else
            {
                otpCodeRequest?.Invoke(true);
                return data;
            }
        }

        internal async void Unlink(UnlinkData unlinkData, Action onUnlinkDevice)
        {
            Debug.Log($"deviceId: {unlinkData.device_id}, appId: {unlinkData.app_id}");

            Tokens tokens = TokenLifeHelper.GetTokens();
            var response = await SmsAuthApi.Unlink(tokens.access, unlinkData);

            if (response.statusCode != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Unlink fail: " + response.statusCode);
                onUnlinkDevice?.Invoke();
            }
            else
            {
                UnityEngine.PlayerPrefs.DeleteKey(UnlinkProcess);
                onUnlinkDevice?.Invoke();
            }
        }

        internal async void Login(LoginData data, Action<IReadOnlyList<string>> onLimitReached,
            Action<bool> onWinkSubscriptionAccessRequest, Action<bool> otpCodeAccepted)
        {
            var response = await SmsAuthApi.Login(data);

            if (response.statusCode != UnityWebRequest.Result.Success)
            {
                Debug.LogError("ValidationError : " + response.statusCode);
                otpCodeAccepted?.Invoke(false);
            }
            else
            {
                otpCodeAccepted?.Invoke(true);
                string token;

                if (response.isBase64Encoded)
                {
                    byte[] bytes = Convert.FromBase64String(response.body);
                    token = Encoding.UTF8.GetString(bytes);
                }
                else
                {
                    token = response.body;
                }

                Tokens tokens = JsonConvert.DeserializeObject<Tokens>(token);
                SaveLoadLocalDataService.Save(tokens, TokenLifeHelper.Tokens);

                if (string.IsNullOrEmpty(tokens.refresh))
                {
                    UnityEngine.PlayerPrefs.SetString(UnlinkProcess, "true");
                    OnLimitDevicesReached(onLimitReached, data.app_id);
                    return;
                }

                await RequestWinkDataBase(data.phone, onWinkSubscriptionAccessRequest);
            }
        }

        internal async void QuickAccess(string phoneNumber, Action onResetLogin, Action<bool> onWinkSubscriptionAccessRequest, Action<bool> onSignInSuccessfully)
        {
            if (UnityEngine.PlayerPrefs.HasKey(UnlinkProcess))
            {
                Debug.Log("Unlinking process wasn't completed. Quick access is unavailable");
                SkipQuickAccess();
                UnityEngine.PlayerPrefs.DeleteKey(UnlinkProcess);
                return;
            }

            Tokens tokens = TokenLifeHelper.GetTokens();

            if (tokens == null)
            {
                Debug.LogError("Tokens don't exist. Quick access is unavailable");
                SkipQuickAccess();
                return;
            }

            string currentToken = string.Empty;

            if (TokenLifeHelper.IsTokenAlive(tokens.access))
            {
                currentToken = tokens.access;
            }
            else if (TokenLifeHelper.IsTokenAlive(tokens.refresh))
            {
                currentToken = await TokenLifeHelper.GetRefreshedToken(tokens.refresh);

                if (string.IsNullOrEmpty(currentToken))
                {
                    SkipQuickAccess();
                    return;
                }
            }
            else
            {
                SkipQuickAccess();
                return;
            }

            var response = await SmsAuthApi.SampleAuth(currentToken);
            var hasSubsc = await RequestWinkDataBase(phoneNumber, onWinkSubscriptionAccessRequest);

            if (response.statusCode == UnityWebRequest.Result.Success)
            {
                onSignInSuccessfully?.Invoke(hasSubsc);
            }
            else
            {
                Debug.LogError($"Quick access Validation Error: {response.reasonPhrase}: {response.statusCode}/Wink: {hasSubsc}");
                SkipQuickAccess();
            }

            void SkipQuickAccess()
            {
                TokenLifeHelper.ClearTokens();
                onResetLogin?.Invoke();
            }
        }

        internal async void DeleteAccount(Action onDeleteAccount)
        {
            Tokens tokens = TokenLifeHelper.GetTokens();
            Response response = await SmsAuthApi.DeleteAccount(tokens.access);

            if (response.statusCode != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Account deletion fail: " + response.statusCode);
            }
            else
            {
                onDeleteAccount?.Invoke();
            }
        }

        internal async void OnLimitDevicesReached(Action<IReadOnlyList<string>> onLimitReached, string app_id)
        {
            Tokens tokens = TokenLifeHelper.GetTokens();
            Response response = await SmsAuthApi.GetDevices(tokens.access, app_id);

            if (response.statusCode != UnityWebRequest.Result.Success)
            {
                Debug.Log("Fail get devices: " + response.statusCode);
            }
            else
            {
                IReadOnlyList<string> devices = JsonConvert.DeserializeObject<List<string>>(response.body);
                onLimitReached?.Invoke(devices);
            }
        }

        internal async void UnlinkDevices(string app_id, string device_id, Action onUnlink = null, Action onTokensNull = null, Action onFail = null)
        {
            Tokens tokens = TokenLifeHelper.GetTokens();

            if (tokens == null)
            {
                Debug.LogError("Unlinking fail: tokens do not exist");
                onTokensNull?.Invoke();
                return;
            }

            Response response = await SmsAuthApi.GetDevices(tokens.access, Application.identifier);

            if (response.statusCode != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Fail get devices: " + response.statusCode);
                onFail?.Invoke();
            }
            else
            {
                List<string> devices = JsonConvert.DeserializeObject<List<string>>(response.body);

                foreach (string device in devices)
                {
                    Debug.Log($"Unlinking device: {device}");

                    response = await SmsAuthApi.Unlink(tokens.access, new UnlinkData { device_id = device, app_id = app_id });

                    if (device == device_id)
                        onUnlink?.Invoke();

                    if (response.statusCode != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"Unlink fail for device {device}: {response.statusCode}");
                        onFail?.Invoke();
                    }
                }
            }

        }

        private async Task<bool> RequestWinkDataBase(string phoneNumber, Action<bool> onWinkSubscriptionAccessRequest)
        {
            Response response = await SmsAuthApi.HasActiveAccount(phoneNumber);

            Debug.Log("Account subscription: " + response.statusCode);

            if (response.statusCode == UnityWebRequest.Result.Success)
            {
                onWinkSubscriptionAccessRequest?.Invoke(true);
                return true;
            }
            else
            {
                onWinkSubscriptionAccessRequest?.Invoke(false);
                return false;
            }
        }
    }
}
