using UnityEngine;

public class Block : MonoBehaviour
{
    public int tileCode;

    public bool isBombable;
    public bool isBurnable;
    public bool isShortBlock;

    public GameObject linkedBlock;


    public void Burn()
    {
        if (!isBurnable) { return; }

        PlaySecretSound();
        Destroy(gameObject);
    }

    public void Bomb()
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
}