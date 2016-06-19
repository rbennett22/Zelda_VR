using UnityEngine;


public class EntranceBlock_DungeonRoom : MonoBehaviour
{
    const float THRESHOLD_DIST_NS = 5.5f;       // north or south
    const float THRESHOLD_DIST_EW = 8.0f;       // east or west


    public DungeonRoom dungeonRoom;
    public DungeonRoomInfo.WallDirection wallDirection;


    float ThresholdDist { get { return DungeonRoomInfo.IsNorthOrSouth(wallDirection) ? THRESHOLD_DIST_NS : THRESHOLD_DIST_EW; } }


    void OnTriggerExit(Collider otherCollider)
    {
        if (!CommonObjects.IsPlayer(otherCollider.gameObject)) { return; }

        Vector3 playerPos = CommonObjects.Player_C.Position;
        if (Vector3.Distance(playerPos, dungeonRoom.Center) < ThresholdDist)
        {
            dungeonRoom.OnPlayerEnteredRoom(wallDirection);
        }
        else
        {
            dungeonRoom.onPlayerExitedRoom();
        }
    }
}