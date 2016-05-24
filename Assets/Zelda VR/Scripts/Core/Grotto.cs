using UnityEngine;

public class Grotto : MonoBehaviour
{
    public static Grotto OccupiedGrotto;        // The Grotto the player is currently in


    public GameObject entranceWalls;            // The part of the Grotto that exists above ground level

    public GameObject flame1, flame2;
    public Transform npcContainer;
    public Transform uniqueItemContainer;
    public Transform giftContainer;
    public ZeldaText giftTextDisplay;

    public GameObject shopContainer;
    public Transform rupeeTriggers;
    public Transform salesItemContainerA, salesItemContainerB, salesItemContainerC;
    public ZeldaText priceTextDisplayA, priceTextDisplayB, priceTextDisplayC;
    public GameObject rupeePriceSymbol;

    
    GrottoSpawnPoint _grottoSpawnPoint;
    EnemySpawnPoint _npcSpawnPoint;
    GameObject _npc;
    Collectible _uniqueCollectibleItem;
    Collectible _giftItem;
    Collectible _salesItemA, _salesItemB, _salesItemC;

    bool _storedFogSetting;
    bool _hasMadeChoice_Gamble;
    bool _hasMadeChoice_PayForInfo;


    public GrottoSpawnPoint GrottoSpawnPoint { get { return _grottoSpawnPoint; } set { _grottoSpawnPoint = value; } }
    public GrottoSpawnPoint.GrottoType GrottoType { get { return _grottoSpawnPoint.grottoType; } }
    public bool PlayerIsInside { get; private set; }

    GameObject NpcSpawnPointPrefab { get { return _grottoSpawnPoint.npcSpawnPointPrefab; } }
    Collectible UniqueCollectiblePrefab { get { return _grottoSpawnPoint.uniqueCollectiblePrefab; } }
    string MessageStr { get { return _grottoSpawnPoint.text; } }   
    bool SoldOut { get { return (_salesItemA == null && _salesItemB == null && _salesItemC == null); } }


    public bool ShopContainerActive
    {
        get { return (shopContainer == null) ? false : shopContainer.activeSelf; }
        set { if (shopContainer != null) { shopContainer.SetActive(value); } }
    }
    public bool RupeeTriggersActive
    {
        get { return (rupeeTriggers == null) ? false : rupeeTriggers.gameObject.activeSelf; }
        set { if (rupeeTriggers != null) { rupeeTriggers.gameObject.SetActive(value); } }
    }


    void Start()
    {
        InstantiateNPCSpawnPoint();

        ShopContainerActive = false;
        RupeeTriggersActive = false;

        entranceWalls.SetActive(_grottoSpawnPoint.showEntranceWalls);
    }

    void InstantiateNPCSpawnPoint()
    {
        if (_npcSpawnPoint != null)
        {
            return;
        }

        GameObject g = Instantiate(NpcSpawnPointPrefab);
        g.transform.SetParent(npcContainer);
        g.transform.localPosition = Vector3.zero;

        _npcSpawnPoint = g.GetComponent<EnemySpawnPoint>();
    }


    public void OnPlayerEnter()
    {
        if (PlayerIsInside) { return; }

        PlayerIsInside = true;
        OccupiedGrotto = this;

        if (WorldInfo.Instance.IsOverworld)
        {
            _storedFogSetting = RenderSettings.fog;
            RenderSettings.fog = false;
        }

        PlaySound_Stairs();
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
    }

    public void OnPlayerExit()
    {
        if (!PlayerIsInside) { return; }

        PlayerIsInside = false;
        OccupiedGrotto = null;

        if (WorldInfo.Instance.IsOverworld)
        {
            RenderSettings.fog = _storedFogSetting;
        }

        if (!Music.Instance.IsPlaying)
        {
            Music.Instance.PlayAppropriateMusic();
        }
        PlaySound_Stairs();

        ShowFlames(false);
        ShowNpc(false);
        DisplayMessage(false);
        ShowTheGoods(false);

        _hasMadeChoice_Gamble = false;
        _hasMadeChoice_PayForInfo = false;
    }


    public void DeliverLetter()
    {
        Inventory.Instance.HasDeliveredLetterToOldWoman = true;

        PlaySound_Secret();

        // TODO

        ShowTheGoods();
    }

    void OnRupeeTrigger(RupeeTrigger rupeeTrigger)
    {
        if (GrottoType == GrottoSpawnPoint.GrottoType.Gamble)
        {
            if (!_hasMadeChoice_Gamble)
            {
                _hasMadeChoice_Gamble = true;
                Gamble(rupeeTrigger.id);
            }
        }
        else if (GrottoType == GrottoSpawnPoint.GrottoType.PayForInfo)
        {
            if (!_hasMadeChoice_PayForInfo)
            {
                PayForInfo(rupeeTrigger.id);
            }
        }
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

        _hasMadeChoice_PayForInfo = true;
    }


    void ShowFlames(bool doShow = true)
    {
        flame1.SetActive(doShow);
        flame2.SetActive(doShow);
        if (doShow)
        {
            PlaySound_Flame();
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
            if (SoldOut) { displayMessage = false; }
        }
        else if (GrottoType == GrottoSpawnPoint.GrottoType.Medicine)
        {
            ShowTheShopItems(doShow);
            if (SoldOut) { displayMessage = false; }
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
            if (SoldOut) { displayMessage = false; }
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

        ShopContainerActive = SoldOut ? false : doShow;
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
        string priceStr = "-10";
        priceTextDisplayA.Text = priceStr;
        priceTextDisplayB.Text = priceStr;
        priceTextDisplayC.Text = priceStr;

        RupeeTriggersActive = doShow;
        ShopContainerActive = doShow;
    }

    void ShowPayForInfoChoices(bool doShow = true)
    {
        priceTextDisplayA.Text = "-" + _grottoSpawnPoint.saleItemPriceA.ToString();
        priceTextDisplayB.Text = "-" + _grottoSpawnPoint.saleItemPriceB.ToString();
        priceTextDisplayC.Text = "-" + _grottoSpawnPoint.saleItemPriceC.ToString();

        RupeeTriggersActive = doShow;
        ShopContainerActive = doShow;
    }

    Collectible InstantiateItem(GameObject prefab, Transform container, int price = 0)
    {
        Collectible c = (Instantiate(prefab) as GameObject).GetComponent<Collectible>();
        c.Grotto = this;
        c.price = price;
        c.transform.SetParent(container);
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
        if (msg == null) { msg = MessageStr; }

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
        iTween.FadeTo(_npc, 
            0.0f, _fadeDuration);
        iTween.FadeTo(MessageBoard.Instance.gameObject, 
            0.0f, _fadeDuration);

        Destroy(_npc, _fadeDuration);
    }


    void PlaySound_Stairs()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.stairs);
    }
    void PlaySound_Secret()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.secret);
    }
    void PlaySound_Flame()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.flame);
    }
}