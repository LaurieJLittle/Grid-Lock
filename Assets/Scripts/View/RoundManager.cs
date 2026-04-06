using System.Collections.Generic;
using GridLock.Config;
using GridLock.Core;
using GridLock.Simulation;
using GridLock.UI;
using UnityEngine;

namespace GridLock.View
{
    // Manages the round, controlling the other main managing classes
    public class RoundManager : MonoBehaviour
    {
        [SerializeField] private RoadNetworkView _networkView;
        [SerializeField] private VehicleViewFactory _vehicleViewFactory;
        [SerializeField] private LevelData _levelData;
        [SerializeField] private ScoreUI _scoreUI;
        [SerializeField] private TimerUI _timerUI;
        [SerializeField] private VehicleMovementConfig _vehicleMovementConfig;
        [SerializeField] private CrossRoadsPrioritization _crossRoadsPrioritization;
        [SerializeField] private CrossRoadsConfig[] _crossRoads;
        [SerializeField] private RoadSegmentConfig[] _roadSegments;
        [SerializeField] private SpawnPointConfig[] _spawnPoints;
        [SerializeField] private ExitPointConfig[] _exitPoints;
        
        private SimulationManager _simulationManager;
        private SpawnManager _spawnManager;
        private RoadNetwork _network;
        private float _spawnTimer;
        private float _levelTimer;
        private bool _roundFinished;
        
        private void Start()
        {
            BuildNetwork();
            _networkView.Init(_network);
            _simulationManager = new SimulationManager(_crossRoadsPrioritization);
            _simulationManager.OnVehicleSpawned += _vehicleViewFactory.ActivatePreview;
            _scoreUI.SetData(_simulationManager);
            _timerUI.SetData(_levelData.TimeLimit);
            
            _spawnManager = new SpawnManager(_network, _vehicleMovementConfig);
            _spawnManager.OnVehicleReadyToSpawn += _simulationManager.AddVehicle;
            _vehicleViewFactory.Init(_network, _spawnManager);
        }
        
        private void BuildNetwork()
        {
            var crossRoadsData = new List<CrossRoadsData>();
            foreach (var crossRoadsConfig in _crossRoads)
            {
                crossRoadsData.Add(crossRoadsConfig.GetCrossRoadsData());
            }
            
            var roadSegmentsData = new List<RoadSegmentData>();
            foreach (var roadSegmentConfig in _roadSegments)
            {
                roadSegmentsData.Add(roadSegmentConfig.GetRoadSegmentData());
            }
            
            var networkLayoutData = new NetworkLayoutData
            {
                CrossRoads = crossRoadsData,
                RoadSegments = roadSegmentsData,
            };
            
            _network = new RoadNetwork();
            _network.Build(networkLayoutData);

            foreach (var spawnPoint in _spawnPoints)
            {
                _network.RegisterSpawnPoint(spawnPoint.RoadSegment.Id.GetHashCode());
            }

            foreach (var exitPoint in _exitPoints)
            {
                _network.RegisterExitPoint(exitPoint.RoadSegment.Id.GetHashCode(), exitPoint.VehicleId);
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
            
            _timerUI.UpdateTimer(_levelTimer);


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
            _timerUI.Hide();
            
            // LL TODO - Add gameover sequence here
        }
    }
}
