using GridLock.Config;
using GridLock.Simulation;
using UnityEngine;

namespace GridLock.View
{
    public class RoadSegmentView : MonoBehaviour
    {
        [SerializeField] private RoadSegmentConfig _roadSegmentConfig;
        [SerializeField] private Transform _roadStart;
        [SerializeField] private Transform _roadEnd;
        [SerializeField] private int _overrideId = int.MinValue;

        public int Id => _overrideId != int.MinValue ? _overrideId : _roadSegmentConfig.Id.GetHashCode();
        public Vector3 RoadStartPosition => _roadStart.position;
        public Vector3 RoadEndPosition => _roadEnd.position;
        public float WorldLength { get; private set; }
        public Vector3 ForwardDirection { get; private set; }

        public void SetIdOverride(int id)
        {
            _overrideId = id;
        }

        public void BindToNetwork(RoadSegment roadSegment)
        {
            Vector3 delta = _roadEnd.position - _roadStart.position;
            WorldLength = delta.magnitude;
            ForwardDirection = delta / WorldLength;
        }
    }
}
