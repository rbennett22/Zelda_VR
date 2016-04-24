using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]

public class DungeonMapView : MonoBehaviour
{
    const int DUNGEON_WIDTH_IN_SECTORS = 8;
    const int DUNGEON_HEIGHT_IN_SECTORS = 8;


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


    public bool DoRenderUnvisitedRooms { private get; set; }
    public bool DoRenderTriforceSymbol { private get; set; }


    void Awake()
    {
        int mapWidthInPixels = DUNGEON_WIDTH_IN_SECTORS * _sectorWidth;
        int mapHeightInPixels = DUNGEON_HEIGHT_IN_SECTORS * _sectorHeight;

        _mapTexture = new Texture2D(mapWidthInPixels, mapHeightInPixels);
        _rawImage = GetComponent<RawImage>();
        _rawImage.texture = _mapTexture;

        ClearMapTexture(_bgColor);

        _triforceSymbolIndentX = (int)(0.5f * (_sectorWidth - _triforceSymbolWidth));
        _triforceSymbolIndentY = (int)(0.5f * (_sectorHeight - _triforceSymbolHeight));
        _linkSymbolIndentX = (int)(0.5f * (_sectorWidth - _linkSymbolWidth));
        _linkSymbolIndentY = (int)(0.5f * (_sectorHeight - _linkSymbolHeight));

        if(!_doRenderHalls)
        {
            _triforceSymbolIndentX++;
            _triforceSymbolIndentY = 0;
            _linkSymbolIndentX++;
            _linkSymbolIndentY = 0;
        }
    }


    public void UpdateDungeonMap()
    {
        ClearMapTexture(_bgColor);

        Vector3 playerPos = CommonObjects.PlayerController_G.transform.position;

        foreach (var room in DungeonFactory.Instance.Rooms)
        {
            Vector2 sector = room.GetSectorIndices();
            int sX, sY;

            //if(_doRenderHalls)
            {
                sX = (int)(_sectorWidth * sector.x);
                sY = (int)(_sectorHeight * sector.y);
            }
            /*else
            {
                sX = (int)((_sectorWidth + 1) * sector.x);
                sY = (int)((_sectorHeight + 1) * sector.y);
            }*/

            // Sector
            if (!room.HideOnMap && (DoRenderUnvisitedRooms || room.PlayerHasVisited))
            {
                if (_doRenderHalls)
                {
                    int hh = _hallHeight;
                    SetColorForArea(sX + hh, sY + hh, _sectorWidth - 2 * hh, _sectorHeight - 2 * hh, _roomColor);

                    RenderHallsForRoom(room);
                }
                else
                {
                    SetColorForArea(sX, sY, _sectorWidth - 1, _sectorHeight - 1, _roomColor);
                }
            }

            // Boss
            if (DoRenderTriforceSymbol && room.ContainsTriforce)
            {
                SetColorForArea(sX + _triforceSymbolIndentX, sY + _triforceSymbolIndentY, _triforceSymbolWidth, _triforceSymbolHeight, _triforceSymbolColor);
            }

            // Link
            if (room == DungeonRoom.GetRoomForPosition(playerPos))
            {
                SetColorForArea(sX + _linkSymbolIndentX, sY + _linkSymbolIndentY, _linkSymbolWidth, _linkSymbolHeight, _linkSymbolColor);
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
            SetColorForArea(sX, EW_Y, hh, hw, _hallColor);
        }
        if (room.CanPassThrough(DungeonRoomInfo.WallDirection.East))
        {
            SetColorForArea(sX + _sectorWidth - hh, EW_Y, hh, hw, _hallColor);
        }
        if (room.CanPassThrough(DungeonRoomInfo.WallDirection.South)) 
        {
            SetColorForArea(NS_X, sY, hw, hh, _hallColor);
        }
        if (room.CanPassThrough(DungeonRoomInfo.WallDirection.North))
        {
            SetColorForArea(NS_X, sY + _sectorHeight - hh, hw, hh, _hallColor);
        }
    }


    void SetColorForArea(Rect block, Color color)
    {
        SetColorForArea((int)block.xMin, (int)block.yMin, (int)block.width, (int)block.height, color);
    }
    void SetColorForArea(int x, int y, int blockWidth, int blockHeight, Color color)
    {
        Color[] colors = new Color[blockWidth * blockHeight];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = color;
        }
        _mapTexture.SetPixels(x, y, blockWidth, blockHeight, colors);
    }
    void ClearMapTexture(Color color)
    {
        SetColorForArea(0, 0, _mapTexture.width, _mapTexture.height, color);
    }
}
