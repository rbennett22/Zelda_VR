using UnityEngine;

public class HealthController : HealthController_Abstract
{
    #region Delegates

    public delegate void HealthChanged_Del(HealthController healthController, int prevHealth, int newHealth);
    public HealthChanged_Del HealthChanged;

    public delegate void DamageTaken_Del(HealthController healthController, ref uint damageAmount, GameObject damageDealer);
    public DamageTaken_Del DamageTaken;

    public delegate void OnHealthRestored_Del(HealthController healthController, uint healAmount);
    public OnHealthRestored_Del HealthRestored;

    public delegate void OnTempInvincibilityActivation_Del(HealthController healthController, bool didActivate);
    public OnTempInvincibilityActivation_Del TempInvincibilityActivation;

    public delegate void OnDeath_Del(HealthController healthController, GameObject killer);
    public OnDeath_Del Death;

    #endregion Delegates


    public int maxHealth = 3;
    public bool isIndestructible;

    public float tempInvincibilityDuration = 1.5f;
    float _tempInvincibilityTimer = 0.0f;
    public bool IsTempInvincActive { get; private set; }

    public bool destroyOnDeath = true;
    public GameObject deadPrefab;
    public Material tempInvincMat;
    Material _origMat;
    public bool doSwapMatOnTempInvinc = true;

    public AudioClip hurtSound, deathSound;


    int _health;
    public int Health
    {
        get { return _health; }
    }

    public bool IsAtFullHealth { get { return _health == maxHealth; } }

    bool _isAlive = true;
    public bool IsAlive
    {
        get { return _isAlive; }
    }


    GameObject _killer;


    void Awake()
    {
        IsTempInvincActive = false;
    }

    void Start()
    {
        _health = maxHealth;
        if (GetComponent<Renderer>())
            _origMat = GetComponent<Renderer>().material;

        if (!tempInvincMat)
            CreateDefaultInvincMat();

        if (GetComponent<Renderer>()) { GetComponent<Renderer>().enabled = false; }
    }

    void CreateDefaultInvincMat()
    {
        if (!_origMat)
            return;

        Shader shader = Shader.Find("Transparent/Diffuse");
        tempInvincMat = new Material(shader);
        tempInvincMat.CopyPropertiesFromMaterial(_origMat);
        Color color = tempInvincMat.color;
        color.a = 0.7f;
        tempInvincMat.color = color;
    }


    void Update()
    {
        if (IsTempInvincActive)
        {
            _tempInvincibilityTimer -= Time.deltaTime;
            if (_tempInvincibilityTimer < 0)
            {
                ActivateTempInvinc(false);
            }
        }
    }


    public override void SetHealth(int newHealth)
    {
        if (!_isAlive || _health == newHealth)
            return;

        if (isIndestructible && newHealth < _health) { return; }

        int prevHealth = _health;

        _health = Mathf.Clamp(newHealth, 0, maxHealth);
        if (_health != prevHealth)
        {
            OnHealthChanged(prevHealth, _health);
        }

        if (_health <= 0)
        {
            Die();
        }
    }
    void OnHealthChanged(int prevHealth, int newHealth)
    {
        if (HealthChanged != null)
            HealthChanged(this, prevHealth, newHealth);
    }

    public bool RestoreHealth()
    {
        return RestoreHealth((uint)maxHealth);
    }
    public override bool RestoreHealth(uint healAmount)
    {
        if (!_isAlive || healAmount == 0)
            return false;
        SetHealth(_health + (int)healAmount);

        OnHealthRestored(healAmount);

        return true;
    }
    void OnHealthRestored(uint healAmount)
    {
        if (HealthRestored != null)
            HealthRestored(this, healAmount);
    }

    public override bool TakeDamage(uint damageAmount, GameObject damageDealer)
    {
        if (!_isAlive || isIndestructible || IsTempInvincActive)
        {
            return false;
        }

        OnDamageTaken(ref damageAmount, damageDealer);

        if (damageAmount == 0)
        {
            return false;
        }

        int newHealth = _health - (int)damageAmount;
        if (newHealth <= 0)
        {
            _killer = damageDealer;
        }

        SetHealth(newHealth);

        // Still alive after taking damage?
        if (_isAlive)
        {
            ActivateTempInvinc();
        }

        if (hurtSound != null)
        {
            SoundFx.Instance.PlayOneShot3D(transform.position, hurtSound);
        }

        return true;
    }
    void OnDamageTaken(ref uint damageAmount, GameObject damageDealer)
    {
        if (DamageTaken != null)
            DamageTaken(this, ref damageAmount, damageDealer);
    }

    // TODO
    public override bool Kill(GameObject killer)
    {
        return Kill(killer);
    }
    public bool Kill(GameObject killer, bool overrideTempInvinc = false)
    {
        if (!_isAlive)
            return false;
        if (isIndestructible)
            return false;
        if (IsTempInvincActive && !overrideTempInvinc)
            return false;

        _killer = killer;

        SetHealth(0);

        return true;
    }

    void Die()
    {
        if (!_isAlive)
        {
            return;
        }

        _isAlive = false;

        OnDeath(this, _killer);

        if (deathSound != null)
        {
            SoundFx.Instance.PlayOneShot3D(transform.position, deathSound);
        }

        if (destroyOnDeath)
        {
            gameObject.SetActive(false);
            Destroy(gameObject, 0.5f);
        }
    }
    void OnDeath(HealthController healthController, GameObject killer)
    {
        if (Death != null)
            Death(this, killer);
    }


    public void Reset()
    {
        _isAlive = true;
        IsTempInvincActive = false;
        _killer = null;

        RestoreHealth();
    }


    public void ActivateTempInvinc(bool doActivate = true)
    {
        if (!_isAlive || IsTempInvincActive == doActivate || tempInvincibilityDuration <= 0)
        {
            return;
        }

        _tempInvincibilityTimer = tempInvincibilityDuration;
        IsTempInvincActive = doActivate;

        if (tempInvincMat && doSwapMatOnTempInvinc)
        {
            GetComponent<Renderer>().material = doActivate ? tempInvincMat : _origMat;
        }

        OnTempInvincibilityActivation(doActivate);
    }
    void OnTempInvincibilityActivation(bool doActivate)
    {
        if (TempInvincibilityActivation != null)
            TempInvincibilityActivation(this, doActivate);
    }
}