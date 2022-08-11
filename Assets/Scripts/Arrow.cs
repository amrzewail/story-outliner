using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public void Set(Vector2 from, Vector2 to, float offset)
    {
        transform.position = from;

        Vector2 delta = to - from;

        float angle = Mathf.Atan2(delta.y, delta.x);

        Vector2 size = ((RectTransform)transform).sizeDelta;
        size.x = Vector2.Distance(from, to) - offset;
        ((RectTransform)transform).sizeDelta = size;

        var angles = ((RectTransform)transform).eulerAngles;
        angles.z = Mathf.Rad2Deg * angle;
        ((RectTransform)transform).eulerAngles = angles;
    }
}
