using UnityEngine;

public class Block : MonoBehaviour, IBombable, IBurnable
{
    public int tileCode;

    public bool isIllusion;
    public bool isBombable;
    public bool isBurnable;
    public bool isShortBlock;

    public GameObject linkedBlock;


    Renderer _renderer;
    Collider _collider;


    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider>();
    }

    void Start()
    {
        if(isIllusion)
        {
            _collider.isTrigger = true;
        }
    }


    void OnTriggerEnter(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;
        if (!CommonObjects.IsPlayer(other)) { return; }

        if (isIllusion)
        {
            PlaySecretSound();
        }
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