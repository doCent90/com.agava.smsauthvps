using TMPro;
using UnityEngine;

public class TextCell : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private GameObject _enabledObject;

    public bool Empty => string.IsNullOrEmpty(_text.text);

    public void SetActive(bool isActive)
    {
        _enabledObject.gameObject.SetActive(isActive);
    }

    public void SetText(string text)
    {
        _text.text = text;
    }
}
