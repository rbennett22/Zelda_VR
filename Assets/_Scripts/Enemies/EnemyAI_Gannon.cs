using UnityEngine;
using System.Collections;


public class EnemyAI_Gannon : EnemyAI 
{
    const float VisibleDuration = 1.3f;
    const float BoundsWidth = 8;
    const float BoundsHeight = 5;


    public EnemyAnimation enemyAnimation;
    public int swordHitsNeededToKill = 4;
    public float attackCooldown = 1.0f;


    int _swordHitsTaken;
    Rect _bounds;
    

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
            if (_doUpdate && !_enemy.IsParalyzed)
            {
                if (!enemyAnimation.GetComponent<Renderer>().enabled)
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
        Vector3 toPlayer = _enemy.PlayerController.transform.position - transform.position;
        toPlayer.Normalize();
        _enemy.weapon.Fire(toPlayer);
    }

    void MoveToRandomLocation()
    {
        float x = Random.RandomRange(_bounds.xMin, _bounds.xMax);
        float z = Random.RandomRange(_bounds.yMin, _bounds.yMax);

        transform.SetX(x);
        transform.SetZ(z);
    }


    void OnHitWithSword(Sword sword)
    {
        if (enemyAnimation.GetComponent<Renderer>().enabled) { return; }

        _swordHitsTaken++;
        UpdatePose();
        StartCoroutine("Appear");
    }

    void OnHitWithSilverArrow()
    {
        if (!enemyAnimation.GetComponent<Renderer>().enabled) { return; }

        if (_swordHitsTaken >= swordHitsNeededToKill)
        {
            GetComponent<HealthController>().Kill(gameObject, true);
        }
    }


    IEnumerator Appear()
    {
        enemyAnimation.GetComponent<Renderer>().enabled = true;
        yield return new WaitForSeconds(VisibleDuration);

        Disappear();
    }

    void UpdatePose()
    {
        if (_swordHitsTaken < swordHitsNeededToKill)
        {
            enemyAnimation.AnimatorInstance.SetTrigger("NextPose"); 
        }
        else
        {
            enemyAnimation.AnimatorInstance.SetTrigger("NextHurtPose"); 
        }
    }

    void Disappear()
    {
        enemyAnimation.GetComponent<Renderer>().enabled = false;
    }

}