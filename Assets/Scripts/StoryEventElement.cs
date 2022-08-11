using Newtonsoft.Json;
using RTLTMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StoryEventElement : GridElement
{
    [SerializeField] TMP_InputField text;

    [Serializable]
    private class Output
    {
        public Data self;
        public string parent;
    }

    [Serializable]
    private class Data
    {
        public string text = "";
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
