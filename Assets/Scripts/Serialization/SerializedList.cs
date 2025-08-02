using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SerializedList<T>
{
    [SerializeField] List<T> list;

    public static implicit operator List<T>(SerializedList<T> serializable) => serializable.list == null ? new List<T>() : serializable.list;
    public static implicit operator string(SerializedList<T> serializable) => serializable.Serialize();

    public SerializedList(List<T> list)
    {
        this.list = list;
    }

    public SerializedList(string str)
    {
        if (str.StartsWith("["))
        {
            str = "{\"list\":" + str + "}";
        }
        this = JsonUtility.FromJson<SerializedList<T>>(str);
    }

    public string Serialize()
    {
        string json = JsonUtility.ToJson(this);
        return json;
    }
}