using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Agava.Wink
{
    public class UnlinkDeviceView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _number;
        [SerializeField] private TMP_Text _deviceId;
        [SerializeField] private TMP_Text _freePlaceText;
        [SerializeField] private Button _closeButton;

        public event Action<UnlinkDeviceView> Closed;

        public bool Empty { get; private set; } = true;
        public string DeviceId => _deviceId.text;

        private void Awake()
        {
            SetFree();
        }

        private void OnEnable()
        {
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        private void OnDisable()
        {
            _closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }

        public void Initialize(string deviceId)
        {
            _deviceId.text = deviceId;
            SetEmpty(false);
        }

        public void SetFree() => SetEmpty(true);

        public void SetNumber(int number)
        {
            _number.text = number.ToString();
        }

        private void OnCloseButtonClicked()
        {
            Closed?.Invoke(this);
        }

        private void SetEmpty(bool empty)
        {
            _deviceId.gameObject.SetActive(empty == false);
            _closeButton.gameObject.SetActive(empty == false);
            _freePlaceText.gameObject.SetActive(empty);
            Empty = empty;
        }
    }
}
