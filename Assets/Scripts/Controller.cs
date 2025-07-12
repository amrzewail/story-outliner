using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Controller : MonoBehaviour
{
    [SerializeField] GridElement _storyEventPrefab;
    [SerializeField] GridElement _characterPrefab;
    [SerializeField] GridElement _notePrefab;
    [SerializeField] TMP_InputField _dataInputField;
    public GridElement storyEventPrefab => _storyEventPrefab;
    public GridElement characterPrefab => _characterPrefab;
    public GridElement notePrefab => _notePrefab;

    public static Controller Instance { get; private set; }

    private static string SavePath
    {
        get
        {
            var dir = new System.IO.DirectoryInfo(Application.dataPath);
            string parentDir = dir.Parent.FullName;
            return $"{parentDir}/Stories";
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 120;

        _dataInputField.text = PlayerPrefs.GetString("DATA", "data");
    }

    public void AddStoryEventCallback()
    {

        GridViewport.Instance.InstantiateElement(storyEventPrefab);
    }
    public void AddCharacterCallback()
    {

        GridViewport.Instance.InstantiateElement(characterPrefab);
    }
    public void AddNoteCallback()
    {

        GridViewport.Instance.InstantiateElement(notePrefab);
    }


    public async void LoadCallback()
    {
        if (File.Exists($"{SavePath}/{_dataInputField.text}.json"))
        {
            Serializer.Deserialize(File.ReadAllText($"{SavePath}/{_dataInputField.text}.json"));

            PlayerPrefs.SetString("DATA", _dataInputField.text);
            PlayerPrefs.Save();
        }
        //else
        //{
        //    GridViewport.Instance.Clear();
        //    ArrowController.Instance.Clear();

        //    if (!string.IsNullOrEmpty(_dataInputField.text))
        //    {
        //        SaveCallback();
        //    }
        //}
    }


    public void SaveCallback()
    {
        var str = Serializer.Serialize();
        File.WriteAllText($"{SavePath}/{_dataInputField.text}.json", str);

        PlayerPrefs.SetString("DATA", _dataInputField.text);
        PlayerPrefs.Save();
    }
}
