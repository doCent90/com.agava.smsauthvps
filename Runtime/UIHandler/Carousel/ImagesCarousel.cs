using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Localization;
using TMPro;
using UnityEngine;

namespace Agava.Wink
{
    public class ImagesCarousel : MonoBehaviour
    {
        private const float OneCycleSeconds = 2.5f;
        private const float PauseSeconds = 1f;

        [SerializeField] private List<CarouselItem> _items;
        [SerializeField] private List<CarouselItemAsset> _assets;
        [Header("Carousel header")]
        [SerializeField] private CarouselItem _headerItem;
        [SerializeField] private TMP_Text _header;

        int _assetIndex = 0;
        private Coroutine _cycle;
        private List<CarouselPosition> _carouselPositions = null;
        private int _headerPositionIndex;

        private void Awake()
        {
            FillCarouselPositions();
            FillItems();
        }

        public void Enable()
        {
            _cycle = StartCoroutine(EndlessCycle());
        }

        public void Disable()
        {
            if (_cycle != null)
            {
                StopCoroutine(_cycle);
                _cycle = null;
            }
        }

        private IEnumerator EndlessCycle()
        {
            WaitForSeconds waitForCycleEnd = new WaitForSeconds(OneCycleSeconds + PauseSeconds);

            while (true)
            {
                OneCycle();
                yield return waitForCycleEnd;
            }
        }

        private void OneCycle()
        {
            CarouselItem item;
            int targetPositionIndex;
            Action<CarouselItem> onEnd;

            for (int i = 0; i < _items.Count; i++)
            {
                item = _items[i];

                if (item.Index == 0)
                {
                    targetPositionIndex = _carouselPositions.Count - 1;
                    item.Hide();

                    onEnd = (item) =>
                    {
                        item.Show();
                        item.Initialize(NextAsset());
                    };
                }
                else
                {
                    targetPositionIndex = item.Index - 1;
                    onEnd = null;
                }

                if (_headerPositionIndex == targetPositionIndex)
                {
                    if (_header != null)
                        _header.text = LeanLocalization.GetTranslationText(item.Description);
                }

                item.SetPositionIndex(targetPositionIndex);
                item.OneCycle(_carouselPositions[targetPositionIndex].Position, _carouselPositions[targetPositionIndex].Scale, OneCycleSeconds, onEnd);
            }
        }

        private void FillItems()
        {
            if (_assets.Count == 0)
            {
                Debug.LogError("Fill sprites!");
                return;
            }

            for (int i = 1; i < _items.Count; i++)
            {
                _items[i].Initialize(NextAsset());
            }
        }

        private void FillCarouselPositions()
        {
            CarouselItem item;
            _carouselPositions = new();

            for (int i = 0; i < _items.Count; i++)
            {
                item = _items[i];

                if (item == _headerItem)
                    _headerPositionIndex = i;

                item.SetPositionIndex(i);
                _carouselPositions.Add(new CarouselPosition(item.transform.localPosition, item.transform.localScale));
            }
        }

        private CarouselItemAsset NextAsset()
        {
            if (_assetIndex == _assets.Count)
                _assetIndex = 0;

            return _assets[_assetIndex++];
        }

        private struct CarouselPosition
        {
            public Vector3 Position { get; private set; }
            public Vector3 Scale { get; private set; }

            public CarouselPosition(Vector3 position, Vector3 scale)
            {
                Position = position;
                Scale = scale;
            }
        }
    }
}
