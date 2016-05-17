using Immersio.Utility;
using System.Collections;
using UnityEngine;

public class Music : Singleton<Music>
{
    public AudioClip intro, overworld_open, overworld_loop, labyrinth, deathMountain, ending;


    public void PlayIntro() { GetComponent<AudioSource>().loop = true; Play(intro); }
    public void PlayOverworld() { PlayOpeningThenLoop(overworld_open, overworld_loop); }
    public void PlayLabyrinth() { GetComponent<AudioSource>().loop = true; Play(labyrinth); }
    public void PlayDeathMountain() { GetComponent<AudioSource>().loop = true; Play(deathMountain); }
    public void PlayEnding() { GetComponent<AudioSource>().loop = false; Play(ending); }

    public void Play(AudioClip clip, ulong delay = 0)
    {
        if (!_isEnabled) { return; }
        if (GetComponent<AudioSource>().clip == clip && GetComponent<AudioSource>().isPlaying) { return; }
        GetComponent<AudioSource>().clip = clip;
        GetComponent<AudioSource>().Play(delay);
    }
    public void PlayOpeningThenLoop(AudioClip openingClip, AudioClip loopClip)
    {
        if (!_isEnabled) { return; }
        if (GetComponent<AudioSource>().isPlaying && (GetComponent<AudioSource>().clip == openingClip || GetComponent<AudioSource>().clip == loopClip)) { return; }

        GetComponent<AudioSource>().loop = false;
        Play(openingClip);

        StartCoroutine("WaitThenPlay", loopClip);
    }
    public IEnumerator WaitThenPlay(AudioClip loopClip)
    {
        while (IsPlaying) { yield return new WaitForSeconds(0.01f); }

        GetComponent<AudioSource>().loop = true;
        Play(loopClip);
    }
    public void Stop() { GetComponent<AudioSource>().Stop(); StopCoroutine("WaitThenPlay"); }
    public void Pause() { GetComponent<AudioSource>().Pause(); }
    public void Resume() { GetComponent<AudioSource>().Play(); }

    public bool IsPlaying { get { return GetComponent<AudioSource>().isPlaying; } }
    public AudioClip ActiveSong { get { return GetComponent<AudioSource>().clip; } }


    bool _isEnabled = true;
    public bool IsEnabled
    {
        get { return _isEnabled; }
        set
        {
            _isEnabled = value;
            if (_isEnabled)
            {
                PlayAppropriateMusic();
            }
            else
            {
                Stop();
            }
        }
    }

    public void ToggleEnabled()
    {
        IsEnabled = !IsEnabled;
    }

    void OnLevelWasLoaded(int level)
    {
        PlayAppropriateMusic();
    }


    public void PlayAppropriateMusic()
    {
        if (!_isEnabled) { return; }

        WorldInfo w = WorldInfo.Instance;
        if (w.IsTitleScene)
        {
            PlayIntro();
        }
        else if (w.IsOverworld)
        {
            PlayOverworld();
        }
        else if (w.IsInDungeon)
        {
            if (w.DungeonNum == 9)
            {
                PlayDeathMountain();
            }
            else
            {
                PlayLabyrinth();
            }
        }
    }
}