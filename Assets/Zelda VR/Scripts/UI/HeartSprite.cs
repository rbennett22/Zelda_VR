using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]

public class HeartSprite : MonoBehaviour
{
    public Sprite emptySprite, halfFullSprite, fullSprite;


    SpriteRenderer _spriteRenderer;


    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        _spriteRenderer.enabled = true;
    }
    void OnDisable()
    {
        _spriteRenderer.enabled = false;
    }


    public void SetToEmpty()
    {
        _spriteRenderer.sprite = emptySprite;
    }
    public void SetToHalfFull()
    {
        _spriteRenderer.sprite = halfFullSprite;
    }
    public void SetToFull()
    {
        _spriteRenderer.sprite = fullSprite;
    }
}