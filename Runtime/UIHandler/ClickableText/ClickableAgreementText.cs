namespace Agava.Wink
{
    internal class ClickableAgreementText : ClickableText
    {
        private void Awake()
        {
            Initialize("agreement", Links.Agreement);
        }
    }
}
