using UnityEngine;

public class RoadSegmentView : MonoBehaviour
{
    [SerializeField] private RoadSegmentConfig _roadSegmentConfig;
    [SerializeField] private Transform _roadStart;
    [SerializeField] private Transform _roadEnd;

    public int Id => _roadSegmentConfig.Id.GetHashCode();
    public Vector3 RoadStartPosition => _roadStart.position;
    public Vector3 RoadEndPosition => _roadEnd.position;

    public void BindToNetwork(RoadSegment roadSegment)
    {
    }
}
