using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] TMP_InputField _input;

    public string Text { get => _input.text; set => _input.text = value; }

    public bool IsInteractable
    {
        set
        {
            if (_interactable != value)
            {
                _interactable = value;
                _canvasGroup.alpha = _interactable ? 1 : 0.65f;
                _canvasGroup.blocksRaycasts = _interactable;
            }
        }
    }

    private bool _interactable = true;
 
    public void Set(Vector2 from, Vector2 to, float fromOffset, float offset)
    {
        Vector2 delta = to - from;

        from += (delta.normalized) * fromOffset;

        transform.position = from;

        float angle = Mathf.Atan2(delta.y, delta.x);

        Vector2 size = ((RectTransform)transform).sizeDelta;
        size.x = Vector2.Distance(from, to) - offset;
        ((RectTransform)transform).sizeDelta = size;

        var angles = ((RectTransform)transform).eulerAngles;
        angles.z = Mathf.Rad2Deg * angle;
        ((RectTransform)transform).eulerAngles = angles;
    
        if (Vector2.Dot(delta, Vector2.right) > 0)
        {
            _input.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            _input.transform.localScale = new Vector3(-1, -1, 1);
        }
    }
}
