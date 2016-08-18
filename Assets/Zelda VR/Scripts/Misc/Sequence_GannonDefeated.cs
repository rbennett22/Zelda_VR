using UnityEngine;
using System.Collections;

public class Sequence_GannonDefeated : Sequence_Base
{
    public EnemyAI_Gannon Gannon { get; set; }


    override protected IEnumerator DoPlay()
    {
        Locations.Instance.LimitControls();
        Gannon.FlashBeforeDeath();

        yield return new WaitForSeconds(1);

        Music.Instance.Stop();
        Music.Instance.PlaySpecialMusicA();
        PlayScream2Sound();
        Gannon.ExplodeIntoPieces();

        yield return new WaitForSeconds(1);

        PlaySecretSound();
        
        // TODO: Room rapidly flashes white for 3 secs(alternating normal/white; on white frame everything black stays black, everything else goes white)

        yield return new WaitForSeconds(3);

        Locations.Instance.RestoreControls();
        Music.Instance.PlayDeathMountain();
        UnsealDoors();

        FinishedPlaying();
    }

    void FinishedPlaying()
    {
        _isPlaying = false;
        Destroy(gameObject);
    }


    void UnsealDoors()
    {
        GetComponent<DungeonRoom>().UnsealDoors();
    }

    void PlayScream2Sound()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.bossScream2);
    }
    void PlaySecretSound()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.secret);
    }
}

/*
(see https://www.youtube.com/watch?v=O0lYnGys3oo @ 4:30)
    - Disable Controls
    - Gannon flashes for 1 sec
        (1 sec...)
    - "Death Mountain" Music stops
    - "Special Music A" plays(need to find this)
    - "Scream 2" sound plays
    - Gannon explodes for 1 sec, creating dust that falls into a pile, and sending out 8 red "sword hits" 
	    (1 sec...)
    - Secret sound plays
    - Room rapidly flashes white for 3 secs(alternating normal/white; on white frame everything black stays black, everything else goes white)
	    (3 secs...)
    - DeathMountain music resumes
    - Sealed doors open
    - Enable Controls
*/
