using UnityEngine;
using System.Collections.Generic;

public class Grotto_Gamble : GrottoExtension_Base
{
    const string PRICE_STR = "-10";


    [SerializeField]
    SaleItemView[] _saleItemViews = new SaleItemView[0];


    bool _hasMadeChoice;


    override public void OnPlayerExit()
    {
        _hasMadeChoice = false;
    }

    override public void ShowTheGoods(bool doShow = true)
    {
        ShowGambleChoices(doShow);
    }
    void ShowGambleChoices(bool doShow = true)
    {
        foreach (SaleItemView v in _saleItemViews)
        {
            v.UpdatePriceText(PRICE_STR);
        }
        Grotto.RupeeTriggersActive = doShow;
    }

    override public void OnRupeeTrigger(RupeeTrigger rupeeTrigger)
    {
        if (_hasMadeChoice) { return; }

        Gamble(rupeeTrigger.id);
    }
    void Gamble(int rupeeTriggerID)
    {
        const int WIN_AMOUNT = 50, LOSE_AMOUNT_1 = -10, LOSE_AMOUNT_2 = -40;

        int winningsA = 0, winningsB = 0, winningsC = 0;

        // Determine winnings randomly
        int rand = Random.Range(0, 3);
        switch (rand)
        {
            case 0: winningsA = WIN_AMOUNT; winningsB = LOSE_AMOUNT_1; winningsC = LOSE_AMOUNT_2; break;
            case 1: winningsB = WIN_AMOUNT; winningsC = LOSE_AMOUNT_1; winningsA = LOSE_AMOUNT_2; break;
            case 2: winningsC = WIN_AMOUNT; winningsA = LOSE_AMOUNT_1; winningsB = LOSE_AMOUNT_2; break;
        }

        // Reveal winnings in text display
        _saleItemViews[0].UpdatePriceText(winningsA.ToString());
        _saleItemViews[1].UpdatePriceText(winningsB.ToString());
        _saleItemViews[2].UpdatePriceText(winningsC.ToString());

        // Reward/Remove winnings from player
        int winnings = 0;
        switch (rupeeTriggerID)
        {
            case 0: winnings = winningsA; break;
            case 1: winnings = winningsB; break;
            case 2: winnings = winningsC; break;
        }
        Inventory inv = Inventory.Instance;
        if (winnings < 0) { inv.SpendRupees(-winnings); }
        else { inv.ReceiveRupees(winnings); }

        _hasMadeChoice = true;
    }
}