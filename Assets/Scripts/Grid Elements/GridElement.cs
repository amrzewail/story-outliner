using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridElement : MonoBehaviour, IPointerClickHandler
{

    public enum Size
    {
        Medium,
        Small,
        Large,
        Dynamic,
        Count
    };

    [Serializable]
    private struct Output
    {
        public string guid;
        public V3 position;
        public Size size;
        public V3 color;
        public V3 rectSize;

        public static Output Default
        {
            get
            {
                return new Output
                {
                    size = Size.Medium,
                    color = new V3(0.34f, 0.34f, 0.34f)
                };
            }
        }
    }

    [Serializable]
    private struct V3
    {
        public float x, y, z;

        public V3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    private Vector3 _lastWorldPosition;
    private Vector3 _screenPosition;
    private Vector2 _normalSize;
    private Vector2 _moveOffset;
    private int _moveFrameIndex;
    private Vector2 _resizeOffset;
    private Camera _camera;

    public Rect Rect
    {
        get
        {
            Vector3 position = Transform.position;
            Vector2 sizeDelta = ((RectTransform)Transform).sizeDelta;
            return new Rect
            {
                x = position.x - sizeDelta.x / 2f,
                y = position.y + sizeDelta.y / 2f,
                width = sizeDelta.x,
                height = sizeDelta.y,
            };
        }

        set
        {
            ((RectTransform)Transform).sizeDelta = new Vector2(value.width, value.height);
            Transform.position = new Vector2(value.x + value.width / 2f, value.y - value.height / 2f);
        }
    }
    public Transform Transform { get; private set; }
    public virtual int SortOrder { get; } = 0;

    public bool IsSelected { get; private set; }

    [SerializeField] CanvasGroup _buttonsGroup;
    [SerializeField] CanvasGroup _inputsGroup;

    protected bool _isMoving = false;

    protected bool _isResizing = false;

    protected Size _size = Size.Medium;

    public Guid guid { get; set; }

    public Size size => _size;


    private void Awake()
    {
        Transform = transform;

        _inputsGroup.interactable = false;

        LayerManager.LayersUpdated += LayersUpdatedCallback;

        ChangeColor(GetColor());
    }

    private void OnDestroy()
    {
        LayerManager.LayersUpdated -= LayersUpdatedCallback;
    }

    private void LayersUpdatedCallback()
    {
        var layer = LayerManager.GetElementLayer(guid);
        var interactable = layer == LayerManager.SelectedLayer;
        _buttonsGroup.interactable = interactable;
        _buttonsGroup.alpha = interactable ? 1 : 0;
        _inputsGroup.interactable = interactable && IsSelected;
        GetComponent<CanvasGroup>().alpha = interactable ? 1 : 0.65f;
        GetComponent<CanvasGroup>().blocksRaycasts = interactable;
    }

    protected virtual void Start()
    {
        _normalSize = ((RectTransform)Transform).sizeDelta;
        _camera = Camera.main;

        Deselect();
    }



    public void MoveClickCallback()
    {
        if (_isMoving) return;
        _isMoving = true;
        _moveFrameIndex = Time.frameCount;
        _moveOffset = Transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        OnStartMove();
    }

    public void MoveDragStartCallback()
    {
        _moveOffset = Transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        OnStartMove();
        _moveFrameIndex = Time.frameCount;
    }

    public void MoveDragCallback(Vector2 dragWorld, RectTransform target)
    {
        Vector3 position = Transform.position + new Vector3(dragWorld.x, dragWorld.y, 0);

        OnMove(position - Transform.position);

        Transform.position = position;
    }

    public void MoveDragEndCallback()
    {
        OnEndMove();
    }

    public async void ColorClickCallback()
    {
        var col = GetColor();
        Color color = await ColorGrid.Instance.Show(col);

        ChangeColor(color);
    }

    protected virtual void LateUpdate()
    {
        if (_isMoving)
        {
            //CameraController.Instance.disable = true;

            Vector3 position = Transform.position;
            position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            position += new Vector3(_moveOffset.x, _moveOffset.y, 0);
            position.z = 0;

            OnMove(position - Transform.position);

            Transform.position = position;

            if (Time.frameCount != _moveFrameIndex && Input.GetMouseButtonUp(0))
            {
                _isMoving = false;
                OnEndMove();
            }
        }

        if (_isResizing)
        {
            var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var rect = Rect;
            Vector2 size = rect.size;
            size.x = mousePosition.x - rect.x - _resizeOffset.x;
            size.y = rect.y - mousePosition.y + _resizeOffset.y;
            rect.size = size;
            Rect = rect;
        }

        switch (_size)
        {
            case Size.Medium:
                ((RectTransform)Transform).sizeDelta = _normalSize;
                break;

            case Size.Small:
                ((RectTransform)Transform).sizeDelta = new Vector2(_normalSize.x, _normalSize.y / 3);
                break;

            case Size.Large:
                ((RectTransform)Transform).sizeDelta = _normalSize * 3;
                break;
        }

        if (_lastWorldPosition != (_lastWorldPosition = Transform.position))
        {
            _screenPosition = _camera.WorldToScreenPoint(_lastWorldPosition);
        }
    }

    protected virtual void ChangeColor(Color color)
    {
        List<Colorable> colorable = GetComponentsInChildren<Colorable>(true).ToList();
        colorable.ForEach(x => x.SetColor(new Color(color.r, color.g, color.b, x.GetColor().a)));
    }

    protected virtual Color GetColor()
    {
        Colorable colorable = GetComponentInChildren<Colorable>();
        if (!colorable) return Color.white;
        return colorable.GetColor();
    }

    public virtual void OnStartMove() { }

    public virtual void OnEndMove() { }
    public virtual void OnMove(Vector2 offset) { }


    public void OnPointerClick(PointerEventData eventData)
    {
    }


    public void ResizeCallback()
    {
        _size = (Size)(((int)_size + 1) % (int)Size.Count);
    }

    public void DynamicResizeCallback()
    {
        _isResizing = !_isResizing;
        _size = Size.Dynamic;
        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _resizeOffset = (Vector2)mousePosition - new Vector2(Rect.x + Rect.width, Rect.y - Rect.height);
    }

    public virtual void DynamicResizeCallback(Vector2 dragWorld, RectTransform target)
    {
        _size = Size.Dynamic;
        var rect = Rect;
        Vector2 size = rect.size;
        size += new Vector2(dragWorld.x, -dragWorld.y);
        rect.size = size;
        Rect = rect;
    }

    public virtual async void Select()
    {
        IsSelected = true;
        Transform.Find("Selection").gameObject.SetActive(true);
        _buttonsGroup.gameObject.SetActive(true);

        await UniTask.NextFrame();
        await UniTask.NextFrame();

        var layer = LayerManager.GetElementLayer(guid);
        var interactable = layer == LayerManager.SelectedLayer;
        _inputsGroup.interactable = interactable;
    }

    public virtual void Deselect()
    {
        IsSelected = false;
        Transform.Find("Selection").gameObject.SetActive(false);
        _buttonsGroup.gameObject.SetActive(false);

        _inputsGroup.interactable = false;
    }

    public virtual float GetConnectionOffset(float angle, ConnectionType type)
    {
        var rect = Rect;

        float w = rect.width / 2f;
        float h = rect.height / 2f;

        float angleRad = angle * Mathf.Deg2Rad;
        float dx = Mathf.Cos(angleRad);
        float dy = Mathf.Sin(angleRad);

        float tx = (dx != 0f) ? w / Mathf.Abs(dx) : float.MaxValue;
        float ty = (dy != 0f) ? h / Mathf.Abs(dy) : float.MaxValue;

        return Mathf.Min(tx, ty);
    }

    public virtual async UniTask<string> Serialize()
    {
        Output data = Output.Default;
        data.guid = guid.ToString();
        data.position = new V3(){
            x = Transform.position.x,
            y = Transform.position.y,
            z = Transform.position.z 
        };
        var color = GetColor();
        data.color = new V3(color.r, color.g, color.b);
        data.size = _size;
        data.rectSize = new V3(Rect.size.x, Rect.size.y, 0);
        return JsonUtility.ToJson(data);
    }

    public virtual void Deserialize(string str)
    {
        Output data = JsonUtility.FromJson<Output>(str);
        guid = new Guid(data.guid);
        _size = data.size;
        if (_size == Size.Dynamic)
        {
            var rect = Rect;
            rect.size = new Vector2(data.rectSize.x, data.rectSize.y);
            Rect = rect;
        }
        Transform.position = new Vector3(data.position.x, data.position.y, data.position.z);
        ChangeColor(new Color(data.color.x, data.color.y, data.color.z));
    }


    public bool IsOutOfScreenBounds()
    {
        return _screenPosition.x < 0 || _screenPosition.x > Screen.width ||
               _screenPosition.y < 0 || _screenPosition.y > Screen.height;
    }
}
