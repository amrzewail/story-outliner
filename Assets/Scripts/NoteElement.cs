using Newtonsoft.Json;
using RTLTMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NoteElement : GridElement
{
    [Serializable]
    private struct Output
    {
        public Data self;
        public string parent;
    }

    [Serializable]
    private struct Data
    {
        public string text;
    }


    [SerializeField] TMP_InputField text;

    private List<GridElement> _insideElements = new List<GridElement>();

    public void ArrowClickCallback()
    {
        ArrowController.Instance.PrepareConnection(guid, ConnectionType.OneWay);
    }

    protected override void OnStartMove()
    {
        _insideElements.Clear();
        var elements = GridViewport.Instance.GetAllElements();

        var rect = Rect;
        for (int i = 0; i < elements.Count; i++)
        {
            var element = elements[i];
            if (element == this) continue;
            var elementRect = element.Rect;
            if (elementRect.x > rect.x && elementRect.y < rect.y && (elementRect.x + elementRect.width) < (rect.x + rect.width) && (elementRect.y - elementRect.height) > (rect.y - rect.height))
            {
                _insideElements.Add(element);
            }
        }
    }

    protected override void OnMove(Vector2 offset)
    {
        for (int i = 0; i < _insideElements.Count; i++)
        {
            _insideElements[i].transform.position += new Vector3(offset.x, offset.y, 0);
        }
    }

    public override string Serialize()
    {
        Output output = new Output();
        output.parent = base.Serialize();

        Data data = new Data();
        if(text.textComponent is RTLTextMeshPro)
        {
            data.text = ((RTLTextMeshPro)text.textComponent).OriginalText;
        }
        else
        {
            data.text = text.textComponent.text;
        }

        output.self = data;

        return JsonConvert.SerializeObject(output);
    }

    public override void Deserialize(string str)
    {
        Output output = JsonConvert.DeserializeObject<Output>(str);
        base.Deserialize(output.parent);

        text.text = output.self.text;
    }
}
