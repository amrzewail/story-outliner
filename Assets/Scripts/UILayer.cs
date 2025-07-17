using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UILayer : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] RectTransform _visibility;
    [SerializeField] RectTransform _selection;
    [SerializeField] TMP_InputField _name;

    private Layer _layer;

    private void Awake()
    {
        _visibility.gameObject.SetActive(true);
        _selection.gameObject.SetActive(true);

        _name.onValueChanged.AddListener(ChangeName);

        LayerManager.LayersUpdated += LayersUpdatedCallback;
    }


    private void OnDestroy()
    {
        LayerManager.LayersUpdated -= LayersUpdatedCallback;
    }

    public void Initialize(Layer layer)
    {
        _layer = layer;
        Refresh();
    }

    private void LayersUpdatedCallback()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (_layer != null)
        {
            _name.text = _layer.name;
            _visibility.gameObject.SetActive(_layer.isVisible);

            _layer.UpdateElementsVisiblity();
        }
        _selection.gameObject.SetActive(LayerManager.SelectedLayer == _layer);
        _name.enabled = LayerManager.SelectedLayer == _layer && _layer != null;
    }

    public void ToggleVisibility()
    {
        if (_layer == null) return;

        _layer.isVisible = !_layer.isVisible;
        Refresh();

        _layer.UpdateElementsVisiblity();
    }

    public void ChangeName(string name)
    {
        if (_layer == null) return;
        if (_layer.name == name) return;
        _layer.name = name;
        Refresh();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        LayerManager.SelectLayer(_layer != null ? _layer.guid : default);
    }
}
