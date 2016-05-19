using UnityEngine;
using System.Collections;

public interface IBombable
{
    void Bomb(BombExplosion sender);
}

public class BombExplosion : MonoBehaviour
{
    public float radius = 1.5f;
    public float duration = 0.3f;

    [SerializeField]
    AudioClip _explodeSound;
    [SerializeField]
    Animator _animator;
    [SerializeField]
    bool _autoDetonateOnStart;


    void Start()
    {
        if(_autoDetonateOnStart)
        {
            Detonate();
        }
    }


    public void Detonate()
    {
        StartCoroutine(Detonate_CR());
    }
    IEnumerator Detonate_CR()
    {
        yield return new WaitForFixedUpdate();

        _animator.SetTrigger("explode");
        PlayExplodeSound();

        ProcessCollisions();    

        Destroy(gameObject, duration);
    }

    void ProcessCollisions()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider hit in colliders)
        {
            if (hit == null) { continue; }

            GameObject g = hit.gameObject;

            IBombable b = g.GetComponent<IBombable>();
            if (b != null)
            {
                Bomb(b);
            }

            if (g.layer == LayerMask.NameToLayer("Walls"))      // TODO
            {
                BlastWall(g);
            }
        }
    }

    void Bomb(IBombable b)
    {
        if (b == null)
            return;
        b.Bomb(this);
    }

    void BlastWall(GameObject wall)
    {
        Transform p1 = wall.transform.parent;
        if (p1 == null) { return; }

        Transform p2 = p1.parent;
        if (p2 == null) { return; }

        DungeonRoom dr = p2.GetComponent<DungeonRoom>();
        if (dr == null) { return; }

        DungeonRoomInfo.WallDirection wallDir = dr.GetWallDirectionForWall(wall);
        if (!dr.Info.IsBombable(wallDir)) { return; }

        Vector3 wallCenter = wall.transform.position;
        wallCenter.y = 0;
        float dist = Vector3.Distance(wallCenter, transform.position);
        if (dist < radius * 1.5f)
        {
            dr.BlowHoleInWall(wallDir);
        }
    }


    void PlayExplodeSound()
    {
        SoundFx.Instance.PlayOneShot3D(transform.position, _explodeSound);
    }
}