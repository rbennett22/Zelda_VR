using UnityEngine;
using System.Collections.Generic;

public class Weapon_Gun : Weapon_Base
{
    [SerializeField]
    GameObject _projectilePrefab;

    [SerializeField]
    Transform _muzzleAnchor;
    public Transform Muzzle { get { return _muzzleAnchor; } }

    [SerializeField]
    int _maxLiveProjectiles = 1;


    List<Projectile_Base> _firedProjectiles = new List<Projectile_Base>();



    override public bool CanAttack { get { return (base.CanAttack && (_firedProjectiles.Count < _maxLiveProjectiles)); } }


    override public void Attack(Vector3 direction)
    {
        if(!CanAttack)
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

        Vector3 spawnPos = (_muzzleAnchor == null) ? transform.position : _muzzleAnchor.position;
        Vector3 offset = p.Length * 0.5f * dir;
        t.position = spawnPos + offset;

        p.PointInDirection(dir);
        p.MoveDirection = dir;


        _firedProjectiles.Add(p);

        return p;
    }


    virtual protected void OnProjectileWillBeDestroyed(Projectile_Base sender)
    {
        _firedProjectiles.Remove(sender);
    }
}