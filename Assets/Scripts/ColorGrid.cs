using Moths.Tweens.Extensions;

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorGrid : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] GridLayoutGroup _gridLayout;
    [SerializeField] Transform _colorsContainer;

    private bool _isColorSelected = false;
    private Color _selectedColor = Color.gray;
    private CanvasGroup _canvasGroup;

    public static ColorGrid Instance { get; private set; }

    public bool IsShowing => gameObject.activeSelf;

    private void Awake()
    {
        Instance = this;
        ((RectTransform)transform).anchoredPosition = Vector3.zero;//.position = Vector3.zero;

        _canvasGroup = GetComponent<CanvasGroup>();

        var colors = GetColors(12, 7);

        _gridLayout.constraintCount = 7;

        int index = 0;
        foreach(Color col in colors)
        {
            Color color = col;

            if (_colorsContainer.childCount <= index)
            {
                Instantiate(_colorsContainer.GetChild(0), _colorsContainer);
            }

            var image = _colorsContainer.GetChild(index).GetComponent<Image>();
            

            image.color = color;

            image.GetComponent<Button>().onClick.AddListener(() => SelectColor(color));

            image.transform.Find("Selected").gameObject.SetActive(false);

            index++;
        }


        this.gameObject.SetActive(false);
    }

    private void UpdateCurrentSelected()
    {
        for (int i = 0; i < _colorsContainer.childCount; i++)
        {
            Image img = _colorsContainer.GetChild(i).GetComponent<Image>();
            Color diff = img.color - _selectedColor;
            if (new Vector3(diff.r, diff.g, diff.b).magnitude <= 0.02f)
            {
                img.transform.Find("Selected").gameObject.SetActive(true);
            }
            else
            {
                img.transform.Find("Selected").gameObject.SetActive(false);
            }
        }
    }

    private void SelectColor(Color col)
    {

        _isColorSelected = true;
        _selectedColor = col;
    }

    private List<Color> GetColors(int range, int depth)
    {
        List<Color> colors = new List<Color>();

        Color color = Color.red;

        Color[] targets = new Color[]
        {
            Color.red,
            Color.green,
            Color.blue,
            Color.red
        };

        if (range < targets.Length) range = targets.Length;

        float step = (range) / targets.Length;
        int currentTarget = 0;

        for(int i = 0; i < range; i++)
        {
            if (i > 0 && i % step == 0)
            {
                currentTarget++;
                if (currentTarget+1 == targets.Length) break;
            }

            float lerp = (float)(i % step) / step;
            color = targets[currentTarget];

            color = Color.Lerp(color, targets[(currentTarget + 1) % targets.Length], lerp);

            colors.AddRange(GetIntensities(color, depth));
        }

        colors.AddRange(GetIntensities(new Color(0.34f, 0.34f, 0.34f), depth));

        return colors;
    }

    private List<Color> GetIntensities(Color col, int depth = 1)
    {
        var colors = new List<Color>();

        depth--;
        if (depth < 0) depth = 0;

        for(int i = 0; i <= depth; i++)
        {
            var d = depth;
            if (d <= 0) d = 1;

            var intensity = (1f - (float)i / d);

            var color = col;// * intensity;

            float grayscale = intensity - 0.5f;
            if (grayscale >= 0) {
                color = Color.Lerp(color, Color.white, grayscale * 1.6f);
            }
            else
            {
                color = Color.Lerp(color, Color.black, -grayscale * 1.6f);
            }
            color.a = 1;
            colors.Add(color);
        }

        return colors;
    }


    public async Task<Color> Show(Color selected)
    {
        this.gameObject.SetActive(true);
        _canvasGroup.alpha = 0;
        _canvasGroup.TweenAlpha(1)
            .SetDuration(0.35f)
            .Play();

        _selectedColor = selected;
        UpdateCurrentSelected();

        await Task.Run(async () =>
        {
            while (_isColorSelected == false && this)
            {
                await Task.Delay(100);
            }
        });

        if (!this) return Color.black;

        _isColorSelected = false;

        _canvasGroup.alpha = 0;
        this.gameObject.SetActive(false);

        return _selectedColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == 0)
        {

            _isColorSelected = true;
        }
    }
}
