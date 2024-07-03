using UnityEngine;
using UnityEngine.UI;
using Agava.Wink;

public class WinkTextButton : MonoBehaviour
{
    [SerializeField] private Text _signIn;
    [SerializeField] private Text _subscribe;

    private bool _authorized => WinkAccessManager.Instance.Authenficated;

    private void Update()
    {
        _signIn.gameObject.SetActive(_authorized == false);
        _subscribe.gameObject.SetActive(_authorized);
    }
}
