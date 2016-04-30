using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Bomb : MonoBehaviour
{

    public int explosionDamage = 1;
    public float explosionRadius = 1.5f;
	public bool beginCountdownOnEnable = true;
	public float countdownDuration = 1.0f;
    public AudioClip explodeSound;


	float _countdownTimer = 0.0f;
	public bool CountdownHasBegun { get; private set; }
	public float TimeRemaining { get { return _countdownTimer; } }
	public float TimeRemainingRatio { get { return Mathf.Max(0, _countdownTimer / countdownDuration); } }

	bool _doDetonateOnNextFixedUpdate = false;
    public Animator animator;
    public string[] invulnerables;  // Enemies that cannot be hurt by bomb 

    public bool IsExploding { get { return animator.GetCurrentAnimatorStateInfo(0).IsTag("Explode"); } }


    List<string> _invulnerables;


	void Awake () 
    {
		CountdownHasBegun = false;
        _invulnerables = new List<string>(invulnerables);
	}

	void OnEnable () 
    {
		if(beginCountdownOnEnable)
			BeginCountDown();
	}


	public void BeginCountDown () 
    {
		if(CountdownHasBegun)
			return;
		_countdownTimer = countdownDuration;
		CountdownHasBegun = true;
	}
	public void AbortCountDown () 
    {
		CountdownHasBegun = false;
	}


	void Update () 
    {
		if(CountdownHasBegun)
        {
			_countdownTimer -= Time.deltaTime;
			if(_countdownTimer <= 0)
            {
				_doDetonateOnNextFixedUpdate = true;		// Will blow up in FixedUpdate
			}
		}
	}

	void FixedUpdate () 
    {
		if(_doDetonateOnNextFixedUpdate)
        {
			Detonate();
			_doDetonateOnNextFixedUpdate = false;
		}
	}


	public void RequestDetonation () 
    {
		_doDetonateOnNextFixedUpdate = true;
	}
	void Detonate () 
    {
        CountdownHasBegun = false;

		Vector3 explosionPos = transform.position;

        Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius);
        foreach (Collider hit in colliders)
        {
            if (hit == null) { continue; }

            GameObject g = hit.gameObject;
            Enemy enemy = g.GetComponent<Enemy>();

            bool blocked = _invulnerables.Contains(g.name);
            if (blocked)
            {
                if (enemy != null && enemy.playSoundWhenBlockingAttack)
                {
                    SoundFx sfx = SoundFx.Instance;
                    sfx.PlayOneShot(sfx.shield);
                }
                continue;
            }

            if (enemy != null)
            {
                ApplyExplosionDamage(g, explosionPos);
            }

            Block b = g.GetComponent<Block>();
            if (b != null && b.isBombable)
            {
                b.Bomb();
            }

            if (g.layer == LayerMask.NameToLayer("Walls"))
            {
                BlastWall(g);
            }
        }

        animator.SetTrigger("explode");

        SoundFx.Instance.PlayOneShot3D(transform.position, explodeSound);

        Destroy(gameObject, 0.3f);
	}

    void ApplyExplosionDamage(GameObject g, Vector3 explosionPos)
    {
        //print("ApplyExplosionDamage: " + g.name);
        if (!g) { return; }

        HealthController health = g.GetComponent<HealthController>();
        if (health == null) { return; }

        uint damageAmount = (uint)explosionDamage;
        health.TakeDamage(damageAmount, gameObject);
    }

    void BlastWall(GameObject wall)
    {
        print(" BlastWall: " + wall.name);

        Transform p1 = wall.transform.parent;
        if (p1 == null) { return; }

        Transform p2 = p1.parent;
        if (p2 == null) { return; }

        DungeonRoom dr = p2.GetComponent<DungeonRoom>();
        if (dr == null) { return; }
        
        DungeonRoomInfo.WallDirection wallDir = dr.GetWallDirectionForWall(wall);
        if(!dr.Info.IsBombable(wallDir)) { return; }
        
        Vector3 wallCenter = wall.transform.position;
        wallCenter.y = 0;
        float dist = Vector3.Distance(wallCenter, transform.position);
        if (dist < explosionRadius * 1.5f)
        {
            dr.BlowHoleInWall(wallDir);
        }
    }

}