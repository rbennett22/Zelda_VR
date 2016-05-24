using Immersio.Utility;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(HealthController))]

public class Player : Singleton<Player>
{
    const float MOON_MODE_GRAVITY_MODIFIER = 1 / 6.0f;
    const float DEFAULT_JINX_DURATION = 2.0f;
    const float DEFAULT_LIKE_LIKE_TRAP_DURATION = 3.5f;
    const int LIKE_LIKE_ESCAPE_COUNT = 8;
    const float WHISTLE_MELODY_DURATION = 4.0f;


    [SerializeField]
    ZeldaPlayerController _playerController;
    public ZeldaPlayerController PlayerController { get { return _playerController; } }


    Inventory _inventory;
    public Inventory Inventory { get { return _inventory; } }

    Weapon_Melee_Sword _equippedSword;
    public bool IsAttackingWithSword { get { return (_equippedSword != null) && _equippedSword.IsAttacking; } }

    Shield_Base _equippedShield;
    public Shield_Base EquippedShield { get { return _equippedShield; } }

    Item _equippedItem;
    public Item EquippedItem { get { return _equippedItem; } }
    Weapon_Base _equippedWeaponB;
    public Weapon_Base EquippedWeaponB { get { return _equippedWeaponB; } }

    [SerializeField]
    Transform _weaponContainerLeft, _weaponContainerRight, _shieldContainer;


    HealthController _healthController;
    public HealthController HealthController { get { return _healthController ?? (_healthController = GetComponent<HealthController>()); } }
    public bool IsAtFullHealth { get { return HealthController.IsAtFullHealth; } }
    public int HealthInHalfHearts { get { return PlayerHealthDelegate.HealthToHalfHearts(HealthController.Health); } }
    public bool IsAlive { get { return HealthController.IsAlive; } }


    public string RegisteredName { get; set; }
    public int DeathCount { get; set; }

    
    public bool IsJinxed { get; private set; }      // Can't use sword when jinxed

    bool _isInLikeLikeTrap;
    int _likeLikeTrapCounter;
    public bool IsInLikeLikeTrap            // Paralyzed, and will lose MagicShield if player doesn't press buttons fast enough
    {
        get { return _isInLikeLikeTrap; }
        private set
        {
            if (value == false && _isInLikeLikeTrap) { HealthController.ActivateTempInvinc(); }
            _isInLikeLikeTrap = value;
            _likeLikeTrapCounter = 0;
            IsParalyzed = value;
        }
    }

    bool _isParalyzed;
    public bool IsParalyzed { get { return _isParalyzed; } set { _isParalyzed = value; _playerController.enabled = !_isParalyzed; } }

    public bool IsInvincible { get { return HealthController.isIndestructible; } set { HealthController.isIndestructible = value; } }
    public void MakeInvincible(float duration)
    {
        StartCoroutine("MakeInvincibleCoroutine", duration);
    }
    IEnumerator MakeInvincibleCoroutine(float duration)
    {
        IsInvincible = true;
        yield return new WaitForSeconds(duration);
        IsInvincible = Cheats.Instance.InvincibilityIsEnabled;
    }

    public bool IsAirJumpingEnabled { get { return _playerController.airJumpingEnabled; } set { _playerController.airJumpingEnabled = value; } }

    int _jumpHeight = 0;
    public int JumpHeight
    {
        get { return _jumpHeight; }
        set
        {
            _jumpHeight = value;
            _playerController.JumpForce = _jumpHeight * 0.25f;
        }
    }

    bool _IsMoonModeEnabled;
    float _normalGravityModifier;
    public bool IsMoonModeEnabled
    {
        get { return _IsMoonModeEnabled; }
        set
        {
            if (!_IsMoonModeEnabled) { _normalGravityModifier = _playerController.GravityModifier; }
            _IsMoonModeEnabled = value;

            float gravMod = _normalGravityModifier;
            if (_IsMoonModeEnabled) { gravMod *= MOON_MODE_GRAVITY_MODIFIER; }
            _playerController.GravityModifier = gravMod;
        }
    }
    

