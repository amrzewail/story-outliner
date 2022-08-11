using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public static CameraController Instance { get; private set; }

    [SerializeField] float _dragSpeed = 0.1f;
    [SerializeField] float _scrollSpeed = 10;
    
    private Vector3 _mousePosition;
    private Camera _camera;

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
        if (!disable)
        {

            if (Input.GetMouseButton(0))
            {
                var delta = Input.mousePosition - _mousePosition;

                transform.position -= delta * _camera.orthographicSize / 1000f * _dragSpeed;
            }

        }
        _camera.orthographicSize -= Input.mouseScrollDelta.y * _scrollSpeed * _camera.orthographicSize;

        _mousePosition = Input.mousePosition;
    }
}
