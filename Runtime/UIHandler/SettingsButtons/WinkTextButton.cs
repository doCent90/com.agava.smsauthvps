using UnityEngine;
using Agava.Wink;

public class WinkTextButton : MonoBehaviour
{
    [SerializeField] private GameObject _signIn;
    [SerializeField] private GameObject _subscribe;

    private bool _authorized => WinkAccessManager.Instance.Authenficated;

    private void Start()
    {
        UpdateText();
    }

    private void Update()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        _signIn.SetActive(_authorized == false);
        _subscribe.SetActive(_authorized);
    }
}
