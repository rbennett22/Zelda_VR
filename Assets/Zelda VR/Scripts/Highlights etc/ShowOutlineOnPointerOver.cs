using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Eyefluence.Utility
{
    public class ShowOutlineOnPointerOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Outline outline;


        void OnEnable()
        {
            if (outline == null)
            {
                outline = GetComponent<Outline>();
            }
            ShowOutline(false);
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowOutline(true);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            ShowOutline(false);
        }


        void ShowOutline(bool doShow)
        {
            if (outline != null)
            {
                outline.enabled = doShow;
            }
        }
    }
}