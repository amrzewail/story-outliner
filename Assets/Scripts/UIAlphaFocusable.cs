using Moths.Tweens.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class UIAlphaFocusable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, Range(0, 1)] float _focusAlpha = 1; 
    [SerializeField, Range(0, 1)] float _blurAlpha;

    private CanvasGroup _canvasGroup;

    private void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = _blurAlpha;
    }

    private void OnDisable()
    {
        _canvasGroup.alpha = _blurAlpha;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _canvasGroup.TweenAlpha(_focusAlpha)
            .SetDuration(0.15f)
            .Play();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _canvasGroup.TweenAlpha(_blurAlpha)
            .SetDuration(0.15f)
            .Play();
    }
}
