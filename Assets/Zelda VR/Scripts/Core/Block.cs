using UnityEngine;

public class Block : MonoBehaviour, IBombable, IBurnable
{
    public int tileCode;

    public bool isBombable;
    public bool isBurnable;
    public bool isShortBlock;

    public GameObject linkedBlock;


    Renderer _renderer;


    void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }


    void IBurnable.Burn(BurningFlame sender)
    {
        if (!isBurnable) { return; }

        PlaySecretSound();
        Destroy(gameObject);
    }

    void IBombable.Bomb(BombExplosion sender)
    {
        if (!isBombable) { return; }

        PlaySecretSound();
        Destroy(gameObject);
    }


    void PlaySecretSound()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.secret);
    }

    void OnDestroy()
    {
        Destroy(linkedBlock);
    }


    public void Colorize(Color color)
    {
        Color c = _renderer.material.color;
        c = color;
        _renderer.material.color = c;
    }
}