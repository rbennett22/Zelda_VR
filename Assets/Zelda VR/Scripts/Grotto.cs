using UnityEngine;


public class Grotto : MonoBehaviour
{
    public static Grotto OccupiedGrotto;        // The Grotto the player is currently in


    public GameObject flame1, flame2;
    public Transform npcContainer;
    public Transform uniqueItemContainer;
    public Transform giftContainer;
    public ZeldaText giftTextDisplay;
    public Transform rupeeTriggers;
    public GameObject shopContainer;
    public Transform salesItemContainerA, salesItemContainerB, salesItemContainerC;
    public ZeldaText priceTextDisplayA, priceTextDisplayB, priceTextDisplayC;
    public GameObject rupeePriceSymbol;
    public GameObject entranceWalls;
    

    GrottoSpawnPoint _grottoSpawnPoint;
    EnemySpawnPoint _npcSpawnPoint;
    GameObject _npc;
    Collectible _uniqueCollectibleItem;
    Collectible _giftItem;
    Collectible _salesItemA, _salesItemB, _salesItemC;
    bool _storedFogSetting;
    bool _hasMadeGambleChoice;
    bool _hasMadePayForInfoChoice;


    public GrottoSpawnPoint GrottoSpawnPoint { get { return _grottoSpawnPoint; } set { _grottoSpawnPoint = value; } }
    public GameObject NpcSpawnPointPrefab { get { return _grottoSpawnPoint.npcSpawnPointPrefab; } }
    public GrottoSpawnPoint.GrottoType GrottoType { get { return _grottoSpawnPoint.grottoType; } }
    public string Text { get { return _grottoSpawnPoint.text; } }
    public Collectible UniqueCollectiblePrefab { get { return _grottoSpawnPoint.uniqueCollectiblePrefab; } }
    public bool NoSalesItemsToShow { get { return (_salesItemA == null && _salesItemB == null && _salesItemC == null); } }
    public bool PlayerIsInside { get; private set; }


    void Start()
    {
        if (_npcSpawnPoint == null)
        {
            _npcSpawnPoint = (Instantiate(NpcSpawnPointPrefab) as GameObject).GetComponent<EnemySpawnPoint>();
            _npcSpawnPoint.transform.parent = npcContainer;
            _npcSpawnPoint.transform.localPosition = Vector3.zero;
        }
        if (shopContainer != null) { shopContainer.gameObject.SetActive(false); }
        if (rupeeTriggers != null) { rupeeTriggers.gameObject.SetActive(false); }

        entranceWalls.SetActive(_grottoSpawnPoint.showEntranceWalls);
    }


    public void OnPlayerEnter()
    {
        if (PlayerIsInside) { return; }

        SoundFx sfx = SoundFx.Instance;
        sfx.PlayOneShot(sfx.stairs);

        if (WorldInfo.Instance.IsOverworld)
        {
            _storedFogSetting = RenderSettings.fog;
            RenderSettings.fog = false;
        }

        Music.Instance.Stop();

        ShowFlames();


        if (GrottoType == GrottoSpawnPoint.GrottoType.UniqueItem)
        {
            if (!_grottoSpawnPoint.HasSpecialResourceBeenTapped)
            {
                ShowNpc();
                ShowTheGoods();
            }
        }
        else if (GrottoType == GrottoSpawnPoint.GrottoType.Shop)
        {
            ShowNpc();
            ShowTheGoods();
        }
        else if (GrottoType == GrottoSpawnPoint.GrottoType.Medicine)
        {
            ShowNpc();

            if (Inventory.Instance.HasDeliveredLetterToOldWoman)
            {
                ShowTheGoods();
            }
        }
        else if (GrottoType == GrottoSpawnPoint.GrottoType.Gift)
        {
            ShowNpc();

            if (!_grottoSpawnPoint.HasSpecialResourceBeenTapped)
            {
                ShowTheGoods();
            }
        }
        else if (GrottoType == GrottoSpawnPoint.GrottoType.PayRupees)
        {
            Inventory.Instance.SpendRupees(-_grottoSpawnPoint.giftAmount);

            ShowNpc();
            ShowTheGoods();
        }
        else if (GrottoType == GrottoSpawnPoint.GrottoType.HeartContainer)
        {
            ShowNpc();

            if (!_grottoSpawnPoint.HasSpecialResourceBeenTapped)
            {
                ShowTheGoods();
            }
        }
        else
        {
            ShowNpc();
            ShowTheGoods();
        }

        PlayerIsInside = true;
        OccupiedGrotto = this;
    }

