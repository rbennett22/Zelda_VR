using UnityEngine;


public class Pushable : MonoBehaviour
{
    const float PushSpeed = 0.01f;


    public enum Direction
    {
        North, East, South, West
    }
    public Direction direction;

    public Block linkedBlock;
    public GameObject[] linkedTiles;
    public bool requiresPowerBracelet;


    Vector3 _slideDirection;
    Vector3 _origin;
    Vector3 _end;
    bool _hasReachedEnd;


    public bool PushingEnabled { get; set; }


    void Awake()
    {
        PushingEnabled = true;
    }

    void Start()
    {
        _slideDirection = VectorForDirection(direction);
        _origin = transform.position;
        _end = _origin + _slideDirection;
    }


    void OnTriggerStay(Collider otherCollider)
    {
        if (!PushingEnabled) { return; }

        GameObject other = otherCollider.gameObject;
        //print("Pushable::OnTriggerEnter: " + other.name);

        if (requiresPowerBracelet)
        {
            if (Inventory.Instance.GetItem("PowerBracelet").count == 0)
            {
                return;
            }
        }

        if (_hasReachedEnd) { return; }

        Vector3 toEnd = _end - transform.position;
        if ((toEnd == Vector3.zero)
            || Vector3.Dot(toEnd, _slideDirection) <= 0)
        {
            transform.position = _end;
            OnReachedEnd();
            return;
        }

        if (CommonObjects.IsPlayer(other))
        {
            TryToPush();
        }
    }

    void TryToPush()
    {
        Vector3 toPlayer = CommonObjects.PlayerController_G.transform.position - transform.position;
        toPlayer.Normalize();

        if (Vector3.Dot(-toPlayer, _slideDirection) > 0.5f)
        {
            Vector3 playerForward = CommonObjects.PlayerController_C.ForwardDirection;
            if (Vector3.Dot(playerForward, _slideDirection) > 0.5f)
            {
                transform.position += _slideDirection * PushSpeed;
            }
        }
    }

    void OnReachedEnd()
    {
        _hasReachedEnd = true;

        if (WorldInfo.Instance.IsInDungeon)
        {
            DungeonRoom dr = DungeonRoom.GetRoomForPosition(transform.position);
            dr.OnPushableWasPushedIntoPosition();
        }
        else
        {
            if (linkedBlock != null)
            {
                Destroy(linkedBlock.gameObject);
            }

            SoundFx.Instance.PlayOneShot(SoundFx.Instance.secret);
        }

        if (linkedTiles != null)
        {
            foreach (var tile in linkedTiles)
            {
                Destroy(tile);
            }
        }
    }

    Vector3 VectorForDirection(Direction dir)
    {
        Vector3 vec = Vector3.zero;
        switch (dir)
        {
            case Direction.North:   vec = new Vector3(0, 0, 1); break;
            case Direction.East:    vec = new Vector3(1, 0, 0); break;
            case Direction.South:   vec = new Vector3(0, 0, -1); break;
            case Direction.West:    vec = new Vector3(-1, 0, 0); break;
            default: break;
        }
        return vec;
    }
	
}
