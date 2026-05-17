using System.Collections.Generic;
using GridLock.Config;
using GridLock.Core;
using GridLock.View;
using UnityEngine;

namespace GridLock.LevelLoader
{
    public class CsvRoundManager : RoundManagerBase
    {
        [SerializeField] private TextAsset _csvFile;
        [SerializeField] private LevelViewBuilder _levelViewBuilder;
        [SerializeField] private RoadNetworkView _roadNetworkView;
        [SerializeField] private CameraController _cameraController;

        protected override RoadNetwork BuildNetwork()
        {
            int[,] grid = RoadGridCsvParser.Parse(_csvFile.text);
            GridAnalysisResult analysis = GridRoadTypeParser.Analyze(grid);
            CsvNetworkBuildData buildData = NetworkExtractor.Extract(grid, analysis);

            var network = new RoadNetwork();
            network.Build(buildData.LayoutData);

            foreach (int spawnId in buildData.SpawnSegmentIds)
            {
                network.RegisterSpawnPoint(spawnId);
            }

            if (buildData.ExitSegmentIds.Count > 0)
            {
                var vehicleIds = new List<VehicleId>();
                foreach (var entry in _levelData.VehicleSpawnEntries)
                {
                    vehicleIds.Add(entry.VehicleConfig.Id);
                }
                network.DistributeExitPoints(buildData.ExitSegmentIds, vehicleIds);
            }

            int rowCount = grid.GetLength(1);
            _levelViewBuilder.BuildViews(buildData, rowCount, _roadNetworkView, analysis);

            if (_cameraController != null)
            {
                var transforms = new List<Transform>();
                foreach (Transform child in _levelViewBuilder.transform)
                {
                    transforms.Add(child);
                }
                _cameraController.FitToTransforms(transforms);
            }

            return network;
        }
    }
}
