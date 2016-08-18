using UnityEngine;
using System.Collections.Generic;

public class Grotto_Shop : GrottoExtension_Base
{
    [SerializeField]
    SaleItemView[] _saleItemViews = new SaleItemView[0];
    [SerializeField]
    GameObject _rupeePriceSymbol;


    List<Collectible> _salesItems = new List<Collectible>();

    bool _hasMadePurchase;


    bool IsSoldOut
    {
        get
        {
            foreach (Collectible c in _salesItems)
            {
                if (c != null) { return false; }
            }
            return true;
        }
    }


    override public void OnPlayerExit()
    {
        _hasMadePurchase = false;
    }

    override public void ShowTheGoods(bool doShow = true)
    {
        ShowTheShopItems(doShow);

        Grotto.DisplayMessage(!IsSoldOut);
    }
    void ShowTheShopItems(bool doShow = true)
    {
        if(_salesItems.Count == 0)
        {
            InstantiateSalesItems();
        }

        for (int i = 0; i < _salesItems.Count; i++)
        {
            Collectible c = _salesItems[i];
            SaleItemView v = _saleItemViews[i];         // TODO: Instantiate these dynamically
            
            if (c == null || v == null) { continue; }

            Item item = Inventory.Instance.GetItem(c.name);
            v.gameObject.SetActive(doShow && !item.IsMaxedOut);
        }

        _rupeePriceSymbol.SetActive(doShow);

        return;
    }

    void InstantiateSalesItems()
    {
        GrottoSpawnPoint gsp = GrottoSpawnPoint;

        for (int i = 0; i < gsp.SaleItemPrefabs.Count; i++)
        {
            Collectible cPrefab = gsp.SaleItemPrefabs[i];
            SaleItemView v = _saleItemViews[i];         // TODO: Instantiate these dynamically

            if (cPrefab == null || v == null) { continue; }

            cPrefab.riseUpWhenCollected = false;

            int price = gsp.SaleItemPrices[i];

            string cName = cPrefab.itemPrefab.name;
            Item item = Inventory.Instance.GetItem(cName);
            if (!item.IsMaxedOut)
            {
                Collectible c = Grotto.InstantiateItem(cPrefab.gameObject, v.transform, price);
                c.name = cName;
                _salesItems.Add(c);
            }

            string priceText = price.ToString();
            v.UpdatePriceText(priceText);
            v.gameObject.SetActive(!item.IsMaxedOut);
        }
    }


    override public void OnGrottoItemCollected(Collectible c)
    {
        if (_hasMadePurchase) { return; }
        _hasMadePurchase = true;

        ShowTheGoods(false);
    }
}