    override protected void Awake()
    {
        base.Awake();

        _inventory = Inventory.Instance;

        RegisteredName = "";
    }


    public void EquipShield(string shieldName)
    {
        if (_equippedShield != null) { DeequipShield(); }

        Item item = _inventory.GetItem(shieldName);
        GameObject prefab = item.shieldPrefab;

        GameObject g = Instantiate(prefab) as GameObject;
        g.name = shieldName;

        Transform t = g.transform;
        t.SetParent(_shieldContainer);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;

        _equippedShield = g.GetComponent<Shield_Base>();
    }
    public void DeequipShield()
    {
        Destroy(_equippedShield);
        _equippedShield = null;
    }

    public void EquipSword(string swordName)
    {
        if (_equippedSword != null) { DeequipSword(); }

        Item item = _inventory.GetItem(swordName);
        GameObject prefab = item.weaponPrefab;

        GameObject g = Instantiate(prefab) as GameObject;
        g.name = swordName;

        Transform t = g.transform;
        t.SetParent(_weaponContainerRight);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;

        _equippedSword = g.GetComponent<Weapon_Melee_Sword>();

        SwordProjectilesEnabled = HealthController.IsAtFullHealth;
    }
    public void DeequipSword()
    {
        Destroy(_equippedSword);
        _equippedSword = null;
    }

    public bool SwordProjectilesEnabled {
        get { return (_equippedSword != null) && _equippedSword.ProjectilesEnabled; }
        set { if (_equippedSword != null) { _equippedSword.ProjectilesEnabled = value; } }
    }

    public void EquipSecondaryItem(string itemName)
    {
        if (_equippedItem != null) { DeequipSecondaryItem(); }

        _equippedItem = _inventory.GetItem(itemName);
        if (_equippedItem.weaponPrefab != null)
        {
            EquipSecondaryWeapon(itemName);
        }
    }
    void EquipSecondaryWeapon(string weaponName)
    {
        Item item = _inventory.GetItem(weaponName);

        GameObject g = Instantiate(item.weaponPrefab) as GameObject;
        g.name = weaponName;

        Transform t = g.transform;
        t.SetParent(_weaponContainerLeft);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;

        _equippedWeaponB = g.GetComponent<Weapon_Base>();
    }
    public void DeequipSecondaryItem()
    {
        Destroy(_equippedWeaponB);
        _equippedWeaponB = null;

        _equippedItem = null;
    }


    void Update()
    {
        if (PauseManager.Instance.IsPaused_Any) { return; }
        if (!IsAlive) { return; }

        if (IsInLikeLikeTrap)
        {
            if (ZeldaInput.GetButtonDown(ZeldaInput.Button.SwordAttack))
            {
                if (++_likeLikeTrapCounter >= LIKE_LIKE_ESCAPE_COUNT)
                {
                    IsInLikeLikeTrap = false;
                }
            }
        }

        if (IsParalyzed) { return; }

        ProcessUserInput();
    }

    void ProcessUserInput()
    {
        if (ZeldaInput.GetButtonDown(ZeldaInput.Button.SwordAttack))
        {
            AttackWithSword();
        }

        if (_equippedItem != null)
        {
            Weapon_Gun_Bow bow = (_equippedWeaponB == null) ? null : _equippedWeaponB.GetComponent<Weapon_Gun_Bow>();

            if (_equippedWeaponB != null)
            {
                // Aim Secondary Weapon
                Transform t = _weaponContainerLeft.transform;
                t.forward = _playerController.LineOfSight;
                if (bow == null)
                {
                    Vector3 fwd = t.forward;
                    fwd.y = 0;
                    t.forward = fwd;
                }
            }

            // Use Secondary Item?
            bool doUseItem = false;
            if (bow != null)
            {
                doUseItem = ZeldaInput.GetButtonUp(ZeldaInput.Button.UseItemB);
            }
            else
            {
                doUseItem = ZeldaInput.GetButtonDown(ZeldaInput.Button.UseItemB);
            }

            if (doUseItem)
            {
                _inventory.UseItemB();

                if (_equippedWeaponB != null)
                { 
                    AttackWithSecondaryWeapon();
                }
            }
        }
    }

