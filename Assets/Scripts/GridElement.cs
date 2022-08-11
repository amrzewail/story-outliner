using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridElement : MonoBehaviour, IPointerClickHandler
{
    [Serializable]
    private class Output
    {
        public string guid;
        public V3 position;
    }

    private class V3
    {
        public float x, y, z;
    }

    protected bool _isMoving = false;

    public string guid { get; set; }

    public void MoveClickCallback()
    {
        _isMoving = true;
    }

    public void ArrowClickCallback()
    {
        ArrowController.Instance.PrepareConnection(guid);
    }

    private void LateUpdate()
    {
        if (_isMoving)
        {
            CameraController.Instance.disable = true;

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
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("pointer click");
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
        return JsonConvert.SerializeObject(data);
    }

    public virtual void Deserialize(string str)
    {
        Output data = JsonConvert.DeserializeObject<Output>(str);
        guid = data.guid;
        transform.position = new Vector3(data.position.x, data.position.y, data.position.z);
    }
}
