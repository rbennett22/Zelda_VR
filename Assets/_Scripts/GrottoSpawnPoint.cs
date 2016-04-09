using UnityEngine;
using System.Collections;


public class GrottoSpawnPoint : MonoBehaviour
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


    public GameObject grottoPrefab;
    public GameObject marker;
    public string grottoName;
    public float spawnDistance = 11;

    public GrottoType grottoType;
    public string text;
    public int giftAmount;

    public GameObject npcSpawnPointPrefab;

    public Collectible uniqueCollectiblePrefab;
    public Collectible giftPrefab;

    public bool showEntranceWalls;
    public bool HasSpecialResourceBeenTapped        // (i.e. HeartContainer or Potion collected, Gift collected, UniqueCollectible collected)
    {
        get { return _overworldInfo.HasGrottoBeenTapped(this); }
        set { _overworldInfo.SetGrottoHasBeenTapped(this, value); }
    }      


    public Collectible saleItemPrefabA, saleItemPrefabB, saleItemPrefabC;
    public int saleItemPriceA, saleItemPriceB, saleItemPriceC;

    public string[] payForInfoText;

    
    OVRPlayerController _ovrPlayerController;
    OverworldInfo _overworldInfo;
    Transform _grottosContainer;
    float _spawnDistanceSqd;
    float _destroyDistanceSqd;


    public Grotto SpawnedGrotto { get; private set; }


    void Awake()
    {
        _ovrPlayerController = GameObject.Find("OVRPlayerController").GetComponent<OVRPlayerController>();
        _grottosContainer = GameObject.Find("Grottos").transform;
        _overworldInfo = GameObject.FindGameObjectWithTag("OverworldInfo").GetComponent<OverworldInfo>();

        _spawnDistanceSqd = spawnDistance * spawnDistance;
        _destroyDistanceSqd = _spawnDistanceSqd + 4;

        marker.SetActive(false);
    }


    public Grotto SpawnGrotto()
    {
        GameObject g = Instantiate(grottoPrefab, transform.position, transform.rotation) as GameObject;
        SpawnedGrotto = g.GetComponent<Grotto>();
        SpawnedGrotto.name = grottoName;
        SpawnedGrotto.transform.parent = _grottosContainer;
        SpawnedGrotto.GrottoSpawnPoint = this;

        return SpawnedGrotto;
    }

    public void DestroyGrotto()
    {
        Destroy(SpawnedGrotto.gameObject);
        SpawnedGrotto = null;
    }


    #region Save/Load

    public class Serializable
    {
        public bool hasSpecialResourceBeenTapped;
    }

    public Serializable GetSerializable()
    {
        Serializable info = new Serializable();

        info.hasSpecialResourceBeenTapped = HasSpecialResourceBeenTapped;

        return info;
    }

    public void InitWithSerializable(Serializable info)
    {
        HasSpecialResourceBeenTapped = info.hasSpecialResourceBeenTapped;
    }

    #endregion

}