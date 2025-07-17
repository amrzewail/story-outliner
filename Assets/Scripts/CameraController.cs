using Moths.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{

    public static CameraController Instance { get; private set; }


    private static PointerEventData _pointerData;
    private static List<RaycastResult> _raycastResults = new List<RaycastResult>();

    [SerializeField] float _dragSpeed = 0.1f;
    [SerializeField] float _scrollSpeed = 10;
    [SerializeField] RangeFloat _zoomRange;
    
    private Vector3 _mousePosition;
    private Camera _camera;
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

        _mousePosition = Input.mousePosition;
    }

    private void UpdateCamera()
    {
        if (disable) return;
        if (ColorGrid.Instance.IsShowing) return;
        if (!IsMouseWithinScreen()) return;

        if (Input.GetMouseButtonDown(1) && IsMouseWithinViewport())
        {
            _isDragging = true;
            _mousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))
        {
            if (_isDragging)
            {
                var delta = Input.mousePosition - _mousePosition;

                transform.position -= delta * _camera.orthographicSize / 1000f * _dragSpeed;
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
}
