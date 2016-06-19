using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI_Wizzrobe_Blue : EnemyAI
{
    public FlickerEffect flickerEffect;
    public EnemyAI_Random enemyAI_Random;


    float _shiftDuration = 0.7f;
    float _attackDuration = 1.5f;
    float _walkDuration = 3.0f;


    /*enum State
    {
        Shifting,
        Attacking,
        Walking
    }*/
    //State _state = State.Walking;


    protected override void Awake()
    {
        base.Awake();

        if (enemyAI_Random != null) { enemyAI_Random.enabled = false; }
    }

    void Start()
    {
        enemyAI_Random.enabled = true;
        //StartCoroutine("WalkAround");
    }


    /*IEnumerator Shift()
    {
        //_state = State.Shifting;
        ActivateFlickering();

        // TODO
        Vector3 shiftToPos = GetRandomShiftPosition();

        yield return new WaitForSeconds(_shiftDuration);

        DeactivateFlickering();
        StartCoroutine("WalkAround");
    }

    IEnumerator Attack()
    {
        //_state = State.Attacking;

        if (_doUpdate && !IsPreoccupied)
        {
            _enemy.Attack();
        }

        yield return new WaitForSeconds(_attackDuration);

        StartCoroutine("WalkAround");
    }

    IEnumerator WalkAround()
    {
        //_state = State.Walking;

        enemyAI_Random.enabled = true;
        enemyAI_Random.TargetPosition = transform.position;

        yield return new WaitForSeconds(_walkDuration);

        enemyAI_Random.enabled = false;
        StartCoroutine("Shift");
    }


    void ActivateFlickering()
    {
        GetComponent<Collider>().enabled = false;
        flickerEffect.enabled = true;
    }
    void DeactivateFlickering()
    {
        GetComponent<Collider>().enabled = true;
        flickerEffect.enabled = false;
    }

    Vector3 GetRandomShiftPosition()
    {
        Vector3 p = transform.position;

        Vector3 pp = Player.Tile.ToVector3();
        pp.x += 0.5f;
        pp.z += 0.5f;
        List<Vector3> possiblePositions = new List<Vector3>();

        //possiblePositions.Add(new Vector3(pp.x + tpDistanceToPlayer, p.y, pp.z));
        //possiblePositions.Add(new Vector3(pp.x - tpDistanceToPlayer, p.y, pp.z));
        //possiblePositions.Add(new Vector3(pp.x, p.y, pp.z + tpDistanceToPlayer));
        //possiblePositions.Add(new Vector3(pp.x, p.y, pp.z - tpDistanceToPlayer));

        for (int i = possiblePositions.Count - 1; i >= 0; i--)
        {
            if(!DoesBoundaryAllowPosition(possiblePositions[i]))
            {
                possiblePositions.RemoveAt(i);
            }
        }

        if (possiblePositions.Count == 0)
        {
            return transform.position;
        }

        int randIdx = Random.Range(0, possiblePositions.Count);
        return possiblePositions[randIdx];
    }*/
}