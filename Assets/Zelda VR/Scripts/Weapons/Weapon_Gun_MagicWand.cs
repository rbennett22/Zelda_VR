using UnityEngine;

public class Weapon_Gun_MagicWand : Weapon_Gun
{
    public bool spawnFlame;

    [SerializeField]
    GameObject _flamePrefab;


    override protected void OnProjectileWillBeDestroyed(Projectile_Base sender)
    {
        base.OnProjectileWillBeDestroyed(sender);

        if(spawnFlame)
        {
            DoSpawnFlame(sender.transform.position);
        }
    }

    void DoSpawnFlame(Vector3 position)
    {
        GameObject g = Instantiate(_flamePrefab, position, Quaternion.identity) as GameObject;
    }
}