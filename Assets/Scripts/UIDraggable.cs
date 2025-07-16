using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIDraggable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public UnityEvent OnStartDrag;
    public UnityEvent<Vector2> OnDrag;
    public UnityEvent<Vector2> OnDragWorld;
    public UnityEvent OnEndDrag;

    private Vector2 _lastMousePosition;
    private bool _isDragging = false;

    private void Update()
    {
        if (_isDragging)
        {
            OnDrag?.Invoke((Vector2)Input.mousePosition - _lastMousePosition);

            OnDragWorld?.Invoke(Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.ScreenToWorldPoint(_lastMousePosition));

            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
                OnEndDrag?.Invoke();
            }
        }
        _lastMousePosition = Input.mousePosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        
        _isDragging = true;
        _lastMousePosition = Input.mousePosition;
        OnStartDrag?.Invoke();
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        _isDragging = false;
        OnEndDrag?.Invoke();
    }
}
