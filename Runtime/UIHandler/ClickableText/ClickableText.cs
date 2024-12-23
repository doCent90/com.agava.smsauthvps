using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Agava.Wink
{
    internal abstract class ClickableText : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TMP_Text _tmpText;

        private string _linkId;
        private string _link;
        private Action _onClick;

        protected void Initialize(string linkId, string link, Action onClick=null)
        {
            _linkId = linkId;
            _link = link;
            _onClick = onClick;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(_linkId) || string.IsNullOrEmpty(_link))
                return;

            int linkIndex = TMP_TextUtilities.FindIntersectingLink(_tmpText, eventData.position, eventData.pressEventCamera);

            if (linkIndex == -1)
                return;

            TMP_LinkInfo linkInfo = _tmpText.textInfo.linkInfo[linkIndex];
            string selectedLink = linkInfo.GetLinkID();

            if (selectedLink == _linkId)
            {
                Debug.LogFormat($"Open link {selectedLink}");
                Application.OpenURL(_link);
                _onClick?.Invoke();
            }
        }
    }
}