    public void OnPlayerExit()
    {
        if (!PlayerIsInside) { return; }

        SoundFx sfx = SoundFx.Instance;
        sfx.PlayOneShot(sfx.stairs);

        if (WorldInfo.Instance.IsOverworld)
        {
            RenderSettings.fog = _storedFogSetting;
        }

        if (!Music.Instance.IsPlaying)
        {
            Music.Instance.PlayAppropriateMusic();
        }

        ShowFlames(false);
        ShowNpc(false);
        DisplayMessage(false);
        ShowTheGoods(false);

        _hasMadeGambleChoice = false;
        _hasMadePayForInfoChoice = false;
        PlayerIsInside = false;
        OccupiedGrotto = null;
    }


    public void DeliverLetter()
    {
        Inventory.Instance.HasDeliveredLetterToOldWoman = true;

        SoundFx.Instance.PlayOneShot(SoundFx.Instance.secret);

        // TODO

        ShowTheGoods();
    }

    void OnRupeeTrigger(RupeeTrigger rupeeTrigger)
    {
        if (GrottoType == GrottoSpawnPoint.GrottoType.Gamble)
        {
            if (!_hasMadeGambleChoice)
            {
                _hasMadeGambleChoice = true;
                Gamble(rupeeTrigger.id);
            }
        }
        else if (GrottoType == GrottoSpawnPoint.GrottoType.PayForInfo)
        {
            if (!_hasMadePayForInfoChoice)
            {
                PayForInfo(rupeeTrigger.id);
            }
        }
    }


    void Gamble(int rupeeTriggerID)
    {
        int winAmount = 50, loseAmount1 = -10, loseAmount2 = -40;
        int winningsA = 0, winningsB = 0, winningsC = 0;

        // Determine winnings randomly
        int rand = Random.Range(0, 3);
        switch (rand)
        {
            case 0: winningsA = winAmount; winningsB = loseAmount1; winningsC = loseAmount2; break;
            case 1: winningsB = winAmount; winningsC = loseAmount1; winningsA = loseAmount2; break;
            case 2: winningsC = winAmount; winningsA = loseAmount1; winningsB = loseAmount2; break;
        }

        // Reveal winnings in text display
        priceTextDisplayA.Text = winningsA.ToString();
        priceTextDisplayB.Text = winningsB.ToString();
        priceTextDisplayC.Text = winningsC.ToString();

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
        
    }

    void PayForInfo(int rupeeTriggerID)
    {
        int payAmount = 0;
        switch (rupeeTriggerID)
        {
            case 0: payAmount = _grottoSpawnPoint.saleItemPriceA; break;
            case 1: payAmount = _grottoSpawnPoint.saleItemPriceB; break;
            case 2: payAmount = _grottoSpawnPoint.saleItemPriceC; break;
            default: break;
        }

        Inventory inv = Inventory.Instance;
        if (!inv.SpendRupees(payAmount)) { return; }

        string info = _grottoSpawnPoint.payForInfoText[rupeeTriggerID];
        DisplayMessage(true, info);

        ShowTheGoods(false);

        _hasMadePayForInfoChoice = true;
    }


    void ShowFlames(bool doShow = true)
    {
        flame1.SetActive(doShow);
        flame2.SetActive(doShow);
        if (doShow)
        {
            SoundFx.Instance.PlayOneShot(SoundFx.Instance.flame);
        }
    }

