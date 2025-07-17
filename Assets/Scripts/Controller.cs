using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using SFB;
using System.Threading.Tasks;

public class Controller : MonoBehaviour
{
    [SerializeField] GridElement _storyEventPrefab;
    [SerializeField] GridElement _characterPrefab;
    [SerializeField] GridElement _notePrefab;
    [SerializeField] TextMeshProUGUI _currentStoryText;

    public GridElement storyEventPrefab => _storyEventPrefab;
    public GridElement characterPrefab => _characterPrefab;
    public GridElement notePrefab => _notePrefab;

    public static Controller Instance { get; private set; }

    private string _currentFile;

    private string CurrentFile
    {
        get => _currentFile;
        set
        {
            _currentFile = value;
            _currentStoryText.text = value;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 120;

        CurrentFile = string.Empty;
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

    public void DeleteSelectionCallback()
    {
        SelectionController.Instance.DeleteSelection();
    }

    public void MoveSelectionCallback()
    {
        SelectionController.Instance.MoveSelection();
    }

    public void NewCallback()
    {
        CurrentFile = string.Empty;
        Serializer.Deserialize("{}");
    }

    public async void LoadCallback()
    {
        string lastFile = PlayerPrefs.GetString("LAST_FILE", "");
        string directory = !string.IsNullOrEmpty(lastFile) ? Path.GetDirectoryName(lastFile) : "";

        await Task.Delay(100);

        string[] files = StandaloneFileBrowser.OpenFilePanel("Select a story", directory, "story", false);

        if (files == null || files.Length == 0) return;

        CurrentFile = files[0];

        var data = File.ReadAllText(CurrentFile);

        Serializer.Deserialize(data);

        PlayerPrefs.SetString("LAST_FILE", CurrentFile);
        PlayerPrefs.Save();
    }

    public void SaveCallback()
    {
        string lastFile = PlayerPrefs.GetString("LAST_FILE", "");
        string directory = !string.IsNullOrEmpty(lastFile) ? Path.GetDirectoryName(lastFile) : "";

        if (string.IsNullOrEmpty(CurrentFile))
        {
            SaveAsCallback();
            return;
        }

        if (string.IsNullOrEmpty(CurrentFile)) return;

        if (File.Exists(CurrentFile))
        {
            File.Copy(CurrentFile, CurrentFile + ".bak", true);
        }

        var str = Serializer.Serialize();
        File.WriteAllText(CurrentFile, str);

        PlayerPrefs.SetString("LAST_FILE", CurrentFile);
        PlayerPrefs.Save();
    }


    public async void SaveAsCallback()
    {
        string lastFile = PlayerPrefs.GetString("LAST_FILE", "");
        string directory = !string.IsNullOrEmpty(lastFile) ? Path.GetDirectoryName(lastFile) : "";

        await Task.Delay(100);

        var newFile = StandaloneFileBrowser.SaveFilePanel("Save a story", directory, "New Story", "story");

        if (string.IsNullOrEmpty(newFile)) return;

        CurrentFile = newFile;
        SaveCallback();
    }


    public void MoveToLayerCallback()
    {
        var selection = SelectionController.Instance.Selection;
        if (selection.Count == 0) return;
        for (int i = 0; i < selection.Count; i++) LayerManager.AddElementToSelectedLayer(selection[i].guid);
    }
}
