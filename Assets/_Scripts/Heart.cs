using UnityEngine;


public class Heart : MonoBehaviour
{

    public Sprite emptySprite, halfFullSprite, fullSprite;


    SpriteRenderer _spriteRenderer;


	void Awake ()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
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