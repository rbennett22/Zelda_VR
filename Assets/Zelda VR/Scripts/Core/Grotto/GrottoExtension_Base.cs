using UnityEngine;

public class GrottoExtension_Base : MonoBehaviour
{
    public Grotto Grotto { get; set; }
    public GrottoSpawnPoint GrottoSpawnPoint { get { return (Grotto == null) ? null : Grotto.GrottoSpawnPoint; } }


    protected bool HasSpecialResourceBeenTapped
    {
        get { return GrottoSpawnPoint.HasSpecialResourceBeenTapped; }
        set { GrottoSpawnPoint.HasSpecialResourceBeenTapped = value; }
    }


    virtual public void OnPlayerEnter() { }
    virtual public void OnPlayerExit() { }

    virtual public bool ShouldShowTheGoods { get { return true; } }
    virtual public void ShowTheGoods(bool doShow = true)
    {
        Grotto.DisplayMessage(doShow);
    }
    virtual public void OnGrottoItemCollected(Collectible c) { }
    virtual public void OnRupeeTrigger(RupeeTrigger rupeeTrigger) { }
}