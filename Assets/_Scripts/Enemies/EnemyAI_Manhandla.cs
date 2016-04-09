using UnityEngine;


public class EnemyAI_Manhandla : EnemyAI 
{

    public float[] speeds = new float[4];


	void Update ()
    {
        if (!_doUpdate) { return; }

        int numMouths = transform.GetChildCount() - 1;
        if (numMouths == 0)
        {
            HealthController hc = GetComponent<HealthController>();
            hc.isIndestructible = false;
            hc.Kill(null, true);
        }
        else
        {
            _enemy.speed = speeds[numMouths - 1];
        }
	}

}