using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Serializer
{
    [Serializable]
    public class Story
    {
        public const int CURRENT_VERSION = 2;

        public int version;
        public string grid;
        public string arrows;
        public string layers;
    }

    public static string Serialize()
    {
        string grid = GridViewport.Instance.Serialize();
        string arrows = ConnectionController.Instance.Serialize();
        string layers = LayerManager.Serialize();

        return JsonConvert.SerializeObject(new Story
        {
            version = Story.CURRENT_VERSION,
            grid = grid,
            arrows = arrows,
            layers = layers,
        });
    }

    public static void Deserialize(string str)
    {
        Story data = JsonConvert.DeserializeObject<Story>(str);

        GridViewport.Instance.Deserialize(data.grid);
        ConnectionController.Instance.Deserialize(data.arrows);
        LayerManager.Deserialize(data.layers);
    }
}
