using UnityEngine;
using Uniblocks;

public class OverworldTerrainEngine : Uniblocks.Engine
{
    [SerializeField]
    TileMap _tileMap;
    public TileMap TileMap { get { return _tileMap; } }
}