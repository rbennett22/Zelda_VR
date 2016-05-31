using UnityEngine;
using Immersio.Utility;
using System.Collections.Generic;

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