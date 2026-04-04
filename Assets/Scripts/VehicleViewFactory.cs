using System.Collections.Generic;
using UnityEngine;

public class VehicleViewFactory : MonoBehaviour
{
    [SerializeField] private VehicleView _vehicleViewPrefab;
    [SerializeField] private RoadNetworkView _networkView;

    private readonly Queue<VehicleView> _previewVehicles = new Queue<VehicleView>();

    public void Init(RoadNetwork network, SpawnManager spawnManager)
    {
        foreach (var segment in network.SpawnSegments)
        {
            segment.OnSpawnPending += (time, config) => CreatePreview(config, segment);
        }

        spawnManager.OnSpawnFailed += HandleSpawnFailed;
    }

    public void ActivatePreview(Vehicle vehicle)
    {
        if (_previewVehicles.Count > 0)
        {
            VehicleView preview = _previewVehicles.Dequeue();
            preview.ActivateFromPreview(vehicle, _networkView);
        }
        else
        {
            VehicleView vehicleView = Instantiate(_vehicleViewPrefab);
            vehicleView.SetData(vehicle, _networkView);
        }
    }

    private void CreatePreview(VehicleConfig config, RoadSegment segment)
    {
        _networkView.GetSegmentStartEnd(segment, out Vector3 start, out Vector3 end);
        Vector3 dir = _networkView.GetSegmentForwardDirection(segment);
        Vector3 position = start - dir * config.CentreToFrontDistance;

        VehicleView preview = Instantiate(_vehicleViewPrefab);
        preview.SetPreviewData(config, position, dir);
        _previewVehicles.Enqueue(preview);
    }

    private void HandleSpawnFailed(RoadSegment segment)
    {
        if (_previewVehicles.Count > 0)
        {
            VehicleView preview = _previewVehicles.Dequeue();
            preview.ShowSpawnFailed();
        }
    }
}
