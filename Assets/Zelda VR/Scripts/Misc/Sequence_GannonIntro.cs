using UnityEngine;
using System.Collections;

public class Sequence_GannonIntro : Sequence_Base
{
    const float HOLD_TRIFORCE_FOR_DURATION = 3.0f;


    public EnemyAI_Gannon Gannon { get; set; }


    override protected IEnumerator DoPlay()
    {
        Locations.Instance.LimitControls();       
        Music.Instance.Stop();
        Music.Instance.PlaySpecialMusicA();
        Gannon.FreezeForIntro();

        // TODO: Link holds up Triforce

        yield return new WaitForSeconds(HOLD_TRIFORCE_FOR_DURATION);

        Locations.Instance.RestoreControls();
        Music.Instance.PlayDeathMountain();
        Gannon.UnfreezeForIntro();

        FinishedPlaying();
    }

    void FinishedPlaying()
    {
        _isPlaying = false;
        Destroy(gameObject);
    }
}

/*
    ---  Sequence Summary ---

    - Disable Controls
    - Initially visible
    - "Death Mountain" Music stops
    - "Special Music A" plays (need to find this)
    - Link holds up Triforce for 3 secs
        (3 secs...)
    - Death Mountain Music plays
    - Enable Controls
*/
