using UnityEngine;
using Immersio.Utility;

public class ZeldaConfig : Singleton<ZeldaConfig> 
{
    public float version;
    public bool isDemo;
    public bool defaultMusicEnabled;


    override protected void Awake()
    {
        base.Awake();

        Music.Instance.IsEnabled = defaultMusicEnabled;
    }
}