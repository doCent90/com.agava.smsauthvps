using System;
using System.Collections.Generic;
using UnityEngine;

namespace Agava.Wink
{
    [Serializable]
    internal class TouchKeyboardPresenter
    {
        [SerializeField] private List<Transform> _target;
        [SerializeField] private float _yOffset = 150f;

        private bool _hasMoved = false;

        private TouchScreenKeyboard _keyboard;

        internal void Construct()
        {
            _keyboard = TouchScreenKeyboard.Open(string.Empty, TouchScreenKeyboardType.NumberPad, false, false, false, false);
            TouchScreenKeyboard.hideInput = true;
        }

        internal void Disable()
        {
            _keyboard.active = false;
        }

        internal void Update() 
        {
            _keyboard.active = true;

            if (Screen.autorotateToPortrait || Screen.autorotateToPortraitUpsideDown 
                || Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
                return;

            //if (TouchScreenKeyboard.visible && _hasMoved == false)
            //    MoveWindow();
            //else if (TouchScreenKeyboard.visible == false && _hasMoved)
            //    ResetWindow();
        }

        //private void MoveWindow()
        //{
        //    _target.ForEach(window => window.position = window.position + new Vector3(0, _yOffset, 0));
        //    _hasMoved = true;
        //}

        //private void ResetWindow()
        //{
        //    _target.ForEach(window => window.position = window.position - new Vector3(0, _yOffset, 0));
        //    _hasMoved = false;
        //}
    }
}
