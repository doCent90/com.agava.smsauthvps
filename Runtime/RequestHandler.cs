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

        internal async void Unlink(UnlinkData unlinkData, Action onResetLogin)
        {
            Debug.Log($"deviceId: {unlinkData.device_id}, appId: {unlinkData.app_id}");

            var tokens = SaveLoadLocalDataService.Load<Tokens>(TokenLifeHelper.Tokens);
            var response = await SmsAuthApi.Unlink(tokens.access, unlinkData);

            if (response.statusCode != UnityWebRequest.Result.Success)
                Debug.LogError("Unlink fail: " + response.statusCode);
            else
                onResetLogin?.Invoke();
        }

        internal async void Login(LoginData data, Action<IReadOnlyList<string>> onLimitReached,
            Action<bool> onWinkSubscriptionAccessRequest, Action<bool> otpCodeAccepted,
            Action onAuthenficationSuccessfully, Action onAuthorizationSuccessfully)
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
                onAuthenficationSuccessfully?.Invoke();
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
                    OnLimitDevicesReached(onLimitReached, data.app_id);
                    return;
                }

                await RequestWinkDataBase(data.phone, onWinkSubscriptionAccessRequest, () =>
                {
                    onAuthorizationSuccessfully?.Invoke();
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

        internal async void DeleteAccount(Action onDeleteAccount)
        {
            var tokens = SaveLoadLocalDataService.Load<Tokens>(TokenLifeHelper.Tokens);
            var response = await SmsAuthApi.DeleteAccount(tokens.access);

            if (response.statusCode != UnityWebRequest.Result.Success)
                Debug.LogError("Account deletion fail: " + response.statusCode);
            else
                onDeleteAccount?.Invoke();
        }

        internal async void OnLimitDevicesReached(Action<IReadOnlyList<string>> onLimitReached, string app_id)
        {
            Tokens tokens = TokenLifeHelper.GetTokens();
            var response = await SmsAuthApi.GetDevices(tokens.access, app_id);

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
