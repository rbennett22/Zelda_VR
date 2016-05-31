using UnityEngine;
using Immersio.Utility;
using System.Collections.Generic;

public class Pushable : MonoBehaviour
{
    const float SLIDE_DURATION = 1.0f;


    public Block linkedBlock;
    public GameObject[] linkedTiles;
    public bool requiresPowerBracelet;


    [SerializeField]
    IndexDirection2.DirectionEnum[] _directions;
    List<IndexDirection2> _directionsList;
    bool CanSlideInDirection(IndexDirection2 dir) { return _directionsList.Contains(dir); }


    bool _isSliding;
    bool _hasFinishedSliding;


    public bool PushingEnabled { get; set; }


    void Awake()
    {
        _directionsList = new List<IndexDirection2>();
        foreach (IndexDirection2.DirectionEnum dir in _directions)
        {
            _directionsList.Add(IndexDirection2.FromDirectionEnum(dir));
        }

        PushingEnabled = true;
    }


    void OnTriggerStay(Collider otherCollider)
    {
        if (!PushingEnabled) { return; }
        if (_isSliding || _hasFinishedSliding) { return; }

        GameObject other = otherCollider.gameObject;
        if (!CommonObjects.IsPlayer(other))
        {
            return;
        }
        if (requiresPowerBracelet && !Inventory.Instance.HasItem("PowerBracelet"))
        {
            return;
        }

        IndexDirection2 dir = GetPushDirection();
        if (!dir.IsZero())
        {
            Slide(dir);
        }
    }

    IndexDirection2 GetPushDirection()
    {
        Player player = CommonObjects.Player_C;
        Vector3 toPlayer = (player.Position - transform.position).normalized;
        float pX = toPlayer.x;
        float pZ = toPlayer.z;

        IndexDirection2 dir;
        if (Mathf.Abs(pX) < Mathf.Abs(pZ))
        {
            dir = (pX < 0) ? IndexDirection2.right : IndexDirection2.left;
        }
        else
        {
            dir = (pZ < 0) ? IndexDirection2.up : IndexDirection2.down;
        }

        // Is player facing the block?  If not, the block will not be pushed
        if (Vector3.Dot(player.ForwardDirection, dir.ToVector3()) < 0.9f)
        {
            dir = IndexDirection2.zero;
        }

        return dir;
    }

    void Slide(IndexDirection2 dir)
    {
        if(!CanSlideInDirection(dir))
        {
            return;
        }

        Vector3 targetPos = transform.position + dir.ToVector3();
        SlideToPosition(targetPos);
    }
    void SlideToPosition(Vector3 pos)
    {
        if (_isSliding)
        {
            return;
        }
        _isSliding = true;

        iTween.MoveTo(gameObject, iTween.Hash(
            "position", pos,
            "time", SLIDE_DURATION,
            "easetype", iTween.EaseType.linear,
            "oncomplete", "OnFinishedSliding"
            ));
    }

    void OnFinishedSliding()
    {
        _isSliding = false;
        _hasFinishedSliding = true;

        if (WorldInfo.Instance.IsInDungeon)
        {
            DungeonRoom dr = DungeonRoom.GetRoomForPosition(transform.position);
            dr.OnPushableWasPushedIntoPosition();
        }

        DestroyLinkedObjects();
        PlaySecretSound();
    }

    void DestroyLinkedObjects()
    {
        if (linkedBlock != null)
        {
            Destroy(linkedBlock.gameObject);
        }

        if (linkedTiles != null)
        {
            foreach (var tile in linkedTiles)
            {
                Destroy(tile);
            }
        }
    }

    void PlaySecretSound()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.secret);
    }
}