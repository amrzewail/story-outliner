using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using RTLTMPro;
using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoteElement : GridElement
{
    [Serializable]
    private struct Output
    {
        public Data self;
        public string parent;
    }

    [Serializable]
    private struct Data
    {
        public string text;
        public string image;
    }


    [SerializeField] TMP_InputField text;
    [SerializeField] Image _image;

    private List<GridElement> _insideElements = new List<GridElement>();

    public IReadOnlyList<GridElement> InsideElements => _insideElements;

    public override int SortOrder { get => -1000000; }

    public void ArrowClickCallback()
    {
        ConnectionController.Instance.PrepareConnection(guid, ConnectionType.OneWay);
    }

    public override void OnStartMove()
    {
        _insideElements.Clear();
        var elements = GridViewport.Instance.GetAllElements();

        var rect = Rect;
        for (int i = 0; i < elements.Count; i++)
        {
            var element = elements[i];
            if (element == this) continue;
            if (!element.gameObject.activeSelf) continue;
            var elementRect = element.Rect;
            elementRect.size = new Vector2(0, 0);
            if (rect.Contains(elementRect))
            {
                _insideElements.Add(element);
            }
        }
    }

    public override void OnMove(Vector2 offset)
    {
        for (int i = 0; i < _insideElements.Count; i++)
        {
            _insideElements[i].transform.position += new Vector3(offset.x, offset.y, 0);
        }
    }

    public override void DynamicResizeCallback(Vector2 dragWorld, RectTransform target)
    {
        base.DynamicResizeCallback(dragWorld, target);

        var children = transform.parent.GetComponentsInChildren<GridElement>()
            .OrderByDescending(x => x.Rect.width * x.Rect.height)
            .ToArray();

        for (int i = 0; i < children.Length; i++)
        {
            children[i].Transform.SetSiblingIndex(i);
        }
    }

    public async void LoadImageCallback()
    {
        if (_image.sprite)
        {
            _image.sprite = null;
            _image.enabled = false;
            return;
        }

        var cameraDisable = CameraController.Instance.disable;
        CameraController.Instance.disable = true;

        var paths = StandaloneFileBrowser.OpenFilePanel("Select an image", "", new[] { new ExtensionFilter("Image Files", "jpg", "png") }, false);
        if (paths != null && paths.Length > 0)
        {
            var sprite = SpriteLoader.LoadSpriteFromFile(paths[0]);
            if (sprite)
            {
                _image.enabled = true;
                _image.sprite = sprite;
            }
        }

        await UniTask.NextFrame();
        await UniTask.NextFrame();
        await UniTask.NextFrame();
        await UniTask.NextFrame();
        await UniTask.NextFrame();

        CameraController.Instance.disable = cameraDisable;
    }

    public override string Serialize()
    {
        Output output = new Output();
        output.parent = base.Serialize();

        Data data = new Data();
        if(text.textComponent is RTLTextMeshPro)
        {
            data.text = ((RTLTextMeshPro)text.textComponent).OriginalText;
        }
        else
        {
            data.text = text.textComponent.text;
        }

        if (_image.sprite)
        {
            data.image = SpriteLoader.Serialize(_image.sprite);
        }

        output.self = data;

        return JsonConvert.SerializeObject(output);
    }

    public override void Deserialize(string str)
    {
        Output output = JsonConvert.DeserializeObject<Output>(str);
        base.Deserialize(output.parent);

        text.text = output.self.text;
        if (!string.IsNullOrEmpty(output.self.image))
        {
            _image.enabled = true;
            if ((_image.sprite = SpriteLoader.Deserialize(output.self.image)))
            {
                var color = GetColor();
                color.a = 1;
                _image.color = color;
            }
            else
            {
                _image.enabled = false;
            }
        }
        else
        {
            _image.enabled = false;
        }
    }
}
