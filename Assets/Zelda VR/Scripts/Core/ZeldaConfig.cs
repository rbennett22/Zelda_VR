using Immersio.Utility;

public class ZeldaConfig : Singleton<ZeldaConfig>
{
    public int version, subVersion, subSubVersion;
    public bool defaultMusicEnabled;

    override protected void Awake()
    {
        base.Awake();

        Music.Instance.enabled = defaultMusicEnabled;
    }
}