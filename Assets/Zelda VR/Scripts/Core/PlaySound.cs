using UnityEngine;

public class PlaySound : MonoBehaviour
{
    public AudioClip clip;


    public void PlayOneShot()
    {
        SoundFx.Instance.PlayOneShot(clip);
    }
    public void PlayOneShot3D(Vector3 pos)
    {
        SoundFx.Instance.PlayOneShot3D(pos, clip);
    }
}