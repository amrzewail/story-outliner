using Moths.Collections;
using UnityEngine;

public class UICameraScalable : MonoBehaviour
{
    public RangeFloat scaleRange = new RangeFloat(0, 1);
    [SerializeField] bool xScale = true;
    [SerializeField] bool yScale = true;

    private void Update()
    {
        Vector3 scale = Vector3.one * Mathf.Lerp(scaleRange.min, scaleRange.max, CameraController.Instance.Zoom);
        if (!xScale) scale.x = transform.localScale.x;
        if (!yScale) scale.y = transform.localScale.y;
        transform.localScale = scale;
    }
}
