using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Serializer
{
    [Serializable]
    public struct Story
    {
        public const int CURRENT_VERSION = 2;

        public int version;
        public string grid;
        public string arrows;
        public string layers;
    }

    public static async UniTask<string> Serialize()
    {
        await UniTask.NextFrame();

        string grid = await GridViewport.Instance.Serialize();
        string arrows = await ConnectionController.Instance.Serialize();
        string layers = await LayerManager.Serialize();

        return JsonUtility.ToJson(new Story
        {
            version = Story.CURRENT_VERSION,
            grid = grid,
            arrows = arrows,
            layers = layers,
        });
    }

    public static void Deserialize(string str)
    {
        Story data = JsonUtility.FromJson<Story>(str);

        GridViewport.Instance.Deserialize(data.grid);
        ConnectionController.Instance.Deserialize(data.arrows);
        LayerManager.Deserialize(data.layers);
    }
}
