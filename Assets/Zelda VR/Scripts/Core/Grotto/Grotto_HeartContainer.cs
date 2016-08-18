using UnityEngine;

public class Grotto_HeartContainer : GrottoExtension_Base
{
    override public bool ShouldShowTheGoods { get { return !HasSpecialResourceBeenTapped; } }

    override public void OnGrottoItemCollected(Collectible c)
    {
        HasSpecialResourceBeenTapped = true;
    }
}