using UnityEngine;
using Immersio.Utility;

public class CommonObjects : Singleton<CommonObjects> 
{
    public static GameObject Player_G { get { return Instance.player_G; } }
    public static GameObject PlayerController_G { get { return Instance.playerController_G; } }

    public static Player Player_C { get { return Instance.player_C; } }
    public static ZeldaPlayerController PlayerController_C { get { return Instance.playerController_C; } }


    public GameObject player_G, playerController_G;
    public Player player_C;
    public ZeldaPlayerController playerController_C;

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
            if (!WorldInfo.Instance.IsOverworld) { return null; }

            OverworldTerrainEngine engine = Uniblocks.Engine.EngineInstance as OverworldTerrainEngine;
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