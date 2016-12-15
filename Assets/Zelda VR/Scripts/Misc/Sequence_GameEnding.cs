using UnityEngine;
using System.Collections;

public class Sequence_GameEnding : Sequence_Base
{
    const float ROOM_FLASH_DURATION = 2.0f;


    override protected IEnumerator DoPlay()
    {
        Locations.Instance.LimitControls();

        Music.Instance.Stop();
        Music.Instance.PlaySpecialMusicB();

        yield return new WaitForSeconds(3);

        // - Zelda says, "Thanks Link, you're the hero of Hyrule."
        // - Link and Zelda each hold up a Triforce
        // - Screen flashes normal/ red for 2 secs

        yield return new WaitForSeconds(ROOM_FLASH_DURATION);

        Music.Instance.PlayEnding();

        // - Text: "Finally, peace returns to Hyrule. This ends the story."
        // - Credits scroll down
        // - Text appears: "Another quest wil start from here. Press the start button."

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

	- "Death Mountain" music stops
	- "Special Music B" plays (need to find this)
	    (3 secs...)
	- Zelda says, 
		"Thanks Link, you're 
		the hero of Hyrule."
	- Link and Zelda each hold up a Triforce
	- Screen flashes normal/red for 2 secs
	    (2 secs...)
	- "Ending" music plays
	- Text appears: 
		"Finally, 
		peace returns to Hyrule. 
		This ends the story."
	- Credits scroll down			(need to get ref)
	- Text appears: 
		"Another quest wil start 
		from here. 
		Press the start button."	(need to get ref)
*/
