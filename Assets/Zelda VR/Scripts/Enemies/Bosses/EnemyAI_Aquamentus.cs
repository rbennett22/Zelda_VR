using UnityEngine;

public class EnemyAI_Aquamentus : EnemyAI 
{
    public Vector3 forwardDirection = new Vector3(-1, 0, 0);
    public Vector3 backwardDirection = new Vector3(1, 0, 0);
    

	void Start () 
    {
        _enemyMove.Mode = EnemyMove.MovementMode.Destination;
        _enemyMove.AlwaysFaceTowardsMoveDirection = false;
        _enemyMove.targetPositionReached_Callback = OnTargetPositionReached;

        transform.forward = forwardDirection;
        MoveDirection = forwardDirection;
	}


    void OnTargetPositionReached(EnemyMove sender, Vector3 moveDirection)
    {
        if (moveDirection == forwardDirection)
        {
            Attack();
            MoveDirection = backwardDirection;
        }
        else
        {
            MoveDirection = forwardDirection;
        }
    }

    void Attack()
    {
        float degreesDelta = Random.Range(1, 30);
        float radiansDelta = degreesDelta * Mathf.PI / 180;

        Vector3 dir = ToPlayer;
        Vector3 dirL = Vector3.RotateTowards(dir, -dir, -radiansDelta, 999);
        Vector3 dirR = Vector3.RotateTowards(dir, -dir,  radiansDelta, 999);

        _enemy.weapon.Fire(dir);
        _enemy.weapon.Fire(dirL);
        _enemy.weapon.Fire(dirR);
    }
}