using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandleFlame : MonoBehaviour
{
    public int damage = 1;
    public float maxLifeTime = 1;
    public float damageRadius = 0.7f;

    public Animator animator;
    public string[] invulnerables;  // Enemies that cannot be hurt by flame


    List<string> _invulnerables;


    void Awake()
    {
        animator.GetComponent<Renderer>().enabled = false;
        animator.SetTrigger("SkipSpawn");
        _invulnerables = new List<string>(invulnerables);
    }

    IEnumerator Start()
    {
        Destroy(gameObject, maxLifeTime);
        yield return new WaitForSeconds(0.01f);
        animator.GetComponent<Renderer>().enabled = true;
    }


    void Update()
    {
        CollisionCheck();
    }

    void CollisionCheck()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, damageRadius);
        foreach (Collider hit in colliders)
        {
            if (hit == null) { continue; }

            GameObject g = hit.gameObject;

            bool blocked = _invulnerables.Contains(g.name);
            if (blocked)
            {
                //SoundFx sfx = SoundFx.Instance;
                //sfx.PlayOneShot(sfx.shield);
                continue;
            }

            Enemy enemy = g.GetComponent<Enemy>();
            if (enemy != null)
            {
                ApplyDamage(g);
            }

            if (CommonObjects.IsPlayer(g))
            {
                GameObject player = g.transform.parent.gameObject;
                ApplyDamage(player);
            }

            Block b = g.GetComponent<Block>();
            if (b != null && b.isBurnable)
            {
                b.Burn();
            }
        }
    }

    void ApplyDamage(GameObject g)
    {
        if (!g) { return; }

        HealthController health = g.GetComponent<HealthController>();
        if (health == null) { return; }

        uint damageAmount = (uint)damage;
        health.TakeDamage(damageAmount, gameObject);
    }
}