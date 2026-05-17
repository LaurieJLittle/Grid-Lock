using GridLock.Config;
using GridLock.Core;
using GridLock.Simulation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GridLock.View
{
    public abstract class RoundManagerBase : MonoBehaviour
    {
        [SerializeField] protected RoundViewManager _roundViewManager;
        [SerializeField] protected LevelData _levelData;
        [SerializeField] protected VehicleMovementConfig _vehicleMovementConfig;
        [SerializeField] protected CrossRoadsPrioritization _crossRoadsPrioritization;

        private SimulationManager _simulationManager;
        private SpawnManager _spawnManager;
        protected RoadNetwork _network;
        private float _spawnTimer;
        private float _levelTimer;
        private bool _roundFinished;

        private void Start()
        {
            _network = BuildNetwork();
            _simulationManager = new SimulationManager(_crossRoadsPrioritization);
            _spawnManager = new SpawnManager(_network, _vehicleMovementConfig, new RouteProvider());
            _spawnManager.OnVehicleReadyToSpawn += (vehicle, segment, config) => _simulationManager.AddVehicle(vehicle, segment);
            _roundViewManager.Init(_simulationManager, _spawnManager, _network, _levelData.TimeLimit);
        }

        protected abstract RoadNetwork BuildNetwork();

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
