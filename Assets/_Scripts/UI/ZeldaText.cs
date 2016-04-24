using UnityEngine;
using UnityEngine.UI;

public class ZeldaText : MonoBehaviour
{
    [SerializeField]
    string _text;
    public string Text {
        get { return _text; }
        set {
            _text = value;
            AssignSpriteFromText(_text);
        }
    }
    void AssignSpriteFromText(string text)
    {
        Texture2D tex = ZeldaFont.Instance.TextureForString(text);
        if (tex == null) { return; }

        Rect r = new Rect(0, 0, tex.width, tex.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);

        Sprite sprite = Sprite.Create(tex, r, pivot);
        if (_image != null)
        {
            _image.sprite = sprite;
        }
        if (_spriteRenderer != null)
        {
            _spriteRenderer.sprite = sprite;
        }
    }


    Image _image;
    SpriteRenderer _spriteRenderer;


    void Awake()
    {
        _image = GetComponent<Image>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        AssignSpriteFromText(_text);
    }
}