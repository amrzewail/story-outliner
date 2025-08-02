using System;
using TMPro;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(TMP_InputField))]
public class UIInputField : MonoBehaviour
{
    private TMP_InputField _inputField;
    private TMP_SelectionCaret[] _carets;

    public static bool IsEditing { get; private set; }

    public bool blockSelection = false;

    private void Awake()
    {
        _inputField = GetComponent<TMP_InputField>();

        _inputField.onSelect.AddListener(SelectCallback);
        _inputField.onEndEdit.AddListener(EndEditCallback);
        _inputField.onDeselect.AddListener(EndEditCallback);
    }

    private void Update()
    {
        _inputField.pointSize = _inputField.textComponent.fontSize;
        _inputField.textComponent.rectTransform.offsetMin = Vector2.zero;
        _inputField.textComponent.rectTransform.offsetMax = Vector2.zero;
        for (int i = 0; i < _carets.Length; i++)
        {
            _carets[i].rectTransform.offsetMin = Vector2.zero;
            _carets[i].rectTransform.offsetMax = Vector2.zero;
        }
    }

    private void Start()
    {
        _carets = GetComponentsInChildren<TMP_SelectionCaret>();
        foreach(var c in _carets)
        {
            c.raycastTarget = false;
        }
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
