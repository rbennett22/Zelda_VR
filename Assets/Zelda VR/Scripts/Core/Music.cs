using Immersio.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]

public class Music : Singleton<Music>
{
    public AudioClip intro, overworld_open, overworld_loop, labyrinth, deathMountain, ending;


    AudioSource _audio;
    AudioClip _queuedClip;


    public bool IsPlaying { get { return _audio.isPlaying; } }
    public AudioClip ActiveClip { get { return _audio.clip; } }
    public float Volume { get { return _audio.volume; } set { _audio.volume = value; } }


    override protected void Awake()
    {
        base.Awake();

        _audio = GetComponent<AudioSource>();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnEnable()
    {
        PlayAppropriateMusic();
    }
    void OnDisable()
    {
        Stop();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayAppropriateMusic();
    }

    public void PlayAppropriateMusic()
    {
        if (!enabled) { return; }

        WorldInfo w = WorldInfo.Instance;
        if (w.IsTitleScreen)
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

    public void PlayIntro() { Play(intro); }
    public void PlayOverworld() { PlayOpeningThenLoop(overworld_open, overworld_loop); }
    public void PlayLabyrinth() { Play(labyrinth); }
    public void PlayDeathMountain() { Play(deathMountain); }
    public void PlaySpecialMusicA() { /* TODO */ }
    public void PlaySpecialMusicB() { /* TODO */ }
    public void PlayEnding() { Play(ending, false); }

    public void Play(AudioClip clip, bool loop = true, ulong delay = 0)
    {
        if (!enabled) { return; }

        _audio.loop = loop;

        if (IsPlaying && ActiveClip == clip) { return; }

        _audio.clip = clip;
        _audio.Play(delay);
    }
    public void PlayOpeningThenLoop(AudioClip openingClip, AudioClip loopingClip)
    {
        if (!enabled) { return; }
        if (IsPlaying && (ActiveClip == openingClip || ActiveClip == loopingClip)) { return; }

        Play(openingClip, false);

        _queuedClip = loopingClip;
    }


    void Update()
    {
        if(_queuedClip != null)
        {
            if(!IsPlaying)
            {
                Play(_queuedClip);
                _queuedClip = null;
            }
        }
    }


    public void Stop() { _audio.Stop(); _queuedClip = null; }

    public void Pause() { _audio.Pause(); }
    public void UnPause() { _audio.UnPause(); }  
}