using UnityEngine;
using System.Collections;


public class PlayerHealthDelegate : MonoBehaviour, IHealthControllerDelegate
{
    static int _healthToHeartRatio = 8;
    public static int HealthToHalfHearts(int health)
    {
        return Mathf.CeilToInt(health / (_healthToHeartRatio * 0.5f));
    }
    public static int HalfHeartsToHealth(int halfHearts)
    {
        return (int)(halfHearts * (_healthToHeartRatio * 0.5f));
    }


    public float pushBackPower = 1;


    Player _player;
    Transform _playerController;
    HealthController _healthController;


    void Awake()
    {
        _player = GetComponent<Player>();
        _playerController = _player.playerController.gameObject.transform;
        _healthController = GetComponent<HealthController>();
        _healthController.hcDelegate = this;
    }


    void IHealthControllerDelegate.OnHealthChanged(HealthController healthController, int newHealth)
    {
        _player.EnableSwordProjectiles(healthController.IsAtFullHealth);
        SoundFx.Instance.PlayLowHealth(_player.HealthInHalfHearts <= 2);
    }
    void IHealthControllerDelegate.OnDamageTaken(HealthController healthController, ref uint damageAmount, GameObject damageDealer)
    {
        damageAmount = (uint)(_player.GetDamageModifier() * damageAmount);

        Vector3 direction = _playerController.position - damageDealer.transform.position;
        direction.y = 0;
        direction.Normalize();
        Push(direction);

        if (damageDealer.name == "Bubble_Clear")
        {
            _player.ActivateJinx();
        }
        else if (damageDealer.name == "LikeLike")
        {
            _player.ActivateLikeLikeTrap(damageDealer);
        }

        Enemy.EnemiesKilledWithoutTakingDamage = 0;
    }

    void IHealthControllerDelegate.OnHealthRestored(HealthController healthController, uint healAmount)
    {
        
    }
    void IHealthControllerDelegate.OnTempInvincibilityActivation(HealthController healthController, bool didActivate = true)
    {

    }
    void IHealthControllerDelegate.OnDeath(HealthController healthController, GameObject killer) 
    {
        _player.DeathCount++;

        StartCoroutine("DeathSequence");
    }


    void Push(Vector3 direction)
    {
        StartCoroutine("PushCoroutine", direction);
    }
    IEnumerator PushCoroutine(Vector3 direction)
    {
        int count = 0;
        int iterations = 4;
        while (++count < iterations)
        {
            CharacterController cc = _playerController.GetComponent<CharacterController>();
            cc.Move(direction * (pushBackPower / iterations));

            yield return new WaitForSeconds(0.016f);
        }
    }


    float _deathSequenceDuration = 2.0f;
    IEnumerator DeathSequence()
    {
        Music.Instance.Stop();
        SoundFx sfx = SoundFx.Instance;
        sfx.PlayLowHealth(false);
        sfx.PlayOneShot(sfx.die);

        Pause.Instance.IsAllowed = false;
        GameplayHUD.Instance.enabled = false;
        _player.IsParalyzed = true;

        yield return new WaitForSeconds(_deathSequenceDuration);

        OverlayGui.Instance.PlayShutterCloseSequence(gameObject);
    }
    Vector3 _playerDeathLocation;
    IEnumerator ShuttersFinishedClosing()
    {
        _playerDeathLocation = _playerController.transform.position;

        Locations.Instance.RespawnPlayer();

        yield return new WaitForSeconds(0.1f);

        _healthController.Reset();
        GameplayHUD.Instance.enabled = true;

        OverlayGui.Instance.PlayShutterOpenSequence(gameObject);
    }
    void ShuttersFinishedOpening()
    {
        if (WorldInfo.Instance.IsInDungeon)
        {
            DungeonRoom dr = DungeonRoom.GetRoomForPosition(_playerDeathLocation);
            if (dr != null)
            {
                dr.OnPlayerDiedInThisRoom();
            }
        }

        _player.IsParalyzed = false;
        _player.EnableSwordProjectiles();
        Pause.Instance.IsAllowed = true;
    }

}