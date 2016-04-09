using UnityEngine;


public class EnemyAI : MonoBehaviour 
{
    protected const float TileOffset = 0.5f;
    protected const float Epsilon = 0.001f;


    protected bool _doUpdate = true;
    protected Enemy _enemy;


    protected void Awake()
    {
        _enemy = GetComponent<Enemy>();
    }


    virtual public void DisableAI()
    {
        _doUpdate = false;
    }
    virtual public void EnableAI()
    {
        _doUpdate = true;
        //TargetPosition = transform.position;
    }


    protected Vector3 EnforceBoundary(Vector3 desiredMoveDirection)
    {
        if (transform.position.x < _enemy.Boundary.xMin)
        {
            desiredMoveDirection = new Vector3(1, 0, 0);
        }
        else if (transform.position.x > _enemy.Boundary.xMax)
        {
            desiredMoveDirection = new Vector3(-1, 0, 0);
        }
        else if (transform.position.z < _enemy.Boundary.yMin)
        {
            desiredMoveDirection = new Vector3(0, 0, 1);
        }
        else if (transform.position.z > _enemy.Boundary.yMax)
        {
            desiredMoveDirection = new Vector3(0, 0, -1);
        }

        return desiredMoveDirection;
    }

}