using UnityEngine;


public class EnemyAI : MonoBehaviour
{
    protected const float TileOffset = 0.5f;
    protected const float Epsilon = 0.001f;


    protected bool _doUpdate = true;
    protected Enemy _enemy;


    virtual protected void Awake()
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


    protected Vector2 GetEnemyPosition2DForTile(Index tile)
    {
        return GetEnemyPosition2DForTile(tile.x, tile.y);
    }
    protected Vector2 GetEnemyPosition2DForTile(int tileX, int tileY)
    {
        Vector2 pos = new Vector2();

        pos.x = tileX + TileOffset;
        pos.y = tileY + TileOffset;

        return pos;
    }

    protected void SetEnemyPosition2DToTile(Index tile)
    {
        SetEnemyPosition2DToTile(tile.x, tile.y);
    }
    protected void SetEnemyPosition2DToTile(int tileX, int tileY)
    {
        Vector2 p = GetEnemyPosition2DForTile(tileX, tileY);
        transform.SetX(p.x);
        transform.SetZ(p.y);
    }
}