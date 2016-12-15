using Immersio.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Actor
{
    [SerializeField]
    ZeldaPlayerController _playerController;
    public ZeldaPlayerController PlayerController { get { return _playerController; } }

    override public Vector3 Position
    {
        get { return _playerController.transform.position; }
        set { _playerController.transform.position = value; }
    }

    public Vector3 ForwardDirection {
        get { return _playerController.ForwardDirection; }
        private set { _playerController.transform.forward = value; }
    }
    public void ForceNewForwardDirection(Vector3 direction)
    {
        ForwardDirection = direction;
    }
    public void ForceNewRotation(Vector3 euler)
    {
        _playerController.transform.eulerAngles = euler;
    }

    public Vector3 LastAttemptedMoveDirection { get { return _playerController.LastAttemptedMotion; } }


    public int HealthInHalfHearts { get { return PlayerHealthDelegate.HealthToHalfHearts(HealthController.Health); } }
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


    public string RegisteredName { get; set; }
    public int DeathCount { get; set; }


    Index2 _prevOccupiedSector;


    #region Inventory & Weapons

    Inventory _inventory;
    public Inventory Inventory { get { return _inventory; } }

    Weapon_Melee_Sword _sword;
    public bool HasSword { get { return _sword != null; } }
    public void EquipSword(string swordName)
    {
        if (HasSword) { DeequipSword(); }

        Item item = _inventory.GetItem(swordName);
        GameObject prefab = item.weaponPrefab;

        GameObject g = Instantiate(prefab) as GameObject;
        g.name = prefab.name;

        Transform t = g.transform;
        t.SetParent(_playerController.WeaponContainerRight);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;

        _sword = g.GetComponent<Weapon_Melee_Sword>();

        SwordProjectilesEnabled = HealthController.IsAtFullHealth;
    }
    public void DeequipSword()
    {
        if (HasSword)
        {
            Destroy(_sword.gameObject);
            _sword = null;
        }
    }
    public bool IsAttackingWithSword { get { return HasSword && _sword.IsAttacking; } }
    public bool SwordProjectilesEnabled
    {
        get { return HasSword && _sword.ProjectilesEnabled; }
        set { if (HasSword) { _sword.ProjectilesEnabled = value; } }
    }

    public void EquipShield(string shieldName)
    {
        if (shield != null) { DeequipShield(); }

        Item item = _inventory.GetItem(shieldName);
        GameObject prefab = item.shieldPrefab;

        GameObject g = Instantiate(prefab) as GameObject;
        g.name = prefab.name;

        Transform t = g.transform;
        t.SetParent(_playerController.ShieldContainer);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;

        shield = g.GetComponent<Shield_Base>();
    }
    public void DeequipShield()
    {
        if (shield != null)
        {
            Destroy(shield.gameObject);
            shield = null;
        }
    }

    Item _equippedItem;
    public void EquipItem(string itemName)
    {
        if (_equippedItem != null) { DeequipItem(); }

        _equippedItem = _inventory.GetItem(itemName);

        GameObject weaponPrefab = _equippedItem.weaponPrefab;
        if (weaponPrefab != null)
        {
            EquipWeaponB(weaponPrefab);
        }
    }
    public void DeequipItem()
    {
        DeequipWeaponB();

        _equippedItem = null;
    }

    void EquipWeaponB(GameObject prefab)
    {
        GameObject g = Instantiate(prefab);
        g.name = prefab.name;

        Transform t = g.transform;
        t.SetParent(_playerController.WeaponContainerLeft);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;

        weapon = g.GetComponent<Weapon_Base>();
    }
    void DeequipWeaponB()
    {
        if (weapon != null)
        {
            Destroy(weapon.gameObject);
            weapon = null;
        }
    }

    #endregion Inventory & Weapons


    #region Status Effects

    const float MOON_MODE_GRAVITY_MODIFIER = 1 / 6.0f;
    const float DEFAULT_JINX_DURATION = 2.0f;
    const float DEFAULT_LIKE_LIKE_TRAP_DURATION = 3.5f;
    const int LIKE_LIKE_ESCAPE_COUNT = 8;


    public bool IsInDungeonRoom { get; set; }    // Player can't attack when he's standing in the corridor between dungeon rooms

    // Jinx:  Player can't use sword
    public bool IsJinxed { get; private set; }      
    public void ActivateJinx(float duration = DEFAULT_JINX_DURATION)
    {
        StartCoroutine("Jinx_CR", duration);
    }
    public void DeactivateJinx()
    {
        if (!IsJinxed) { return; }

        StopCoroutine("Jinx_CR");
        IsJinxed = false;
    }
    IEnumerator Jinx_CR(float duration)
    {
        IsJinxed = true;
        yield return new WaitForSeconds(duration);
        IsJinxed = false;
    }

    // LikeLikeTrap:  Player is Paralyzed, and will lose MagicShield if he doesn't repeatedly press Attack button fast enough
    bool _isInLikeLikeTrap;
    int _likeLikeTrapCounter;
    public bool IsInLikeLikeTrap
    {
        get { return _isInLikeLikeTrap; }
        private set
        {
            if (value == false && _isInLikeLikeTrap)
            {
                HealthController.ActivateTempInvinc();
            }
            _isInLikeLikeTrap = value;
            _likeLikeTrapCounter = 0;
            IsParalyzed = value;
        }
    }
    Enemy _likeLikeTrappingPlayer;
    public void ActivateLikeLikeTrap(Enemy likeLike, float duration = DEFAULT_LIKE_LIKE_TRAP_DURATION)
    {
        if (IsInLikeLikeTrap) { return; }

        _likeLikeTrappingPlayer = likeLike;
        StartCoroutine("LikeLikeTrap_CR", duration);
    }
    IEnumerator LikeLikeTrap_CR(float duration)
    {
        IsInLikeLikeTrap = true;
        _likeLikeTrappingPlayer.enabled = false;

        yield return new WaitForSeconds(duration);

        if (_likeLikeTrappingPlayer != null)
        {
            _likeLikeTrappingPlayer.enabled = true;

            if (IsInLikeLikeTrap)
            {
                // If player didn't escape from trap in time, he loses MagicShield
                _inventory.RemoveItem("MagicShield");
            }
        }

        IsInLikeLikeTrap = false;
    }
    void UpdateLikeLikeTrap()
    {
        if (!IsInLikeLikeTrap)
        {
            return;
        }
        
        if (ZeldaInput.GetButtonDown(ZeldaInput.Button.SwordAttack))
        {
            if (++_likeLikeTrapCounter >= LIKE_LIKE_ESCAPE_COUNT)
            {
                IsInLikeLikeTrap = false;
            }
        }
    }

    bool _isParalyzed;
    public bool IsParalyzed { get { return _isParalyzed; } set { _isParalyzed = value; _playerController.enabled = !_isParalyzed; } }
    public void ActivateParalyze(float duration)
    {
        StartCoroutine("Paralyze_CR", duration);
    }
    IEnumerator Paralyze_CR(float duration)
    {
        IsParalyzed = true;
        yield return new WaitForSeconds(duration);
        IsParalyzed = false;
    }

    public bool IsInvincible { get { return HealthController.isIndestructible; } set { HealthController.isIndestructible = value; } }
    public void MakeInvincible(float duration)
    {
        StartCoroutine("MakeInvincible_CR", duration);
    }
    IEnumerator MakeInvincible_CR(float duration)
    {
        IsInvincible = true;
        yield return new WaitForSeconds(duration);
        IsInvincible = Cheats.Instance.InvincibilityIsEnabled;
    }


    public bool IsAirJumpingEnabled { get { return _playerController.airJumpingEnabled; } set { _playerController.airJumpingEnabled = value; } }
    public bool IsFlyingEnabled { get { return _playerController.flyingEnabled; } set { _playerController.flyingEnabled = value; } }

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

    #endregion Status Effects


    #region Events

    public delegate void OccupiedSectorChangedEventHandler(Index2 prevSector, Index2 newSector);
    public event OccupiedSectorChangedEventHandler OccupiedSectorChanged;

    void OnOccupiedSectorChanged(Index2 prevSector, Index2 newSector)
    {
        //print("OnOccupiedSectorChanged() :  prevSector = " + prevSector + "  newSector = " + newSector);

        if (OccupiedSectorChanged != null)
            OccupiedSectorChanged(prevSector, newSector);
    }

    #endregion Events


    override protected void Awake()
    {
        base.Awake();

        _inventory = Inventory.Instance;

        RegisteredName = "";
    }


    void Update()
    {
        UpdateEvents();


        if (PauseManager.Instance.IsPaused_Any) { return; }
        if (!IsAlive) { return; }

        if (IsInLikeLikeTrap)
        {
            UpdateLikeLikeTrap();
        }

        if (IsParalyzed) { return; }


        ProcessUserInput();

        EnsureNoWorldFallThrough();
    }

    void UpdateEvents()
    {
        if (WorldInfo.Instance.IsOverworld)
        {
            Index2 s = GetOccupiedOverworldSector();
            if (s != _prevOccupiedSector)
            {
                OnOccupiedSectorChanged(_prevOccupiedSector, s);
                _prevOccupiedSector = s;
            }
        }
    }

    void ProcessUserInput()
    {
        if (ZeldaInput.GetButtonDown(ZeldaInput.Button.SwordAttack))
        {
            AttackWithSword();
        }

        if (_equippedItem != null)
        {
            Weapon_Gun_Bow bow = (weapon == null) ? null : weapon.GetComponent<Weapon_Gun_Bow>();

            if (weapon != null)
            {
                // Aim WeaponB
                /*Transform t = _playerController.WeaponContainerLeft;
                t.forward = _playerController.LineOfSight;
                if (bow == null)
                {
                    Vector3 fwd = t.forward;
                    fwd.y = 0;
                    t.forward = fwd;
                }*/
            }

            // Use Item?
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
                if (weapon != null)
                { 
                    AttackWithWeaponB();
                }

                _inventory.UseItemB();
            }
        }
    }

    void AttackWithSword()
    {
        if (!HasSword || !_sword.CanAttack)
        {
            return;
        }
        if (IsJinxed)
        {
            return;
        }
        if (WorldInfo.Instance.IsInDungeon && !IsInDungeonRoom)
        {
            return;
        }

        _sword.Attack();
    }

    void AttackWithWeaponB()
    {
        if (weapon == null || !weapon.CanAttack)
        {
            return;
        }

        bool canAttack = true;

        Weapon_Gun_MagicWand wand = weapon.GetComponent<Weapon_Gun_MagicWand>();     // TODO
        if (wand != null)
        {
            wand.spawnFlame = Inventory.Instance.HasItem("MagicBook");
        }

        Weapon_Gun_Bow bow = weapon.GetComponent<Weapon_Gun_Bow>();     // TODO
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
            weapon.Attack(ForwardDirection);
        }
    }


    public float GetDamageNullifier()
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

    override public bool CanBlockAttack(bool isBlockableByWoodenShield, bool isBlockableByMagicShield, Vector3 directionOfAttack)
    {
        // TODO: Determine isBlockableByWoodenShield and isBlockableByMagicShield using a lookup table internally

        if (_healthController.isIndestructible)
        {
            return true;
        }

        if (IsAttackingWithSword)
        {
            return false;
        }

        bool shieldCanBlockAttack = (shield != null) && shield.CanBlockAttack(directionOfAttack);
        if (!shieldCanBlockAttack)
        {
            return false;
        }

        if (isBlockableByWoodenShield && _inventory.HasItem("WoodenShield"))
        {
            return true;
        }
        if (isBlockableByMagicShield && _inventory.HasItem("MagicShield"))
        {
            return true;
        }

        return false;
    }


    public void ParalyzeAllNearbyEnemies(float duration)
    {
        foreach (Enemy enemy in GetEnemiesInCurrentRoomOrSector())
        {
            enemy.Paralyze(duration);
        }
    }

    public List<Enemy> GetEnemiesInCurrentRoomOrSector()
    {
        List<Enemy> enemies = new List<Enemy>();

        if (WorldInfo.Instance.IsOverworld)
        {
            enemies = FindObjectsInSector<Enemy>(GetOccupiedOverworldSector());
        }
        else if (WorldInfo.Instance.IsInDungeon)
        {
            enemies = GetOccupiedDungeonRoom().Enemies;
        }

        return enemies;
    }


    void EnsureNoWorldFallThrough()
    {
        const float THRESHOLD_Y = -8f;

        if (_playerController.IsGrounded) { return; }
        if (Position.y > THRESHOLD_Y) { return; }

        ReturnToGroundLevel();
    }

    public void ReturnToGroundLevel()
    {
        _playerController.Stop();

        float groundY = WorldInfo.Instance.WorldOffset.y;
        float playerOffset = _playerController.Height * 0.5f;
        _playerController.transform.SetY(groundY + playerOffset + 0.5f);
    }


    bool _playerGroundCollisionEnabled;
    public bool PlayerGroundCollisionEnabled
    {
        get { return _playerGroundCollisionEnabled; }
        set
        {
            if (Cheats.Instance.GhostModeIsEnabled)
            {
                return;
            }

            _playerGroundCollisionEnabled = value;

            int playerLayer = CommonObjects.Player_G.layer;
            int groundLayer = LayerMask.NameToLayer("Ground");
            int blocksLayer = LayerMask.NameToLayer("Blocks");

            Physics.IgnoreLayerCollision(playerLayer, groundLayer, !_playerGroundCollisionEnabled);
            Physics.IgnoreLayerCollision(playerLayer, blocksLayer, !_playerGroundCollisionEnabled);
        }
    }
}