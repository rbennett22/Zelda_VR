using UnityEngine;


public class SubDungeon : MonoBehaviour
{
    public static SubDungeon OccupiedSubDungeon;        // The SubDungeon the player is currently in


    public Collectible uniqueItem;
    public Transform uniqueItemContainer;
    public Transform warpInLocation;
    public Transform enemySpawnPointsContainer;


    public SubDungeonSpawnPoint SpawnPoint { get; set; }
    public DungeonRoom ParentDungeonRoom { get { return SpawnPoint.ParentDungeonRoom; } }


    void Start()
    {
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        if (enemySpawnPointsContainer == null) { return; }

        foreach (var sp in enemySpawnPointsContainer.GetComponentsInChildren<EnemySpawnPoint>())
        {
            sp.SpawnEnemy();
        }
    }


    public void OnPlayerEnteredPortal(SubDungeonPortal portal)
    {
        Player player = CommonObjects.Player_C;
        ZeldaPlayerController pc = player.PlayerController;
        Transform t = pc.transform;

        SubDungeon destinationSubDungeon = SpawnPoint.warpTo.SpawnSubDungeon();
        Transform warpToLocation = destinationSubDungeon.warpInLocation;

        Vector3 eulerDiff = warpToLocation.eulerAngles - portal.transform.eulerAngles;

        Vector3 offset = t.position - portal.transform.position;
        offset = Quaternion.Euler(eulerDiff) * offset;
        t.position = warpToLocation.position + offset;

        //OVRCameraRig camRig = pc.GetComponentInChildren<OVRCameraRig>();
        //Transform camera = camRig.transform;
        //camera.eulerAngles += eulerDiff;
        Vector3 newEuler = pc.transform.eulerAngles + eulerDiff;
        player.ForceNewRotation(newEuler);

        pc.Stop();


        ParentDungeonRoom.onPlayerExitedRoom();
        destinationSubDungeon.ParentDungeonRoom.OnPlayerEnteredRoom();

        /*print("warpToLocation.eulerAngles: " + warpToLocation.eulerAngles);
        print("portal.transform.eulerAngles: " + portal.transform.eulerAngles);
        print("eulerDiff: " + eulerDiff);
        print("t.forward: " + t.forward);*/
    }
}