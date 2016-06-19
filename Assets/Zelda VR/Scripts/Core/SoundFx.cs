using Immersio.Utility;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class SoundFx : Singleton<SoundFx>
{
    [SerializeField]
    GameObject _soundSourcePrefab;

    public AudioClip stairs, unlock, key, secret, select, pause, cursor, enemyStun, lowHealth, shield, flame, text,
        arrow, bombBlow, bombDrop, boomerang, maficalRod, sword, swordShoot, bossScream1, bossScream2,
        heart, item, rupee, hit, hurt, kill, die, fanfare, sealDoor,
        triforceFanfare, whistle, gameOver;


    public void PlayOneShot(AudioClip clip)
    {
        GetComponent<AudioSource>().PlayOneShot(clip);
    }

    public void PlayOneShot3D(Vector3 pos, AudioClip clip)
    {
        if (clip == null) { return; }

        GameObject g = Instantiate(_soundSourcePrefab, pos, Quaternion.identity) as GameObject;
        g.name = "Temp AudioSource";
        g.GetComponent<AudioSource>().PlayOneShot(clip);
        Destroy(g, clip.length);
    }


    AudioSource _lowHealthAudioSource;
    public void SetLowHealthSoundIsPlaying(bool doPlay = true)
    {
        if (doPlay)
        {
            if (_lowHealthAudioSource == null)
            {
                GameObject g = Instantiate(_soundSourcePrefab) as GameObject;
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