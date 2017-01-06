using Immersio.Utility;
using UnityEngine;

public class CommonObjects : Singleton<CommonObjects>
{
    /*static Player FindPlayer()
    {
        // TODO: cache result
        return FindObjectOfType<Player>();
    }

    public static GameObject Player_G { get { return Instance.player_G ?? (Instance.player_G = FindPlayer().gameObject); } }   
    public static GameObject PlayerController_G { get { return Instance.playerController_G ?? (Instance.playerController_G = FindPlayer().PlayerController.gameObject); } }

    public static Player Player_C { get { return Instance.player_C ?? (Instance.player_C = FindPlayer()); } }
    public static ZeldaPlayerController PlayerController_C { get { return Instance.playerController_C ?? (Instance.playerController_C = FindPlayer().PlayerController); } }
    */

    public static GameObject Player_G { get { return Instance.player_G; } }
    public static GameObject PlayerController_G { get { return Instance.playerController_G; } }

    public static Player Player_C { get { return Instance.player_C; } }
    public static ZeldaPlayerController PlayerController_C { get { return Instance.playerController_C; } }


    public static Transform ProjectilesContainer { get { return Instance._projectilesContainer; } }
    public static Transform EnemiesContainer { get { return Instance._enemiesContainer; } }


    public GameObject player_G;
    public Player player_C;
    public GameObject playerController_G;
    public ZeldaPlayerController playerController_C;


    public GameObject primaryCamera;
    public static GameObject PrimaryCamera { get { return Instance.primaryCamera; } }
    static GameObject FindPrimaryCamera()
    {
        GameObject cam = null;

        if (Camera.main != null)
        {
            cam = Camera.main.gameObject;
        }
        else
        {
            GameObject[] cams = GameObject.FindGameObjectsWithTag("MainCamera");
            if (cams != null && cams.Length > 0)
            {
                cam = cams[0];
            }
        }

        return cam;
    }


    [SerializeField]
    Transform _projectilesContainer, _enemiesContainer;

    public Canvas headSpaceCanvas;

    public GameObject enemyDeathAnimation;
    public Material[] enemyStatueMaterials;
    public GameObject invisibleBlockStatuePrefab;


    public static bool IsPlayer(GameObject g)
    {
        if (g.tag == "Player")
        {
            return true;
        }
        Transform p = g.transform.parent;
        return (p != null) && (p.tag == "Player");
    }

    public Material GetEnemyStatueMaterialForDungeon(int dungeonNum)
    {
        return enemyStatueMaterials[dungeonNum - 1];
    }

    public static TileMap OverworldTileMap
    {
        get
        {
            OverworldTerrainEngine engine = OverworldTerrainEngine.Instance;
            return (engine == null) ? null : engine.TileMap;
        }
    }

    public static DungeonFactory CurrentDungeonFactory
    {
        get
        {
            GameObject g = GameObject.FindGameObjectWithTag("DungeonFactory");
            if (g == null) { return null; }
            return g.GetComponent<DungeonFactory>();
        }
    }
}