    void ShowNpc(bool doShow = true)
    {
        if (doShow)
        {
            if (_npc == null)
            {
                _npc = _npcSpawnPoint.SpawnEnemy();
            }
        }
        else
        {
            Destroy(_npc); 
            _npc = null;
        }
    }


    void ShowTheGoods(bool doShow = true)
    {
        bool displayMessage = doShow;

        if (GrottoType == GrottoSpawnPoint.GrottoType.UniqueItem)
        {
            ShowTheUniqueItem(doShow);
        }
        else if (GrottoType == GrottoSpawnPoint.GrottoType.Shop)
        {
            ShowTheShopItems(doShow);
            if (NoSalesItemsToShow) { displayMessage = false; }
        }
        else if (GrottoType == GrottoSpawnPoint.GrottoType.Medicine)
        {
            ShowTheShopItems(doShow);
            if (NoSalesItemsToShow) { displayMessage = false; }
        }
        else if (GrottoType == GrottoSpawnPoint.GrottoType.Gift)
        {
            ShowGift(doShow);
        }
        else if (GrottoType == GrottoSpawnPoint.GrottoType.PayRupees)
        {
            ShowRupeeDeduction(doShow);
        }
        else if (GrottoType == GrottoSpawnPoint.GrottoType.Gamble)
        {
            ShowGambleChoices(doShow);
        }
        else if (GrottoType == GrottoSpawnPoint.GrottoType.HeartContainer)
        {
            ShowTheShopItems(doShow, false);
            if (NoSalesItemsToShow) { displayMessage = false; }
        }
        else if (GrottoType == GrottoSpawnPoint.GrottoType.PayForInfo)
        {
            ShowPayForInfoChoices(doShow);
        }

        if (displayMessage) { DisplayMessage(); }
    }

    void ShowTheUniqueItem(bool doShow = true)
    {
        if (_uniqueCollectibleItem == null)
        {
            _uniqueCollectibleItem = InstantiateItem(UniqueCollectiblePrefab.gameObject, uniqueItemContainer);
        }
        uniqueItemContainer.gameObject.SetActive(doShow);
    }

    void ShowTheShopItems(bool doShow = true, bool showPrice = true)
    {
        int priceA = _grottoSpawnPoint.saleItemPriceA;
        int priceB = _grottoSpawnPoint.saleItemPriceB;
        int priceC = _grottoSpawnPoint.saleItemPriceC;

        Inventory inv = Inventory.Instance;
        bool showA = false;
        bool showB = false;
        bool showC = false;

        if (_salesItemA == null)
        {
            Collectible a = _grottoSpawnPoint.saleItemPrefabA;
            if (a != null)
            {
                a.riseUpWhenCollected = false;

                Item itemA = inv.GetItem(a.itemPrefab.name);
                showA = itemA.count != itemA.maxCount;
                if (showA)
                {
                    _salesItemA = InstantiateItem(a.gameObject, salesItemContainerA, priceA);
                }
                else
                {
                    priceA = 0;
                }
            }
        }
        if (_salesItemB == null)
        {
            Collectible b = _grottoSpawnPoint.saleItemPrefabB;
            if (b != null)
            {
                b.riseUpWhenCollected = false;

                Item itemB = inv.GetItem(b.itemPrefab.name);
                showB = itemB.count != itemB.maxCount;
                if (showB)
                {
                    _salesItemB = InstantiateItem(b.gameObject, salesItemContainerB, priceB);
                }
                else
                {
                    priceB = 0;
                }
            }
        }
        if (_salesItemC == null)
        {
            Collectible c = _grottoSpawnPoint.saleItemPrefabC;
            if (c != null)
            {
                c.riseUpWhenCollected = false;

                Item itemC = inv.GetItem(c.itemPrefab.name);
                showC = itemC.count != itemC.maxCount;
                if (showC)
                {
                    _salesItemC = InstantiateItem(c.gameObject, salesItemContainerC, priceC);
                }
                else
                {
                    priceC = 0;
                }
            }
        }

        if (showPrice)
        {
            priceTextDisplayA.Text = (priceA == 0) ? " " : priceA.ToString();
            priceTextDisplayB.Text = (priceB == 0) ? " " : priceB.ToString();
            priceTextDisplayC.Text = (priceC == 0) ? " " : priceC.ToString();
        }
        rupeePriceSymbol.SetActive(showPrice);

        if (NoSalesItemsToShow) 
        { 
            shopContainer.gameObject.SetActive(false); 
        }
        else
        {
            shopContainer.gameObject.SetActive(doShow);
        }
    }

