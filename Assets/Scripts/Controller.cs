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
    [SerializeField] TMP_InputField _dataInputField;
    public GridElement storyEventPrefab => _storyEventPrefab;
    public GridElement characterPrefab => _characterPrefab;

    public static Controller Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _dataInputField.text = PlayerPrefs.GetString("DATA", "data");
    }

    public void AddStoryEventCallback()
    {

        GridViewport.Instance.InstantiateElement(_storyEventPrefab);
    }
    public void AddCharacterCallback()
    {

        GridViewport.Instance.InstantiateElement(_characterPrefab);
    }


    public async void LoadCallback()
    {
        if (File.Exists($"{Application.dataPath}/{_dataInputField.text}.json"))
        {
            Serializer.Deserialize(File.ReadAllText($"{Application.dataPath}/{_dataInputField.text}.json"));

            PlayerPrefs.SetString("DATA", _dataInputField.text);
            PlayerPrefs.Save();
        }
        else
        {
            GridViewport.Instance.Clear();
            ArrowController.Instance.Clear();

            if (!string.IsNullOrEmpty(_dataInputField.text))
            {
                SaveCallback();
            }
        }
    }


    public void SaveCallback()
    {
        var str = Serializer.Serialize();

        File.WriteAllText($"{Application.dataPath}/{_dataInputField.text}.json", str);

        PlayerPrefs.SetString("DATA", _dataInputField.text);
        PlayerPrefs.Save();
    }
}
