using Moths.Collections;
using UnityEngine;

public class UICameraScalable : MonoBehaviour
{
    [Range(0, 1)]
    public float heightRatio = 0.15f;
    public RangeFloat scaleRange = new RangeFloat(0, 1);

    private void Update()
    {
        var worldRect = CameraController.Instance.WorldRect;

        float height = ((RectTransform)transform).sizeDelta.y;

        float targetHeight = heightRatio * worldRect.height;

        transform.localScale = Vector3.one * Mathf.Clamp(targetHeight / height, scaleRange.min, scaleRange.max);
    }
}
