using UnityEngine;
using System.Collections.Generic;
using Immersio.Utility;


public class MessageBoard : Singleton<MessageBoard>
{

    public Vector3 displayOffset;


    void Start()
    {
        GetComponent<Renderer>().enabled = false;
    }


    public void Display(string text, Vector3 position)
    {
        Display(text, position, Vector3.zero);
    }
    public void Display(string text, Vector3 position, Vector3 facingDirection)
    {
        SetText(text);

        transform.position = position + displayOffset;
        if (facingDirection != Vector3.zero)
        {
            transform.forward = facingDirection;
        }
        GetComponent<Renderer>().enabled = true;

        iTween.FadeTo(MessageBoard.Instance.gameObject, 1.0f, 0.1f);
    }

    public void Hide()
    {
        GetComponent<Renderer>().enabled = false;
    }

    public void SetText(string text)
    {
        Texture2D tex = ZeldaFont.Instance.TextureForString(text);
        if (tex == null) { return; }

        Rect r = new Rect(0, 0, tex.width, tex.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(tex, r, pivot);
    }

}