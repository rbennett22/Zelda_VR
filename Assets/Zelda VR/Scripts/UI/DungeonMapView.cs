using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]

public class DungeonMapView : MonoBehaviour
{
    // The following are all measured in pixels
    [SerializeField]
    int _sectorWidth = 8, _sectorHeight = 8,
        _hallWidth = 2, _hallHeight = 1,
        _triforceSymbolWidth = 3, _triforceSymbolHeight = 3,
        _linkSymbolWidth = 3, _linkSymbolHeight = 3;

    [SerializeField]
    Color _bgColor = Color.clear,
        _roomColor = Color.black,
        _hallColor = Color.black,
        _triforceSymbolColor = Color.red,
        _linkSymbolColor = Color.green;

    [SerializeField]
    bool _doRenderHalls;


    RawImage _rawImage;
    Texture2D _mapTexture;

    int _triforceSymbolIndentX, _triforceSymbolIndentY;
    int _linkSymbolIndentX, _linkSymbolIndentY;

    int _sectorsWide, _sectorsHigh;


    public bool DoRenderUnvisitedRooms { private get; set; }
    public bool DoRenderTriforceSymbol { private get; set; }


    public void Init(int sectorsWide, int sectorsHigh)
    {
        _sectorsWide = Mathf.Max(0, sectorsWide);
        _sectorsHigh = Mathf.Max(0, sectorsHigh);


        if (_mapTexture != null)
        {
            Destroy(_mapTexture);
        }

        int mapWidthInPixels = _sectorsWide * _sectorWidth;
        int mapHeightInPixels = _sectorsHigh * _sectorHeight;

        _mapTexture = new Texture2D(mapWidthInPixels, mapHeightInPixels);
        _mapTexture.Clear(_bgColor);

        _rawImage = GetComponent<RawImage>();
        _rawImage.texture = _mapTexture;


        _triforceSymbolIndentX = (int)(0.5f * (_sectorWidth - _triforceSymbolWidth));
        _triforceSymbolIndentY = (int)(0.5f * (_sectorHeight - _triforceSymbolHeight));
        _linkSymbolIndentX = (int)(0.5f * (_sectorWidth - _linkSymbolWidth));
        _linkSymbolIndentY = (int)(0.5f * (_sectorHeight - _linkSymbolHeight));

        if (!_doRenderHalls)
        {
            _triforceSymbolIndentX++;
            _triforceSymbolIndentY = 0;
            _linkSymbolIndentX++;
            _linkSymbolIndentY = 0;
        }
    }

    public void UpdateMap()
    {
        if (_mapTexture == null)
        {
            Debug.Log("_mapTexture is null.  You probably need to call Init");
            return;
        }

        _mapTexture.Clear(_bgColor);

        Vector3 playerPos = CommonObjects.Player_C.Position;

        foreach (var room in CommonObjects.CurrentDungeonFactory.Rooms)
        {
            Vector2 sector = room.GetSectorIndices();
            int sX = (int)(_sectorWidth * sector.x);
            int sY = (int)(_sectorHeight * sector.y);

            // Sector
            if (!room.HideOnMap && (DoRenderUnvisitedRooms || room.PlayerHasVisited))
            {
                if (_doRenderHalls)
                {
                    int hh = _hallHeight;
                    _mapTexture.SetColorForArea(sX + hh, sY + hh, _sectorWidth - 2 * hh, _sectorHeight - 2 * hh, _roomColor);

                    RenderHallsForRoom(room);
                }
                else
                {
                    _mapTexture.SetColorForArea(sX, sY, _sectorWidth - 1, _sectorHeight - 1, _roomColor);
                }
            }

            // Triforce
            if (DoRenderTriforceSymbol && room.ContainsTriforce)
            {
                _mapTexture.SetColorForArea(sX + _triforceSymbolIndentX, sY + _triforceSymbolIndentY, _triforceSymbolWidth, _triforceSymbolHeight, _triforceSymbolColor);
            }

            // Link
            if (room == DungeonRoom.GetRoomForPosition(playerPos))
            {
                _mapTexture.SetColorForArea(sX + _linkSymbolIndentX, sY + _linkSymbolIndentY, _linkSymbolWidth, _linkSymbolHeight, _linkSymbolColor);
            }
        }

        _mapTexture.Apply();
    }

    void RenderHallsForRoom(DungeonRoom room)
    {
        int hw = _hallWidth;
        int hh = _hallHeight;

        Vector2 sector = room.GetSectorIndices();
        int sX = (int)(_sectorWidth * sector.x);
        int sY = (int)(_sectorHeight * sector.y);

        int NS_X = (int)(sX + 0.5f * (_sectorWidth - hw));
        int EW_Y = (int)(sY + 0.5f * (_sectorHeight - hw));

        if (room.CanPassThrough(DungeonRoomInfo.WallDirection.West))
        {
            _mapTexture.SetColorForArea(sX, EW_Y, hh, hw, _hallColor);
        }
        if (room.CanPassThrough(DungeonRoomInfo.WallDirection.East))
        {
            _mapTexture.SetColorForArea(sX + _sectorWidth - hh, EW_Y, hh, hw, _hallColor);
        }
        if (room.CanPassThrough(DungeonRoomInfo.WallDirection.South))
        {
            _mapTexture.SetColorForArea(NS_X, sY, hw, hh, _hallColor);
        }
        if (room.CanPassThrough(DungeonRoomInfo.WallDirection.North))
        {
            _mapTexture.SetColorForArea(NS_X, sY + _sectorHeight - hh, hw, hh, _hallColor);
        }
    }
}