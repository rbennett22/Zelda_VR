using UnityEngine;
using Immersio.Utility;

public class Actor : MonoBehaviour 
{
    virtual public Vector3 Position
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    public Index2 Tile
    {
        get
        {
            Vector3 p = Position;
            return new Index2((int)p.x, (int)p.z);
            //return new Index2(Position - WorldInfo.Instance.WorldOffset - TileMap.TileExtents);     // TODO: This should work (?)
        }
        set
        {
            Vector3 pos = value.ToVector3() + WorldInfo.Instance.WorldOffset + TileMap.TileExtents;
            pos.y = Position.y;
            Position = pos;
        }
    }

    public Index2 GetOccupiedOverworldSector()
    {
        TileMap tileMap = CommonObjects.OverworldTileMap;
        if (tileMap == null)
        {
            return new Index2();
        }
        return tileMap.GetSectorForPosition(Position);
    }
    public DungeonRoom GetOccupiedDungeonRoom()
    {
        if (!WorldInfo.Instance.IsInDungeon) { return null; }

        return DungeonRoom.GetRoomForPosition(Position);
    }


    protected HealthController _healthController;
    public HealthController HealthController { get { return _healthController ?? (_healthController = GetComponent<HealthController>()); } }
    public bool IsAlive { get { return HealthController.IsAlive; } }
    public bool IsAtFullHealth { get { return HealthController.IsAtFullHealth; } }


    public Weapon_Base weapon;
    public bool HasWeapon { get { return weapon != null; } }

    public Shield_Base shield;
    public bool HasShield { get { return shield != null; } }
    public bool playSoundWhenBlockingAttack = true;

    virtual public bool CanBlockAttack(bool isBlockableByWoodenShield, bool isBlockableByMagicShield, Vector3 directionOfAttack)
    {
        // TODO

        if (_healthController.isIndestructible)
        {
            return true;
        }

        return HasShield && shield.CanBlockAttack(directionOfAttack);
    }


    virtual protected void Awake () 
	{
        _healthController = GetComponent<HealthController>();
    }
}