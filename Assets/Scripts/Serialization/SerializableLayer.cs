using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public struct SerializableLayer
{
    public string guid;
    public string name;
    public bool isVisible;
    public List<string> elements;

    public static implicit operator Layer(SerializableLayer serializable)
    {
        var layer = new Layer
        {
            guid = new Guid(serializable.guid),
            name = serializable.name,
            isVisible = serializable.isVisible,
        };
        layer.elements = new HashSet<Guid>();
        for (int i = 0; i < serializable.elements.Count; i++) layer.elements.Add(new Guid(serializable.elements[i]));
        return layer;
    }

    public static implicit operator SerializableLayer(Layer layer)
    {
        return new SerializableLayer
        {
            guid = layer.guid.ToString(),
            name = layer.name,
            isVisible = layer.isVisible,
            elements = layer.elements.Select(x => x.ToString()).ToList()
        };
    }
}