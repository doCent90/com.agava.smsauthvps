namespace Agava.Wink
{
    internal class ClickablePrivacyText : ClickableText
    {
        private void Awake()
        {
            Initialize("policy", Links.Privacy);
        }
    }
}
