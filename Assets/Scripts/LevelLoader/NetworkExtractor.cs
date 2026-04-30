using System.Collections.Generic;
using GridLock.Config;
using GridLock.Core;
using GridLock.Simulation;
using UnityEngine;

namespace GridLock.LevelLoader
{
    public class CsvNetworkBuildData
    {
        public NetworkLayoutData LayoutData = new NetworkLayoutData();
        public List<int> SpawnSegmentIds = new List<int>();
        public List<int> ExitSegmentIds = new List<int>();
        public Dictionary<int, Vector2Int> CrossRoadsPositions = new Dictionary<int, Vector2Int>();
        public List<ConnectionPair> ConnectionPairs = new List<ConnectionPair>();
    }

    public struct ConnectionPair
    {
        public int ForwardSegmentId;
        public int ReverseSegmentId;
        public Vector2Int FromPos;
        public Vector2Int ToPos;
        public bool IsNorthSouth;
    }

    public static class NetworkExtractor
    {
        // Direction offsets: North = row-1, South = row+1, East = col+1, West = col-1
        private static readonly Dictionary<Direction, (int dCol, int dRow)> kDirectionOffsets =
            new Dictionary<Direction, (int, int)>
            {
                { Direction.North, (0, -1) }, // counterintuitive but because top of csv is row 0, this follows.
                { Direction.South, (0, 1) },
                { Direction.East, (1, 0) },
                { Direction.West, (-1, 0) },
            };

        public static CsvNetworkBuildData Extract(int[,] grid, GridAnalysisResult analysis)
        {
            int colCount = grid.GetLength(0);
            int rowCount = grid.GetLength(1);

            var result = new CsvNetworkBuildData();
            var crossRoadsIdLookup = new Dictionary<(int col, int row), int>();

            // Assign crossroads IDs and create CrossRoadsData
            foreach (var pos in analysis.CrossRoadsCells)
            {
                int id = pos.x * rowCount + pos.y;
                crossRoadsIdLookup[(pos.x, pos.y)] = id;
                result.CrossRoadsPositions[id] = pos;

                result.LayoutData.CrossRoads.Add(new CrossRoadsData
                {
                    Id = id,
                    InitialTrafficLightState = TrafficLightState.NorthSouth,
                });
            }

            // Track processed connections to avoid duplicates
            var processedConnections = new HashSet<(int, int)>();

            // Collect spawn/exit cell positions for quick lookup
            var spawnCellSet = new HashSet<(int, int)>();
            foreach (var pos in analysis.SpawnCells)
            {
                spawnCellSet.Add((pos.x, pos.y));
            }
            var exitCellSet = new HashSet<(int, int)>();
            foreach (var pos in analysis.ExitCells)
            {
                exitCellSet.Add((pos.x, pos.y));
            }

            int nextSegmentId = 0;

            // For each crossroads, trace outward in each cardinal direction
            foreach (var crossRoadsPos in analysis.CrossRoadsCells)
            {
                int crossRoadsId = crossRoadsIdLookup[(crossRoadsPos.x, crossRoadsPos.y)];

                foreach (var dirKvp in kDirectionOffsets)
                {
                    Direction direction = dirKvp.Key;
                    int dCol = dirKvp.Value.dCol;
                    int dRow = dirKvp.Value.dRow;

                    int startCol = crossRoadsPos.x + dCol;
                    int startRow = crossRoadsPos.y + dRow;

                    // Check if the adjacent cell in this direction is a valid road/crossroads cell
                    if (!IsInBounds(startCol, startRow, colCount, rowCount)
                        || grid[startCol, startRow] == 0)
                    {
                        continue;
                    }

                    // Adjacent crossroads without road cells between them is not supported
                    if (analysis.CellTypes[startCol, startRow] == CellType.CrossRoads)
                    {
                        Debug.LogWarning($"Adjacent crossroads at ({crossRoadsPos.x},{crossRoadsPos.y}) and ({startCol},{startRow}) with no road cells between them");
                        continue;
                    }

                    // Walk along road cells in this direction
                    var roadCells = new List<Vector2Int>();
                    bool hasSpawn = false;
                    bool hasExit = false;
                    int curRow = startRow;
                    int curCol = startCol;

                    while (IsInBounds(curCol, curRow, colCount, rowCount)
                           && analysis.CellTypes[curCol, curRow] == CellType.Road)
                    {
                        roadCells.Add(new Vector2Int(curCol, curRow));
                        if (spawnCellSet.Contains((curCol, curRow)))
                        {
                            hasSpawn = true;
                        }
                        if (exitCellSet.Contains((curCol, curRow)))
                        {
                            hasExit = true;
                        }
                        curRow += dRow;
                        curCol += dCol;
                    }

                    if (roadCells.Count == 0)
                    {
                        continue;
                    }

                    // Check what we reached at the end of the chain
                    if (IsInBounds(curCol, curRow, colCount, rowCount)
                        && analysis.CellTypes[curCol, curRow] == CellType.CrossRoads)
                    {
                        // Interior connection: crossroads → road cells → crossroads
                        int otherCrossRoadsId = crossRoadsIdLookup[(curCol, curRow)];
                        CreateCrossRoadsPairIfNew(
                            result, processedConnections, ref nextSegmentId,
                            crossRoadsId, otherCrossRoadsId,
                            crossRoadsPos, new Vector2Int(curCol, curRow),
                            roadCells.Count, direction,
                            hasSpawn, hasExit);
                    }
                    else
                    {
                        // Edge connection: crossroads → road cells → boundary
                        CreateEdgeSegments(
                            result, ref nextSegmentId,
                            crossRoadsId, crossRoadsPos,
                            roadCells, direction,
                            hasSpawn, hasExit);
                    }
                }
            }

            return result;
        }

