using UnityEngine;
using System.Collections.Generic;

public class Grotto : MonoBehaviour
{
    public enum GrottoType
    {
        UniqueItem,
        Shop,
        Gamble,
        PayRupees,
        Gift,
        Message,
        Medicine,
        Warp,
        HeartContainer,
        PayForInfo
    }


    public static Grotto OccupiedGrotto;        // The Grotto the player is currently in


    public GameObject entranceWalls;            // The part of the Grotto that exists above ground level
    public GameObject flame1, flame2;
    public Transform npcContainer;
    public Transform rupeeTriggers;
    public ZeldaText priceTextDisplayA, priceTextDisplayB, priceTextDisplayC;

    public GrottoPortal warpA, warpB, warpC;        // TODO: move to Grotto_Warp.cs


    [SerializeField]
    EntranceBlock_Underground _entranceBlock;
    [SerializeField]
    ExitBlock_Underground _exitBlock;


    protected GrottoSpawnPoint _grottoSpawnPoint;
    EnemySpawnPoint _npcSpawnPoint;
    GameObject _npc;

    bool _storedFogSetting;


    #region Grotto Extension

    [SerializeField]
    GameObject _uniqueItemPrefab, _shopPrefab, _gamblePrefab, _payRupeesPrefab, giftPrefab,
        messagePrefab, _medicinePrefab, warpPrefab, _heartContainerPrefab, _payForInfoPrefab;

    Dictionary<GrottoType, GameObject> _extensionPrefabForGrottoType;
    Dictionary<GrottoType, GameObject> ExtensionPrefabForGrottoType
    {
        get
        {
            if (_extensionPrefabForGrottoType == null)
            {
                _extensionPrefabForGrottoType = new Dictionary<GrottoType, GameObject>() {
                    { GrottoType.UniqueItem, _uniqueItemPrefab },
                    { GrottoType.Shop, _shopPrefab },
                    { GrottoType.Gamble, _gamblePrefab },
                    { GrottoType.PayRupees, _payRupeesPrefab },
                    { GrottoType.Gift, giftPrefab },
                    { GrottoType.Message, messagePrefab },
                    { GrottoType.Medicine, _medicinePrefab },
                    { GrottoType.Warp, warpPrefab },
                    { GrottoType.HeartContainer, _heartContainerPrefab },
                    { GrottoType.PayForInfo, _payForInfoPrefab }
                };
            }
            return _extensionPrefabForGrottoType;
        }
    }
    GameObject GetExtensionPrefabForType(GrottoType type)
    {
        if (ExtensionPrefabForGrottoType == null) { return null; }

        GameObject g;
        ExtensionPrefabForGrottoType.TryGetValue(type, out g);
        return g;
    }

    GrottoExtension_Base _extension;
    public GrottoExtension_Base Extension { get { return _extension ?? (_extension = InstantiateExtension(_type)); } }
    GrottoExtension_Base InstantiateExtension(GrottoType type)
    {
        GameObject prefab = GetExtensionPrefabForType(type);
        if (prefab == null)
        {
            return null;
        }

        GameObject g = Instantiate(prefab, transform.position, transform.rotation) as GameObject;
        g.name = type.ToString();
        g.transform.SetParent(transform);

        GrottoExtension_Base e = g.GetComponent<GrottoExtension_Base>();
        e.Grotto = this;

        return e;
    }

    #endregion Grotto Extension


    public GrottoSpawnPoint GrottoSpawnPoint { get { return _grottoSpawnPoint; } set { _grottoSpawnPoint = value; } }
    GrottoType _type;
    public GrottoType Type {
        get { return _type; }
        set {
            if (value == _type) { return; }
            _type = value;

            if (_extension != null)
            {
                Destroy(_extension.gameObject);
            }
            _extension = InstantiateExtension(_type);
        }
    }
    public bool PlayerIsInside { get; private set; }

    GameObject NpcSpawnPointPrefab { get { return _grottoSpawnPoint.npcSpawnPointPrefab; } }
    string MessageStr { get { return _grottoSpawnPoint.text; } }   


    public bool RupeeTriggersActive
    {
        get { return (rupeeTriggers == null) ? false : rupeeTriggers.gameObject.activeSelf; }
        set { if (rupeeTriggers != null) { rupeeTriggers.gameObject.SetActive(value); } }
    }
    public bool WarpsActive
    {
        get { return warpA.gameObject.activeSelf; }
        set {
            warpA.gameObject.SetActive(value);
            warpB.gameObject.SetActive(value);
            warpC.gameObject.SetActive(value);
        }
    }


