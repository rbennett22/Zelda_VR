using UnityEngine;
using Immersio.Utility;
using System.Collections.Generic;

[RequireComponent(typeof(HealthController))]

public class Actor : MonoBehaviour 
{
    virtual public Vector3 Position
    {
        get { return transform.position; }
        set { transform.position = value; }
    }
    virtual public Vector2 PositionXZ
    {
        get { return new Vector2(Position.x, Position.z); }
        set { Position = new Vector3(value.x, Position.y, value.y); }
    }

    public Index2 Tile
    {
        get { return PositionToTile(Position); }
        set { PositionXZ = TileToPosition_Center(value); }
    }

    public Index2 GetOccupiedOverworldSector()
    {
        TileMap tileMap = CommonObjects.OverworldTileMap;
        if (tileMap == null)
        {
            return new Index2();
        }
        return tileMap.GetSectorContainingPosition(Position);
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


    // TODO: move these methods elsewhere

    static public Index2 PositionToTile(Vector3 pos)
    {
        return new Index2((int)pos.x, (int)pos.z);
        //return new Index2(pos - WorldInfo.Instance.WorldOffset - TileMap.TileExtents);     // TODO: This should work (?)
    }
    static public Vector2 TileToPosition_Center(Index2 tile)
    {
        Vector3 p = tile.ToVector3() + WorldInfo.Instance.WorldOffset + TileMap.TileExtents;
        return new Vector2(p.x, p.z);
    }

    static public List<T> FindObjectsInSector<T>(Index2 sector) where T : Component
    {
        if (!WorldInfo.Instance.IsOverworld)
        {
            return new List<T>();
        }

        TileMap tm = CommonObjects.OverworldTileMap;
        Rect sBounds = tm.GetBoundsForSector(sector);

        return FindObjectsInArea<T>(sBounds);
    }
    static public List<T> FindObjectsInArea<T>(Rect area) where T : Component
    {
        List<T> actors = new List<T>();
        foreach (T obj in FindObjectsOfType<T>())
        {
            Vector3 p = obj.transform.position;
            Vector2 p2 = new Vector2(p.x, p.z);
            if (area.Contains(p2))
            {
                actors.Add(obj);
            }
        }
        return actors;
    }


    /*public static GameObject GetContainerForActorType<T>() where T : Actor
    {
        GameObject container = GameObject.FindGameObjectWithTag("Enemies");

        if (typeof(T) == typeof(Enemy))     // TODO: This line needs Testing
        {
            container = GameObject.FindGameObjectWithTag("Enemies");
        }

        return container;
    }*/
}