using System;
using TMPro;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(TMP_InputField))]
public class UIInputField : MonoBehaviour
{
    private TMP_InputField _inputField;

    public static bool IsEditing { get; private set; }

    public bool blockSelection = false;

    private void Awake()
    {
        _inputField = GetComponent<TMP_InputField>();

        _inputField.onSelect.AddListener(SelectCallback);
        _inputField.onEndEdit.AddListener(EndEditCallback);
        _inputField.onDeselect.AddListener(EndEditCallback);
    }

    private void Start()
    {
        GetComponentsInChildren<TMP_SelectionCaret>().ToList().ForEach(c => c.raycastTarget = false);
    }

    private void EndEditCallback(string arg0)
    {
        Debug.Log("End InputField Edit");
        IsEditing = false;
    }

    private void SelectCallback(string arg0)
    {
        Debug.Log("Start InputField Edit");
        IsEditing = true;
    }
}
