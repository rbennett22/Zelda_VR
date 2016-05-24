using UnityEngine;

public class Item_Clock : MonoBehaviour
{
    public float clockDuration = 10.0f;

    void OnItemUsed()
    {
        CommonObjects.Player_C.MakeInvincible(clockDuration);
        CommonObjects.Player_C.ParalyzeAllNearbyEnemies(clockDuration);
    }
}