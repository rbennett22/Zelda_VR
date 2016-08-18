using UnityEngine;

public class Grotto_PayRupees : GrottoExtension_Base
{
    [SerializeField]
    Transform _giftContainer;
    [SerializeField]
    ZeldaText _giftTextDisplay;


    Collectible _giftItem;
    Collectible GiftPrefab { get { return GrottoSpawnPoint.giftPrefab; } }
    int GiftAmount { get { return GrottoSpawnPoint.giftAmount; } }


    override public void ShowTheGoods(bool doShow = true)
    {
        Inventory.Instance.SpendRupees(-GiftAmount);

        ShowRupeeDeduction(doShow);

        Grotto.DisplayMessage(doShow);
    }

    void ShowRupeeDeduction(bool doShow = true)
    {
        _giftTextDisplay.Text = GiftAmount.ToString();
        _giftContainer.gameObject.SetActive(doShow);
    }
}