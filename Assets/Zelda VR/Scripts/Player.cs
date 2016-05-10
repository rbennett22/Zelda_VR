using UnityEngine;
using System.Collections;
using Immersio.Utility;

public class Player : Singleton<Player> 
{
    const float ShieldBlockDotThreshold = 0.6f;   // [0-1].  Closer to 1 means player has to be facing an incoming projectile more directly in order to block it.
    const float MoonModeGravityModifier = 1 / 6.0f;
    const float DefaultJinxDuration = 2.0f;
    const float DefaultLikeLikeTrapDuration = 3.5f;
    const int LikeLikeEscapeCount = 8;
    const float WhistleMelodyDuration = 4.0f;


    [SerializeField]
    ZeldaPlayerController _playerController;
    public ZeldaPlayerController PlayerController { get { return _playerController; } }


    Inventory _inventory;
    GameObject _sword;
    GameObject _equippedItem;

    HealthController _healthController;
    public HealthController HealthController { get { return _healthController ?? (_healthController = GetComponent<HealthController>()); } }

    public string RegisteredName { get; set; }
    public int DeathCount { get; set; }

    public bool IsAtFullHealth { get { return HealthController.IsAtFullHealth; } }
    public int HealthInHalfHearts { get { return PlayerHealthDelegate.HealthToHalfHearts(HealthController.Health); } }

    int _jumpHeight = 0;
    public int JumpHeight { 
        get { return _jumpHeight; }
        set {
            _jumpHeight = value;
            _playerController.JumpForce = _jumpHeight * 0.25f;
        } 
    }
    public Inventory Inventory { get { return _inventory; } }
    public GameObject EquippedItem { get { return _equippedItem; } }

    public Transform weaponContainerLeft, weaponContainerRight;

    public bool IsJinxed { get; set; }      // Can't use sword when jinxed
    public bool IsInLikeLikeTrap            // Paralyzed, and will lose MagicShield if player doesn't press buttons fast enough
    {          
        get { return _isInLikeLikeTrap; } 
        private set {
            if (value == false && _isInLikeLikeTrap) { HealthController.ActivateTempInvinc(); }
            _isInLikeLikeTrap = value; 
            _likeLikeTrapCounter = 0; 
            IsParalyzed = value;
        } 
    }
    public bool IsParalyzed { get { return _isParalyzed; } set { _isParalyzed = value; _playerController.enabled = !_isParalyzed; } }
    public bool IsInvincible { get { return HealthController.isIndestructible; } set { HealthController.isIndestructible = value; } }
    public bool IsAirJumpingEnabled { get { return _playerController.airJumpingEnabled; } set { _playerController.airJumpingEnabled = value; } }

    bool _IsMoonModeEnabled;
    float _normalGravityModifier;
    public bool IsMoonModeEnabled { 
        get { return _IsMoonModeEnabled; } 
        set {
            if (!_IsMoonModeEnabled) { _normalGravityModifier = _playerController.GravityModifier; }
            _IsMoonModeEnabled = value;

            float gravMod = _normalGravityModifier;
            if (_IsMoonModeEnabled) { gravMod *= MoonModeGravityModifier; }
            _playerController.GravityModifier = gravMod;
        }
    }
    public bool IsDead { get { return !HealthController.IsAlive; } }

    public bool IsAttackingWithSword
    {
        get
        {
            if (_sword == null) { return false; }
            return _sword.GetComponent<Sword>().IsAttacking;
        }
    }

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


    bool _isInLikeLikeTrap;                 
    int _likeLikeTrapCounter;
    bool _isParalyzed;


    override protected void Awake()
    {
        base.Awake();

        _inventory = Inventory.Instance;

        RegisteredName = "";
    }


    public void EquipSword(string swordName)
    {
        if (_sword != null) { DeequipSword(); }

        Item sworditem = _inventory.GetItem(swordName);
        GameObject swordPrefab = sworditem.weaponPrefab;

        _sword = Instantiate(swordPrefab) as GameObject;
        _sword.name = swordName;
        _sword.transform.parent = weaponContainerRight;
        _sword.transform.localPosition = Vector3.zero;
        _sword.transform.localRotation = Quaternion.Euler(90, 0, 0);

        EnableSwordProjectiles(HealthController.IsAtFullHealth);
    }

    public void DeequipSword()
    {
        Destroy(_sword);
        _sword = null;
    }

    public void EnableSwordProjectiles(bool doEnable = true)
    {
        if (_sword == null) { return; }
        Sword st = _sword.GetComponent<Sword>();
        st.projectileEnabled = doEnable;
    }


    public void EquipSecondaryItem(string itemName)
    {
        if (_equippedItem != null) { DeequipSecondaryItem(); }

        Item item = _inventory.GetItem(itemName);
        if (item.weaponPrefab != null)
        {
            EquipSecondaryWeapon(itemName);
        }
        else
        {
            _equippedItem = item.gameObject;
        }
    }

    void EquipSecondaryWeapon(string weaponName)
    {
        Item weaponItem = _inventory.GetItem(weaponName);

        _equippedItem = Instantiate(weaponItem.weaponPrefab) as GameObject;
        _equippedItem.name = weaponName;
        _equippedItem.transform.parent = weaponContainerLeft;
        _equippedItem.transform.localPosition = Vector3.zero;
        _equippedItem.transform.localRotation = Quaternion.identity;
    }

