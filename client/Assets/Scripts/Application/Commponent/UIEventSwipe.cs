using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIEventSwipe : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [System.Serializable] public class SwipeEvent : UnityEvent<Vector2> { }
    private Vector2? start;

    public SwipeEvent OnSwiping { private set; get; } = new SwipeEvent();

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if (start == null) return;
        var diff = start.Value - eventData.position;

        OnSwiping?.Invoke(diff);
        // Debug.Log(diff);

    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        start = eventData.position;
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        start = null;
    }
}
