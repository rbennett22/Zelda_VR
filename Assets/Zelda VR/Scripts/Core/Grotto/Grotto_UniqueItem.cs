using UnityEngine;

public class Grotto_UniqueItem : GrottoExtension_Base
{
    [SerializeField]
    Transform _uniqueItemContainer;


    Collectible _uniqueCollectibleItem;
    Collectible UniqueCollectiblePrefab { get { return GrottoSpawnPoint.uniqueCollectiblePrefab; } }


    override public bool ShouldShowTheGoods { get { return !HasSpecialResourceBeenTapped; } }

    override public void ShowTheGoods(bool doShow = true)
    {
        ShowTheUniqueItem(doShow);

        Grotto.DisplayMessage(doShow);
    }

    void ShowTheUniqueItem(bool doShow = true)
    {
        if (_uniqueCollectibleItem == null)
        {
            _uniqueCollectibleItem = Grotto.InstantiateItem(UniqueCollectiblePrefab.gameObject, _uniqueItemContainer);
        }
        _uniqueItemContainer.gameObject.SetActive(doShow);
    }


    override public void OnGrottoItemCollected(Collectible c)
    {
        Grotto.FadeAwayNpc();
        HasSpecialResourceBeenTapped = true;
    }
}