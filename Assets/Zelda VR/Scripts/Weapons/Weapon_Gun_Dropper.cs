using UnityEngine;

public class Weapon_Gun_Dropper : Weapon_Gun
{
    public float dropDistance = 1.0f;


    override protected Projectile_Base FireProjectile(Vector3 dir)
    {
        Projectile_Base p = base.FireProjectile(dir);
        AdjustProjectilePosition(p);
        return p;
    }

    void AdjustProjectilePosition(Projectile_Base p)
    {
        Vector3 newPos;

        Vector3 pos = transform.position;
        Vector3 forward = transform.forward;

        Ray ray = new Ray(pos, forward * dropDistance);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, dropDistance))
        {
            newPos = pos + forward * (hitInfo.distance - p.Length * 0.5f);
        }
        else
        {
            newPos = pos + forward * dropDistance;
        }

        p.transform.position = newPos;
    }
}