        private static void CreateCrossRoadsPairIfNew(
            CsvNetworkBuildData result,
            HashSet<(int, int)> processedConnections,
            ref int nextSegmentId,
            int crossRoadsIdA, int crossRoadsIdB,
            Vector2Int posA, Vector2Int posB,
            int roadCellCount, Direction dirAtoB,
            bool hasSpawn, bool hasExit)
        {
            // Use sorted pair to avoid processing the same connection twice
            int minId = Mathf.Min(crossRoadsIdA, crossRoadsIdB);
            int maxId = Mathf.Max(crossRoadsIdA, crossRoadsIdB);
            if (processedConnections.Contains((minId, maxId)))
            {
                return;
            }
            processedConnections.Add((minId, maxId));

            Direction dirBtoA = GetOpposite(dirAtoB);
            int capacity = Mathf.Max(1, roadCellCount);

            // Segment from A to B (forward)
            int segIdAtoB = nextSegmentId++;
            result.LayoutData.RoadSegments.Add(new RoadSegmentData
            {
                Id = segIdAtoB,
                FromCrossRoadsId = crossRoadsIdA,
                ToCrossRoadsId = crossRoadsIdB,
                Direction = dirAtoB,
                Capacity = capacity,
            });

            // Segment from B to A (reverse)
            int segIdBtoA = nextSegmentId++;
            result.LayoutData.RoadSegments.Add(new RoadSegmentData
            {
                Id = segIdBtoA,
                FromCrossRoadsId = crossRoadsIdB,
                ToCrossRoadsId = crossRoadsIdA,
                Direction = dirBtoA,
                Capacity = capacity,
            });

            bool isNS = dirAtoB == Direction.North || dirAtoB == Direction.South;
            result.ConnectionPairs.Add(new ConnectionPair
            {
                ForwardSegmentId = segIdAtoB,
                ReverseSegmentId = segIdBtoA,
                FromPos = posA,
                ToPos = posB,
                IsNorthSouth = isNS,
            });

            // Handling interior segments that happen to be spawn/exit
            if (hasSpawn)
            {
                result.SpawnSegmentIds.Add(segIdAtoB);
                result.SpawnSegmentIds.Add(segIdBtoA);
            }
            if (hasExit)
            {
                result.ExitSegmentIds.Add(segIdAtoB);
                result.ExitSegmentIds.Add(segIdBtoA);
            }
        }

        private static void CreateEdgeSegments(
            CsvNetworkBuildData result,
            ref int nextSegmentId,
            int crossRoadsId, Vector2Int crossRoadsPos,
            List<Vector2Int> roadCells, Direction dirFromCrossRoads,
            bool hasSpawn, bool hasExit)
        {
            int capacity = Mathf.Max(1, roadCells.Count);
            Direction dirToCrossRoads = GetOpposite(dirFromCrossRoads);

            Vector2Int edgePos = roadCells[roadCells.Count - 1];

            // Outbound: from crossroads toward edge (ToCrossRoads = null via -1)
            int outboundId = nextSegmentId++;
            result.LayoutData.RoadSegments.Add(new RoadSegmentData
            {
                Id = outboundId,
                FromCrossRoadsId = crossRoadsId,
                ToCrossRoadsId = -1,
                Direction = dirFromCrossRoads,
                Capacity = capacity,
            });

            // Inbound: from edge toward crossroads (FromCrossRoads = null via -1)
            int inboundId = nextSegmentId++;
            result.LayoutData.RoadSegments.Add(new RoadSegmentData
            {
                Id = inboundId,
                FromCrossRoadsId = -1,
                ToCrossRoadsId = crossRoadsId,
                Direction = dirToCrossRoads,
                Capacity = capacity,
            });

            bool isNS = dirFromCrossRoads == Direction.North || dirFromCrossRoads == Direction.South;
            result.ConnectionPairs.Add(new ConnectionPair
            {
                ForwardSegmentId = outboundId,
                ReverseSegmentId = inboundId,
                FromPos = crossRoadsPos,
                ToPos = edgePos,
                IsNorthSouth = isNS,
            });

            // Spawn segments are inbound (vehicles enter from edge toward crossroads)
            if (hasSpawn)
            {
                result.SpawnSegmentIds.Add(inboundId);
            }

            // Exit segments are outbound (vehicles leave from crossroads toward edge)
            if (hasExit)
            {
                result.ExitSegmentIds.Add(outboundId);
            }
        }

        private static Direction GetOpposite(Direction direction)
        {
            switch (direction)
            {
                case Direction.North: return Direction.South;
                case Direction.South: return Direction.North;
                case Direction.East: return Direction.West;
                case Direction.West: return Direction.East;
                default: return Direction.None;
            }
        }

        private static bool IsInBounds(int col, int row, int colCount, int rowCount)
        {
            return col >= 0 && col < colCount && row >= 0 && row < rowCount;
        }
    }
}
