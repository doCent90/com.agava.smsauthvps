using System;
using UnityEngine;
using UnityEngine.UI;

namespace Agava.Wink
{
    internal class KeyboardButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private KeyCode _keyCode;

        public event Action<KeyCode> Clicked;

        private void OnDestroy() => _button.onClick.RemoveListener(OnClicked);
        private void Awake() => _button.onClick.AddListener(OnClicked);
        private void OnClicked() => Clicked?.Invoke(_keyCode);
    }
}