    void ShowGift(bool doShow = true)
    {
        if (_giftItem == null)
        {
            Collectible g = _grottoSpawnPoint.giftPrefab;
            if (g != null)
            {
                _giftItem = InstantiateItem(g.gameObject, giftContainer);
            }
        }
        giftTextDisplay.Text = _grottoSpawnPoint.giftAmount.ToString();
        giftContainer.gameObject.SetActive(doShow);
    }

    void ShowRupeeDeduction(bool doShow = true)
    {
        giftTextDisplay.Text = _grottoSpawnPoint.giftAmount.ToString();
        giftContainer.gameObject.SetActive(doShow);
    }

    void ShowGambleChoices(bool doShow = true)
    {
        rupeeTriggers.gameObject.SetActive(doShow);

        string priceStr = "-10";
        priceTextDisplayA.Text = priceStr;
        priceTextDisplayB.Text = priceStr;
        priceTextDisplayC.Text = priceStr;

        shopContainer.gameObject.SetActive(doShow);
    }

    void ShowPayForInfoChoices(bool doShow = true)
    {
        rupeeTriggers.gameObject.SetActive(doShow);

        priceTextDisplayA.Text = "-" + _grottoSpawnPoint.saleItemPriceA.ToString();
        priceTextDisplayB.Text = "-" + _grottoSpawnPoint.saleItemPriceB.ToString();
        priceTextDisplayC.Text = "-" + _grottoSpawnPoint.saleItemPriceC.ToString();

        shopContainer.gameObject.SetActive(doShow);
    }

    Collectible InstantiateItem(GameObject prefab, Transform container, int price = 0)
    {
        Collectible c = (Instantiate(prefab) as GameObject).GetComponent<Collectible>();
        c.Grotto = this;
        c.price = price;
        c.transform.parent = container;
        c.transform.localPosition = Vector3.zero;

        return c;
    }


    public void OnGrottoItemCollected(Collectible c)
    {
        if (GrottoType == GrottoSpawnPoint.GrottoType.UniqueItem)
        {
            FadeAwayNpc();
            ShowTheGoods(false);
            _grottoSpawnPoint.HasSpecialResourceBeenTapped = true;
        }
        else if (GrottoType == GrottoSpawnPoint.GrottoType.Gift)
        {
            Inventory.Instance.ReceiveRupees(_grottoSpawnPoint.giftAmount - 1);
            ShowTheGoods(false);
            _grottoSpawnPoint.HasSpecialResourceBeenTapped = true;
        }
        else if (GrottoType == GrottoSpawnPoint.GrottoType.HeartContainer)
        {
            ShowTheGoods(false);
            _grottoSpawnPoint.HasSpecialResourceBeenTapped = true;
        }
        else
        {
            ShowTheGoods(false);
        }
    }

    void DisplayMessage(bool doShow = true, string msg = null)
    {
        if (msg == null) { msg = Text; }

        if (doShow)
        {
            Vector3 pos = _npc.transform.position;
            Vector3 msgFacingDir = transform.forward;
            MessageBoard.Instance.Display(msg, pos, msgFacingDir);
        }
        else
        {
            MessageBoard.Instance.Hide();
        }
    }

    const float _fadeDuration = 2.0f;
    void FadeAwayNpc()
    {
        iTween.FadeTo(_npc, 0.0f, _fadeDuration);
        iTween.FadeTo(MessageBoard.Instance.gameObject, 0.0f, _fadeDuration);

        Destroy(_npc, _fadeDuration);
    }

}