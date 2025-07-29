using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Colorable : MonoBehaviour
{
    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void SetColor(Color color)
    {
        if (!_image) _image = GetComponent<Image>();
        _image.color = color;
    }

    public Color GetColor()
    {
        if (!_image) _image = GetComponent<Image>();
        return _image.color;
    }
}
