using Cysharp.Threading.Tasks;
using Moths.Collections;
using RTLTMPro;
using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimelineElement : GridElement
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
        public List<SerializableSlice> slices;
    }


    [Serializable]
    private struct SerializableSlice
    {
        public string year;
        public float position;

        public static implicit operator SerializableSlice(Slice slice)
        {
            return new SerializableSlice
            {
                year = slice.year,
                position = slice.position,
            };
        }
    }

    private struct Slice
    {
        private RectTransform _rectTransform;
        private TMP_InputField _yearInput;
        private UIDraggable _draggable;
        private Button _deleteButton;
        private RectTransform _yearBubble;

        public static Slice Create(RectTransform sliceGameObject)
        {
            var slice = new Slice();
            sliceGameObject.gameObject.SetActive(true);
            slice._rectTransform = sliceGameObject;
            slice._yearInput = sliceGameObject.GetComponentInChildren<TMP_InputField>();
            slice._draggable = sliceGameObject.GetComponentInChildren<UIDraggable>();
            slice._deleteButton = sliceGameObject.GetComponentInChildren<Button>();
            slice._yearBubble = (RectTransform)sliceGameObject.Find("Year Bubble");
            slice.isSelected = false;
            return slice;
        }

        public string year
        {
            get => _yearInput.text;
            set => _yearInput.text = value;
        }

        public float position
        {
            get => _rectTransform.anchoredPosition.x;
            set => _rectTransform.anchoredPosition = new Vector2(value, _rectTransform.anchoredPosition.y);
        }

        public Vector2 yearBubbleWorldPosition
        {
            get => yearBubble.transform.position;
            set => yearBubble.transform.position = new Vector3(value.x, value.y, yearBubble.transform.position.z);
        }

        public RectTransform Transform => _rectTransform;
        public UIDraggable draggable => _draggable;
        public Button deleteButton => _deleteButton;
        public RectTransform yearBubble => _yearBubble;

        public bool isSelected
        {
            set
            {
                var focusable = _draggable.GetComponent<UIAlphaFocusable>();
                focusable.blurAlpha = value ? 1 : 0;
                if (!value)
                {
                    focusable.Blur();
                }
                deleteButton.gameObject.SetActive(value);
            }
        }
        

        public void Destroy()
        {
            GameObject.Destroy(_rectTransform.gameObject);
        }

        public override bool Equals(object obj)
        {
            return obj is Slice slice && slice._rectTransform == _rectTransform;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_rectTransform);
        }
    }


    [SerializeField] TMP_InputField text;
    [SerializeField] CanvasGroup _slicesGroup;
    [SerializeField] RectTransform mainSlice;
    [SerializeField] Button createSliceButton;

    private List<Slice> _slices = new List<Slice>();
    private List<GridElement> _insideElements = new List<GridElement>();
    private Slice _lastSelectedSlice;

    public IReadOnlyList<GridElement> InsideElements => _insideElements;
    public override int SortOrder { get => -1000000; }

    public static bool IsEditing = false;

    protected override void Start()
    {
        base.Start();

        mainSlice.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (IsSelected)
        {
            // slice button
            {
                var sliceBtnRect = (RectTransform)createSliceButton.transform;
                var mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var rect = Rect;
                if (mouseWorld.x > rect.x && mouseWorld.x < rect.x + rect.width)
                {
                    sliceBtnRect.anchoredPosition = new Vector2(sliceBtnRect.parent.InverseTransformPoint(mouseWorld).x + Rect.width / 2, sliceBtnRect.anchoredPosition.y);
                }
            }
        }

        // move year bubble off screen
        if (_slices.Count > 0)
        {
            var cameraRect = CameraController.Instance.WorldRect;
            Vector2 bubbleSize = Vector2.Scale(_slices[0].yearBubble.localScale, Vector2.Scale(new Vector2(0.5f, 1), _slices[0].yearBubble.sizeDelta));
            RangeFloat xBounds = new RangeFloat(Rect.x + bubbleSize.x, Rect.x + Rect.width - bubbleSize.x);

            float yMinCamera = Mathf.Min(cameraRect.y - cameraRect.height + bubbleSize.y, Rect.y);
            float xMinCamera = Mathf.Clamp(cameraRect.min.x + bubbleSize.x, xBounds.min, xBounds.max);
            float xMaxCamera = Mathf.Clamp(cameraRect.max.x - bubbleSize.x, xBounds.min, xBounds.max);


            Slice leftSlice = default;
            Slice rightSlice = default;

            for (int i = 0; i < _slices.Count; i++)
            {
                var slice = _slices[i];
                Vector3 slicePos = _slices[i].Transform.position;

                slice.yearBubble.anchoredPosition = Vector2.zero;

                yMinCamera = Mathf.Max(yMinCamera, slice.yearBubbleWorldPosition.y);

                if (slice.yearBubbleWorldPosition.x < xMinCamera)
                {
                    if (!leftSlice.Transform) leftSlice = slice;
                    else if (slice.position > leftSlice.position) leftSlice = slice;
                }
                if (slice.yearBubbleWorldPosition.x > xMaxCamera)
                {
                    if (!rightSlice.Transform) rightSlice = slice;
                    else if (slice.position < rightSlice.position) rightSlice = slice;
                }
                else if (slice.yearBubbleWorldPosition.y < yMinCamera)
                {
                    slice.yearBubbleWorldPosition = new Vector2(slice.yearBubbleWorldPosition.x, yMinCamera);
                }
            }

            if (leftSlice.Transform)
            {
                leftSlice.yearBubbleWorldPosition = new Vector2(xMinCamera, yMinCamera);
            }

            if (rightSlice.Transform)
            {
                rightSlice.yearBubbleWorldPosition = new Vector2(xMaxCamera, yMinCamera);
            }
        }
    }

    public void ArrowClickCallback()
    {
        ConnectionController.Instance.PrepareConnection(guid, ConnectionType.OneWay);
    }

    private void RefreshInsideElements()
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

    public override void OnStartMove()
    {
        RefreshInsideElements();
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

    public void StartSliceDragCallback()
    {
        _size = Size.Dynamic;
        RefreshInsideElements();
    }

    public void SliceDragCallback(Vector2 dragWorld, RectTransform target)
    {
        for (int i = 0; i < _slices.Count; i++)
        {
            var slice = _slices[i];

            RectTransform sliceDraggable = (RectTransform)slice.draggable.transform;

            if (sliceDraggable.position.x < target.position.x) continue;

            var rect = Rect;

            if (sliceDraggable == target)
            {
                for (int j = 0; j < _insideElements.Count; j++)
                {
                    if (_insideElements[j].Rect.x - rect.x >= slice.position)
                    {
                        _insideElements[j].transform.position += Vector3.right * dragWorld.x;
                    }
                }

                rect.width += dragWorld.x;

                Rect = rect;
            }

            slice.position += dragWorld.x;
            slice.position = Mathf.Clamp(slice.position, 0, rect.width);
        }
    }

    public void CreateSliceCallback()
    {
        _slices.Add(CreateSlice());
    }

    private Slice CreateSlice()
    {
        var sliceGO = GameObject.Instantiate(mainSlice, mainSlice.parent);
        var slice = Slice.Create(sliceGO);
        slice.position = ((RectTransform)createSliceButton.transform).anchoredPosition.x;

        slice.deleteButton.onClick.AddListener(() =>
        {
            slice.Destroy();
            _slices.Remove(slice);
        });

        slice.draggable.OnStartDrag.AddListener(() =>
        {
            IsEditing = true;
            if (_lastSelectedSlice.Transform && _lastSelectedSlice.Transform == slice.Transform) return;
            if (_lastSelectedSlice.Transform)
            {
                _lastSelectedSlice.isSelected = false;
            }
            _lastSelectedSlice = slice;
            _lastSelectedSlice.isSelected = true;
        });

        slice.draggable.OnEndDrag.AddListener(() =>
        {
            IsEditing = false;
        });

        return slice;
    }

    public override void Select()
    {
        base.Select();
        _slicesGroup.interactable = true;
        _slicesGroup.blocksRaycasts = true;

        if (_lastSelectedSlice.Transform)
        {
            _lastSelectedSlice.isSelected = false;
            _lastSelectedSlice = default;
        }
    }

    public override void Deselect()
    {
        base.Deselect();
        _slicesGroup.interactable = false;
        _slicesGroup.blocksRaycasts = false;

        if (_lastSelectedSlice.Transform)
        {
            _lastSelectedSlice.isSelected = false;
            _lastSelectedSlice = default;
        }

        for (int i = 0; i < _slices.Count; i++)
        {
            _slices[i].yearBubble.anchoredPosition = Vector2.zero;
        }
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

        data.slices = new List<SerializableSlice>();
        for (int i = 0; i < _slices.Count; i++) data.slices.Add(_slices[i]);

        output.self = data;

        return JsonUtility.ToJson(output);
    }

    public override void Deserialize(string str)
    {
        Output output = JsonUtility.FromJson<Output>(str);
        base.Deserialize(output.parent);

        text.text = output.self.text;

        if (output.self.slices != null)
        {
            for (int i = 0; i < output.self.slices.Count; i++)
            {
                var slice = CreateSlice();

                slice.position = output.self.slices[i].position;
                slice.year = output.self.slices[i].year;

                _slices.Add(slice);
            }
        }
    }
}
