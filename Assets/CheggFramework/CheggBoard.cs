using System;
using UnityEngine;

namespace Chegg.Framework
{
    [Serializable]
    public struct CheggTile
    {
        public CheggTileType TileType;
        public CheggZoneType ZoneType;

        public CheggTile(CheggTileType tileType, CheggZoneType zoneType)
        {
            TileType = tileType;
            ZoneType = zoneType;
        }
    }

    [Serializable]
    public sealed class CheggBoardState
    {
        public const int Width = 10;
        public const int Height = 8;

        private readonly CheggTile[,] _tiles = new CheggTile[Width, Height];

        public CheggBoardState()
        {
            Initialize();
        }

        public bool IsOnBoard(Vector2Int position)
        {
            return position.x >= 0 && position.x < Width && position.y >= 0 && position.y < Height;
        }

        public CheggTile GetTile(Vector2Int position)
        {
            if (!IsOnBoard(position))
            {
                throw new IndexOutOfRangeException($"Position is outside board: {position}");
            }

            return _tiles[position.x, position.y];
        }

        public bool IsWater(Vector2Int position)
        {
            return GetTile(position).TileType == CheggTileType.Water;
        }

        public bool IsBridge(Vector2Int position)
        {
            return GetTile(position).TileType == CheggTileType.Bridge;
        }

        public bool IsSpawnZoneFor(Vector2Int position, CheggPlayer player)
        {
            var zone = GetTile(position).ZoneType;
            return player == CheggPlayer.Red ? zone == CheggZoneType.RedSpawn : zone == CheggZoneType.BlueSpawn;
        }

        private void Initialize()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    CheggTileType tileType = CheggTileType.Ground;
                    CheggZoneType zoneType = CheggZoneType.Neutral;

                    if (y <= 1)
                    {
                        zoneType = CheggZoneType.RedSpawn;
                    }
                    else if (y >= Height - 2)
                    {
                        zoneType = CheggZoneType.BlueSpawn;
                    }

                    if (y == 3 || y == 4)
                    {
                        tileType = CheggTileType.Water;
                    }

                    bool inCenterBridge = x >= 4 && x <= 5 && y >= 3 && y <= 4;
                    if (inCenterBridge)
                    {
                        tileType = CheggTileType.Bridge;
                    }

                    _tiles[x, y] = new CheggTile(tileType, zoneType);
                }
            }
        }
    }
}
