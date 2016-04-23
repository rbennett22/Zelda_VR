using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]

public class ZeldaText : MonoBehaviour
{

    public string text;
    public string Text { get { return text; } set { text = value; AssignSpriteFromText(text); } }


    SpriteRenderer _spriteRenderer;


    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (text != null)
        {
            AssignSpriteFromText(text);
        }
    }

    void AssignSpriteFromText(string text)
    {
        Texture2D tex = ZeldaFont.Instance.TextureForString(text);
        if (tex == null) { return; }

        Rect r = new Rect(0, 0, tex.width, tex.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);

        _spriteRenderer.sprite = Sprite.Create(tex, r, pivot);
    }

}