    void AttackWithSword()
    {
        if (_equippedSword == null || !_equippedSword.CanAttack)
        {
            return;
        }
        if (IsJinxed)
        {
            return;
        }

        _equippedSword.Attack();
    }

    void AttackWithSecondaryWeapon()
    {
        if (_equippedWeaponB == null || !_equippedWeaponB.CanAttack)
        {
            return;
        }

        bool canAttack = true;

        Weapon_Gun_MagicWand wand = _equippedWeaponB.GetComponent<Weapon_Gun_MagicWand>();     // TODO
        if (wand != null)
        {
            wand.spawnFlame = Inventory.Instance.HasItem("MagicBook");
        }

        Weapon_Gun_Bow bow = _equippedWeaponB.GetComponent<Weapon_Gun_Bow>();     // TODO
        if (bow != null)
        {
            canAttack = false;
            if (_inventory.HasItem("WoodenArrow") || _inventory.HasItem("SilverArrow"))
            {
                if (_inventory.HasItem("Rupee"))
                {
                    _inventory.UseItem("Rupee");
                    canAttack = true;
                }
            }
        }

        if (canAttack)
        {
            _equippedWeaponB.Attack(_playerController.ForwardDirection);
        }
    }


    public void UseWhistle()
    {
        StartCoroutine("UseWhistle_CR");
    }
    int _nextWarpDungeonNum = 1;
    IEnumerator UseWhistle_CR()
    {
        if (IsJinxed)
        {
            DeactivateJinx();
        }

        ParalyzeAllNearbyEnemies(WHISTLE_MELODY_DURATION);
        ActivateParalyze(WHISTLE_MELODY_DURATION);

        Music.Instance.Stop();
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.whistle);

        yield return new WaitForSeconds(WHISTLE_MELODY_DURATION);

        Music.Instance.PlayAppropriateMusic();

