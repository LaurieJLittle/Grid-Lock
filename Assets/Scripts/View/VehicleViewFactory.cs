using System.Collections.Generic;
using GridLock.Config;
using GridLock.Simulation;
using UnityEngine;

namespace GridLock.View
{
    public class VehicleViewFactory : MonoBehaviour
    {
        [SerializeField] private VehicleView _vehicleViewPrefab;
        [SerializeField] private RoadNetworkView _networkView;

        private readonly Queue<(VehicleView View, VehicleConfig Config)> _previewVehicles = new Queue<(VehicleView, VehicleConfig)>();
        private RoundViewManager _roundViewManager;

        public void Init(IReadOnlyRoadNetwork network, SpawnManager spawnManager, RoundViewManager roundViewManager)
        {
            _roundViewManager = roundViewManager;

            foreach (var segment in network.SpawnSegments)
            {
                segment.OnSpawnPending += (time, config) => CreatePreview(config, segment);
            }

            spawnManager.OnVehicleReadyToSpawn += OnVehicleReadyToSpawn;
            spawnManager.OnSpawnFailed += HandleSpawnFailed;
        }

        private void OnVehicleReadyToSpawn(Vehicle vehicle, RoadSegment segment, VehicleConfig config)
        {
            if (_previewVehicles.Count > 0)
            {
                var preview = _previewVehicles.Dequeue();
                preview.View.ActivateFromPreview(vehicle, preview.Config, _networkView, _roundViewManager);
            }
            else
            {
                VehicleView vehicleView = Instantiate(_vehicleViewPrefab);
                vehicleView.SetData(vehicle, config, _networkView, _roundViewManager);
            }
        }

        private void CreatePreview(VehicleConfig config, RoadSegment segment)
        {
            _networkView.GetSegmentStartEnd(segment, out Vector3 start, out Vector3 end);
            Vector3 dir = _networkView.GetSegmentForwardDirection(segment);
            Vector3 position = start - dir * config.CentreToFrontDistance;

            VehicleView preview = Instantiate(_vehicleViewPrefab);
            preview.SetPreviewData(config, position, dir);
            _previewVehicles.Enqueue((preview, config));
        }

        private void HandleSpawnFailed(RoadSegment segment)
        {
            if (_previewVehicles.Count > 0)
            {
                var preview = _previewVehicles.Dequeue();
                preview.View.ShowSpawnFailed();
            }
        }
    }
}
