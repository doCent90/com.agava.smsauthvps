using UnityEngine;
using TMPro;

public class SignInButton : MonoBehaviour
{
    [SerializeField] private PhoneNumberFormatting _numbersInputField;
    [Header("Buttons")]
    [SerializeField] private GameObject _active;
    [SerializeField] private GameObject _unactive;

    private void Update()
    {
        bool numberFilled = _numbersInputField.NumberFilled;

        _active.SetActive(numberFilled);
        _unactive.SetActive(!numberFilled);
    }
}
