using Moths.Tweens;
using Moths.Tweens.Extensions;
using UnityEngine;

public class UILoadingIndicator : MonoBehaviour
{
    private bool _isShowing = false;

    private CanvasGroup _canvasGroup;

    private void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;

        _isShowing = false;
    }

    private void Update()
    {
        bool show = false;
        if (Controller.Instance.IsWriting) show = true;

        if (show != _isShowing)
        {
            Show(show);
        }

        if (_canvasGroup.alpha > 0)
        {
            transform.eulerAngles -= Vector3.forward * 360 * Time.deltaTime;
        }
    }

    private void Show(bool show)
    {
        _isShowing = show;

        if (show)
        {
            transform.localScale = Vector3.zero;
            transform.TweenLocalScale(Vector3.one)
                .SetDuration(0.25f)
                .SetEase(Ease.OutBack)
                .Play();
            _canvasGroup.TweenAlpha(1f)
                .SetDuration(0.125f)
                .Play();
        }
        else
        {
            _canvasGroup.TweenAlpha(0f)
                .SetDuration(0.25f)
                .Play();
        }
    }
}
