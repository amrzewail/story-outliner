using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectionController : MonoBehaviour
{
    private Image _selectionImage;
    private Vector2 _originalPosition;
    private Rect _selectionRect;
    private bool _isSelecting = false;

    private bool _isMoving = false;
    private Vector2 _lastMouseWorldPosition;
    private Vector2 _mouseDownPosition;

    private List<GridElement> _selectedElements = new List<GridElement>();
    private HashSet<GridElement> _selectedMoveExclusions = new HashSet<GridElement>();

    public static SelectionController Instance { get; private set; }

    public bool HasSelection => _selectedElements != null && _selectedElements.Count > 0;

    public IReadOnlyList<GridElement> Selection => _selectedElements;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _selectionImage = new GameObject("Selection Box", typeof(Image)).GetComponent<Image>();

        _selectionImage.transform.SetParent(transform.parent);
        _selectionImage.color = new Color(0.5f, 0.5f, 1, 0.25f);
        _selectionImage.rectTransform.pivot = new Vector2(0, 1);

        _selectionImage.enabled = false;
    }

    private void Update()
    {
        if (!CameraController.IsMouseWithinScreen()) return;

        if (Input.GetMouseButtonDown(0) && CameraController.IsMouseWithinViewport() && !CameraController.IsMouseOverInteractiveElement())
        {
            _mouseDownPosition = Input.mousePosition;
            _lastMouseWorldPosition = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _isSelecting = true;
            StartSelection();
        }

        if (Input.GetMouseButtonUp(0) && _isSelecting)
        {
            ClearSelection();


            float dpi = Screen.dpi;
            if (dpi <= 0) dpi = 96f;

            float mmThreshold = 1.5f;
            float pixelThreshold = mmThreshold * dpi / 25.4f;
            float pixelThresholdSquared = pixelThreshold * pixelThreshold;

            var mousePosition = (Vector2)Input.mousePosition;
            if ((mousePosition - _mouseDownPosition).sqrMagnitude < pixelThresholdSquared)
            {
                EndSelection(false);

                if (_selectedElements.Count > 0) _selectedElements.ForEach(x => x.Deselect());
                _selectedElements.Clear();
                var elements = GridViewport.Instance.GetAllElements()
                    .Where(x => x.gameObject.activeSelf && x.Rect.Contains(new Rect(_lastMouseWorldPosition, Vector2.zero)))
                    .ToList();
                elements.Sort((x1, x2) => (x1.SortOrder + x1.transform.GetSiblingIndex()).CompareTo(x2.SortOrder + x2.transform.GetSiblingIndex()));

                if (elements.Count > 0) Select(elements.Last());
            }
            else
            {
                EndSelection(true);
            }

            _isSelecting = false;
        }

        if (_isSelecting)
        {
            Vector2 currentPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            var pos = _selectionRect.position = _originalPosition;
            var size = currentPosition - _originalPosition;


            size.y *= -1;

            if(size.x < 0)
            {
                pos.x += size.x;
                size.x *= -1;
            }

            if(size.y < 0)
            {
                pos.y -= size.y;
                size.y *= -1;
            }

            _selectionRect.position = _selectionImage.transform.position = pos;
            _selectionRect.size = _selectionImage.rectTransform.sizeDelta = size;
        }

        if (!UIInputField.IsEditing && !TimelineElement.IsEditing && Input.GetKeyDown(KeyCode.G) && CameraController.IsMouseWithinViewport())
        {
            MoveSelection();
        }

        if (!UIInputField.IsEditing && !TimelineElement.IsEditing && Input.GetKeyDown(KeyCode.Delete) && CameraController.IsMouseWithinViewport())
        {
            DeleteSelection();
        }

        if (_isMoving)
        {
            var currentMouseWorldPosition = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);

            var delta = currentMouseWorldPosition - _lastMouseWorldPosition;

            if(_selectedElements != null)
            {
                _selectedMoveExclusions.Clear();

                for (int i = 0; i < _selectedElements.Count; i++)
                {
                    if (_selectedElements[i] is not NoteElement) continue;
                    var insideElements = ((NoteElement)_selectedElements[i]).InsideElements;
                    for (int j = 0; j < insideElements.Count; j++) _selectedMoveExclusions.Add(insideElements[j]);
                }

                for (int i = 0; i < _selectedElements.Count; i++)
                {
                    var x = _selectedElements[i];
                    if (_selectedMoveExclusions.Contains(x)) continue;

                    x.transform.position += (Vector3)delta;
                    x.OnMove(delta);
                }
            }

            _lastMouseWorldPosition = currentMouseWorldPosition;

            if (Input.GetMouseButtonDown(0))
            {
                _isMoving = false;
            }
        }
    }

    public void MoveSelection()
    {
        if (HasSelection)
        {
            _isMoving = !_isMoving;
            _lastMouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (_selectedElements != null)
            {
                if (_isMoving)
                {
                    _selectedElements.ForEach(x => x.OnStartMove());
                }
                else
                {
                    _selectedElements.ForEach(x => x.OnEndMove());
                }
            }
        }
    }

    public void DeleteSelection()
    {
        if (HasSelection)
        {
            foreach (var element in _selectedElements)
            {
                GridViewport.Instance.DeleteElement(element);
            }
            _selectedElements.Clear();
            _isMoving = false;
        }
    }

    public void Select(GridElement element)
    {
        _selectedElements.Clear();
        _selectedElements.Add(element);
        element.Select();
    }

    private void ClearSelection()
    {
        if (_selectedElements != null)
        {
            _selectedElements.ForEach(x =>
            {
                if (x) x.Deselect();
            });
            _selectedElements.Clear();
            _isMoving = false;
        }
    }

    private void StartSelection()
    {
        _selectionImage.transform.SetAsLastSibling();

        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _selectionRect.position = pos;
        _originalPosition = pos;

        _isSelecting = true;

        _selectionImage.enabled = true;
    }

    private void EndSelection(bool grabElements)
    {
        _isSelecting = false;

        _selectionImage.enabled = false;

        if (!grabElements) return;

        var elements = GridViewport.Instance.GetAllElements().Where(x => x.gameObject.activeSelf && _selectionRect.Contains(x.Rect));

        if (elements.Any())
        {
            _selectedElements = elements.ToList();
            _selectedElements.ForEach(x => x.Select());
        }
    }
}
