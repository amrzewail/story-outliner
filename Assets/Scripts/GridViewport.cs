using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

public class GridViewport : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Serializable]
    public class Data
    {
        public List<string> storyEvents;
    }

    public static GridViewport Instance { get; private set; }

    private List<GridElement> _storyEvents = new List<GridElement>();

    private void Start()
    {
        Instance = this;
    }

    public GridElement InstantiateElement(GridElement g)
    {
        g = Instantiate(g, this.transform.parent).GetComponent<GridElement>();

        var position = g.transform.position;
        position = CameraController.Instance.transform.position;
        position.z = 0;

        g.transform.position = position;
        g.guid = Guid.NewGuid().ToString();

        if (g is StoryEventElement)
        {
            _storyEvents.Add(g);
        }

        return g;
    }

    public T InstantiateChild<T>(T obj) where T : Object
    {
        T g = Instantiate(obj, this.transform.parent);
        return g;
    }

    public GridElement GetElement(string guid)
    {
        foreach(var e in _storyEvents)
        {
            if (e.guid.Equals(guid))
            {
                return e;
            }
        }
        return null;
    }

    public void SetBehind(Transform target)
    {
        target.SetSiblingIndex(transform.GetSiblingIndex() + 1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CameraController.Instance.disable = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        CameraController.Instance.disable = false;
    }

    public virtual string Serialize()
    {
        Data data = new Data();
        data.storyEvents = new List<string>();
        foreach(var e in _storyEvents)
        {
            data.storyEvents.Add(e.Serialize());
        }
        return JsonConvert.SerializeObject(data);
    }

    public virtual void Deserialize(string str)
    {
        foreach (var e in _storyEvents)
        {
            Destroy(e.gameObject);
        }
        _storyEvents.Clear();

        Data data = JsonConvert.DeserializeObject<Data>(str);


        foreach (var e in data.storyEvents)
        {
            var element = InstantiateElement(Controller.Instance.storyEventPrefab);
            element.Deserialize(e);
        }
    }
}
