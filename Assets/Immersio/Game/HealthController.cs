using UnityEngine;


public interface IHealthControllerDelegate 
{
	void OnHealthChanged (HealthController healthController, int newHealth);
    void OnDamageTaken(HealthController healthController, ref uint damageAmount, GameObject damageDealer);
	void OnHealthRestored (HealthController healthController, uint healAmount);
	void OnTempInvincibilityActivation (HealthController healthController, bool didActivate = true);
	void OnDeath (HealthController healthController, GameObject killer);
}


public class HealthController : MonoBehaviour 
{

	public IHealthControllerDelegate hcDelegate;

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
	public int Health {
		get { return _health; }
	}

    public bool IsAtFullHealth { get { return _health == maxHealth; } }

	bool _isAlive = true;
	public bool IsAlive {
		get { return _isAlive; }
	}


	GameObject _killer;


    void Awake()
    {
        IsTempInvincActive = false;
    }

	void Start () 
    {
		_health = maxHealth;
		if(GetComponent<Renderer>())
			_origMat = GetComponent<Renderer>().material;

		if(!tempInvincMat)
			CreateDefaultInvincMat();

        if (GetComponent<Renderer>()) { GetComponent<Renderer>().enabled = false; }
	}

	void CreateDefaultInvincMat () {
		if(!_origMat)
			return;

		Shader shader = Shader.Find("Transparent/Diffuse");
		tempInvincMat = new Material(shader);
		tempInvincMat.CopyPropertiesFromMaterial(_origMat);
		Color color = tempInvincMat.color;
		color.a = 0.7f;
		tempInvincMat.color = color;
	}


	void Update () 
    {
		if(IsTempInvincActive)
        {
			_tempInvincibilityTimer -= Time.deltaTime;
			if(_tempInvincibilityTimer < 0)
            {
				ActivateTempInvinc(false);
			}
		}
	}


	void OnDeath ()
    {
		_isAlive = false;
        if (hcDelegate != null)
        {
            hcDelegate.OnDeath(this, _killer);
        }

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


	#region Public Actions

	public bool RestoreHealth (uint healAmount = 999999) {
		if(!_isAlive || healAmount == 0)
			return false;
		SetHealth(_health + (int)healAmount);

		if(hcDelegate != null)
			hcDelegate.OnHealthRestored(this, healAmount);

		return true;
	}

	public bool TakeDamage (uint damageAmount, GameObject damageDealer) 
    {
		if(!_isAlive || isIndestructible || IsTempInvincActive)
			return false;

		if(hcDelegate != null)
            hcDelegate.OnDamageTaken(this, ref damageAmount, damageDealer);

        if (damageAmount == 0)
            return false;

		int newHealth = _health - (int)damageAmount;
		if(newHealth <= 0)
			_killer = damageDealer;
		SetHealth(newHealth);

		// Still alive after taking damage?
		if(_isAlive)
			ActivateTempInvinc();

        if (hurtSound != null)
        {
            SoundFx.Instance.PlayOneShot3D(transform.position, hurtSound);
        }

		return true;
	}

    public bool Kill(GameObject killer, bool overrideTempInvinc = false)
    {
        if (!_isAlive)
			return false;
        if (isIndestructible)
            return false;
		if(IsTempInvincActive && !overrideTempInvinc)
			return false;

		_killer = killer;

		SetHealth(0);

		return true;
	}

	public void Reset () 
    {
		_isAlive = true;
		IsTempInvincActive = false;
		_killer = null;

        RestoreHealth();
	}


    public void SetHealth(int newHealth)
    {
        if (!_isAlive || _health == newHealth)
            return;

        if (isIndestructible && newHealth < _health) { return; }

        _health = Mathf.Clamp(newHealth, 0, maxHealth);

        if (hcDelegate != null)
            hcDelegate.OnHealthChanged(this, _health);

        if (_health <= 0)
            OnDeath();
    }

    public void ActivateTempInvinc(bool doActivate = true)
    {
        if (!_isAlive || IsTempInvincActive == doActivate || tempInvincibilityDuration <= 0)
            return;

        _tempInvincibilityTimer = tempInvincibilityDuration;
        IsTempInvincActive = doActivate;

        if (tempInvincMat && doSwapMatOnTempInvinc)
        {
            GetComponent<Renderer>().material = doActivate ? tempInvincMat : _origMat;
        }

        if (hcDelegate != null)
            hcDelegate.OnTempInvincibilityActivation(this, doActivate);
    }

	#endregion

}
