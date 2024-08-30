using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Baracuda.UI.Components
{
    public class PointerEvents : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] [Required] private Image targetGraphic;

        public Image TargetGraphic => targetGraphic;
        public event Action<PointerEvents> PointerExit;
        public event Action<PointerEvents> PointerEnter;
        public event Action<PointerEvents> PointerDown;

        private void OnValidate()
        {
            targetGraphic ??= GetComponent<Image>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PointerEnter?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PointerExit?.Invoke(this);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            PointerDown?.Invoke(this);
        }

        private void OnDestroy()
        {
            targetGraphic.ShutdownTweens();
        }
    }
}