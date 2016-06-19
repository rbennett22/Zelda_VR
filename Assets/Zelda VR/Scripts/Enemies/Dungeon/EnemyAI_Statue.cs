#pragma warning disable 0162 // unreachable code detected

using UnityEngine;

public class EnemyAI_Statue : EnemyAI
{
    const bool FIRES_AT_PLAYER = false;


    public float baseAttackCooldown = 2.0f;
    public float randomAttackCooldownOffset = 0.5f;


    float _lastAttackTime = float.NegativeInfinity;
    float _attackCooldown;


    protected override void Awake()
    {
        base.Awake();

        if (WorldInfo.Instance.IsInDungeon)
        {
            _enemy.DungeonRoomRef = DungeonRoom.GetRoomForPosition(transform.position);

            int dungeonNum = WorldInfo.Instance.DungeonNum;
            GetComponent<Renderer>().material = CommonObjects.Instance.GetEnemyStatueMaterialForDungeon(dungeonNum);
        }

        InstantiateInvisibleBlock();
    }

    void InstantiateInvisibleBlock()
    {
        GameObject block = Instantiate(CommonObjects.Instance.invisibleBlockStatuePrefab) as GameObject;
        DungeonFactory df = CommonObjects.CurrentDungeonFactory;
        if (df != null)
        {
            block.transform.SetParent(df.blocksContainer);
        }

        Vector3 pos = transform.position;
        pos.y = transform.localScale.y * 0.5f;
        block.transform.position = pos;
    }

    void Start()
    {
        ResetCooldownTimer();
    }


    void Update()
    {
        if (!_doUpdate) { return; }
        if (IsPreoccupied) { return; }

        bool timesUp = (Time.time - _lastAttackTime >= _attackCooldown);
        if (timesUp)
        {
            Attack();

            GetComponent<Renderer>().enabled = true;        // TODO: not sure where it is being set to false
        }
    }

    void Attack()
    {
        Vector3 dir = DirectionToPlayer;

        if (!FIRES_AT_PLAYER)
        {
            Vector3 roomCenter = _enemy.DungeonRoomRef.Center;
            Vector3 toCenter = roomCenter - transform.position;
            toCenter.y = 0;

            dir = toCenter.normalized;
        }

        _enemy.Attack(dir);

        ResetCooldownTimer();
    }

    void ResetCooldownTimer()
    {
        _lastAttackTime = Time.time;
        _attackCooldown = baseAttackCooldown + Random.Range(-randomAttackCooldownOffset, randomAttackCooldownOffset);
    }
}