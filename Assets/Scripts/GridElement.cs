using Newtonsoft.Json;
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
    private class Output
    {
        public string guid;
        public V3 position;
        public Size size = Size.Medium;
        public V3 color = new V3(0.34f, 0.34f, 0.34f);
        public V3 rectSize;
    }

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

    private Vector2 _normalSize;
    private Vector2 _moveOffset;
    private int _moveFrameIndex;
    private Button _moveButton;

    public Rect Rect
    {
        get
        {
            return new Rect
            {
                x = transform.position.x - ((RectTransform)transform).sizeDelta.x / 2f,
                y = transform.position.y + ((RectTransform)transform).sizeDelta.y / 2f,
                width = ((RectTransform)transform).sizeDelta.x,
                height = ((RectTransform)transform).sizeDelta.y,
            };
        }

        set
        {
            ((RectTransform)transform).sizeDelta = new Vector2(value.width, value.height);
            transform.position = new Vector2(value.x + value.width / 2f, value.y - value.height / 2f);
        }
    }

    protected bool _isMoving = false;

    protected bool _isResizing = false;

    protected Size _size = Size.Medium;

    public string guid { get; set; }

    public Size size => _size;

    public void MoveClickCallback()
    {
        if (_isMoving) return;
        _isMoving = true;
        _moveFrameIndex = Time.frameCount;
        _moveOffset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        OnStartMove();
    }

    public async void ColorClickCallback()
    {
        var col = GetColor();
        Color color = await ColorGrid.Instance.Show(col);

        ChangeColor(color);
    }


    protected virtual void Start()
    {
        _moveButton = transform.Find("MoveButton").GetComponent<Button>();

        _normalSize = ((RectTransform)transform).sizeDelta;

        Deselect();
    }


    protected virtual void LateUpdate()
    {
        _moveButton.interactable = !_isMoving;

        if (_isMoving)
        {
            //CameraController.Instance.disable = true;

            Vector3 position = transform.position;
            position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            position += new Vector3(_moveOffset.x, _moveOffset.y, 0);
            position.z = 0;

            OnMove(position - transform.position);

            transform.position = position;

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
            size.x = mousePosition.x - rect.x;
            size.y = rect.y - mousePosition.y;
            rect.size = size;
            Rect = rect;
        }

        switch (_size)
        {
            case Size.Medium:
                ((RectTransform)transform).sizeDelta = _normalSize;
                break;

            case Size.Small:
                ((RectTransform)transform).sizeDelta = new Vector2(_normalSize.x, _normalSize.y / 3);
                break;

            case Size.Large:
                ((RectTransform)transform).sizeDelta = _normalSize * 3;
                break;
        }
    }

    protected virtual void ChangeColor(Color color)
    {
        List<Colorable> colorable = GetComponentsInChildren<Colorable>().ToList();
        colorable.ForEach(x => x.SetColor(new Color(color.r, color.g, color.b, x.GetColor().a)));
    }

    protected virtual Color GetColor()
    {
        Colorable colorable = GetComponentInChildren<Colorable>();
        return colorable.GetColor();
    }

    protected virtual void OnStartMove() { }

    protected virtual void OnEndMove() { }
    protected virtual void OnMove(Vector2 offset) { }


    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("pointer click");
    }


    public void ResizeCallback()
    {
        _size = (Size)(((int)_size + 1) % (int)Size.Count);
    }

    public void DynamicResizeCallback()
    {
        _isResizing = !_isResizing;
        _size = Size.Dynamic;
    }

    public virtual void Select()
    {
        transform.Find("Selection").gameObject.SetActive(true);
    }

    public virtual void Deselect()
    {
        transform.Find("Selection").gameObject.SetActive(false);
    }

    public virtual float GetConnectionOffset(float angle, ConnectionType type)
    {
        Vector2 size = ((RectTransform)transform).sizeDelta;
        float diagonal = Mathf.Sqrt(Mathf.Pow(size.x, 2) + Mathf.Pow(size.y, 2));
        float diagAngle = Mathf.Atan2(size.y, size.x) * Mathf.Rad2Deg;

        float lerp;
        float side;

        if (angle < 0) angle += 180;
        if (angle > 180 - diagAngle || angle < diagAngle)
        {
            if (angle > 180 - diagAngle) angle = 180 - angle;
            lerp = angle / diagAngle;
            side = size.x;
        }
        else
        {
            // min: diagAngle
            // max: 180 - diagAngle

            // new min: 0
            // new max: 180 - diagAngle * 2

            angle -= diagAngle;
            if (angle > 90 - diagAngle)
            {
                angle = 180 - diagAngle * 2 - angle;
            }
            lerp = 1 - angle / (90 - diagAngle);
            side = size.y;
        }

        lerp = Mathf.Pow(lerp, 2);

        return Mathf.Lerp(side, diagonal, lerp) / 2;
    }

    public virtual string Serialize()
    {
        Output data = new Output();
        data.guid = guid;
        data.position = new V3(){
            x = transform.position.x,
            y = transform.position.y,
            z = transform.position.z 
        };
        var color = GetColor();
        data.color = new V3(color.r, color.g, color.b);
        data.size = _size;
        data.rectSize = new V3(Rect.size.x, Rect.size.y, 0);
        return JsonConvert.SerializeObject(data);
    }

    public virtual void Deserialize(string str)
    {
        Output data = JsonConvert.DeserializeObject<Output>(str);
        guid = data.guid;
        _size = data.size;
        if (_size == Size.Dynamic)
        {
            var rect = Rect;
            rect.size = new Vector2(data.rectSize.x, data.rectSize.y);
            Rect = rect;
        }
        transform.position = new Vector3(data.position.x, data.position.y, data.position.z);
        ChangeColor(new Color(data.color.x, data.color.y, data.color.z));
    }
}
