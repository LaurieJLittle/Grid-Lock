using System.Collections.Generic;
using GridLock.Config;
using GridLock.Core;
using GridLock.Simulation;
using GridLock.View;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GridLock.LevelLoader
{
    public class CsvRoundManager : MonoBehaviour
    {
        [SerializeField] private TextAsset _csvFile;
        [SerializeField] private LevelData _levelData;
        [SerializeField] private VehicleMovementConfig _vehicleMovementConfig;
        [SerializeField] private CrossRoadsPrioritization _crossRoadsPrioritization;
        [SerializeField] private RoundViewManager _roundViewManager;
        [SerializeField] private LevelViewBuilder _levelViewBuilder;
        [SerializeField] private RoadNetworkView _roadNetworkView;
        [SerializeField] private CameraController _cameraController;

        private SimulationManager _simulationManager;
        private SpawnManager _spawnManager;
        private RoadNetwork _network;
        private float _spawnTimer;
        private float _levelTimer;
        private bool _roundFinished;

        private void Start()
        {
            BuildNetworkFromCsv();
            _simulationManager = new SimulationManager(_crossRoadsPrioritization);
            _spawnManager = new SpawnManager(_network, _vehicleMovementConfig, new RouteProvider());
            _spawnManager.OnVehicleReadyToSpawn += _simulationManager.AddVehicle;
            _roundViewManager.Init(_simulationManager, _spawnManager, _network, _levelData.TimeLimit);
        }

        private void BuildNetworkFromCsv()
        {
            int[,] grid = RoadGridCsvParser.Parse(_csvFile.text);
            GridAnalysisResult analysis = GridRoadTypeParser.Analyze(grid);
            CsvNetworkBuildData buildData = NetworkExtractor.Extract(grid, analysis);

            _network = new RoadNetwork();
            _network.Build(buildData.LayoutData);

            foreach (int spawnId in buildData.SpawnSegmentIds)
            {
                _network.RegisterSpawnPoint(spawnId);
            }

            if (buildData.ExitSegmentIds.Count > 0)
            {
                var vehicleIds = new List<VehicleId>();
                foreach (var entry in _levelData.VehicleSpawnEntries)
                {
                    vehicleIds.Add(entry.VehicleConfig.Id);
                }
                _network.DistributeExitPoints(buildData.ExitSegmentIds, vehicleIds);
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
        }

        private void FixedUpdate()
        {
            if (_roundFinished)
            {
                return;
            }

            _spawnTimer += Time.fixedDeltaTime;
            _levelTimer += Time.fixedDeltaTime;

            if (_levelTimer > _levelData.TimeLimit)
            {
                RoundFinished();
                return;
            }

            _simulationManager.UpdateSimulation(Time.fixedDeltaTime);
            _roundViewManager.UpdateTimer(_levelTimer);
            _spawnManager.Update(Time.fixedDeltaTime);

            if (_spawnTimer >= _levelData.GetSpawnIntervalForTime(_levelTimer))
            {
                _spawnTimer = 0f;
                VehicleConfig vehicleConfig = _levelData.GetRandomVehicleForTime(_levelTimer);
                _spawnManager.QueueSpawn(vehicleConfig);
            }
        }

        private void RoundFinished()
        {
            _roundFinished = true;
            _roundViewManager.OnRoundFinished();
        }

        public void Retry()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
