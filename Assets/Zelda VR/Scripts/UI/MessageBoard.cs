using Immersio.Utility;
using UnityEngine;

public class MessageBoard : Singleton<MessageBoard>
{
    public Vector3 displayOffset;


    SpriteRenderer _spriteRenderer;


    override protected void Awake()
    {
        base.Awake();

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.enabled = false;
    }


    public void Show(string text, Vector3 position, Vector3 facingDirection)
    {
        SetText(text);

        transform.position = position + displayOffset;
        transform.forward = -facingDirection;

        _spriteRenderer.enabled = true;

        iTween.FadeTo(gameObject, 1.0f, 0.1f);
    }

    public void Hide()
    {
        _spriteRenderer.enabled = false;
    }

    public void SetText(string text)
    {
        Texture2D tex = ZeldaFont.Instance.TextureForString(text);
        if (tex == null) { return; }

        Rect r = new Rect(0, 0, tex.width, tex.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);

        _spriteRenderer.sprite = Sprite.Create(tex, r, pivot);
    }
}