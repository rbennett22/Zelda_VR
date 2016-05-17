using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]

public class OverlayView : MonoBehaviour
{
    RawImage _rawImage;


    public float Transparency
    {
        get { return _rawImage.color.a; }
        set
        {
            Color c = _rawImage.color;
            c.a = value;
            _rawImage.color = c;
        }
    }


    void Awake()
    {
        _rawImage = GetComponent<RawImage>();
    }


    public void FadeIn(float duration)
    {
        _rawImage.CrossFadeAlpha(1, duration, true);
    }
    public void FadeOut(float duration)
    {
        _rawImage.CrossFadeAlpha(0, duration, true);
    }
}