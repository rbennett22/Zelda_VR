using UnityEngine;


public class EntranceBlock_DungeonRoom : MonoBehaviour
{
    const float EntranceThresholdDistance_NorthSouth = 5.5f;
    const float EntranceThresholdDistance_EastWest = 8.0f;


    public DungeonRoom dungeonRoom;
    public DungeonRoomInfo.WallDirection wallDirection;


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


    void OnTriggerExit(Collider otherCollider)
    {
        GameObject other = otherCollider.gameObject;
        if (!CommonObjects.IsPlayer(other)) { return; }

        Vector3 roomPos = dungeonRoom.transform.position;
        Vector3 playerPos = CommonObjects.Player_C.Position;
        if (Vector3.Distance(playerPos, roomPos) < _entranceThresholdDistance)
        {
            OnEnteredRoom();
        }
        else
        {
            OnExitedRoom();
        }
    }

    void OnEnteredRoom()
    {
        dungeonRoom.onPlayerEnteredRoom(wallDirection);
    }
    void OnExitedRoom()
    {
        dungeonRoom.onPlayerExitedRoom();
    }
}