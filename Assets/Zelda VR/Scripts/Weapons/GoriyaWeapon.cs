using UnityEngine;


public class GoriyaWeapon : Weapon 
{

    public Boomerang boomerang;


    override public bool IsAttacking { get { return !boomerang.CanUse; } }


    override public void Fire(Vector3 direction)
    {
        Vector3 dir = (direction == Vector3.zero) ? transform.forward : direction;

        if (damage >= 0)
        {
            boomerang.damage = damage;
        }

        boomerang.Throw(transform, dir);
    }


    void OnDestroy()
    {
        if (boomerang != null)
        {
            Destroy(boomerang.gameObject);
            boomerang = null;
        }
    }

}