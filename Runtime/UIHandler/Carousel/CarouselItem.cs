using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Agava.Wink
{
    public class CarouselItem : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private Image _mask;

        private Coroutine _coroutine;

        public int Index { get; private set; }
        public string Description { get; private set; }

        public void SetPositionIndex(int index)
        {
            Index = index;
        }

        public void Initialize(CarouselItemAsset asset)
        {
            _image.sprite = asset.Sprite;
            Description = asset.Description;
        }

        public void Hide()
        {
            _image.enabled = false;
            _mask.enabled = false;
        }

        public void Show()
        {
            _image.enabled = true;
            _mask.enabled = true;
        }

        public void Stop()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
            }
        }

        public void OneCycle(Vector3 targetPosition, Vector3 targetScale, float duration, Action<CarouselItem> onEnd = null)
        {
            _coroutine = StartCoroutine(Moving(targetPosition, targetScale, duration));

            IEnumerator Moving(Vector3 targetPosition, Vector3 targetScale, float duration)
            {
                Vector3 startScale = transform.localScale;
                Vector3 startPosition = transform.localPosition;

                if (duration > 0)
                {
                    float elapsedTime = 0;
                    float delta;

                    while (elapsedTime < duration)
                    {
                        elapsedTime += Time.unscaledDeltaTime;
                        delta = elapsedTime / duration;

                        transform.localPosition = Vector3.Lerp(startPosition, targetPosition, delta);
                        transform.localScale = Vector3.Lerp(startScale, targetScale, delta);

                        yield return null;
                    }
                }

                transform.localPosition = targetPosition;
                transform.localScale = targetScale;
                onEnd?.Invoke(this);

                _coroutine = null;
            }
        }
    }
}
