using UnityEngine;
using Immersio.Utility;


public class CommonObjects : Singleton<CommonObjects> 
{
    public static GameObject Player_G { get { return Instance.player_G; } }
    public static GameObject PlayerController_G { get { return Instance.playerController_G; } }

    public static Player Player_C { get { return Instance.player_C; } }
    public static OVRPlayerController PlayerController_C { get { return Instance.playerController_C; } }


    public GameObject player_G, playerController_G;
    public Player player_C;
    public OVRPlayerController playerController_C;

    public GameObject enemyDeathAnimation;
    public Material[] enemyStatueMaterials;
    public GameObject invisibleBlockStatuePrefab;


    public static bool IsPlayer(GameObject g)
    {
        if (g.tag != "Player") { g = g.transform.parent.gameObject; }
        return (g.tag == "Player");
    }

    public Material GetEnemyStatueMaterialForDungeon(int dungeonNum)
    {
        return enemyStatueMaterials[dungeonNum - 1];
    }

}