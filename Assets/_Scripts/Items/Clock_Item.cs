using UnityEngine;

public class Clock_Item : MonoBehaviour
{
    public float clockDuration = 10.0f;


	void OnItemUsed () 
    {
        CommonObjects.Player_C.MakeInvincible(clockDuration);
        CommonObjects.Player_C.ParalyzeAllNearbyEnemies(clockDuration);
	}

}