        if (WorldInfo.Instance.IsInDungeon)
        {
            DungeonRoom dr = OccupiedDungeonRoom();
            if (dr != null)
            {
                EnemyAI_Digdogger digdogger = null;
                foreach (var enemy in dr.Enemies)
                {
                    digdogger = enemy.GetComponent<EnemyAI_Digdogger>();
                    if (digdogger != null && !digdogger.HasSplit)
                    {
                        digdogger.SplitIntoBabies();
                        break;
                    }
                }
            }
        }
        else if (WorldInfo.Instance.IsOverworld)
        {
            GameObject g = GameObject.FindGameObjectWithTag("Dungeon7Entrance");
            float dist = Vector3.Distance(g.transform.position, _playerController.transform.position);
            if (dist < 7)
            {
                g.GetComponent<Dungeon7Entrance>().EmptyLake();
            }
            else
            {
                Locations.Instance.WarpToOverworldDungeonEntrance(_nextWarpDungeonNum);

                // Determine next dungeon to warp to (if Whistle is blown again);
                Inventory inv = Inventory.Instance;
                bool canWarpToDungeon = false;
                do
                {
                    if (++_nextWarpDungeonNum > 8)
                    {
                        _nextWarpDungeonNum = 1;
                    }

                    canWarpToDungeon = inv.HasTriforcePieceForDungeon(_nextWarpDungeonNum);
                } while (!canWarpToDungeon);
            }
        }
    }


    public DungeonRoom OccupiedDungeonRoom()
    {
        if (!WorldInfo.Instance.IsInDungeon) { return null; }

        return DungeonRoom.GetRoomForPosition(_playerController.transform.position);
    }

    public float GetDamageModifier()
    {
        float mod = 1;
        if (_inventory.HasItem("RedRing"))
        {
            mod = 0.25f;
        }
        else if (_inventory.HasItem("BlueRing"))
        {
            mod = 0.5f;
        }
        return mod;
    }

    public bool CanBlockAttack(bool isBlockableByWoodenShield, bool isBlockableByMagicShield, Vector3 directionOfAttack)
    {
        // TODO: Determine isBlockableByWoodenShield and isBlockableByMagicShield using a lookup table internally

        if (IsAttackingWithSword)
        {
            return false;
        }

        bool canBlock = false;

        bool shieldCanBlockAttack = (_equippedShield != null) && _equippedShield.CanBlockAttack(directionOfAttack);
        if (shieldCanBlockAttack)
        {
            if (isBlockableByWoodenShield && _inventory.HasItem("WoodenShield"))
            {
                canBlock = true;
            }
            else if (isBlockableByMagicShield && _inventory.HasItem("MagicShield"))
            {
                canBlock = true;
            }
        }

        return canBlock;
    }


    public void ActivateParalyze(float duration)
    {
        StartCoroutine("ParalyzeCoroutine", duration);
    }
    IEnumerator ParalyzeCoroutine(float duration)
    {
        IsParalyzed = true;
        yield return new WaitForSeconds(duration);
        IsParalyzed = false;
    }

    public void ActivateJinx(float duration = DEFAULT_JINX_DURATION)
    {
        StartCoroutine("JinxCoroutine", duration);
    }
    public void DeactivateJinx()
    {
        if (!IsJinxed) { return; }

        StopCoroutine("JinxCoroutine");
        IsJinxed = false;
    }
    IEnumerator JinxCoroutine(float duration)
    {
        IsJinxed = true;
        yield return new WaitForSeconds(duration);
        IsJinxed = false;
    }

    EnemyAI_Random _likeLikeTrappingPlayer;
    public void ActivateLikeLikeTrap(GameObject likeLike, float duration = DEFAULT_LIKE_LIKE_TRAP_DURATION)
    {
        if (IsInLikeLikeTrap) { return; }
        _likeLikeTrappingPlayer = likeLike.GetComponent<EnemyAI_Random>();
        StartCoroutine("LikeLikeTrapCoroutine", duration);
    }
    IEnumerator LikeLikeTrapCoroutine(float duration)
    {
        IsInLikeLikeTrap = true;
        _likeLikeTrappingPlayer.enabled = false;
        yield return new WaitForSeconds(duration);

        if (_likeLikeTrappingPlayer != null)
        {
            _likeLikeTrappingPlayer.enabled = true;
            // If player didn't escape from trap in time, he loses MagicShield
            if (IsInLikeLikeTrap) { _inventory.RemoveItem("MagicShield"); }
        }

        IsInLikeLikeTrap = false;
    }


    public void ParalyzeAllNearbyEnemies(float duration)
    {
        if (WorldInfo.Instance.IsOverworld)
        {
            GameObject enemiesContainer = GameObject.FindGameObjectWithTag("Enemies");
            foreach (Transform child in enemiesContainer.transform)
            {
                Enemy enemy = child.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.Paralyze(duration);
                }
            }
        }
        else if (WorldInfo.Instance.IsInDungeon)
        {
            DungeonRoom roomPlayerIsIn = OccupiedDungeonRoom();
            foreach (Enemy enemy in roomPlayerIsIn.Enemies)
            {
                enemy.Paralyze(duration);
            }
        }
    }


    public void ForceNewForwardDirection(Vector3 direction)
    {
        //playerController.transform.rotation = Quaternion.Euler(lerpedEuler);
        _playerController.transform.forward = direction;
    }

    public void ForceNewEulerAngles(Vector3 euler)
    {
        _playerController.transform.eulerAngles = euler;
    }


    public void RestoreHearts(int hearts)
    {
        RestoreHalfHearts(hearts * 2);
    }
    public void RestoreHalfHearts(int halfHearts)
    {
        if (halfHearts <= 0) { return; }

        int healAmount = PlayerHealthDelegate.HalfHeartsToHealth(halfHearts);
        HealthController.RestoreHealth((uint)healAmount);
    }
}