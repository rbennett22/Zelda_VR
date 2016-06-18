using UnityEngine;

public class GrottoSpawnPoint : MonoBehaviour
{
    public GameObject grottoPrefab;
    public GameObject marker;           // Marks the Grotto's ground-level entrance location

    public Grotto.GrottoType grottoType;
    public string text;
    public int giftAmount;

    public GameObject npcSpawnPointPrefab;

    public Collectible uniqueCollectiblePrefab;
    public Collectible giftPrefab;

    public bool showEntranceWalls;                  // Entrance Walls are the part of the Grotto that exists above ground level
    public bool HasSpecialResourceBeenTapped        // (i.e. HeartContainer or Potion collected, Gift collected, UniqueCollectible collected)
    {
        get { return _overworldInfo.HasGrottoBeenTapped(this); }
        set { _overworldInfo.SetGrottoHasBeenTapped(this, value); }
    }


    public Collectible saleItemPrefabA, saleItemPrefabB, saleItemPrefabC;
    public int saleItemPriceA, saleItemPriceB, saleItemPriceC;

    public string[] payForInfoText;


    OverworldInfo _overworldInfo;
    Transform _grottosContainer;


    public Grotto SpawnedGrotto { get; private set; }


    void Awake()
    {
        // TODO: Don't use GameObject.Find

        _grottosContainer = GameObject.Find("Grottos").transform;
        _overworldInfo = GameObject.FindGameObjectWithTag("OverworldInfo").GetComponent<OverworldInfo>();

        marker.SetActive(false);
    }


    public Grotto SpawnGrotto()
    {
        if(SpawnedGrotto != null)
        {
            //Debug.LogWarning("SpawnedGrotto already exists.  It will be Destroyed and replaced with a new instance.");
            DestroyGrotto();
        }
        SpawnedGrotto = InstantiateGrotto();
        return SpawnedGrotto;
    }

    Grotto InstantiateGrotto()
    {
        GameObject g = Instantiate(grottoPrefab, transform.position, transform.rotation) as GameObject;
        g.name = "Grotto - " + grottoType.ToString();
        g.transform.SetParent(_grottosContainer);

        Grotto gr = g.GetComponent<Grotto>();
        gr.GrottoSpawnPoint = this;
        gr.Type = grottoType;

        return gr;
    }

    public void DestroyGrotto()
    {
        if(SpawnedGrotto == null)
            return;
        
        Destroy(SpawnedGrotto.gameObject);
        SpawnedGrotto = null;
    }


    #region Serialization

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

    #endregion Serialization
}