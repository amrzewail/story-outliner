using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

public class GridViewport : MonoBehaviour
{
    [Serializable]
    public class Data
    {
        public const int CURRENT_VERSION = 1;

        public int version = CURRENT_VERSION;
        public List<string> storyEvents;
        public List<string> characters;
        public List<string> notes;
        public List<string> timelines;
    }

    public static GridViewport Instance { get; private set; }

    private List<GridElement> _storyEvents = new List<GridElement>();
    private List<GridElement> _characters = new List<GridElement>();
    private List<GridElement> _notes = new List<GridElement>();
    private List<GridElement> _timelines = new List<GridElement>();

    private Dictionary<Guid, GridElement> _allElements = new Dictionary<Guid, GridElement>();

    private void Start()
    {
        Instance = this;
    }

    private void DeserializeElement(List<string> serializedList, GridElement prefab)
    {
        if (serializedList == null) return;
        foreach (var e in serializedList)
        {
            var element = InstantiateElement(prefab);
            _allElements.Remove(element.guid);
            element.Deserialize(e);
            _allElements.TryAdd(element.guid, element);
        }
    }

    public void Clear()
    {
        foreach (var e in _allElements)
        {
            Destroy(e.Value.gameObject);
        }
        _storyEvents.Clear();
        _characters.Clear();
        _notes.Clear();
        _timelines.Clear();
        _allElements.Clear();
    }

    public GridElement InstantiateElement(GridElement g)
    {
        var parent = transform.parent;
        if (g is NoteElement) parent = parent.Find("Notes");
        if (g is TimelineElement) parent = parent.Find("Notes");

        g = Instantiate(g, parent).GetComponent<GridElement>();

        var position = g.transform.position;
        position = CameraController.Instance.transform.position;
        position.z = 0;

        g.transform.position = position;
        g.guid = Guid.NewGuid();

        if (g is StoryEventElement)
        {
            _storyEvents.Add(g);
        }
        else if (g is CharacterElement)
        {
            _characters.Add(g);
        }
        else if (g is NoteElement)
        {
            _notes.Add(g);
        }
        else if (g is TimelineElement)
        {
            _timelines.Add(g);
        }

        _allElements[g.guid] = g;

        if (LayerManager.SelectedLayer != null)
        {
            LayerManager.AddElementToSelectedLayer(g.guid);
        }

        return g;
    }

    public T InstantiateChild<T>(T obj) where T : Object
    {
        T g = Instantiate(obj, this.transform.parent);
        return g;
    }

    public void DeleteElement(GridElement g)
    {
        if (!g) return;

        if (g is StoryEventElement)
        {
            _storyEvents.Remove(g);
        }
        else if (g is CharacterElement)
        {
            _characters.Remove(g);
        }
        else if (g is NoteElement)
        {
            _notes.Remove(g);
        }
        else if (g is TimelineElement)
        {
            _timelines.Remove(g);
        }

        _allElements.Remove(g.guid);

        Destroy(g.gameObject);
    }

    public GridElement GetElement(Guid guid)
    {
        if (_allElements.TryGetValue(guid, out var e)) return e;
        return null;
    }

    public List<GridElement> GetAllElements()
    {
        return _allElements.Values.ToList();
    }

    public void SetBehind(Transform target)
    {
        target.SetSiblingIndex(transform.GetSiblingIndex() + 2);
    }


    public virtual string Serialize()
    {
        Data data = new Data();
        data.storyEvents = new List<string>();
        data.characters = new List<string>();
        data.notes = new List<string>();
        data.timelines = new List<string>();

        _notes = _notes.OrderByDescending(e => e.Rect.width * e.Rect.height).ToList();

        foreach(var e in _storyEvents) data.storyEvents.Add(e.Serialize());
        foreach(var e in _characters) data.characters.Add(e.Serialize());
        foreach(var e in _notes) data.notes.Add(e.Serialize());
        foreach(var e in _timelines) data.timelines.Add(e.Serialize());

        return JsonConvert.SerializeObject(data);
    }

    public virtual void Deserialize(string str)
    {
        Clear();

        if (string.IsNullOrEmpty(str)) return;

        Data data = JsonConvert.DeserializeObject<Data>(str);

        DeserializeElement(data.storyEvents, Controller.Instance.storyEventPrefab);
        DeserializeElement(data.characters, Controller.Instance.characterPrefab);
        DeserializeElement(data.notes, Controller.Instance.notePrefab);
        DeserializeElement(data.timelines, Controller.Instance.timelinePrefab);
    }
}
