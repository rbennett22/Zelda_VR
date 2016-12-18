using UnityEngine;
using System.Collections.Generic;

public class Grotto_PayForInfo : GrottoExtension_Base
{
    [SerializeField]
    SaleItemView[] _saleItemViews = new SaleItemView[0];


    bool _hasMadePurchase;


    override public void OnPlayerExit()
    {
        _hasMadePurchase = false;
    }

    override public void ShowTheGoods(bool doShow = true)
    {
        ShowPayForInfoChoices(doShow);

        Grotto.DisplayMessage(doShow);
    }
    void ShowPayForInfoChoices(bool doShow = true)
    {
        _saleItemViews[0].UpdatePriceText("-" + GrottoSpawnPoint.saleItemPriceA.ToString());
        _saleItemViews[1].UpdatePriceText("-" + GrottoSpawnPoint.saleItemPriceB.ToString());
        _saleItemViews[2].UpdatePriceText("-" + GrottoSpawnPoint.saleItemPriceC.ToString());

        Grotto.RupeeTriggersActive = doShow;
    }

    override public void OnRupeeTrigger(RupeeTrigger rupeeTrigger)
    {
        if (_hasMadePurchase) { return; }

        PayForInfo(rupeeTrigger.id);
    }
    void PayForInfo(int rupeeTriggerID)
    {
        int payAmount = 0;
        switch (rupeeTriggerID)
        {
            case 0: payAmount = GrottoSpawnPoint.saleItemPriceA; break;
            case 1: payAmount = GrottoSpawnPoint.saleItemPriceB; break;
            case 2: payAmount = GrottoSpawnPoint.saleItemPriceC; break;
            default: break;
        }

        Inventory inv = Inventory.Instance;
        if (!inv.SpendRupees(payAmount)) { return; }

        ShowTheGoods(false);

        string info = GrottoSpawnPoint.payForInfoText[rupeeTriggerID];
        Grotto.DisplayMessage(true, info);

        _hasMadePurchase = true;
    }
}