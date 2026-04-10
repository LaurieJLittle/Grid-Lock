using GridLock.Config;
using GridLock.Simulation;
using GridLock.UI;
using UnityEngine;

namespace GridLock.View
{
    public class RoundViewManager : MonoBehaviour
    {
        [SerializeField] private VehicleViewFactory _vehicleViewFactory;
        [SerializeField] private RoadNetworkView _roadNetworkView;
        [SerializeField] private ScoreUI _scoreUI;
        [SerializeField] private TimerUI _timerUI;
        [SerializeField] private ScoreParticleFactory _scoreParticleFactory;
        
        private Camera _mainCamera;

        public void Init(SimulationManager simulationManager, SpawnManager spawnManager, RoadNetwork network, float timeLimit)
        {
            _mainCamera = Camera.main;
            _roadNetworkView.Init(network);
            simulationManager.OnVehicleSpawned += _vehicleViewFactory.ActivatePreview;
            _scoreUI.SetData(simulationManager);
            _timerUI.SetData(timeLimit);
            _vehicleViewFactory.Init(network, spawnManager, this);
        }

        public void SpawnScoreParticle(Vector3 sourceWorldPosition, VehicleId vehicleId, int pointsToAward)
        {
            var screenPos = _scoreUI.transform.position;
            Vector3 end = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            _scoreParticleFactory.Spawn(sourceWorldPosition, end, vehicleId, () => _scoreUI.AddToDisplayScore(pointsToAward));
        }

        public void UpdateTimer(float levelTimer)
        {
            _timerUI.UpdateTimer(levelTimer);
        }

        public void OnRoundFinished()
        {
            _timerUI.Hide();
        }
    }
}
