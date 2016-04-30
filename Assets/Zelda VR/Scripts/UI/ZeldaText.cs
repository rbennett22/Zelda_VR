using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]

public class ZeldaText : MonoBehaviour
{
    [SerializeField]
    string _text;
    public string Text {
        get { return _text; }
        set {
            if(value == _text)
            {
                return;
            }
            _text = value;
            AssignSpriteFromText(_text);
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


    void Update()
    {
        if(!Application.isPlaying)
        {
            Update_EditModeOnly();
        }
    }
    void Update_EditModeOnly()
    {
        Text = _text;
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
}