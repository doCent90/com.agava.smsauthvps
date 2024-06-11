using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Agava.Wink
{
    internal class HelloWindowPresenter : WindowPresenter
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField, Min(0)] private float _presentTime;

        public void Enable(Action onEnd = null)
        {
            EnableCanvasGroup(_canvasGroup);

            StartCoroutine(Waiting());
            IEnumerator Waiting()
            {
                yield return new WaitForSecondsRealtime(_presentTime);

                Disable();
                onEnd?.Invoke();
            }
        }

        public override void Enable() { }

        public override void Disable() => DisableCanvasGroup(_canvasGroup);
    }
}
