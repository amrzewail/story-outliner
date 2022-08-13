using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectionController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Image _selectionImage;
    private Vector2 _originalPosition;
    private Rect _selectionRect;
    private bool _isSelecting = false;

    private bool _isPointerInside = false;
    private bool _isMoving = false;
    private Vector2 _lastMouseWorldPosition;

    private List<GridElement> _selectedElements;

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

        if (Input.GetKeyDown(KeyCode.G))
        {
            if (_selectedElements != null && _selectedElements.Count > 0)
            {
                if(!_isMoving && _isPointerInside)
                {
                    _isMoving = true;
                }
                else
                {
                    _isMoving = false;
                }
                _lastMouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (_selectedElements != null && _selectedElements.Count > 0)
            {
                foreach(var element in _selectedElements)
                {
                    GridViewport.Instance.DeleteElement(element);
                }
                _selectedElements.Clear();
                _isMoving = false;
            }
        }


        if (_isMoving)
        {

            var currentMouseWorldPosition = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);

            var delta = currentMouseWorldPosition - _lastMouseWorldPosition;

            if(_selectedElements != null)
            {
                _selectedElements.ForEach(x => x.transform.position += (Vector3)delta);
            }

            _lastMouseWorldPosition = currentMouseWorldPosition;
        }
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
                _selectedElements.ForEach(x => x.Deselect());
                _selectedElements.Clear();
                _isMoving = false;
            }

        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {

        Debug.Log("pointer up selection");

        _isSelecting = false;

        _selectionImage.enabled = false;

        if (eventData.button == PointerEventData.InputButton.Left)
        {

            var elements = GridViewport.Instance.GetAllElements().Where(x => Contains(_selectionRect, x.transform.position));

            if (elements.Any())
            {
                _selectedElements = elements.ToList();
                _selectedElements.ForEach(x => x.Select());
            }

        }
    }

    private bool Contains(Rect rect, Vector2 point)
    {
        if(rect.xMin <= point.x && rect.xMax >= point.x)
        {
            if(rect.yMin >= point.y && rect.yMin - rect.height <= point.y)
            {
                return true;
            }
        }
        return false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerInside = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isPointerInside = true;
    }
}
