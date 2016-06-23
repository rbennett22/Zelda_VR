public class EnemyAI_Manhandla : EnemyAI
{
    const int MAX_NUM_MOUTHS = 4;


    public float[] speeds = new float[MAX_NUM_MOUTHS];

    override public float Radius { get { return 1.0f; } }

    public int NumMouths { get { return transform.childCount - 1; } }


    void Update()
    {
        if (!_doUpdate) { return; }
        if (IsPreoccupied) { return; }

        if (NumMouths == 0)
        {
            _healthController.isIndestructible = false;
            _healthController.Kill(null, true);
        }
        else
        {
            _enemyMove.speed = speeds[NumMouths - 1];
        }
    }
}