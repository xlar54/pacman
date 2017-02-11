using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacMan
{
    public class Map
    {
        public enum TileType
        {
            WALL,
            DOT,
            EMPTY,
            POWERPELLET,
            GHOSTONLY,
            LEFTTUNNEL,
            RIGHTTUNNEL,
            GATE
        }

        private int[,] map = new int[34, 28] { 
                  { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, 
                  { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                  { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                  { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                  { 0,3,1,1,1,1,1,1,1,1,1,1,1,0,0,1,1,1,1,1,1,1,1,1,1,1,3,0 },
                  { 0,1,0,0,0,0,1,0,0,0,0,0,1,0,0,1,0,0,0,0,0,1,0,0,0,0,1,0 },
                  { 0,1,0,0,0,0,1,0,0,0,0,0,1,0,0,1,0,0,0,0,0,1,0,0,0,0,1,0 },
                  { 0,1,0,0,0,0,1,0,0,0,0,0,1,0,0,1,0,0,0,0,0,1,0,0,0,0,1,0 },
                  { 0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0 },
                  { 0,1,0,0,0,0,1,0,0,1,0,0,0,0,0,0,0,0,1,0,0,1,0,0,0,0,1,0 },
                  { 0,1,0,0,0,0,1,0,0,1,0,0,0,0,0,0,0,0,1,0,0,1,0,0,0,0,1,0 },
                  { 0,1,1,1,1,1,1,0,0,1,1,1,1,0,0,1,1,1,1,0,0,1,1,1,1,1,1,0 },
                  { 0,0,0,0,0,0,1,0,0,0,0,0,2,0,0,2,0,0,0,0,0,1,0,0,0,0,0,0 },
                  { 0,0,0,0,0,0,1,0,0,0,0,0,2,0,0,2,0,0,0,0,0,1,0,0,0,0,0,0 },
                  { 0,0,0,0,0,0,1,0,0,2,2,2,2,2,2,2,2,2,2,0,0,1,0,0,0,0,0,0 },
                  { 0,0,0,0,0,0,1,0,0,2,0,0,0,7,7,0,0,0,2,0,0,1,0,0,0,0,0,0 },
                  { 0,0,0,0,0,0,1,0,0,2,0,0,4,4,4,4,0,0,2,0,0,1,0,0,0,0,0,0 },
                  { 0,5,2,2,2,2,1,2,2,2,0,0,4,4,4,4,0,0,2,2,2,1,2,2,2,2,6,0 },
                  { 0,0,0,0,0,0,1,0,0,2,0,0,0,0,0,0,0,0,2,0,0,1,0,0,0,0,0,0 },
                  { 0,0,0,0,0,0,1,0,0,2,0,0,0,0,0,0,0,0,2,0,0,1,0,0,0,0,0,0 },
                  { 0,0,0,0,0,0,1,0,0,2,2,2,2,2,2,2,2,2,2,0,0,1,0,0,0,0,0,0 },
                  { 0,0,0,0,0,0,1,0,0,2,0,0,0,0,0,0,0,0,2,0,0,1,0,0,0,0,0,0 },
                  { 0,0,0,0,0,0,1,0,0,2,0,0,0,0,0,0,0,0,2,0,0,1,0,0,0,0,0,0 },
                  { 0,1,1,1,1,1,1,1,1,1,1,1,1,0,0,1,1,1,1,1,1,1,1,1,1,1,1,0 },
                  { 0,1,0,0,0,0,1,0,0,0,0,0,1,0,0,1,0,0,0,0,0,1,0,0,0,0,1,0 },
                  { 0,1,0,0,0,0,1,0,0,0,0,0,1,0,0,1,0,0,0,0,0,1,0,0,0,0,1,0 },
                  { 0,3,1,1,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,1,1,3,0 },
                  { 0,0,0,1,0,0,1,0,0,1,0,0,0,0,0,0,0,0,1,0,0,1,0,0,1,0,0,0 },
                  { 0,0,0,1,0,0,1,0,0,1,0,0,0,0,0,0,0,0,1,0,0,1,0,0,1,0,0,0 },
                  { 0,1,1,1,1,1,1,0,0,1,1,1,1,0,0,1,1,1,1,0,0,1,1,1,1,1,1,0 },
                  { 0,1,0,0,0,0,0,0,0,0,0,0,1,0,0,1,0,0,0,0,0,0,0,0,0,0,1,0 },
                  { 0,1,0,0,0,0,0,0,0,0,0,0,1,0,0,1,0,0,0,0,0,0,0,0,0,0,1,0 },
                  { 0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0 },
                  { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
              };

        private int[,] orgMap;

        public Map()
        {
            orgMap = (int[,])map.Clone();
        }

        public void ClearTile(int x, int y)
        {
            map[x, y] = 2;
        }

        public TileType GetTileType(int x, int y)
        {
            switch(map[x,y])
            {
                case 0: return TileType.WALL;
                case 1: return TileType.DOT;
                case 2: return TileType.EMPTY;
                case 3: return TileType.POWERPELLET;
                case 4: return TileType.GHOSTONLY;
                case 5: return TileType.LEFTTUNNEL;
                case 6: return TileType.RIGHTTUNNEL;
                case 7: return TileType.GATE;
                default:
                    return TileType.WALL;
            }
        }

        public List<Direction> PossibleEntityDirections(Entity e)
        {
            List<Direction> possibleDirections = new List<Direction>();

            if (!e.IsPlayerEntity)
            {
                if (GetTileType(e.X, e.Y + 1) != TileType.WALL) possibleDirections.Add(Direction.RIGHT);
                if (GetTileType(e.X, e.Y - 1) != TileType.WALL) possibleDirections.Add(Direction.LEFT);
                if (GetTileType(e.X + 1, e.Y) != TileType.WALL) possibleDirections.Add(Direction.DOWN);

                if (GetTileType(e.X - 1, e.Y) != TileType.WALL)
                {
                    bool addUp = false;

                    if(GetTileType(e.X - 1, e.Y) == TileType.GATE && e.DotCounter == 0)
                        addUp = true;
                    else if (GetTileType(e.X - 1, e.Y) != TileType.GATE)
                    {
                        // Ghosts may not move up at these locations
                        if ((e.X == 14 || e.X == 26) && (e.Y == 12 || e.Y == 15) && e.State != EntityState.Frightened)
                            addUp = false;
                        else
                            addUp = true;
                    }

                    if(addUp) possibleDirections.Add(Direction.UP);
                }
            }
            else
            {
                if (GetTileType(e.X, e.Y + 1) != TileType.WALL && GetTileType(e.X, e.Y + 1) != TileType.GHOSTONLY && GetTileType(e.X, e.Y + 1) != TileType.GATE) possibleDirections.Add(Direction.RIGHT);
                if (GetTileType(e.X, e.Y - 1) != TileType.WALL && GetTileType(e.X, e.Y - 1) != TileType.GHOSTONLY && GetTileType(e.X, e.Y - 1) != TileType.GATE) possibleDirections.Add(Direction.LEFT);
                if (GetTileType(e.X - 1, e.Y) != TileType.WALL && GetTileType(e.X - 1, e.Y) != TileType.GHOSTONLY && GetTileType(e.X - 1, e.Y) != TileType.GATE) possibleDirections.Add(Direction.UP);
                if (GetTileType(e.X + 1, e.Y) != TileType.WALL && GetTileType(e.X + 1, e.Y) != TileType.GHOSTONLY && GetTileType(e.X + 1, e.Y) != TileType.GATE) possibleDirections.Add(Direction.DOWN);
            }

            return possibleDirections;
        }

        public int GetDotCount()
        {
            int counter=0;

            for (int x = 0; x < 34; x++)
                for (int y = 0; y < 28; y++)
                    if (map[x, y] == 1 || map[x, y] == 3)
                        counter++;

            return counter;
        }

        public void Reset()
        {
            map = (int[,])orgMap.Clone();
        }

        public bool IsEntityIsAboveGate(Entity e)
        {
            if (e.X == 14 && (e.Y == 13 || e.Y == 14))
                return true;
            else
                return false;
        }

        public void GetTileAtLocation(int screenX, int screenY, out int tileX, out int tileY)
        {
            tileX = Math.Abs(screenY / 20);
            tileY = Math.Abs(screenX / 20);
        }
    }
}
