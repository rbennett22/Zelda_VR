using System.Collections;
using UnityEngine;

public class EnemyAI_Gannon : EnemyAI
{
    const float VisibleDuration = 1.3f;
    const float BoundsWidth = 8;
    const float BoundsHeight = 5;


    public int swordHitsNeededToKill = 4;
    public float attackCooldown = 1.0f;


    int _swordHitsTaken;
    Rect _bounds;


    public bool Visible
    {
        get { return AnimatorInstance.GetComponent<Renderer>().enabled; }
        set { AnimatorInstance.GetComponent<Renderer>().enabled = value; }
    }


    void Start()
    {
        Vector3 center = _enemy.DungeonRoomRef.transform.position;
        _bounds = new Rect(
            center.x - BoundsWidth * 0.5f,
            center.z - BoundsHeight * 0.5f,
            BoundsWidth, BoundsHeight
            );

        Disappear();

        StartCoroutine("Tick");
    }


    IEnumerator Tick()
    {
        while (true)
        {
            if (_doUpdate && !IsPreoccupied)
            {
                if (!Visible)
                {
                    Attack();
                    MoveToRandomLocation();
                }
            }
            yield return new WaitForSeconds(attackCooldown);
        }
    }

    void Attack()
    {
        _enemy.weapon.Fire(ToPlayer);
    }

    void MoveToRandomLocation()
    {
        float x = Random.Range(_bounds.xMin, _bounds.xMax);
        float z = Random.Range(_bounds.yMin, _bounds.yMax);

        // TODO: Use SetEnemyPosition2DToTile()?

        transform.SetX(x);
        transform.SetZ(z);
    }


    void OnHitWithSword(Sword sword)
    {
        if (Visible) { return; }

        _swordHitsTaken++;

        UpdatePose();
        StartCoroutine("Appear");
    }

    void OnHitWithSilverArrow()
    {
        if (!Visible) { return; }

        if (_swordHitsTaken >= swordHitsNeededToKill)
        {
            _healthController.Kill(gameObject, true);
        }
    }


    IEnumerator Appear()
    {
        Visible = true;

        yield return new WaitForSeconds(VisibleDuration);

        Disappear();
    }

    void UpdatePose()
    {
        if (_swordHitsTaken < swordHitsNeededToKill)
        {
            AnimatorInstance.SetTrigger("NextPose");
        }
        else
        {
            AnimatorInstance.SetTrigger("NextHurtPose");
        }
    }

    void Disappear()
    {
        Visible = false;
    }
}