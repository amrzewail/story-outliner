using Moths.Tweens.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArrowConnectable : MonoBehaviour, IPointerClickHandler
{
    private CanvasGroup _canvasGroup;

    [SerializeField] GridElement _targetElement;

    [SerializeField] List<GameObject> _disable;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ConnectionController.Instance.isMakingConnection)
        {
            ConnectionController.Instance.MakeConnection(_targetElement.guid);
        }
    }

    private void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void LateUpdate()
    {
        if (ConnectionController.Instance.isMakingConnection)
        {
            if (!_canvasGroup.blocksRaycasts)
            {
                _canvasGroup.TweenAlpha(1).SetDuration(0.15f).Play();
                _canvasGroup.blocksRaycasts = true;
                foreach (var g in _disable)
                {
                    g.SetActive(false);
                }
            }
        }
        else if (_canvasGroup.blocksRaycasts)
        {
            _canvasGroup.TweenAlpha(0).SetDuration(0.15f).Play();
            _canvasGroup.blocksRaycasts = false;
            foreach (var g in _disable)
            {
                g.SetActive(true);
            }
        }
    }
}
