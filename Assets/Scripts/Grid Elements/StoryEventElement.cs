using Cysharp.Threading.Tasks;
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
    public void ArrowClickCallback()
    {
        ConnectionController.Instance.PrepareConnection(guid, ConnectionType.OneWay);
    }

    public override async UniTask<string> Serialize()
    {
        Output output = new Output();
        output.parent = await base.Serialize();

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

        return JsonUtility.ToJson(output);
    }

    public override void Deserialize(string str)
    {
        Output output = JsonUtility.FromJson<Output>(str);
        base.Deserialize(output.parent);

        text.text = output.self.text;
    }
}
