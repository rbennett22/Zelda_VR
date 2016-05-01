using UnityEngine;
using Uniblocks;

public class OverworldVoxelTerrainGenerator : TerrainGenerator
{
    [SerializeField]
    TileMapData _tileMapData;
    [SerializeField]
    TileTexture _tileTexture;


    void Awake()
    {
        InitTileMapData();
        InitTileTexture();
    }

    void InitTileMapData()
    {
        _tileMapData = Engine.EngineInstance.GetComponent<TileMapData>();
        if (_tileMapData != null)
        {
            if (!_tileMapData.HasLoaded)
            {
                _tileMapData.LoadMap();
            }
        }
    }

    void InitTileTexture()
    {
        _tileTexture = Engine.EngineInstance.GetComponent<TileTexture>();
    }


    public override void GenerateVoxelData ()
    {
        if(_tileMapData == null)
        {
            InitTileMapData();
            if (_tileMapData == null)
                return;
        }
        if(_tileTexture == null)
        {
            InitTileTexture();
            if (_tileTexture == null)
                return;
        }

        ZeldaVRSettings settings = ZeldaVRSettings.Instance;

        int SideLength = Engine.ChunkSideLength;

        int chunkXX = SideLength * chunk.ChunkIndex.x;
        int chunkYY = SideLength * chunk.ChunkIndex.y;
        int chunkZZ = SideLength * chunk.ChunkIndex.z;

        for (int y = 0; y < SideLength; y++)
        {
            int yy = y + chunkYY;     // get absolute height for the voxel
            if (yy >= 1) { continue; }

            for (int x = 0; x < SideLength; x++)
            {
                int xx = x + chunkXX;
                if (xx < 0 || xx > _tileMapData.TilesWide - 1) { continue; }

                for (int z = 0; z < SideLength; z++)
                {
                    int zz = z + chunkZZ;
                    if (zz < 0 || zz > _tileMapData.TilesHigh - 1) { continue; }

                    // Get tileCode at OW tile position
                    int tileCode = _tileMapData.Tile(xx, zz);
                    Index tileMapIndex = _tileTexture.IndexForTileCode(tileCode);

                    ushort data = (ushort)(tileMapIndex.y * settings.tileMapWidthInTiles_WithoutFiller + tileMapIndex.x + 1);

                    chunk.SetVoxelSimple(x, y, z, data);
                }
			}
		}
	}

    /*int GetVoxelDataForTileMapIndex(Index index)
    {
        foreach (ushort voxelID in chunk.VoxelData)
        {
            Voxel voxel = Engine.GetVoxelType(voxelID);
            if(voxel.VTexture.Length == 0) { continue; }

            Vector2 vTex = voxel.VTexture[0];
            if(vTex.x == index.x && vTex.y == index.y)
            {
                return voxelID;
            }
        }

        return -1;
    }*/
}