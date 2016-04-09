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

        SoundFx.Instance.PlayOneShot(SoundFx.Instance.secret);
        Destroy(gameObject);
    }

    public void Bomb()
    {
        if (!isBombable) { return; }

        SoundFx.Instance.PlayOneShot(SoundFx.Instance.secret);
        Destroy(gameObject);
    }


    void OnDestroy()
    {
        Destroy(linkedBlock);
    }

}