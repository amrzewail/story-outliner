using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridElement : MonoBehaviour, IPointerClickHandler
{

    public enum Size
    {
        Medium,
        Small,
        Large,
        Count
    };

    [Serializable]
    private class Output
    {
        public string guid;
        public V3 position;
        public Size size = Size.Medium;
    }

    private class V3
    {
        public float x, y, z;
    }

    private Vector2 _normalSize;

    protected bool _isMoving = false;

    protected Size _size = Size.Medium;

    public string guid { get; set; }

    public Size size => _size;

    public void MoveClickCallback()
    {
        _isMoving = true;
    }

    protected virtual void Start()
    {
        _normalSize = ((RectTransform)transform).sizeDelta;

        Deselect();
    }


    protected virtual void LateUpdate()
    {
        if (_isMoving)
        {
            //CameraController.Instance.disable = true;

            Vector3 position = transform.position;

            position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            position -= transform.Find("MoveButton").transform.localPosition * 0.5f;

            position.z = 0;

            transform.position = position;

            if (Input.GetMouseButtonDown(0))
            {
                _isMoving = false;
            }
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

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("pointer click");
    }


    public void ResizeCallback()
    {
        _size = (Size)(((int)_size + 1) % (int)Size.Count);
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
        if (type == ConnectionType.Marriage)
        {
            return 0;
        }
        else
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
        data.size = _size;
        return JsonConvert.SerializeObject(data);
    }

    public virtual void Deserialize(string str)
    {
        Output data = JsonConvert.DeserializeObject<Output>(str);
        guid = data.guid;
        transform.position = new Vector3(data.position.x, data.position.y, data.position.z);
        _size = data.size;
    }
}
