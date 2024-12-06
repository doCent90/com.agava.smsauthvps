using UnityEngine;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    [Preserve]
    public abstract class WindowPresenter : MonoBehaviour
    {
        [field: SerializeField] public WindowType Type {  get; private set; }

        public bool Enabled { get; private set; } = false;

        public abstract void Enable();
        public abstract void Disable();

        protected void EnableCanvasGroup(CanvasGroup canvas)
        {
            canvas.alpha = 1;
            canvas.interactable = true;
            canvas.blocksRaycasts = true;
            Enabled = true;
        }

        protected void DisableCanvasGroup(CanvasGroup canvas)
        {
            canvas.alpha = 0;
            canvas.interactable = false;
            canvas.blocksRaycasts = false;
            Enabled = false;
        }
    }
}
