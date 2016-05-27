using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI_Wizzrobe : EnemyAI
{
    const float OffscreenOffset = -30;      // How far to offset the Wizzrobe's y position when it is "invisible"


    public FlickerEffect flickerEffect;
    public int tpDistanceToPlayer = 3;      // How close to the player the Wizzrobe will teleport
    public bool walks;                      // Does the Wizzrobe walk around at all or just teleport
    public EnemyAI_Random enemyAI_Random;


    float _fadeDuration = 0.7f;
    float _attackDuration = 1.5f;
    float _walkDuration = 3.0f;
    float _teleportDuration = 3.0f;
    float _teleportDurationRandomOffset = 0.5f;


    enum State
    {
        FadingIn,
        Attacking,
        Walking,
        FadingAway,
        InvisibleIdle
    }
    //State _state = State.InvisibleIdle;


    protected override void Awake()
    {
        base.Awake();

        if (enemyAI_Random != null) { enemyAI_Random.enabled = false; }
    }

    void Start()
    {
        Reappear();
        StartCoroutine("FadeIn");
    }


    IEnumerator FadeAway()
    {
        //_state = State.FadingAway;
        ActivateFlickering();
        yield return new WaitForSeconds(_fadeDuration);

        DeactivateFlickering();
        StartCoroutine("InvisibleIdle");
    }

    IEnumerator InvisibleIdle()
    {
        //_state = State.InvisibleIdle;
        Disappear();
        float teleportDuration = _teleportDuration + Random.Range(-_teleportDurationRandomOffset, _teleportDurationRandomOffset);

        yield return new WaitForSeconds(teleportDuration);

        Reappear();
        StartCoroutine("FadeIn");
    }

    IEnumerator FadeIn()
    {
        //_state = State.FadingIn;
        ActivateFlickering();

        yield return new WaitForSeconds(_fadeDuration);

        DeactivateFlickering();
        StartCoroutine("Attack");
    }

    IEnumerator Attack()
    {
        //_state = State.Attacking;

        if (_doUpdate && !IsPreoccupied)
        {
            _enemy.Attack();
        }

        yield return new WaitForSeconds(_attackDuration);

        if (walks)
        {
            StartCoroutine("WalkAround");
        }
        else
        {
            StartCoroutine("FadeAway");
        }
    }

    IEnumerator WalkAround()
    {
        //_state = State.Walking;

        enemyAI_Random.enabled = true;
        enemyAI_Random.TargetPosition = transform.position;

        yield return new WaitForSeconds(_walkDuration);

        enemyAI_Random.enabled = false;
        StartCoroutine("FadeAway");
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

    void Disappear()
    {
        AnimatorInstance.gameObject.SetActive(false);
        GetComponent<Collider>().enabled = false;
        transform.AddToY(OffscreenOffset);  // Move offscreen to prevent collision with player
    }

    void Reappear()
    {
        AnimatorInstance.gameObject.SetActive(true);
        GetComponent<Collider>().enabled = true;
        transform.AddToY(-OffscreenOffset);

        transform.position = GetRandomTeleportPosition();

        Vector3 dir = DirectionToPlayer;
        if (Mathf.Abs(dir.x / dir.z) < 1)
        {
            dir.x = 0;
        }
        else
        {
            dir.z = 0;
        }
        transform.forward = dir;
    }

    Vector3 GetRandomTeleportPosition()
    {
        Vector3 newPosition;

        Vector3 pp = Player.Position;
        List<Vector3> possiblePositions = new List<Vector3>();

        possiblePositions.Add(new Vector3(pp.x + tpDistanceToPlayer, pp.y, pp.z));
        possiblePositions.Add(new Vector3(pp.x - tpDistanceToPlayer, pp.y, pp.z));
        possiblePositions.Add(new Vector3(pp.x, pp.y, pp.z + tpDistanceToPlayer));
        possiblePositions.Add(new Vector3(pp.x, pp.y, pp.z - tpDistanceToPlayer));

        DungeonRoom dr = _enemy.DungeonRoomRef;
        for (int i = possiblePositions.Count - 1; i >= 0; i--)
        {
            Vector3 p = possiblePositions[i];
            if (p.x < dr.Bounds.xMin + 0.5f) { possiblePositions.RemoveAt(i); continue; }
            if (p.x > dr.Bounds.xMax - 0.5f) { possiblePositions.RemoveAt(i); continue; }
            if (p.z < dr.Bounds.yMin + 0.5f) { possiblePositions.RemoveAt(i); continue; }
            if (p.z > dr.Bounds.yMax - 0.5f) { possiblePositions.RemoveAt(i); continue; }
        }

        if (possiblePositions.Count == 0)
        {
            newPosition = transform.position;
        }
        else
        {
            int randIdx = Random.Range(0, possiblePositions.Count);
            newPosition = possiblePositions[randIdx];
        }

        return newPosition;
    }
}