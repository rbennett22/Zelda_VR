using UnityEngine;
using Immersio.Utility;


[RequireComponent(typeof(AudioSource))]
public class SoundFx : Singleton<SoundFx> 
{

    public GameObject soundSourcePrefab;
    public AudioClip stairs, unlock, key, secret, select, pause, cursor, enemyStun, lowHealth, shield, flame, text;
    public AudioClip arrow, bombBlow, bombDrop, boomerang, maficalRod, sword, swordShoot, bossScream1, bossScream2;
    public AudioClip heart, item, rupee, hit, hurt, kill, die, fanfare, sealDoor;
    public AudioClip triforceFanfare, whistle, gameOver;


    public void PlayOneShot(AudioClip clip)
    {
        GetComponent<AudioSource>().PlayOneShot(clip);
    }

    public void PlayOneShot3D(Vector3 pos, AudioClip clip)
    {
        if (clip == null) { return; }

        GameObject g = Instantiate(soundSourcePrefab, pos, Quaternion.identity) as GameObject;
        g.name = "Temp AudioSource";
        g.GetComponent<AudioSource>().PlayOneShot(clip);
        Destroy(g, clip.length);
    }


    AudioSource _lowHealthAudioSource;
    public void PlayLowHealth(bool doPlay = true)
    {
        if (doPlay)
        {
            if (_lowHealthAudioSource == null)
            {
                GameObject g = Instantiate(soundSourcePrefab) as GameObject;
                _lowHealthAudioSource = g.GetComponent<AudioSource>();
            }

            _lowHealthAudioSource.clip = lowHealth;
            _lowHealthAudioSource.loop = true;
            _lowHealthAudioSource.Play();
        }
        else
        {
            if (_lowHealthAudioSource != null)
            {
                _lowHealthAudioSource.Stop();
            }
        }
    }
	
}
