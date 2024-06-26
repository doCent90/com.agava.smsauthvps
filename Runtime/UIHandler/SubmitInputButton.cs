using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    [Preserve]
    internal class SubmitInputButton : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour _inputFieldFormatting;
        [Header("Buttons")]
        [SerializeField] private GameObject _active;
        [SerializeField] private GameObject _unactive;

        private IInputFieldFormatting InputFieldFormatting => (IInputFieldFormatting)_inputFieldFormatting;

        private void Update()
        {
            bool inputDone = InputFieldFormatting.InputDone;

            _active.SetActive(inputDone);
            _unactive.SetActive(!inputDone);
        }

        private void OnValidate()
        {
            if (_inputFieldFormatting && !(_inputFieldFormatting is IInputFieldFormatting))
            {
                Debug.LogError(nameof(_inputFieldFormatting) + " needs to implement " + nameof(IInputFieldFormatting));
                _inputFieldFormatting = null;
            }
        }
    }
}
