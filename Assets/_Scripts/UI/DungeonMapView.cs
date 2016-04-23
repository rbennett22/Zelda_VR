using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]

public class DungeonMapView : MonoBehaviour
{
    const int PIXELS_WIDE = 8 * SectorWidth;
    const int PIXELS_HIGH = 8 * SectorHeight;

    const int SectorWidth = 8, SectorHeight = 8;
    const int HallWidth = 2, HallHeight = 1;
    const int BossWidth = 3, BossHeight = 3;
    const int LinkWidth = 3, LinkHeight = 3;

    static Color ROOM_COLOR = Color.black;
    static Color HALL_COLOR = ROOM_COLOR;
    static Color BOSS_ICON_COLOR = Color.red;
    static Color LINK_ICON_COLOR = Color.green;


    public bool DoRenderMapIcon { get; set; }
    public bool DoRenderCompassIcon { get; set; }


    RawImage _rawImage;
    Texture2D _mapTexture;

    int _bossIndentX, _bossIndentY;
    int _linkIndentX, _linkIndentY;


    void Awake()
    {
        _mapTexture = new Texture2D(PIXELS_WIDE, PIXELS_HIGH);
        ClearMapTexture();

        _rawImage = GetComponent<RawImage>();
        _rawImage.texture = _mapTexture;

        _bossIndentX = (int)(0.5f * (SectorWidth - BossWidth));
        _bossIndentY = (int)(0.5f * (SectorHeight - BossHeight));

        _linkIndentX = (int)(0.5f * (SectorWidth - LinkWidth));
        _linkIndentY = (int)(0.5f * (SectorHeight - LinkHeight));
    }


    public void RenderMap()
    {
        ClearMapTexture();

        int hh = HallHeight;
        Vector3 playerPos = CommonObjects.PlayerController_G.transform.position;

        foreach (var room in DungeonFactory.Instance.Rooms)
        {
            Vector2 sector = room.GetGridIndices();
            int sX = (int)(SectorWidth * sector.x);
            int sY = (int)(SectorHeight * sector.y);

            // Sector
            if (!room.HideOnMap && (DoRenderMapIcon || room.PlayerHasVisited))
            {
                // Room
                SetColorForArea(sX + hh, sY + hh, SectorWidth - 2 * hh, SectorHeight - 2 * hh, ROOM_COLOR);

                RenderHallsForRoom(room);
            }

            // Boss
            if (DoRenderCompassIcon && room.ContainsTriforce)
            {
                SetColorForArea(sX + _bossIndentX, sY + _bossIndentY, BossWidth, BossHeight, BOSS_ICON_COLOR);
            }

            // Link
            if (room == DungeonRoom.GetRoomForPosition(playerPos))
            {
                SetColorForArea(sX + _linkIndentX, sY + _linkIndentY, LinkWidth, LinkHeight, LINK_ICON_COLOR);
            }
        }

        _mapTexture.Apply();
    }

    void RenderHallsForRoom(DungeonRoom room)
    {
        int hw = HallWidth;
        int hh = HallHeight;

        Vector2 sector = room.GetGridIndices();
        int sX = (int)(SectorWidth * sector.x);
        int sY = (int)(SectorHeight * sector.y);

        int NS_X = (int)(sX + 0.5f * (SectorWidth - hw));
        int EW_Y = (int)(sY + 0.5f * (SectorHeight - hw));

        if (room.CanPassThrough(DungeonRoomInfo.WallDirection.West))
        {
            SetColorForArea(sX, EW_Y, hh, hw, HALL_COLOR);
        }
        if (room.CanPassThrough(DungeonRoomInfo.WallDirection.East))
        {
            SetColorForArea(sX + SectorWidth - hh, EW_Y, hh, hw, HALL_COLOR);
        }
        if (room.CanPassThrough(DungeonRoomInfo.WallDirection.South)) 
        {
            SetColorForArea(NS_X, sY, hw, hh, HALL_COLOR);
        }
        if (room.CanPassThrough(DungeonRoomInfo.WallDirection.North))
        {
            SetColorForArea(NS_X, sY + SectorHeight - hh, hw, hh, HALL_COLOR);
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
    void ClearMapTexture()
    {
        SetColorForArea(0, 0, _mapTexture.width, _mapTexture.height, Color.clear);
    }
}
