using UnityEngine;

public class RoadSegmentView : MonoBehaviour
{
    [SerializeField] private RoadSegmentConfig _roadSegmentConfig;
    [SerializeField] private Transform _roadStart;
    [SerializeField] private Transform _roadEnd;

    public int Id => _roadSegmentConfig.Id.GetHashCode();
    public Vector3 RoadStartPosition => _roadStart.position;
    public Vector3 RoadEndPosition => _roadEnd.position;
    public float WorldLength { get; private set; }
    public Vector3 ForwardDirection { get; private set; }

    public void BindToNetwork(RoadSegment roadSegment)
    {
        Vector3 delta = _roadEnd.position - _roadStart.position;
        WorldLength = delta.magnitude;
        ForwardDirection = delta / WorldLength;
    }
}
