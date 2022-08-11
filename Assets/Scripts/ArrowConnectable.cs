using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArrowConnectable : MonoBehaviour, IPointerClickHandler
{
    private Image _image;

    [SerializeField] GridElement _targetElement;

    [SerializeField] List<GameObject> _disable;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ArrowController.Instance.isMakingConnection)
        {
            ArrowController.Instance.MakeConnection(_targetElement.guid);
        }
    }

    private void Start()
    {
        _image = GetComponent<Image>();
    }

    private void LateUpdate()
    {
        if (ArrowController.Instance.isMakingConnection)
        {
            _image.enabled = true;
            foreach(var g in _disable)
            {
                g.SetActive(false);
            }
        }
        else
        {
            _image.enabled = false;
            foreach (var g in _disable)
            {
                g.SetActive(true);
            }
        }
    }
}
