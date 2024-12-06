using UnityEngine;
using UnityEngine.UI;

namespace Agava.Wink
{
    [RequireComponent(typeof(Button))]
    public class ButtonTextSwap : MonoBehaviour
    {
        [SerializeField] private GameObject _activeButtonText;
        [SerializeField] private GameObject _disabledButtonText;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void Update()
        {
            _activeButtonText.SetActive(_button.interactable);
            _disabledButtonText.SetActive(!_button.interactable);
        }
    }
}
