using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectionController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Image _selectionImage;
    private Vector2 _originalPosition;
    private Rect _selectionRect;
    private bool _isSelecting = false;

    private bool _isMoving = false;
    private Vector2 _lastMouseWorldPosition;

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


        if (Input.GetMouseButtonDown(0) && CameraController.IsMouseWithinViewport())
        {
            _lastMouseWorldPosition = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0) && !_isSelecting)
        {
            var mousePosition = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if ((mousePosition - _lastMouseWorldPosition).sqrMagnitude < 0.5f * 0.5f)
            {
                if (_selectedElements.Count > 0) _selectedElements.ForEach(x => x.Deselect());
                _selectedElements.Clear();
                var elements = GridViewport.Instance.GetAllElements()
                    .Where(x => Contains(x.Rect, new Rect(mousePosition, Vector2.zero)))
                    .ToList();
                elements.Sort((x1, x2) => ((x1 is NoteElement ? 0 : 10000000) + x1.transform.GetSiblingIndex()).CompareTo((x2 is NoteElement ? 0 : 10000000) + x2.transform.GetSiblingIndex()));

                if (elements.Count > 0) Select(elements.Last());
            }
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

        if (Input.GetKeyDown(KeyCode.G) && CameraController.IsMouseWithinViewport())
        {
            MoveSelection();
        }

        if (Input.GetKeyDown(KeyCode.Delete) && CameraController.IsMouseWithinViewport())
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

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            _selectionImage.transform.SetAsLastSibling();

            Vector3 pos = Camera.main.ScreenToWorldPoint(eventData.position);
            _selectionRect.position = pos;
            _originalPosition = pos;

            _isSelecting = true;

            _selectionImage.enabled = true;


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
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isSelecting = false;

        _selectionImage.enabled = false;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            var elements = GridViewport.Instance.GetAllElements().Where(x => Contains(_selectionRect, x.Rect));

            if (elements.Any())
            {
                _selectedElements = elements.ToList();
                _selectedElements.ForEach(x => x.Select());
            }
        }
    }


    private bool Contains(Rect rect, Rect element)
    {
        var elementRect = element;
        if (elementRect.x > rect.x && elementRect.y < rect.y && (elementRect.x + elementRect.width) < (rect.x + rect.width) && (elementRect.y - elementRect.height) > (rect.y - rect.height))
        {
            return true;
        }
        return false;
    }
}
