using Moths.Collections;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{

    public static CameraController Instance { get; private set; }

    public Rect WorldRect
    {
        get
        {
            Rect offsets = default;
            offsets.width = 0.15f;

            float height = 2f * _camera.orthographicSize;
            float width = height * _camera.aspect;
            Vector2 center = _camera.transform.position;

            var camRect = new Rect(
                center.x - width / 2f * (1f - offsets.x),
                center.y + height / 2f * (1f - offsets.y),
                width * (1f - offsets.width),
                height * (1f - offsets.height)
            );

            return camRect;
        }
    }

    public Camera Camera => _camera;

    private static PointerEventData _pointerData;
    private static List<RaycastResult> _raycastResults = new List<RaycastResult>();

    private static Camera _camera;

    [SerializeField] float _dragSpeed = 0.1f;
    [SerializeField] float _scrollSpeed = 10;
    [SerializeField] RangeFloat _zoomRange;
    
    private Vector3 _mousePosition;
    private bool _isDragging = false;

    public bool disable = false;

    private void Start()
    {
        Instance = this;

        disable = false;
        _camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {

        UpdateCamera();

        _mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
    }

    private void UpdateCamera()
    {
        if (disable) return;
        if (ColorGrid.Instance.IsShowing) return;
        if (!IsMouseWithinScreen()) return;

        if (Input.GetMouseButtonDown(1) && IsMouseWithinViewport())
        {
            _isDragging = true;
            _mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(1))
        {
            if (_isDragging)
            {
                var delta = _camera.ScreenToWorldPoint(Input.mousePosition) - _mousePosition;

                transform.position -= delta;
            }
        }
        else
        {
            _isDragging = false;
        }

        var scrollDelta = Input.mouseScrollDelta.y;

        if (!Mathf.Approximately(scrollDelta, 0) && IsMouseWithinViewport())
        {
            _camera.orthographicSize -= scrollDelta * _scrollSpeed * _camera.orthographicSize;

            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, _zoomRange.min, _zoomRange.max);
        }
    }

    public static bool IsMouseWithinScreen()
    {
        Vector3 pos = Input.mousePosition;
        return pos.x >= 0 && pos.x <= Screen.width &&
               pos.y >= 0 && pos.y <= Screen.height;
    }

    public static bool IsMouseWithinViewport()
    {
        if (_pointerData == null)
        {
            _pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition,
            };
        }

        _pointerData.position = Input.mousePosition;

        _raycastResults.Clear();

        EventSystem.current.RaycastAll(_pointerData, _raycastResults);

        if (_raycastResults.Count > 0)
        {
            GameObject hoveredObject = _raycastResults[0].gameObject;
            return hoveredObject.layer != 6;
        }

        return true;
    }

    public static bool IsWithinCameraBounds(Rect worldRect)
    {
        return Instance.WorldRect.Contains(worldRect);
    }

    public static bool IsMouseOverInteractiveElement()
    {
        if (_pointerData == null)
        {
            _pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition,
            };
        }

        _pointerData.position = Input.mousePosition;

        _raycastResults.Clear();

        EventSystem.current.RaycastAll(_pointerData, _raycastResults);

        if (_raycastResults.Count > 0)
        {
            var g = _raycastResults[0].gameObject;
            UIInputField inputField = null;
            var result =  g.GetComponent<Button>() || g.GetComponent<UIDraggable>() || ((inputField = g.GetComponent<UIInputField>()) && inputField.blockSelection);
            return result;
        }

        return false;
    }
}
