using System.Collections;
using UnityEngine;

public class PlayerHealthDelegate : MonoBehaviour
{
    const float DEATH_SEQUENCE_DURATION = 2.0f;
    const int NUM_HALF_HEARTS_AFTER_RESPAWN = 6;


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

    Vector3 _playerDeathLocation;


    void Awake()
    {
        _player = GetComponent<Player>();
        _playerController = _player.PlayerController.gameObject.transform;
        _healthController = GetComponent<HealthController>();

        _healthController.HealthChanged += HealthChanged;
        _healthController.DamageTaken += DamageTaken;
        _healthController.Death += Death;
    }


    void HealthChanged(HealthController healthController, int prevHealth, int newHealth)
    {
        _player.SwordProjectilesEnabled = healthController.IsAtFullHealth;
        SoundFx.Instance.PlayLowHealth(_player.HealthInHalfHearts <= 2);
    }

    void DamageTaken(HealthController healthController, ref uint damageAmount, GameObject damageDealer)
    {
        damageAmount = (uint)(_player.GetDamageNullifier() * damageAmount);

        Vector3 direction = _playerController.position - damageDealer.transform.position;
        direction.y = 0;
        direction.Normalize();
        Push(direction);

        SoundFx.Instance.PlayOneShot(SoundFx.Instance.hurt);

        Enemy enemy = damageDealer.GetComponent<Enemy>();

        // TODO

        if (damageDealer.name == "Bubble_Clear")
        {
            _player.ActivateJinx();
        }
        else if (damageDealer.name == "LikeLike")
        {
            _player.ActivateLikeLikeTrap(enemy);
        }

        Enemy.EnemiesKilledWithoutTakingDamage = 0;
    }

    void Death(HealthController healthController, GameObject killer)
    {
        _player.DeathCount++;

        StartCoroutine("DeathSequence");
    }
    IEnumerator DeathSequence()
    {
        Music.Instance.Stop();
        SoundFx sfx = SoundFx.Instance;
        sfx.PlayLowHealth(false);
        sfx.PlayOneShot(sfx.die);

        OverlayViewController.Instance.ShowPlayerDiedOverlay(DEATH_SEQUENCE_DURATION);

        GameplayHUDViewController.Instance.HideView();
        PauseManager.Instance.IsPauseAllowed_Inventory = false;
        PauseManager.Instance.IsPauseAllowed_Options = false;
        _player.IsParalyzed = true;

        yield return new WaitForSeconds(DEATH_SEQUENCE_DURATION);

        OverlayShuttersViewController.Instance.PlayCloseAndOpenSequence(RespawnPlayer, ShuttersFinishedOpening, 0.1f);
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


    void RespawnPlayer()
    {
        StartCoroutine(RespawnPlayer_CR());
    }
    IEnumerator RespawnPlayer_CR()
    {
        OverlayViewController.Instance.HidePlayerDiedOverlay();

        _playerDeathLocation = _playerController.transform.position;
        Locations.Instance.RespawnPlayer();

        yield return new WaitForSeconds(0.1f);

        _healthController.Reset();
        _healthController.SetHealth(HalfHeartsToHealth(NUM_HALF_HEARTS_AFTER_RESPAWN));
        GameplayHUDViewController.Instance.ShowView();
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
        _player.SwordProjectilesEnabled = true;

        if (WorldInfo.Instance.IsPausingAllowedInCurrentScene())
        {
            PauseManager.Instance.IsPauseAllowed_Inventory = true;
            PauseManager.Instance.IsPauseAllowed_Options = true;
        }
    }
}