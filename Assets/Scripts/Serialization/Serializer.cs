using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Serializer
{
    [Serializable]
    public class Data
    {
        public string grid;
        public string arrows;
    }

    public static string Serialize()
    {
        string grid = GridViewport.Instance.Serialize();
        string arrows = ConnectionController.Instance.Serialize();

        return JsonConvert.SerializeObject(new Data
        {
            grid = grid,
            arrows = arrows
        });
    }

    public static void Deserialize(string str)
    {
        Data data = JsonConvert.DeserializeObject<Data>(str);

        GridViewport.Instance.Deserialize(data.grid);
        ConnectionController.Instance.Deserialize(data.arrows);
    }
}
