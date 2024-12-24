using System;
using Io.AppMetrica;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    [Preserve]
    public static class AnalyticsWinkService
    {
        /// <summary>
        /// Auditory
        /// </summary>

        public static void SendStartApp(string appId) => SendEvent("App run", GetJson("App run", appId));
        public static void SendSanId(string sanId)
        {
            SendEvent("SanId", GetJson("SanId", sanId));
            Debug.LogWarning("Analytics: SanId: " + sanId);
        }

        public static void SendSex(string sex) => SendEvent($"Sex {sex}");//N/A
        public static void SendAge(string age) => SendEvent($"Age {age}");//N/A

        /// <summary>
        /// User data
        /// </summary>
        public static void SendHasActiveAccountNewUser(bool hasActiveAcc)
        {
            SendEvent("Has Active Account New User", GetJson("New Account", hasActiveAcc.ToString()));
            Debug.LogWarning("Analytics: Has Active Account New User: " + hasActiveAcc);
        }

        public static void SendHasActiveAccountUser(bool hasActiveAcc)
        {
            SendEvent("Has Active Account Regular User", GetJson("Regular Account", hasActiveAcc.ToString()));
            Debug.LogWarning("Analytics: Has Active Account Regular User: " + hasActiveAcc);
        }

        /// <summary>
        /// Retention
        /// </summary>
        public static void SendAverageSessionLength(int time) => SendEvent("Average Session Length", GetJson("New Account", time.ToString()));

        /// <summary>
        /// First time events
        /// </summary>
        public static void SendSubscribeOfferWindow() => SendEvent("Subscribe Offer Window (Unsigned user)");
        public static void SendHelloWindow() => SendEvent("Hello Window (Signed user)");
        public static void SendEnterPhoneWindow() => SendEvent("Enter Phone Window");
        public static void SendOnEnteredPhoneWindow() => SendEvent("On Entered Phone");
        public static void SendEnterOtpCodeWindow() => SendEvent("Enter Otp Code Window");
        public static void SendOnEnteredOtpCodeWindow() => SendEvent("On Entered Otp Code");
        public static void SendPayWallWindow() => SendEvent("PayWall Window");
        public static void SendPayWallRedirect() => SendEvent("PayWall Redirect");
        public static void SendFirstOpen() => SendEvent("First Open Game");
        public static void SendSupportLink() => SendEvent("Support Link");
        public static void SendSubscriptionLink() => SendEvent("About Subscription Link");
        public static void SendDeleteWindow() => SendEvent("Delete Window");
        public static void SendCloseStartWindow() => SendEvent("Close Start Window");
        public static void SendHaveWinkButtonClick() => SendEvent("Click Have Wink Button");

        private static string GetJson(string name, string value)
        {
            Data data = new Data()
            {
                Name = name,
                Value = value
            };

            return JsonConvert.SerializeObject(data);
        }

        internal class Data
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        private static void SendEvent(string eventName)
        {
            AppMetrica.ReportEvent(eventName);
        }

        private static void SendEvent(string eventName, string json)
        {
            try
            {
                AppMetrica.ReportEvent(eventName, json);
            }
            catch (Exception ex)
            {
                Debug.Log("AppMetrica error:");
                Debug.Log(ex.Message);
            }
        }
    }
}
