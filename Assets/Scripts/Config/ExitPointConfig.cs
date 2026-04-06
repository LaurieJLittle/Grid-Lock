using UnityEngine;

namespace GridLock.Config
{
    public class ExitPointConfig : MonoBehaviour
    {
        [SerializeField] private RoadSegmentConfig _roadSegment;
        [SerializeField] private VehicleId _vehicleId;

        public RoadSegmentConfig RoadSegment => _roadSegment;
        public VehicleId VehicleId => _vehicleId;
    }
}
