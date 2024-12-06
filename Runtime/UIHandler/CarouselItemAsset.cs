using Lean.Localization;
using UnityEngine;

namespace Agava.Wink
{
    [CreateAssetMenu(fileName = "CarouselItemAsset", menuName = "Create new CarouselItemAsset", order = 51)]
    public class CarouselItemAsset : ScriptableObject
    {
        [field: SerializeField] public Sprite Sprite { get; private set; }
        [field: SerializeField, LeanTranslationName] public string Description { get; private set; }
    }
}