    public void DeequipSecondaryItem()
    {
        if (_equippedItem != null && _equippedItem.GetComponent<Item>() == null)
        {
            Destroy(_equippedItem);
        }
        _equippedItem = null;
    }


    void Update()
    {
        if (PauseManager.Instance.IsPaused_Any) { return; }
        if (IsDead) { return; }

        if (IsInLikeLikeTrap)
        {
            if (ZeldaInput.GetButtonDown(ZeldaInput.Button.SwordAttack))
            {
                if (++_likeLikeTrapCounter >= LikeLikeEscapeCount)
                {
                    IsInLikeLikeTrap = false;
                }
            }
        }

        if (IsParalyzed) { return; }


        if (ZeldaInput.GetButtonDown(ZeldaInput.Button.SwordAttack))
        {
            if (_sword != null && !IsJinxed)
            {
                _sword.GetComponent<Sword>().Attack();
            }
        }

        weaponContainerLeft.transform.forward = _playerController.LineOfSight;

        if (_equippedItem != null)
        {
            Bow bow = _equippedItem.GetComponent<Bow>();
            if (bow != null)
            {
                if (ZeldaInput.GetButtonUp(ZeldaInput.Button.UseItemB))
                {
                    UseSecondaryItem();
                }
            }
            else
            {
                Vector3 frwd = weaponContainerLeft.transform.forward;
                frwd.y = 0;
                weaponContainerLeft.transform.forward = frwd;

                if (ZeldaInput.GetButtonDown(ZeldaInput.Button.UseItemB))
                {
                    UseSecondaryItem();
                }
            }
        }
    }

    void UseSecondaryItem()
    {
        BombDropper w = _equippedItem.GetComponent<BombDropper>();
        if (w != null)
        {
            if (w.CanUse)
            { 
                w.DropBomb();
                _inventory.UseItemB(); 
            }
            return;
        }

        Candle c = _equippedItem.GetComponent<Candle>();
        if (c != null)
        {
            if (c.CanUse)
            {
                c.DropFlame();
            }
            return;
        }

        Boomerang b = _equippedItem.GetComponent<Boomerang>();
        if (b != null)
        {
            if (b.CanUse) 
            {
                b.Throw(weaponContainerLeft, _playerController.ForwardDirection);
            }
            return;
        }

        Bow bow = _equippedItem.GetComponent<Bow>();
        if (bow != null)
        {
            if (_inventory.HasItem("WoodenArrow") || _inventory.HasItem("SilverArrow"))
            {
                if (bow.CanUse && _inventory.HasItem("Rupee"))
                {
                    bow.Fire();
                    _inventory.UseItem("Rupee");
                }
            }
            return;
        }

        MagicWand wand = _equippedItem.GetComponent<MagicWand>();
        if (wand != null)
        {
            if (wand.CanUse)
            {
                wand.spawnFlame = Inventory.Instance.HasItem("MagicBook");
                wand.Fire();
            }
            return;
        }

        BaitDropper bd = _equippedItem.GetComponent<BaitDropper>();
        if (bd != null)
        {
            if (bd.CanUse)
            {
                bd.DropBait();
            }
            return;
        }

        if (_equippedItem.name == "Whistle")
        {
            StartCoroutine("UseWhistle");
            return;
        }

        _inventory.UseItemB();
    }

    int _nextWarpDungeonNum = 1;
    IEnumerator UseWhistle()
    {
        if (IsJinxed)
        {
            DeactivateJinx();
        }

        ParalyzeAllNearbyEnemies(WhistleMelodyDuration);
        ActivateParalyze(WhistleMelodyDuration);

        Music.Instance.Stop();
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.whistle);

        yield return new WaitForSeconds(WhistleMelodyDuration);

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

    public bool CanBlockAttack(bool isBlockableByWoodenShield, bool isBlockableByMagicShield, Vector3 attacksForwardDirection)
    {
        if (_sword != null && _sword.GetComponent<Sword>().IsAttacking)
        {
            return false;
        }

        bool canBlock = false;

        bool attackIsDirectlyInFrontOfPlayer = Vector3.Dot(_playerController.ForwardDirection, -attacksForwardDirection) > ShieldBlockDotThreshold;
        if (attackIsDirectlyInFrontOfPlayer)
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

    public void ActivateJinx(float duration = DefaultJinxDuration)
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
    public void ActivateLikeLikeTrap(GameObject likeLike, float duration = DefaultLikeLikeTrapDuration)
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
            if (IsInLikeLikeTrap) { _inventory.GetItem("MagicShield").count = 0; }
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


    public void RestoreHalfHearts(int halfHearts)
    {
        if (halfHearts <= 0) { return; }

        int healAmount = PlayerHealthDelegate.HalfHeartsToHealth(halfHearts);
        HealthController.RestoreHealth((uint)healAmount);
    }
    public void RestoreHearts(int hearts)
    {
        RestoreHalfHearts(hearts * 2);
    }
}