using UnityEngine;

[ExecuteInEditMode]
public class UICopyAnchoredPosition : MonoBehaviour
{
    public RectTransform target;

    public bool horizontal;
    public bool vertical;

    private void Update()
    {
        var position = ((RectTransform)transform).anchoredPosition;
        
        if (horizontal) position.x = target.anchoredPosition.x;
        if (vertical) position.y = target.anchoredPosition.y;

        ((RectTransform)transform).anchoredPosition = position;
    }
}