    void Awake()
    {
        _entranceBlock.Grotto = this;
        _exitBlock.Grotto = this;
    }

    virtual protected void Start()
    {
        InstantiateNPCSpawnPoint();

        RupeeTriggersActive = false;
        WarpsActive = false;

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
        ShowNpc();

        Extension.OnPlayerEnter();

        if (Extension.ShouldShowTheGoods)
        {
            Extension.ShowTheGoods();
        }


        /*bool doShowTheGoods = true;

        if (Type == GrottoType.UniqueItem)
        {
            doShowTheGoods = !_grottoSpawnPoint.HasSpecialResourceBeenTapped;
        }
        else if (Type == GrottoType.Medicine)
        {
            doShowTheGoods = Inventory.Instance.HasDeliveredLetterToOldWoman;
        }
        else if (Type == GrottoType.Gift)
        {
            doShowTheGoods = !_grottoSpawnPoint.HasSpecialResourceBeenTapped;
        }
        else if (Type == GrottoType.PayRupees)
        {
            Inventory.Instance.SpendRupees(-_grottoSpawnPoint.giftAmount);
        }
        else if (Type == GrottoType.Warp)
        {
            WarpsActive = true;
        }
        else if (Type == GrottoType.HeartContainer)
        {
            doShowTheGoods = !_grottoSpawnPoint.HasSpecialResourceBeenTapped;
        }

        if (doShowTheGoods)
        {
            ShowTheGoods();
        }*/
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

        Extension.ShowTheGoods(false);


        Extension.OnPlayerExit();
    }


    void OnRupeeTriggerWasTriggered(RupeeTrigger rupeeTrigger)
    {
        Extension.OnRupeeTrigger(rupeeTrigger);
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


    /*void ShowTheGoods(bool doShow = true)
    {
        bool displayMessage = doShow;

        if (Type == GrottoType.UniqueItem)
        {
            ShowTheUniqueItem(doShow);
        }
        if (Type == GrottoType.Shop)
        {
            ShowTheShopItems(doShow);
            if (IsSoldOut) { displayMessage = false; }
        }
        if (Type == GrottoType.Medicine)
        {
            ShowTheShopItems(doShow);
            if (IsSoldOut) { displayMessage = false; }
        }
        if (Type == GrottoType.Gift)
        {
            ShowGift(doShow);
        }
        if (Type == GrottoType.PayRupees)
        {
            ShowRupeeDeduction(doShow);
        }
        if (Type == GrottoType.Gamble)
        {
            ShowGambleChoices(doShow);
        }
        if (Type == GrottoType.HeartContainer)
        {
            ShowTheShopItems(doShow, false);
            if (IsSoldOut) { displayMessage = false; }
        }
        if (Type == GrottoType.PayForInfo)
        {
            ShowPayForInfoChoices(doShow);
        }

        if (displayMessage)
        {
            DisplayMessage();
        }
    }*/


    public Collectible InstantiateItem(GameObject prefab, Transform container, int price = 0)
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
        Extension.OnGrottoItemCollected(c);
        Extension.ShowTheGoods(false);
    }


    public void DeliverLetter()
    {
        (Extension as Grotto_Medicine).DeliverLetter();     // TODO
    }
    public void OnPlayerEnteredPortal(GrottoPortal portal)
    {
        (Extension as Grotto_Warp).OnPlayerEnteredPortal(portal);       // TODO
    }


    public void DisplayMessage(bool doShow = true, string msg = null)
    {
        if (msg == null) { msg = MessageStr; }

        if (doShow)
        {
            Vector3 pos = _npcSpawnPoint.transform.position;
            Vector3 dir = -transform.forward;
            MessageBoard.Instance.Show(msg, pos, dir);
        }
        else
        {
            MessageBoard.Instance.Hide();
        }
    }


    const float NPC_FADE_DURATION = 2.0f;
    public void FadeAwayNpc()
    {
        iTween.FadeTo(_npc, 0.0f, NPC_FADE_DURATION);
        iTween.FadeTo(MessageBoard.Instance.gameObject, 0.0f, NPC_FADE_DURATION);

        Destroy(_npc, NPC_FADE_DURATION);
    }


    void PlaySound_Stairs()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.stairs);
    }
    void PlaySound_Flame()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.flame);
    }
}