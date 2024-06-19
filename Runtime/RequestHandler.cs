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

namespace Agava.Wink
{
    /// <summary>
    ///    Requests/response to/from YBD fuction.
    /// </summary>
    internal class RequestHandler
    {
        internal async Task<LoginData> Regist(string phoneNumber, string uniqueId, Action<bool> otpCodeRequest)
        {
            LoginData data = new()
            {
                phone = phoneNumber,
                otp_code = "0",
                device_id = uniqueId,
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

        internal async void Unlink(string deviceId, Action onResetLogin)
        {
            Debug.Log(deviceId);

            var tokens = SaveLoadLocalDataService.Load<Tokens>(TokenLifeHelper.Tokens);
            var resopnse = await SmsAuthApi.Unlink(tokens.access, deviceId);

            if (resopnse.statusCode != UnityWebRequest.Result.Success)
                Debug.LogError("Unlink fail: " + resopnse.statusCode);
            else
                onResetLogin?.Invoke();
        }

        internal async void Login(LoginData data, Action<IReadOnlyList<string>> onLimitReached,
            Action<bool> onWinkSubscriptionAccessRequest, Action<bool> otpCodeAccepted,
            Action onSuccessed, Action onAuthorizedSuccessfully = null)
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
                    OnLimitDevicesReached(onLimitReached);
                    return;
                }

                await RequestWinkDataBase(data.phone, onWinkSubscriptionAccessRequest, () =>
                {
                    onSuccessed?.Invoke();
                    onAuthorizedSuccessfully?.Invoke();
                });
            }
        }

        internal async void QuickAccess(string phoneNumber, Action onSuccessed,
            Action onResetLogin, Action<bool> onWinkSubscriptionAccessRequest,
            Action onAuthorizedSuccessfully = null)
        {
            var tokens = SaveLoadLocalDataService.Load<Tokens>(TokenLifeHelper.Tokens);

            if (tokens == null)
            {
                Debug.LogError("Tokens not exhist");
                onResetLogin?.Invoke();
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
                    onResetLogin?.Invoke();
                    return;
                }
            }
            else
            {
                onResetLogin?.Invoke();
                SaveLoadLocalDataService.Delete(TokenLifeHelper.Tokens);
                return;
            }

            var response = await SmsAuthApi.SampleAuth(currentToken);
            var hasSubsc = await RequestWinkDataBase(phoneNumber, onWinkSubscriptionAccessRequest, onAuthorizedSuccessfully);

            if (response.statusCode == UnityWebRequest.Result.Success)
            {
                onAuthorizedSuccessfully?.Invoke();

                if (hasSubsc)
                    onSuccessed?.Invoke();
            }
            else
            {
                Debug.LogError($"Quick access Validation Error: {response.reasonPhrase}: {response.statusCode}/Wink: {hasSubsc}");
                TokenLifeHelper.ClearTokens();
                onResetLogin?.Invoke();
            }
        }

        internal async void OnLimitDevicesReached(Action<IReadOnlyList<string>> onLimitReached)
        {
            Tokens tokens = TokenLifeHelper.GetTokens();
            var response = await SmsAuthApi.GetDevices(tokens.access);

            if (response.statusCode != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error");
            }
            else
            {
                IReadOnlyList<string> devices = JsonConvert.DeserializeObject<List<string>>(response.body);
                onLimitReached?.Invoke(devices);
            }
        }

        private async Task<bool> RequestWinkDataBase(string phoneNumber, Action<bool> onWinkSubscriptionAccessRequest, Action onSuccessed)
        {
            var response = await SmsAuthApi.HasActiveAccount(phoneNumber);
#if UNITY_EDITOR || TEST
            Debug.Log("Account subscription: " + response.statusCode);
#endif
            if (response.statusCode == UnityWebRequest.Result.Success)
            {
                onWinkSubscriptionAccessRequest?.Invoke(true);
                onSuccessed?.Invoke();
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
