using Cysharp.Threading.Tasks;
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
        public string details;
        public string image;
    }


    [SerializeField] TMP_InputField text;
    [SerializeField] TMP_InputField details;
    [SerializeField] Image _image;

    private bool _isAsyncWriting = false;
    private string _serializedImage;

    private Sprite ImageSprite
    {
        get => _image.sprite;
        set => SetImageSprite(value).Forget();
    }

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

    private async UniTaskVoid SetImageSprite(Sprite sprite)
    {
        _image.sprite = sprite;
        
        if (_isAsyncWriting) await UniTask.WaitUntil(() => _isAsyncWriting == false);

        _isAsyncWriting = true;
        _serializedImage = await SpriteLoader.Serialize(sprite);
        _isAsyncWriting = false;
    }

    public async void LoadImageCallback()
    {
        if (_image.sprite)
        {
            ImageSprite = null;
            _image.enabled = false;
            return;
        }

        var cameraDisable = CameraController.Instance.disable;
        CameraController.Instance.disable = true;

        var paths = StandaloneFileBrowser.OpenFilePanel("Select an image", "", new[] { new ExtensionFilter("Image Files", "jpg", "png") }, false);
        if (paths != null && paths.Length > 0)
        {
            if (_isAsyncWriting) await UniTask.WaitUntil(() => _isAsyncWriting == false);

            _isAsyncWriting = true;
            var sprite = await SpriteLoader.LoadSpriteFromFile(paths[0]);
            _isAsyncWriting = false;
            
            if (sprite)
            {
                ImageSprite = sprite;
                _image.enabled = true;
            }
        }

        await UniTask.NextFrame();
        await UniTask.NextFrame();
        await UniTask.NextFrame();
        await UniTask.NextFrame();
        await UniTask.NextFrame();

        CameraController.Instance.disable = cameraDisable;
    }

    public override async UniTask<string> Serialize()
    {
        Output output = new Output();
        output.parent = await base.Serialize();

        Data data = new Data();
        if(text.textComponent is RTLTextMeshPro)
        {
            data.text = ((RTLTextMeshPro)text.textComponent).OriginalText;
        }
        else
        {
            data.text = text.textComponent.text;
        }

        if(details.textComponent is RTLTextMeshPro)
        {
            data.details = ((RTLTextMeshPro)details.textComponent).OriginalText;
        }
        else
        {
            data.details = details.textComponent.text;
        }

        if (ImageSprite)
        {
            if (_isAsyncWriting) await UniTask.WaitUntil(() => _isAsyncWriting == false);
            data.image = _serializedImage;
        }

        output.self = data;

        return JsonUtility.ToJson(output);
    }

    public override void Deserialize(string str)
    {
        Output output = JsonUtility.FromJson<Output>(str);
        base.Deserialize(output.parent);

        text.text = output.self.text;
        details.text = output.self.details;

        if (!string.IsNullOrEmpty(output.self.image))
        {
            _image.enabled = true;
            _serializedImage = output.self.image;
            if (_image.sprite = SpriteLoader.Deserialize(_serializedImage))
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
