namespace Agava.Wink
{
    internal class ClickableSubscriptionText : ClickableText
    {
        private void Awake()
        {
            Initialize("subscription", Links.Subscription, () => AnalyticsWinkService.SendSubscriptionLink());
        }
    }
}
