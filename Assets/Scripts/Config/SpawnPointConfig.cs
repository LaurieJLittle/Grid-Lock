using UnityEngine;

namespace GridLock.Config
{
    public class SpawnPointConfig : MonoBehaviour
    {
        [SerializeField] private RoadSegmentConfig _roadSegment;

        public RoadSegmentConfig RoadSegment => _roadSegment;
    }
}
