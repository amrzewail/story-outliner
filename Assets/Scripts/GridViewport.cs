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
        public List<string> characters;
    }

    public static GridViewport Instance { get; private set; }

    private List<GridElement> _storyEvents = new List<GridElement>();
    private List<GridElement> _characters = new List<GridElement>();


    private List<GridElement> _allElements = new List<GridElement>();

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
        }else if (g is CharacterElement)
        {
            _characters.Add(g);
        }

        _allElements.Add(g);

        return g;
    }

    public T InstantiateChild<T>(T obj) where T : Object
    {
        T g = Instantiate(obj, this.transform.parent);
        return g;
    }

    public void DeleteElement(GridElement g)
    {
        if (g is StoryEventElement)
        {
            _storyEvents.Remove(g);
        }
        else if (g is CharacterElement)
        {
            _characters.Remove(g);
        }

        _allElements.Remove(g);

        Destroy(g.gameObject);
    }

    public GridElement GetElement(string guid)
    {
        foreach(var e in _allElements)
        {
            if (e.guid.Equals(guid))
            {
                return e;
            }
        }
        return null;
    }

    public List<GridElement> GetAllElements()
    {
        return _allElements;
    }

    public void SetBehind(Transform target)
    {
        target.SetSiblingIndex(transform.GetSiblingIndex() + 1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //CameraController.Instance.disable = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //CameraController.Instance.disable = false;
    }

    public virtual string Serialize()
    {
        Data data = new Data();
        data.storyEvents = new List<string>();
        data.characters = new List<string>();
        foreach(var e in _storyEvents)
        {
            data.storyEvents.Add(e.Serialize());
        }
        foreach(var e in _characters)
        {
            data.characters.Add(e.Serialize());
        }
        return JsonConvert.SerializeObject(data);
    }

    public virtual void Deserialize(string str)
    {
        Clear();

        Data data = JsonConvert.DeserializeObject<Data>(str);

        if (data.storyEvents != null)
        {
            foreach (var e in data.storyEvents)
            {
                var element = InstantiateElement(Controller.Instance.storyEventPrefab);
                element.Deserialize(e);
            }
        }
        if (data.characters != null)
        {
            foreach (var e in data.characters)
            {
                var element = InstantiateElement(Controller.Instance.characterPrefab);
                element.Deserialize(e);
            }
        }
    }

    public void Clear()
    {
        foreach (var e in _allElements)
        {
            Destroy(e.gameObject);
        }
        _storyEvents.Clear();
        _characters.Clear();
        _allElements.Clear();
    }
}
