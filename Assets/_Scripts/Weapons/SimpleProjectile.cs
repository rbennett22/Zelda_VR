using UnityEngine;
using System.Collections.Generic;


public class SimpleProjectile : MonoBehaviour
{

    public float speed;
    public Vector3 direction;
    public int damage = 1;
    public float maxLifeTime = 3;
    public bool woodenShieldBlocks, magicShieldBlocks;

    public string[] kills;          // Enemies that can be killed in one hit by this projectile
    public string[] drillsThrough;  // Enemies that this projectile can hit and keep moving
    public string[] invulnerables;  // Enemies that cannot be hurt by projectile 

    public GameObject weapon;


    List<string> _kills;
    List<string> _drillsThrough;
    List<string> _invulnerables;


    void Awake()
    {
        _kills = new List<string>(kills);
        _drillsThrough = new List<string>(drillsThrough);
        _invulnerables = new List<string>(invulnerables);
    }

    void Start()
    {
        Destroy(gameObject, maxLifeTime);
    }


	void Update () 
    {
        transform.position += direction * speed * Time.deltaTime;
	}


    void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
        //print("SimpleProjectile --> OnCollisionEnter: " + other.name);

        HealthController hc = other.GetComponent<HealthController>();
        if (hc == null)
        {
            hc = other.transform.parent.GetComponent<HealthController>();
        }
        if (hc != null)
        {
            Player player = hc.GetComponent<Player>();
            if (player != null)
            {
                OnHitPlayer(player);
            }
            else
            {
                Enemy enemy = hc.GetComponent<Enemy>();
                if (enemy != null)
                {
                    OnHitEnemy(enemy);
                }
            }
        }

        if (!_drillsThrough.Contains(other.name))
        {
            Destroy(gameObject);
        }
    }

    void OnHitPlayer(Player player)
    {
        bool blocked = player.CanBlockAttack(woodenShieldBlocks, magicShieldBlocks, direction);
        if (blocked)
        {
            SoundFx sfx = SoundFx.Instance;
            sfx.PlayOneShot(sfx.shield);
        }
        else
        {
            HealthController hc = player.GetComponent<HealthController>();
            hc.TakeDamage((uint)damage, transform.gameObject);
        }
    }

    void OnHitEnemy(Enemy enemy)
    {
        bool blocked = _invulnerables.Contains(enemy.name) || enemy.CanBlockAttack(direction);
        if (blocked)
        {
            if (enemy.playSoundWhenBlockingAttack)
            {
                SoundFx sfx = SoundFx.Instance;
                sfx.PlayOneShot(sfx.shield);
            }
        }
        else
        {
            HealthController hc = enemy.GetComponent<HealthController>();
            if (_kills.Contains(hc.name))
            {
                hc.Kill(gameObject);
            }
            else
            {
                hc.TakeDamage((uint)damage, gameObject);
            }
        }
    }

}
