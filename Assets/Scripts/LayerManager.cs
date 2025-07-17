using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;


public class Layer
{
    public Guid guid;
    public string name;
    public bool isVisible;
    public HashSet<Guid> elements = new HashSet<Guid>();

    public void UpdateElementsVisiblity()
    {
        if (isVisible)
        {
            LayerManager.ShowLayerElements(guid);
        }
        else
        {
            LayerManager.HideLayerElements(guid);
        }
    }
}

public static class LayerManager
{
    private static List<Layer> _layers;

    public static Layer SelectedLayer { get; private set; }

    public static IReadOnlyList<Layer> Layers => _layers;

    public static event Action LayersDeserialized;
    public static event Action LayersUpdated;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        _layers = new List<Layer>();
    }

    public static string Serialize()
    {
        List<SerializableLayer> serializables = new List<SerializableLayer>();
        for (int i = 0; i < _layers.Count; i++) serializables.Add(_layers[i]);
        return JsonConvert.SerializeObject(serializables);
    }

    public static void Deserialize(string str)
    {
        SelectedLayer = null;

        _layers.Clear();
        if (string.IsNullOrEmpty(str)) return;
        List<SerializableLayer> serializables = JsonConvert.DeserializeObject<List<SerializableLayer>>(str);
        for (int i = 0; i < serializables.Count; i++) _layers.Add(serializables[i]);

        LayersDeserialized?.Invoke();
        LayersUpdated?.Invoke();
    }

    public static Layer GetLayer(Guid guid)
    {
        for (int i = 0; i < _layers.Count; i++)
        {
            if (_layers[i].guid == guid)
            {
                return _layers[i];
            }
        }
        return null;
    }

    public static Layer CreateLayer()
    {
        var layer = new Layer();
        layer.guid = Guid.NewGuid();
        layer.name = "New Layer";
        layer.isVisible = true;
        SelectedLayer = layer;
        _layers.Add(layer);
        LayersUpdated?.Invoke();
        return layer;
    }

    public static void DeleteLayer(Guid guid)
    {
        for (int i = 0; i < _layers.Count; i++)
        {
            if (_layers[i].guid == guid)
            {
                var elements = _layers[i].elements;
                foreach (var e in elements)
                {
                    GridViewport.Instance.DeleteElement(GridViewport.Instance.GetElement(e));
                }
                _layers.RemoveAt(i);

                if (SelectedLayer != null && SelectedLayer.guid == guid)
                {
                    if (i > 0) SelectedLayer = _layers[i - 1];
                    else SelectedLayer = null;
                }
                return;
            }
        }
    }

    public static void SelectLayer(Guid guid)
    {
        SelectedLayer = GetLayer(guid);
        LayersUpdated?.Invoke();
    }

    public static void GetLayerElements(Guid layerGuid, List<GridElement> elements)
    {
        if (elements == null) return;
        var layer = GetLayer(layerGuid);
        var allElements = GridViewport.Instance.GetAllElements();
        for (int i = 0; i < allElements.Count; i++)
        {
            if (!layer.elements.Contains(allElements[i].guid)) continue;
            elements.Add(allElements[i]);
        }
    }

    public static void ShowLayerElements(Guid guid)
    {
        var list = new List<GridElement>();
        GetLayerElements(guid, list);
        for (int i = 0; i < list.Count; i++) list[i].gameObject.SetActive(true);
    }

    public static void HideLayerElements(Guid guid)
    {
        var list = new List<GridElement>();
        GetLayerElements(guid, list);
        for (int i = 0; i < list.Count; i++) list[i].gameObject.SetActive(false);
    }

    public static void AddElementToSelectedLayer(Guid elementGuid)
    {
        var currentElementLayer = GetElementLayer(elementGuid);
        if (currentElementLayer != null)
        {
            currentElementLayer.elements.Remove(elementGuid);
        }
        if (SelectedLayer != null)
        {
            SelectedLayer.elements.Add(elementGuid);
        }
        LayersUpdated?.Invoke();
    }

    public static Layer GetElementLayer(Guid elementGuid)
    {
        for(int i = 0; i < _layers.Count; i++)
        {
            if (_layers[i].elements.Contains(elementGuid)) return _layers[i];
        }
        return null;
    }

    public static int GetLayerIndex(Guid guid)
    {
        var layer = GetLayer(guid);
        if (layer == null) return -1;
        return _layers.IndexOf(layer);
    }

    public static void ChangeLayerIndex(Guid guid, int newIndex)
    {
        if (newIndex < 0) return;

        var layer = GetLayer(guid);
        if (layer == null) return;

        int currentIndex = _layers.IndexOf(layer);
        if (currentIndex == -1) return;

        // Clamp newIndex to valid range
        newIndex = Mathf.Min(newIndex, _layers.Count - 1);

        // Remove and re-insert at new index
        _layers.RemoveAt(currentIndex);
        _layers.Insert(newIndex, layer);
    }
}
