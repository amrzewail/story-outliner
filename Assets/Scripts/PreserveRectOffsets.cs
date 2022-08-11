using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreserveRectOffsets : MonoBehaviour
{
    private Vector2 _offsetMin;
    private Vector2 _offsetMax;

    void Start()
    {
        _offsetMin = ((RectTransform)transform).offsetMin;
        _offsetMax = ((RectTransform)transform).offsetMax;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ((RectTransform)transform).offsetMin = _offsetMin;
        ((RectTransform)transform).offsetMax = _offsetMax;
    }
}
