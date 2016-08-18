using UnityEngine;

public class Grotto_Gift : GrottoExtension_Base
{
    [SerializeField]
    Transform _giftContainer;
    [SerializeField]
    ZeldaText _giftTextDisplay;


    Collectible _giftItem;
    Collectible GiftPrefab { get { return GrottoSpawnPoint.giftPrefab; } }
    int GiftAmount { get { return GrottoSpawnPoint.giftAmount; } }


    override public bool ShouldShowTheGoods { get { return !HasSpecialResourceBeenTapped; } }

    override public void ShowTheGoods(bool doShow = true)
    {
        ShowGift(doShow);

        Grotto.DisplayMessage(doShow);
    }

    void ShowGift(bool doShow = true)
    {
        if (_giftItem == null)
        {
            Collectible c = GiftPrefab;
            if (c != null)
            {
                _giftItem = Grotto.InstantiateItem(c.gameObject, _giftContainer);
            }
        }
        _giftTextDisplay.Text = GiftAmount.ToString();
        _giftContainer.gameObject.SetActive(doShow);
    }


    override public void OnGrottoItemCollected(Collectible c)
    {
        Inventory.Instance.ReceiveRupees(GiftAmount - 1);
        HasSpecialResourceBeenTapped = true;
    }
}