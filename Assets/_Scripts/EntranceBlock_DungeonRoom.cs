using UnityEngine;


public class EntranceBlock_DungeonRoom : MonoBehaviour
{
    const float EntranceThresholdDistance_NorthSouth = 5.5f;
    const float EntranceThresholdDistance_EastWest = 8.0f;


    public DungeonRoom dungeonRoom;
    public DungeonRoomInfo.WallDirection wallDirection;


    Vector3 _dungeonRoomPosition;
    float _entranceThresholdDistance;


    void Awake()
    {
        if (DungeonRoomInfo.IsNorthOrSouth(wallDirection))
        {
            _entranceThresholdDistance = EntranceThresholdDistance_NorthSouth;
        }
        else
        {
            _entranceThresholdDistance = EntranceThresholdDistance_EastWest;
        }
    }

    void Start()
    {
        _dungeonRoomPosition = dungeonRoom.transform.position;
    }


    /*void OnTriggerEnter(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;
        print("EntranceBlock_DungeonRoom --> OnTriggerEnter: " + other.name);
        if (!CommonObjects.IsPlayer(other)) { return; }
    }*/

    void OnTriggerExit(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;
        //print("EntranceBlock_DungeonRoom --> OnTriggerExit: " + other.name);
        if (!CommonObjects.IsPlayer(other)) { return; }

        Vector3 playerPos = Player.Instance.playerController.transform.position;

        if (Vector3.Distance(playerPos, _dungeonRoomPosition) < _entranceThresholdDistance)
            { OnEnteredRoom(); }
        else             
            { OnExitedRoom(); }
    }

    void OnEnteredRoom()
    {
        //print("OnEnteredRoom");
        dungeonRoom.onPlayerEnteredRoom(wallDirection);
    }

    void OnExitedRoom()
    {
        //print("OnExitedRoom");
        dungeonRoom.onPlayerExitedRoom();
    }

}
