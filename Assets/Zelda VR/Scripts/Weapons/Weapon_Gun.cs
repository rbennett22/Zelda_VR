using UnityEngine;
using System.Collections.Generic;

public class Weapon_Gun : Weapon_Base
{
    const float SNAP_PROJECTILE_DIR_THRESHOLD = 15;


    [SerializeField]
    bool _snapFireDirectionToXZ;
    [SerializeField]
    GameObject _projectilePrefab;

    [SerializeField]
    Transform _muzzleAnchor;
    public Vector3 MuzzlePosition { get { return (_muzzleAnchor == null) ? transform.position : _muzzleAnchor.position; } }

    [SerializeField]
    int _maxLiveProjectiles = 1;


    List<Projectile_Base> _firedProjectiles = new List<Projectile_Base>();


    override public bool CanAttack { get { return (base.CanAttack && (_firedProjectiles.Count < _maxLiveProjectiles)); } }


    override public void Attack(Vector3 direction)
    {
        if (!CanAttack)
        {
            return;
        }
        base.Attack(direction);

        Vector3 dir = (direction == Vector3.zero) ? transform.forward : direction;
        FireProjectile(dir);
    }

    virtual protected Projectile_Base FireProjectile(Vector3 dir)
    {
        GameObject g = Instantiate(_projectilePrefab) as GameObject;

        Projectile_Base p = g.GetComponent<Projectile_Base>();
        p.onProjectileWillBeDestroyed_Callback = OnProjectileWillBeDestroyed;


        Transform t = p.transform;
        t.SetParent(CommonObjects.ProjectilesContainer);

        Vector3 offset = p.Length * 0.5f * dir;
        t.position = MuzzlePosition + offset;


        p.PointInDirection(dir);
        p.MoveDirection = dir;

        if (_snapFireDirectionToXZ)
        {
            SnapProjectileDirectionXZ(p, SNAP_PROJECTILE_DIR_THRESHOLD);
        }


        _firedProjectiles.Add(p);

        return p;
    }

    virtual protected void OnProjectileWillBeDestroyed(Projectile_Base sender)
    {
        _firedProjectiles.Remove(sender);
    }


    void SnapProjectileDirectionXZ(Projectile_Base p, float threshold)
    {
        Vector3 d = p.MoveDirection;

        if (Vector3.Angle(d, Vector3.left) < threshold)
        {
            d = Vector3.left;
            SnapProjectilePositionZ(p);
        }
        if (Vector3.Angle(d, Vector3.right) < threshold)
        {
            d = Vector3.right;
            SnapProjectilePositionZ(p);
        }

        if (Vector3.Angle(d, Vector3.back) < threshold)
        {
            d = Vector3.back;
            SnapProjectilePositionX(p);
        }
        if (Vector3.Angle(d, Vector3.forward) < threshold)
        {
            d = Vector3.forward;
            SnapProjectilePositionX(p);
        }

        if (d != p.MoveDirection)
        {
            p.PointInDirection(d);
            p.MoveDirection = d;
        }
    }
    void SnapProjectilePositionX(Projectile_Base p)
    {
        Transform t = p.transform;
        t.SetX(Mathf.Floor(t.position.x + 0.5f));
    }
    void SnapProjectilePositionZ(Projectile_Base p)
    {
        Transform t = p.transform;
        t.SetZ(Mathf.Floor(t.position.z + 0.5f));
    }
}