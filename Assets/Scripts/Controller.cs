using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Controller : MonoBehaviour
{
    [SerializeField] GridElement _storyEventPrefab;

    public GridElement storyEventPrefab => _storyEventPrefab;

    public static Controller Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void AddStoryEventCallback()
    {

        GridViewport.Instance.InstantiateElement(_storyEventPrefab);
    }
    public void LoadCallback()
    {
        if (File.Exists(Application.dataPath + "/data.json"))
        {
            Serializer.Deserialize(File.ReadAllText(Application.dataPath + "/data.json"));
        }
    }


    public void SaveCallback()
    {
        var str = Serializer.Serialize();

        File.WriteAllText(Application.dataPath + "/data.json", str);
    }
}
