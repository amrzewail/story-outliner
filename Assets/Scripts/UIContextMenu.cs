using Moths.Tweens.Extensions;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIContextMenu : MonoBehaviour
{
    private static UIContextMenu _current;

    private int _openFrameIndex;

    [RuntimeInitializeOnLoadMethod]
    private static void Initialize()
    {
        _current = null;
    }

    private void Awake()
    {
        if (Time.frameCount > 0) return;

        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (_openFrameIndex == Time.frameCount) return;

        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonDown(1))
        {
            _current = null;
            gameObject.SetActive(false);
        }
    }

    public void Toggle()
    {
        if (_current != null && _current != this) _current.gameObject.SetActive(false);
        _current = null;

        gameObject.SetActive(!gameObject.activeSelf);

        if (gameObject.activeSelf)
        {
            _current = this;
            _openFrameIndex = Time.frameCount;

            GetComponent<CanvasGroup>().alpha = 0;
            GetComponent<CanvasGroup>().TweenAlpha(1)
                .SetDuration(0.15f)
                .Play();
        }
    }

    public void ToggleHover()
    {
        if (!_current) return;
        if (_current == this) return;
        Toggle();
    }
}
