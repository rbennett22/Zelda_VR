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
    HealthController _hc;

    Vector3 _playerDeathLocation;


    void Awake()
    {
        _player = GetComponent<Player>();
        _hc = GetComponent<HealthController>();

        _hc.HealthChanged += HealthChanged;
        _hc.DamageTaken += DamageTaken;
        _hc.Death += Death;
    }


    void HealthChanged(HealthController healthController, int prevHealth, int newHealth)
    {
        _player.SwordProjectilesEnabled = healthController.IsAtFullHealth;
        SetLowHealthSoundIsPlaying(_player.HealthInHalfHearts <= 2);
    }

    void DamageTaken(HealthController healthController, ref uint damageAmount, GameObject damageDealer)
    {
        damageAmount = (uint)(_player.GetDamageNullifier() * damageAmount);

        if (damageAmount > 0)
        {
            Vector3 direction = _player.Position - damageDealer.transform.position;
            direction.y = 0;
            direction.Normalize();
            Push(direction);

            PlayHurtSound();

            Enemy.EnemiesKilledWithoutTakingDamage = 0;
        }

        // TODO

        Enemy enemy = damageDealer.GetComponent<Enemy>();

        if (damageDealer.name == "Bubble_Clear")
        {
            _player.ActivateJinx();
        }
        else if (damageDealer.name == "LikeLike")
        {
            _player.ActivateLikeLikeTrap(enemy);
        }
    }

    void Death(HealthController healthController, GameObject killer)
    {
        _player.DeathCount++;

        StartCoroutine("DeathSequence");
    }
    IEnumerator DeathSequence()
    {
        Music.Instance.Stop();

        SetLowHealthSoundIsPlaying(false);
        PlayDeathSound();

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
            CharacterController cc = _player.PlayerController.GetComponent<CharacterController>();
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

        _playerDeathLocation = _player.Position;
        Locations.Instance.RespawnPlayer();

        yield return new WaitForSeconds(0.1f);

        _hc.Reset();
        _hc.SetHealth(HalfHeartsToHealth(NUM_HALF_HEARTS_AFTER_RESPAWN));
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


    void PlayHurtSound()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.hurt);
    }
    void PlayDeathSound()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.die);
    }
    void SetLowHealthSoundIsPlaying(bool doPlay)
    {
        SoundFx.Instance.SetLowHealthSoundIsPlaying();
    }
}