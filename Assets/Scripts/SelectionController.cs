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
        if (!IsMouseWithinScreen()) return;

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
                _isMoving = !_isMoving;
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

            if (Input.GetMouseButtonDown(0))
            {
                _isMoving = false;
            }
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


    private bool IsMouseWithinScreen()
    {
        Vector3 pos = Input.mousePosition;
        return pos.x >= 0 && pos.x <= Screen.width &&
               pos.y >= 0 && pos.y <= Screen.height;
    }
}
