using System.Collections.Generic;
using UnityEngine;

namespace GridLock.LevelLoader
{
    public enum CellType
    {
        Empty,
        Road,
        CrossRoads,
    }

    public class GridAnalysisResult
    {
        public CellType[,] CellTypes;
        public List<Vector2Int> CrossRoadsCells = new List<Vector2Int>();
        public List<Vector2Int> SpawnCells = new List<Vector2Int>();
        public List<Vector2Int> ExitCells = new List<Vector2Int>();
    }

    public static class GridRoadTypeParser
    {
        private static readonly Vector2Int[] kNeighbouringCells =
            { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };
        
        /// <summary>
        /// Translates grid array values into a Result class that maps all the different road type positions (not in real space still)
        /// value translations:
        /// 1 = regular roadsegment or crossroads depending on number of neighbours
        /// 2 = entry point (always regular roadsegment)
        /// 3 = exit point (always regular roadsegment)
        /// 4 = entry AND exit point (always regular roadsegment)
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static GridAnalysisResult Analyze(int[,] grid)
        {
            int colCount = grid.GetLength(0);
            int rowCount = grid.GetLength(1);

            var result = new GridAnalysisResult
            {
                CellTypes = new CellType[colCount, rowCount],
            };

            for (int y = 0; y < rowCount; y++)
            {
                for (int x = 0; x < colCount; x++)
                {
                    int value = grid[x, y];
                    if (value == 0)
                    {
                        result.CellTypes[x, y] = CellType.Empty;
                        continue;
                    }

                    int neighborCount = CountNonZeroNeighbors(grid, x, y, colCount, rowCount);

                    // Values 2, 3, 4 are always road segments (entry/exit points)
                    // Value 1 with 3+ neighbors is a crossroads, otherwise road
                    if (value == 1 && neighborCount >= 3)
                    {
                        result.CellTypes[x, y] = CellType.CrossRoads;
                        result.CrossRoadsCells.Add(new Vector2Int(x, y));
                    }
                    else
                    {
                        result.CellTypes[x, y] = CellType.Road;
                    }

                    if (value == 2 || value == 4)
                    {
                        result.SpawnCells.Add(new Vector2Int(x, y));
                    }

                    if (value == 3 || value == 4)
                    {
                        result.ExitCells.Add(new Vector2Int(x, y));
                    }
                }
            }

            return result;
        }

        private static int CountNonZeroNeighbors(int[,] grid, int x, int y, int colCount, int rowCount)
        {
            int neighbourCount = 0;
            for (int i = 0; i < 4; i++) // check the 4 adjacent cells
            {
                int neighbourX = x + kNeighbouringCells[i].x;
                int neighbourY = y + kNeighbouringCells[i].y;
                
                if (neighbourX >= 0 && neighbourX < colCount 
                    && neighbourY >= 0 && neighbourY < rowCount 
                    && grid[neighbourX, neighbourY] != 0)
                {
                    neighbourCount++;
                }
            }
            return neighbourCount;
        }
    }
}
