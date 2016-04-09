using UnityEngine;
using System.Collections;


public class EnemyHealthDelegate : MonoBehaviour, IHealthControllerDelegate
{
    const float PushBackPower = 7;


    public EnemyAnimation enemyAnim;
    public bool flashWhenDamaged = true;


    void Awake()
    {
        GetComponent<HealthController>().hcDelegate = this;
    }


    void IHealthControllerDelegate.OnHealthChanged(HealthController healthController, int newHealth)
    {

    }
    void IHealthControllerDelegate.OnDamageTaken(HealthController healthController, ref uint damageAmount, GameObject damageDealer)
    {
        EnemyAI_Zol zol = GetComponent<EnemyAI_Zol>();
        if (zol != null)
        {
            if (damageAmount == 1)
            {
                zol.SpawnGels();
                healthController.Kill(damageDealer, true);
            }
        }

        EnemyAI_Gohma gohma = GetComponent<EnemyAI_Gohma>();
        if (gohma != null)
        {
            if (gohma.IsEyeClosed)
            {
                damageAmount = 0;
                SoundFx sfx = SoundFx.Instance;
                sfx.PlayOneShot(sfx.shield);
            }
        }

        EnemyAI_Gannon gannon = GetComponent<EnemyAI_Gannon>();
        if (gannon != null)
        {
            Sword sword = damageDealer.GetComponent<Sword>();
            if (sword != null)
            {
                damageAmount = 0;
                SendMessage("OnHitWithSword", sword, SendMessageOptions.RequireReceiver);
            }
            else if(damageDealer.name == "SilverArrow_Projectile")
            {
                damageAmount = 0;
                SendMessage("OnHitWithSilverArrow", SendMessageOptions.RequireReceiver);
            }
        }

        if (damageAmount > 0)
        {
            if (GetComponent<Enemy>().pushBackOnhit)
            {
                if(DoesWeaponApplyPushbackForce(damageDealer))
                {
                    Vector3 direction = transform.position - damageDealer.transform.position;
                    direction.y = 0;
                    direction.Normalize();

                    Vector3 force = direction * PushBackPower;
                    GetComponent<Enemy>().Push(force);
                }
            }
        }
    }
    void IHealthControllerDelegate.OnHealthRestored(HealthController healthController, uint healAmount)
    {

    }
    void IHealthControllerDelegate.OnTempInvincibilityActivation(HealthController healthController, bool didActivate = true)
    {
        if (flashWhenDamaged && enemyAnim != null)
        {
            enemyAnim.ActivateFlash(didActivate);
        }
    }
    void IHealthControllerDelegate.OnDeath(HealthController healthController, GameObject killer)
    {
        EnemyAI_Moldorm moldorm = GetComponent<EnemyAI_Moldorm>();
        if (moldorm != null)
        {
            OnMoldormDeath(moldorm);
            return;
        }

        EnemyAI_Vire vire = GetComponent<EnemyAI_Vire>();
        if (vire != null)
        {
            if (killer.name != "MagicSword_Weapon" && killer.name != "MagicSword_Projectile")
            {
                vire.SpawnKeese();
            }
        }

        EnemyAI_GleeokHead gleeokHead = GetComponent<EnemyAI_GleeokHead>();
        if (gleeokHead != null)
        {
            gleeokHead.gleeok.SendMessage("OnHeadDied", gleeokHead, SendMessageOptions.RequireReceiver);
        }

        EnemyAI_PatraSmall smallPatra = GetComponent<EnemyAI_PatraSmall>();
        if (smallPatra != null)
        {
            smallPatra.ParentPatra.SendMessage("OnSmallPatraDied", smallPatra, SendMessageOptions.RequireReceiver);
        }

        EnemyAI_DigdoggerSmall smallDigdogger = GetComponent<EnemyAI_DigdoggerSmall>();
        if (smallDigdogger != null)
        {
            smallDigdogger.ParentDigdogger.SendMessage("OnBabyDied", smallDigdogger, SendMessageOptions.RequireReceiver);
        }
        
        Enemy e = GetComponent<Enemy>();
        DungeonRoom dr = e.DungeonRoomRef;
        EnemyItemDrop itemDrop = GetComponent<EnemyItemDrop>();

        Enemy.EnemiesKilled++;
        Enemy.EnemiesKilledWithoutTakingDamage++;

        if (itemDrop != null) { itemDrop.DropRandomItem(); }
        if (dr != null) { dr.OnRoomEnemyDied(e); }
        if (enemyAnim != null) { enemyAnim.PlayDeathAnimation(); }

        SendMessage("OnEnemyDeath", SendMessageOptions.DontRequireReceiver);
    }

    void OnMoldormDeath(EnemyAI_Moldorm moldorm)
    {
        Enemy e = GetComponent<Enemy>();
        DungeonRoom dr = e.DungeonRoomRef;
        EnemyItemDrop itemDrop = GetComponent<EnemyItemDrop>();

        if (dr != null) { dr.OnRoomEnemyDied(e); }
        if (enemyAnim != null) { enemyAnim.PlayDeathAnimation(); }

        if (moldorm.IsLastWormPiece)
        {
            Enemy.EnemiesKilled++;
            Enemy.EnemiesKilledWithoutTakingDamage++;

            if (itemDrop != null) { itemDrop.DropRandomItem(); }
        }

        moldorm.OnDeath();
        return;
    }


    // TODO: this is a hack.  the Weapon should store whether it does pushback
    bool DoesWeaponApplyPushbackForce(GameObject weapon)
    {
        Boomerang boomerang = weapon.GetComponent<Boomerang>();
        if (boomerang != null) { return false; }

        CandleFlame candleFlame = weapon.GetComponent<CandleFlame>();
        if (candleFlame != null) { return false; }

        return true;
    }
}