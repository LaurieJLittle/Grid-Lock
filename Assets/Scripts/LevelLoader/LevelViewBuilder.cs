using System.Collections.Generic;
using GridLock.View;
using UnityEngine;

namespace GridLock.LevelLoader
{
    public class LevelViewBuilder : MonoBehaviour
    {
        [SerializeField] private GameObject _roadSegmentNsPrefab;
        [SerializeField] private GameObject _roadSegmentEwPrefab;
        [SerializeField] private CrossRoadView _crossRoadsPrefab;
        [SerializeField] private float _crossRoadsWidth = 1f;
        [SerializeField] private float _crossRoadsHeight = 1f;
        [SerializeField] private float _roadCellWidth = 0.5f;
        [SerializeField] private float _roadCellHeight = 0.5f;
        [SerializeField] private float _laneOffset = 0.12f;

        private float[] _colPositions;
        private float[] _rowPositions;
        private float[] _colWidths;
        private float[] _rowHeights;

        public void BuildViews(
            CsvNetworkBuildData buildData,
            int rowCount,
            RoadNetworkView roadNetworkView,
            GridAnalysisResult analysis)
        {
            int colCount = analysis.CellTypes.GetLength(0);
            ComputePositions(analysis.CellTypes, colCount, rowCount);

            var crossRoadViews = new CrossRoadView[buildData.CrossRoadsPositions.Count];
            int crossRoadsIndex = 0;

            foreach (var kvp in buildData.CrossRoadsPositions)
            {
                int id = kvp.Key;
                Vector2Int pos = kvp.Value;
                Vector3 worldPos = GridToWorld(pos);

                CrossRoadView view = Instantiate(_crossRoadsPrefab, worldPos, Quaternion.identity, transform);
                view.SetIdOverride(id);
                crossRoadViews[crossRoadsIndex++] = view;
            }

            var segmentViewList = new List<RoadSegmentView>();

            foreach (var pair in buildData.ConnectionPairs)
            {
                GameObject prefab = pair.IsNorthSouth ? _roadSegmentNsPrefab : _roadSegmentEwPrefab;

                Vector3 fromCenter = GridToWorld(pair.FromPos);
                Vector3 toCenter = GridToWorld(pair.ToPos);
                Vector3 fromWorld = GetCellEdge(pair.FromPos, toCenter, analysis.CellTypes);
                Vector3 toWorld = GetCellEdge(pair.ToPos, fromCenter, analysis.CellTypes);
                Vector3 midpoint = (fromWorld + toWorld) * 0.5f;

                GameObject instance = Instantiate(prefab, transform);
                instance.transform.position = midpoint;

                RoadSegmentView[] views = instance.GetComponentsInChildren<RoadSegmentView>();
                if (views.Length < 2)
                {
                    Debug.LogError("Road prefab must contain two RoadSegmentView components");
                    continue;
                }

                // Prefab child order: views[0] = North/East bound, views[1] = South/West bound
                // Match forward segment to the correct visual based on direction
                int forwardViewIndex;
                int reverseViewIndex;

                if (pair.IsNorthSouth)
                {
                    // views[0] = North bound (bottom→top), views[1] = South bound (top→bottom)
                    bool forwardGoesSouth = fromWorld.y > toWorld.y;
                    forwardViewIndex = forwardGoesSouth ? 1 : 0;
                    reverseViewIndex = forwardGoesSouth ? 0 : 1;
                }
                else
                {
                    // views[0] = East bound (left→right), views[1] = West bound (right→left)
                    bool forwardGoesEast = fromWorld.x < toWorld.x;
                    forwardViewIndex = forwardGoesEast ? 0 : 1;
                    reverseViewIndex = forwardGoesEast ? 1 : 0;
                }

                views[forwardViewIndex].SetIdOverride(pair.ForwardSegmentId);
                segmentViewList.Add(views[forwardViewIndex]);

                views[reverseViewIndex].SetIdOverride(pair.ReverseSegmentId);
                segmentViewList.Add(views[reverseViewIndex]);
            }

            roadNetworkView.SetViews(crossRoadViews, segmentViewList.ToArray());
        }

        private void ComputePositions(CellType[,] cellTypes, int colCount, int rowCount)
        {
            _colPositions = new float[colCount];
            _colWidths = new float[colCount];
            float xCursor = 0f;
            for (int col = 0; col < colCount; col++)
            {
                float width = ColumnHasCrossRoads(cellTypes, col, rowCount)
                    ? _crossRoadsWidth
                    : _roadCellWidth;
                _colWidths[col] = width;
                _colPositions[col] = xCursor + width * 0.5f;
                xCursor += width;
            }

            _rowPositions = new float[rowCount];
            _rowHeights = new float[rowCount];
            float yCursor = 0f;
            for (int row = rowCount - 1; row >= 0; row--)
            {
                float height = RowHasCrossRoads(cellTypes, row, colCount)
                    ? _crossRoadsHeight
                    : _roadCellHeight;
                _rowHeights[row] = height;
                _rowPositions[row] = yCursor + height * 0.5f;
                yCursor += height;
            }
        }

        private static bool ColumnHasCrossRoads(CellType[,] cellTypes, int col, int rowCount)
        {
            for (int row = 0; row < rowCount; row++)
            {
                if (cellTypes[col, row] == CellType.CrossRoads)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool RowHasCrossRoads(CellType[,] cellTypes, int row, int colCount)
        {
            for (int col = 0; col < colCount; col++)
            {
                if (cellTypes[col, row] == CellType.CrossRoads)
                {
                    return true;
                }
            }
            return false;
        }

        private Vector3 GetCellEdge(Vector2Int pos, Vector3 otherCenter, CellType[,] cellTypes)
        {
            Vector3 center = GridToWorld(pos);
            float halfW = _colWidths[pos.x] * 0.5f;
            float halfH = _rowHeights[pos.y] * 0.5f;

            Vector3 dir = otherCenter - center;
            // CrossRoads: offset toward other (inner edge)
            // Road: offset away from other (outer edge)
            float sign = cellTypes[pos.x, pos.y] == CellType.CrossRoads ? 1f : -1f;
            float offsetX = dir.x != 0f ? Mathf.Sign(dir.x) * sign * halfW : 0f;
            float offsetY = dir.y != 0f ? Mathf.Sign(dir.y) * sign * halfH : 0f;

            return center + new Vector3(offsetX, offsetY, 0f);
        }

        private Vector3 GridToWorld(Vector2Int pos)
        {
            return new Vector3(_colPositions[pos.x], _rowPositions[pos.y], 0f);
        }
    }
}
