using UnityEngine;

public class CollectibleSpawnPoint : MonoBehaviour 
{
    public GameObject collectiblePrefab;
    public bool hideUnderArmos;


    public bool HasBeenCollected { get; set; }
    public Collectible SpawnedCollectible { get; private set; }


    void Awake()
    {
        GetComponent<Renderer>().enabled = false;
    }


	public GameObject SpawnCollectible () 
    {
        if (HasBeenCollected) { return null; }

        GameObject g = Instantiate(collectiblePrefab) as GameObject;
        g.name = collectiblePrefab.name;
        g.transform.position = transform.position;

        Transform collectiblesContainer = GameObject.Find("Special Collectibles").transform;
        g.transform.parent = collectiblesContainer;

        SpawnedCollectible = g.GetComponent<Collectible>();
        SpawnedCollectible.SpawnPoint = this;

        if (hideUnderArmos) { g.SetActive(false); }

        return g;
	}

    public void DestroySpawnedCollectible()
    {
        if(SpawnedCollectible == null)
        {
            return;
        }
        Destroy(SpawnedCollectible.gameObject);
        SpawnedCollectible = null;
    }

    public void OnItemCollected(Collectible c)
    {
        if (c == null || c != SpawnedCollectible) { return; }

        HasBeenCollected = true;
        SpawnedCollectible = null;
    }


    /*#region Serialization

    public class Serializable
    {
        public bool hasBeenCollected;
    }

    public Serializable GetSerializable()
    {
        Serializable info = new Serializable();

        info.hasBeenCollected = HasBeenCollected;

        return info;
    }

    public void InitWithSerializable(Serializable info)
    {
        HasBeenCollected = info.hasBeenCollected;
    }

    #endregion*/

}