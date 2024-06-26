using System;
using System.Collections.Generic;
using UnityEngine;

namespace Agava.Wink
{
    [Serializable]
    internal class WindowScalerPresenter
    {
        [SerializeField] private List<Transform> _target;
        [SerializeField] private float _yOffset = 150f;

        private bool _hasMoved = false;

        internal void Construct()
        {
            TouchScreenKeyboard.hideInput = true;
        }

        internal void Update() 
        {
            if (Screen.autorotateToPortrait || Screen.autorotateToPortraitUpsideDown 
                || Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
                return;

            if (TouchScreenKeyboard.visible && _hasMoved == false)
                MoveWindow();
            else if (TouchScreenKeyboard.visible == false && _hasMoved)
                ResetWindow();
        }

        private void MoveWindow()
        {
            _target.ForEach(window => window.position = window.position + new Vector3(0, _yOffset, 0));
            _hasMoved = true;
        }

        private void ResetWindow()
        {
            _target.ForEach(window => window.position = window.position - new Vector3(0, _yOffset, 0));
            _hasMoved = false;
        }
    }
}
