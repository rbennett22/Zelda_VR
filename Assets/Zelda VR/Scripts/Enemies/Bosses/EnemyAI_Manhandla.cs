public class EnemyAI_Manhandla : EnemyAI
{
    public float[] speeds = new float[4];


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
            _enemyMove.Speed = speeds[NumMouths - 1];
        }
    }
}