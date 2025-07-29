using UnityEngine;

public static class RectExtensions
{
    public static bool Contains(this Rect rect, Rect element)
    {
        var elementRect = element;
        if (elementRect.x > rect.x && elementRect.y < rect.y && (elementRect.x + elementRect.width) < (rect.x + rect.width) && (elementRect.y - elementRect.height) > (rect.y - rect.height))
        {
            return true;
        }
        return false;
    }
}
