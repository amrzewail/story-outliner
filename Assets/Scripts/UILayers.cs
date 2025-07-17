using System;
using System.Collections.Generic;
using UnityEngine;

public class UILayers : MonoBehaviour
{
    [SerializeField] UILayer _baseLayer;

    private List<UILayer> _layers = new List<UILayer>();

    private void Start()
    {
        RefreshLayers();

        LayerManager.LayersDeserialized += LayersDeserializedCallback;
    }

    private void OnDestroy()
    {
        LayerManager.LayersDeserialized -= LayersDeserializedCallback;
    }

    private void LayersDeserializedCallback()
    {
        RefreshLayers();
    }

    private void RefreshLayers()
    {
        for (int i = 0; i < _layers.Count; i++)
        {
            Destroy(_layers[i].gameObject);
        }

        _layers.Clear();

        var layers = LayerManager.Layers;

        for (int i = 0; i < layers.Count; i++)
        {
            var uiLayer = Instantiate(_baseLayer, _baseLayer.transform.parent);
            uiLayer.Initialize(layers[i]);
            _layers.Add(uiLayer);
        }
    }

    public void AddLayerCallback()
    {
        LayerManager.CreateLayer();
        RefreshLayers();
    }

    public void RemoveSelectedLayerCallback()
    {
        if (LayerManager.SelectedLayer == null) return;
        LayerManager.DeleteLayer(LayerManager.SelectedLayer.guid);
        RefreshLayers();
    }


    public void LayerDownCallback()
    {
        if (LayerManager.SelectedLayer == null) return;
        var layerIndex = LayerManager.GetLayerIndex(LayerManager.SelectedLayer.guid);
        layerIndex++;
        LayerManager.ChangeLayerIndex(LayerManager.SelectedLayer.guid, layerIndex);
        RefreshLayers();
    }

    public void LayerUpCallback()
    {
        if (LayerManager.SelectedLayer == null) return;
        var layerIndex = LayerManager.GetLayerIndex(LayerManager.SelectedLayer.guid);
        layerIndex--;
        if (layerIndex < 0) return;
        LayerManager.ChangeLayerIndex(LayerManager.SelectedLayer.guid, layerIndex);
        RefreshLayers();
    }
}
